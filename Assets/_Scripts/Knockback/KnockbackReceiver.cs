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
        targetPosition = startPosition + new Vector3(direction.x, 0f, direction.z) * finalDistance;
        
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
                    navMeshAgent.Warp(transform.position);
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
    /// Force stop current knockback (for external interruption)
    /// </summary>
    public void ForceStopKnockback()
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }
        
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