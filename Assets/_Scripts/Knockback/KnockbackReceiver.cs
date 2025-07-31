using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Events;

public class KnockbackReceiver : MonoBehaviour, IKnockbackReceiver
{
    [Header("Entity Properties")]
    [SerializeField] private float mass = 1f;
    [SerializeField] private bool canReceiveKnockback = true;
    
    [Header("NavMesh Integration")]
    [SerializeField] private bool hasNavMeshAgent = true;
    [SerializeField] private bool disableNavMeshDuringKnockback = true;
    [SerializeField] private bool warpNavMeshAfterKnockback = true;
    
    [Header("Animation Integration")]
    [SerializeField] private Animator animator;
    [SerializeField] private string knockbackBoolParameter = "isKnockedBack";
    [SerializeField] private string knockbackTriggerParameter = "knockbackTrigger";
    
    [Header("Audio/Visual")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Transform effectSpawnPoint;
    
    [Header("Collision Detection")]
    [SerializeField] private LayerMask wallLayerMask = -1; // What layers count as walls/obstacles
    [SerializeField] private float collisionCheckRadius = 0.5f; // Radius for collision checking
    [SerializeField] private bool enableCollisionDetection = true; // Enable wall collision detection
    
    [Header("Stuck Detection")]
    [SerializeField] private float stuckThreshold = 0.1f; // Min movement to not be considered stuck
    [SerializeField] private float stuckCheckInterval = 0.2f; // How often to check if stuck
    [SerializeField] private float maxStuckTime = 1.0f; // Max time before forcing knockback end
    
    [Header("Events")]
    public UnityEvent OnKnockbackStart;
    public UnityEvent OnKnockbackEnd;
    public UnityEvent<float> OnKnockbackProgress; // Passes 0-1 progress value
    
    // Interface properties
    public bool CanReceiveKnockback => canReceiveKnockback && !isBeingKnockedBack && enabled;
    public float Mass => mass;
    public Transform Transform => transform;
    public bool IsBeingKnockedBack => isBeingKnockedBack;
    
    // Private state
    private bool isBeingKnockedBack = false;
    private NavMeshAgent navMeshAgent;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Coroutine knockbackCoroutine;
    private bool navMeshWasEnabled;
    private float originalNavMeshSpeed;
    
    // Stuck detection state
    private Vector3 lastStuckCheckPosition;
    private float stuckTimer = 0f;
    private Coroutine stuckDetectionCoroutine;
    
    // Cached components
    private Rigidbody rb;
    private CharacterController characterController;
    
    private void Awake()
    {
        // Cache components
        if (hasNavMeshAgent)
            navMeshAgent = GetComponent<NavMeshAgent>();
        
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        
        if (effectSpawnPoint == null)
            effectSpawnPoint = transform;
    }
    
    private void Start()
    {
        // Store original NavMesh settings
        if (navMeshAgent != null)
        {
            originalNavMeshSpeed = navMeshAgent.speed;
        }
    }
    
    public void ReceiveKnockback(Vector3 direction, KnockbackData knockbackData, float multiplier = 1f)
    {
        if (!CanReceiveKnockback) return;
        
        // Stop any existing knockback
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }
        
        // Calculate final distance
        float finalDistance = knockbackData.GetScaledDistance(mass) * multiplier;
        
        // Store positions (Y component unchanged)
        startPosition = transform.position;
        Vector3 proposedTarget = startPosition + new Vector3(direction.x, 0f, direction.z) * finalDistance;
        
        // Validate NavMesh position for enemies
        targetPosition = ValidateNavMeshPosition(proposedTarget, finalDistance, direction);
        
        // Start knockback sequence
        knockbackCoroutine = StartCoroutine(KnockbackSequence(knockbackData));
    }
    
    private IEnumerator KnockbackSequence(KnockbackData data)
    {
        // Mark as being knocked back
        isBeingKnockedBack = true;
        KnockbackManager.Instance.RegisterActiveKnockback(this);
        
        // Initial setup
        OnKnockbackStarted(data);
        
        // Impact pause (for that satisfying "hit" feeling)
        if (data.impactPause > 0f)
        {
            yield return new WaitForSeconds(data.impactPause);
        }
        
        // Disable movement systems
        PrepareForKnockback();
        
        // Start stuck detection
        StartStuckDetection();
        
        // Spawn impact effect
        SpawnImpactEffect(data);
        
        // Play sound
        PlayKnockbackSound(data);
        
        // Execute movement
        yield return StartCoroutine(ExecuteKnockbackMovement(data));
        
        // Recovery pause
        if (data.recoveryTime > 0f)
        {
            yield return new WaitForSeconds(data.recoveryTime);
        }
        
        // Cleanup
        StopStuckDetection();
        OnKnockbackEnded(data);
        RestoreAfterKnockback();
        
        // Mark as finished
        isBeingKnockedBack = false;
        KnockbackManager.Instance.UnregisterActiveKnockback(this);
        knockbackCoroutine = null;
    }
    
    private IEnumerator ExecuteKnockbackMovement(KnockbackData data)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < data.duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / data.duration;
            
            // Evaluate curve
            float curveValue = data.movementCurve.Evaluate(progress);
            
            // Calculate current position (horizontal only)
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, curveValue);
            
            // Preserve original Y position
            currentPos.y = startPosition.y;
            
            // Apply movement based on available components
            ApplyMovement(currentPos);
            
            // Fire progress event
            OnKnockbackProgress.Invoke(progress);
            
            yield return null;
        }
        
        // Ensure final position is set
        Vector3 finalPos = targetPosition;
        finalPos.y = startPosition.y;
        ApplyMovement(finalPos);
    }
    
    private void ApplyMovement(Vector3 targetPos)
    {
        if (characterController != null && characterController.enabled)
        {
            // Use CharacterController.Move
            Vector3 movement = targetPos - transform.position;
            characterController.Move(movement);
        }
        else
        {
            // Direct transform movement
            transform.position = targetPos;
        }
    }
    
    private void PrepareForKnockback()
    {
        // Disable NavMesh
        if (navMeshAgent != null && disableNavMeshDuringKnockback)
        {
            navMeshWasEnabled = navMeshAgent.enabled;
            if (navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.enabled = false;
            }
        }
        
        // Disable Rigidbody physics
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Set animation parameters
        if (animator != null)
        {
            if (!string.IsNullOrEmpty(knockbackBoolParameter))
                animator.SetBool(knockbackBoolParameter, true);
            
            if (!string.IsNullOrEmpty(knockbackTriggerParameter))
                animator.SetTrigger(knockbackTriggerParameter);
        }
    }
    
    private void RestoreAfterKnockback()
    {
        // Restore NavMesh
        if (navMeshAgent != null && disableNavMeshDuringKnockback)
        {
            navMeshAgent.enabled = navMeshWasEnabled;
            if (navMeshAgent.enabled)
            {
                if (warpNavMeshAfterKnockback)
                {
                    // Ensure the final position is valid on NavMesh before warping
                    Vector3 currentPos = transform.position;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(currentPos, out hit, 2.0f, NavMesh.AllAreas))
                    {
                        // Use the closest valid NavMesh position
                        Vector3 safePos = hit.position;
                        safePos.y = currentPos.y; // Preserve Y
                        transform.position = safePos;
                        navMeshAgent.Warp(safePos);
                        Debug.Log($"✅ NavMesh: Safely warped {gameObject.name} to valid NavMesh position after knockback");
                    }
                    else
                    {
                        // Fallback: warp to original position if no valid position found nearby
                        Debug.LogWarning($"⚠️ NavMesh: Could not find valid NavMesh position for {gameObject.name}, warping to start position");
                        navMeshAgent.Warp(startPosition);
                        transform.position = startPosition;
                    }
                }
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = originalNavMeshSpeed;
            }
        }
        
        // Restore Rigidbody
        if (rb != null)
        {
            rb.isKinematic = false;
        }
        
        // Reset animation
        if (animator != null && !string.IsNullOrEmpty(knockbackBoolParameter))
        {
            animator.SetBool(knockbackBoolParameter, false);
        }
    }
    
    private void SpawnImpactEffect(KnockbackData data)
    {
        if (data.impactEffect != null && effectSpawnPoint != null)
        {
            Instantiate(data.impactEffect, effectSpawnPoint.position, effectSpawnPoint.rotation);
        }
    }
    
    private void PlayKnockbackSound(KnockbackData data)
    {
        if (data.knockbackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(data.knockbackSound);
        }
    }
    
    public void OnKnockbackStarted(KnockbackData data)
    {
        OnKnockbackStart.Invoke();
    }
    
    public void OnKnockbackEnded(KnockbackData data)
    {
        OnKnockbackEnd.Invoke();
        
        // ENHANCED: Coordinate with knockdown state for enemies
        if (gameObject.CompareTag("Enemy"))
        {
            HandleEnemyKnockbackEnd();
        }
    }
    
    /// <summary>
    /// Handle knockback end for enemy entities - coordinate with knockdown state
    /// </summary>
    private void HandleEnemyKnockbackEnd()
    {
        // For Enemy_Basic
        if (TryGetComponent(out Enemy_Basic enemyBasic))
        {
            // If the enemy is still knocked down animation-wise, let the animation handle the reset
            if (enemyBasic.anim != null && enemyBasic.anim.GetBool("isKnockedDown"))
            {
                Debug.Log($"🔄 Knockback ended for {gameObject.name} - keeping isKnockedDown=true until animation calls GetUp()");
                // Don't reset isKnockedDown here - let the animation event handle it via GetUp()
            }
            else
            {
                // If not in knockdown animation, restore movement immediately
                if (!enemyBasic.isStunned && !enemyBasic.dead && enemyBasic.agent != null && 
                    enemyBasic.agent.enabled && enemyBasic.agent.isOnNavMesh)
                {
                    enemyBasic.agent.isStopped = false;
                    enemyBasic.agent.velocity = Vector3.zero;
                    enemyBasic.agent.speed = enemyBasic.setSpeed;
                    Debug.Log($"✅ Knockback ended for {gameObject.name} - movement restored (no knockdown)");
                }
            }
        }
        // For Worker
        else if (TryGetComponent(out Worker worker))
        {
            if (worker.anim != null && worker.anim.GetBool("isKnockedDown"))
            {
                Debug.Log($"🔄 Knockback ended for Worker {gameObject.name} - keeping isKnockedDown=true until animation calls GetUp()");
            }
        }
    }
    
    /// <summary>
    /// Validates and adjusts knockback target position to ensure it stays on NavMesh and doesn't hit walls
    /// </summary>
    private Vector3 ValidateNavMeshPosition(Vector3 proposedTarget, float originalDistance, Vector3 direction)
    {
        // If entity doesn't have NavMesh agent, still check for wall collisions
        if (navMeshAgent == null || !hasNavMeshAgent)
        {
            if (enableCollisionDetection)
            {
                return ValidateCollisionPath(proposedTarget, direction, originalDistance);
            }
            return proposedTarget;
        }
        
        // First check for wall collisions along the path
        if (enableCollisionDetection)
        {
            Vector3 collisionSafeTarget = ValidateCollisionPath(proposedTarget, direction, originalDistance);
            
            // If collision detection reduced the distance, use that target for NavMesh validation
            if (Vector3.Distance(startPosition, collisionSafeTarget) < Vector3.Distance(startPosition, proposedTarget))
            {
                proposedTarget = collisionSafeTarget;
                Debug.Log($"🚧 Collision: Reduced knockback distance for {gameObject.name} due to wall collision");
            }
        }
        
        // Check if proposed target is on NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(proposedTarget, out hit, 1.0f, NavMesh.AllAreas))
        {
            // Target is valid, use the closest point on NavMesh for precision
            Vector3 validTarget = hit.position;
            validTarget.y = startPosition.y; // Preserve original Y
            
            // Check if we had to adjust the position significantly
            float adjustmentDistance = Vector3.Distance(proposedTarget, validTarget);
            if (adjustmentDistance > 0.5f)
            {
                Debug.Log($"🔧 NavMesh: Adjusted knockback target for {gameObject.name} by {adjustmentDistance:F2} units to stay on NavMesh");
            }
            
            return validTarget;
        }
        
        // Target is invalid, find maximum valid distance in the knockback direction
        Debug.Log($"⚠️ NavMesh: Proposed knockback target for {gameObject.name} is off NavMesh, finding safe alternative");
        Vector3 safestTarget = FindMaxValidKnockbackDistance(direction, originalDistance);
        
        float reducedDistance = Vector3.Distance(startPosition, safestTarget);
        Debug.Log($"🛡️ NavMesh: Reduced knockback distance for {gameObject.name} from {originalDistance:F2} to {reducedDistance:F2} units");
        
        return safestTarget;
    }
    
    /// <summary>
    /// Validates the collision path to ensure knockback doesn't go through walls
    /// </summary>
    private Vector3 ValidateCollisionPath(Vector3 proposedTarget, Vector3 direction, float maxDistance)
    {
        Vector3 origin = startPosition;
        origin.y += collisionCheckRadius; // Raise origin slightly to check at character height
        
        Vector3 targetDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        float totalDistance = Vector3.Distance(startPosition, proposedTarget);
        
        // Perform spherecast to check for walls along the path
        RaycastHit hit;
        if (Physics.SphereCast(origin, collisionCheckRadius, targetDirection, out hit, totalDistance, wallLayerMask))
        {
            // Hit a wall, reduce the distance to just before the collision
            float safeDistance = Mathf.Max(0.1f, hit.distance - collisionCheckRadius);
            Vector3 safeTarget = startPosition + targetDirection * safeDistance;
            safeTarget.y = startPosition.y; // Preserve Y position
            
            Debug.Log($"🛑 Wall detected for {gameObject.name}! Reduced knockback from {totalDistance:F2} to {safeDistance:F2} units");
            return safeTarget;
        }
        
        // No collision detected, return original target
        return proposedTarget;
    }
    
    /// <summary>
    /// Finds the maximum valid knockback distance along the given direction
    /// </summary>
    private Vector3 FindMaxValidKnockbackDistance(Vector3 direction, float maxDistance)
    {
        float searchDistance = maxDistance;
        Vector3 testPosition;
        NavMeshHit hit;
        
        // Binary search to find maximum valid distance
        float minDistance = 0f;
        float maxValidDistance = 0f;
        
        // Start with smaller increments and work up
        for (int attempts = 0; attempts < 10; attempts++)
        {
            searchDistance = (minDistance + maxDistance) * 0.5f;
            testPosition = startPosition + new Vector3(direction.x, 0f, direction.z) * searchDistance;
            
            if (NavMesh.SamplePosition(testPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                // This distance is valid, try going further
                minDistance = searchDistance;
                maxValidDistance = searchDistance;
            }
            else
            {
                // This distance is too far, reduce maximum
                maxDistance = searchDistance;
            }
            
            // If we've found a good enough approximation, break
            if (Mathf.Abs(maxDistance - minDistance) < 0.1f)
                break;
        }
        
        // Use the maximum valid distance found
        Vector3 finalTarget = startPosition + new Vector3(direction.x, 0f, direction.z) * maxValidDistance;
        
        // Final validation and adjustment
        if (NavMesh.SamplePosition(finalTarget, out hit, 2.0f, NavMesh.AllAreas))
        {
            finalTarget = hit.position;
        }
        
        // Preserve Y position
        finalTarget.y = startPosition.y;
        
        return finalTarget;
    }
    
    /// <summary>
    /// Starts stuck detection to monitor if entity gets stuck during knockback
    /// </summary>
    private void StartStuckDetection()
    {
        StopStuckDetection(); // Stop any existing detection
        lastStuckCheckPosition = transform.position;
        stuckTimer = 0f;
        stuckDetectionCoroutine = StartCoroutine(StuckDetectionLoop());
    }
    
    /// <summary>
    /// Stops stuck detection
    /// </summary>
    private void StopStuckDetection()
    {
        if (stuckDetectionCoroutine != null)
        {
            StopCoroutine(stuckDetectionCoroutine);
            stuckDetectionCoroutine = null;
        }
        stuckTimer = 0f;
    }
    
    /// <summary>
    /// Coroutine that monitors for stuck entities during knockback
    /// </summary>
    private IEnumerator StuckDetectionLoop()
    {
        while (isBeingKnockedBack)
        {
            yield return new WaitForSeconds(stuckCheckInterval);
            
            // Check if we've moved enough since last check
            float distanceMoved = Vector3.Distance(transform.position, lastStuckCheckPosition);
            
            if (distanceMoved < stuckThreshold)
            {
                // We haven't moved enough, increment stuck timer
                stuckTimer += stuckCheckInterval;
                
                if (stuckTimer >= maxStuckTime)
                {
                    // We've been stuck too long, force end the knockback
                    Debug.LogWarning($"🚨 Entity {gameObject.name} stuck during knockback for {stuckTimer:F2}s! Force ending knockback.");
                    ForceStopKnockback();
                    yield break;
                }
            }
            else
            {
                // We moved enough, reset stuck timer
                stuckTimer = 0f;
                lastStuckCheckPosition = transform.position;
            }
        }
    }
    
    /// <summary>
    /// Force stop current knockback (for external interruption)
    /// </summary>
    public void ForceStopKnockback()
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }
        
        // Stop stuck detection
        StopStuckDetection();
        
        if (isBeingKnockedBack)
        {
            RestoreAfterKnockback();
            isBeingKnockedBack = false;
            KnockbackManager.Instance.UnregisterActiveKnockback(this);
        }
    }
    
    /// <summary>
    /// Temporarily disable knockback reception
    /// </summary>
    public void SetKnockbackEnabled(bool enabled)
    {
        canReceiveKnockback = enabled;
    }
    
    private void OnValidate()
    {
        mass = Mathf.Max(0.1f, mass);
    }
    
    private void OnDisable()
    {
        ForceStopKnockback();
    }
} 