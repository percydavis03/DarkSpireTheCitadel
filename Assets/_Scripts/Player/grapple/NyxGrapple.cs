using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NyxGrapple : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float grappleRange = 10f;
    public float grappleAngle = 120f;
    // public LayerMask grappleLayerMask = -1; // No longer needed - using tag-based detection only
    public Transform nyxTransform;
    public Transform grappleOrigin;
    public Animator nyxAnimator;
    
    [Header("Targeting System")]
    [SerializeField] private NyxTargetingSystem targetingSystem; // Reference to shared targeting system
    
    [Header("Input System")]
    public PlayerInputActions playerControls;
    private InputAction grapple;
    
    [Header("Hand/Joint Settings")]
    public Transform grappleJoint; // The hand/joint that reaches to targets
    public float jointMoveSpeed = 10f;
    public Collider jointTrigger; // Collider on the joint to detect when it reaches targets
    public bool useIK = false; // IK approach (doesn't work well with mixamo rigs)
    public bool disableAnimatorDuringGrapple = false; // Keep animator running for body animation
    public bool overrideHandInLateUpdate = true; // Override hand position after animation runs
    
    [Header("Pull Settings")]
    public float minDistanceFromOrigin = 2f;
    public float pullForce = 20000f; // Much stronger force for heavy objects
    public float maxPullSpeed = 10f;
    public bool useSpringJoint = false; // Keep false to see manual pulling
    public float springStrength = 50f;
    public float springDamper = 5f;
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public bool showDebugRay = true;
    public bool useGizmos = true;
    public Color debugRayColor = Color.red;
    public Color debugHitColor = Color.green;
    
    // State Variables
    private bool isGrappling = false;           // Animation is playing
    private bool isGrappleableInRange = false;  // Valid target detected
    private bool isHandReaching = false;        // Hand moving to target
    private bool isPulling = false;             // Actively pulling target
    
    // Target Information
    private ITargetable currentTargetInterface; // New interface-based target
    private Grappleable currentTarget; // Keep for backward compatibility
    private Rigidbody currentTargetRigidbody;
    private RaycastHit lastHit;
    private Vector3 originalJointPosition;
    private Quaternion originalJointRotation;
    private Transform originalJointParent;
    private SpringJoint currentSpringJoint;
    
    // Hand override tracking
    private bool shouldOverrideHand = false;
    private bool isHandAttachedToTarget = false;
    private Vector3 targetHandPosition;
    private Quaternion targetHandRotation;
    private Vector3 handRestPosition;
    private Quaternion handRestRotation;
    private Transform attachedGrapplePoint;
    
    void Awake()
    {
        // Initialize PlayerInputActions
        playerControls = new PlayerInputActions();
        
        SetupReferences();
        StoreOriginalJointTransform();
        SetupTargetingSystem();
    }
    
    void OnEnable()
    {
        // Enable grapple input action
        grapple = playerControls.General.Grapple;
        grapple.Enable();
        
        ResetAllStates();
    }
    
    void OnDisable()
    {
        // Disable grapple input action
        if (grapple != null)
        {
            grapple.Disable();
        }
    }
    
    void Update()
    {
        // TEMP: Create test cube with T key
        if (Input.GetKeyDown(KeyCode.T))
        {
            CreateTestCube();
        }
        
        DetectGrappleableInRange();
        HandleGrappleInput();
        UpdateAnimator();
        CheckPullingConditions();
    }
    
    void CreateTestCube()
    {
        Vector3 spawnPosition = grappleOrigin.position + grappleOrigin.forward * 3f + Vector3.up * 1f;
        
        // Create test cube
        GameObject testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testCube.name = "GRAPPLE TEST CUBE";
        testCube.transform.position = spawnPosition;
        testCube.transform.localScale = Vector3.one * 0.5f;
        
        // Set tag
        testCube.tag = "CanBeGrappled";
        
        // Add components
        Grappleable grappleable = testCube.AddComponent<Grappleable>();
        grappleable.canBeGrappled = true;
        grappleable.pullable = true;
        
        Rigidbody rb = testCube.AddComponent<Rigidbody>();
        rb.mass = 1f;
        
        // Make it bright green so it's visible
        Renderer renderer = testCube.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.green;
            renderer.material = mat;
        }
        
        Debug.Log($"✅ Created grapple test cube at {spawnPosition}");
        Debug.Log("🎯 Stand close to it, look at it, and hold G!");
    }
    
    void LateUpdate()
    {
        // Override hand position after animator runs (so body animation plays normally)
        if (shouldOverrideHand && grappleJoint != null)
        {
            // If hand is attached to target, follow the grapple point
            if (isHandAttachedToTarget && attachedGrapplePoint != null)
            {
                grappleJoint.position = attachedGrapplePoint.position;
                
                // Look towards the grapple origin to maintain natural hand orientation
                Vector3 directionToOrigin = (grappleOrigin.position - attachedGrapplePoint.position).normalized;
                if (directionToOrigin != Vector3.zero)
                {
                    grappleJoint.rotation = Quaternion.LookRotation(directionToOrigin);
                }
                
                // Debug every 30 frames to show attachment is working
                if (enableDebugLogs && Time.frameCount % 30 == 0)
                {
                    Debug.Log($"Hand following attached target: {attachedGrapplePoint.position}");
                }
            }
            else
            {
                // Normal reaching movement (hand moving to target)
                grappleJoint.position = targetHandPosition;
                grappleJoint.rotation = targetHandRotation;
                
                // Debug every 30 frames to show override is working
                if (enableDebugLogs && Time.frameCount % 30 == 0)
                {
                    Debug.Log($"Hand reaching to target: {targetHandPosition}");
                }
            }
        }
    }
    
    void SetupReferences()
    {
        if (nyxTransform != null)
            grappleOrigin = nyxTransform;
        else if (grappleOrigin == null)
            grappleOrigin = transform;
            
        // Setup joint trigger if it exists
        if (jointTrigger != null)
        {
            jointTrigger.isTrigger = true;
            // Add GrappleJointTrigger component if it doesn't exist
            if (jointTrigger.GetComponent<GrappleJointTrigger>() == null)
            {
                GrappleJointTrigger triggerScript = jointTrigger.gameObject.AddComponent<GrappleJointTrigger>();
                triggerScript.Initialize(this);
            }
        }
    }
    
    void StoreOriginalJointTransform()
    {
        if (grappleJoint != null)
        {
            originalJointPosition = grappleJoint.localPosition;
            originalJointRotation = grappleJoint.localRotation;
            originalJointParent = grappleJoint.parent;
            
            // Initialize target hand position to current position
            targetHandPosition = grappleJoint.position;
            targetHandRotation = grappleJoint.rotation;
        }
    }
    
    void ResetAllStates()
    {
        isGrappling = false;
        isGrappleableInRange = false;
        isHandReaching = false;
        isPulling = false;
        currentTarget = null;
        currentTargetRigidbody = null;
        
        // Stop hand override and detach from target
        shouldOverrideHand = false;
        isHandAttachedToTarget = false;
        attachedGrapplePoint = null;
        
        // Restore animator if it was disabled
        if (disableAnimatorDuringGrapple && nyxAnimator != null && !nyxAnimator.enabled)
        {
            nyxAnimator.enabled = true;
            if (enableDebugLogs) Debug.Log("Restored animator");
        }
        
        CleanupSpringJoint();
        ResetJointPosition();
        
        if (enableDebugLogs) Debug.Log("NyxGrapple: All states reset");
    }
    
    void SetupTargetingSystem()
    {
        // Get or create targeting system
        if (targetingSystem == null)
        {
            targetingSystem = GetComponent<NyxTargetingSystem>();
            if (targetingSystem == null)
            {
                Debug.LogWarning("NyxGrapple: No NyxTargetingSystem found. Grapple targeting will use fallback method.");
                return;
            }
        }
        
        // Configure targeting system for grapple-specific parameters
        targetingSystem.ConfigureTargeting(
            grappleRange, 
            grappleAngle, 
            new TargetType[] { TargetType.Grappleable, TargetType.Enemy }, // Allow both grappleable objects and enemies
            true // Require line of sight
        );
        
        if (enableDebugLogs)
            Debug.Log("NyxGrapple: Targeting system configured for grapple parameters");
    }
    
    void DetectGrappleableInRange()
    {
        // If we're already grappling a target, keep it locked during the process
        if (isGrappling && currentTarget != null && (isHandReaching || isPulling))
        {
            isGrappleableInRange = true;
            if (enableDebugLogs && !isGrappleableInRange) 
                Debug.Log($"Target locked during grappling: {currentTarget.name}");
            return;
        }
        
        bool wasInRange = isGrappleableInRange;
        isGrappleableInRange = false;
        
        // TEMPORARILY DISABLE targeting system to test fallback detection
        // Use targeting system if available
        if (false) // targetingSystem != null)
        {
            // Get best grappleable target from targeting system
            var bestTarget = targetingSystem.BestTarget;
            
            if (bestTarget != null && (bestTarget.TargetType == TargetType.Grappleable || bestTarget.TargetType == TargetType.Enemy))
            {
                // Since Grappleable now implements ITargetable directly, we can cast it directly
                Grappleable grappleable = bestTarget as Grappleable;
                
                if (grappleable != null && grappleable.canBeGrappled && !grappleable.IsBeingGrappled)
                {
                    currentTargetInterface = bestTarget;
                    currentTarget = grappleable;
                    isGrappleableInRange = true;
                    
                    // Create a fake raycast hit for backwards compatibility
                    lastHit = new RaycastHit();
                }
            }
        }
        else
        {
            // Fallback to original detection method if no targeting system
            DetectGrappleableInRangeFallback();
        }
        
        // Log state changes
        if (enableDebugLogs && wasInRange != isGrappleableInRange)
        {
            Debug.Log($"Grappleable in range: {isGrappleableInRange} - Target: {(currentTarget ? currentTarget.name : "None")}");
        }
        
        if (!isGrappleableInRange)
        {
            currentTarget = null;
            currentTargetInterface = null;
        }
    }
    
    void DetectGrappleableInRangeFallback()
    {
        // Original detection method as fallback
        Vector3 rayOrigin = grappleOrigin.position;
        Vector3 rayDirection = grappleOrigin.forward;
        
        Collider[] colliders = Physics.OverlapSphere(rayOrigin, grappleRange);
        
        // DEBUG: Show what we're finding
        if (enableDebugLogs && colliders.Length > 0)
        {
            Debug.Log($"FALLBACK: Found {colliders.Length} objects within {grappleRange} units");
            foreach (Collider col in colliders)
            {
                float distance = Vector3.Distance(col.transform.position, rayOrigin);
                Vector3 directionToTarget = (col.transform.position - rayOrigin).normalized;
                float angleToTarget = Vector3.Angle(rayDirection, directionToTarget);
                bool hasCorrectTag = col.CompareTag("CanBeGrappled") || col.CompareTag("Enemy");
                Grappleable grappleable = col.GetComponent<Grappleable>();
                bool hasGrappleable = grappleable != null;
                bool canBeGrappled = hasGrappleable && grappleable.canBeGrappled;
                Debug.Log($"  - {col.name}: Distance={distance:F1}, Angle={angleToTarget:F1}°, Tag={col.tag}, ValidTag={hasCorrectTag}, HasGrappleable={hasGrappleable}, CanBeGrappled={canBeGrappled}");
            }
        }
        
        float closestDistance = float.MaxValue;
        RaycastHit closestHit = new RaycastHit();
        bool validTargetFound = false;
        
        foreach (Collider collider in colliders)
        {
            Vector3 directionToTarget = (collider.transform.position - rayOrigin).normalized;
            float angleToTarget = Vector3.Angle(rayDirection, directionToTarget);
            
            if (angleToTarget <= grappleAngle && (collider.CompareTag("CanBeGrappled") || collider.CompareTag("Enemy")))
            {
                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, directionToTarget, grappleRange);
                
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider == collider)
                    {
                        // Try to get Grappleable component - check collider and parent
                        Grappleable grappleable = hit.collider.GetComponent<Grappleable>();
                        if (grappleable == null)
                        {
                            grappleable = hit.collider.GetComponentInParent<Grappleable>();
                        }
                        
                        if (grappleable != null && grappleable.canBeGrappled && !grappleable.IsBeingGrappled)
                        {
                            if (hit.distance < closestDistance)
                            {
                                closestDistance = hit.distance;
                                closestHit = hit;
                                validTargetFound = true;
                                currentTarget = grappleable;
                                if (enableDebugLogs) Debug.Log($"FALLBACK: Valid target found - {grappleable.name}");
                            }
                        }
                        break;
                    }
                }
            }
        }
        
        isGrappleableInRange = validTargetFound;
        lastHit = closestHit;
        
        if (enableDebugLogs && !validTargetFound && colliders.Length > 0)
        {
            Debug.LogWarning("FALLBACK: Objects found but none are valid grappleable targets!");
        }
    }
    
    void HandleGrappleInput()
    {
        // DEBUG: Show what's happening with input
        if (enableDebugLogs && grapple.IsPressed())
        {
            Debug.Log($"G key held - isGrappling: {isGrappling}, targetInRange: {isGrappleableInRange}, currentTarget: {(currentTarget ? currentTarget.name : "None")}");
        }
        
        // Grapple key pressed - start grappling animation
        if (grapple.WasPressedThisFrame() && !isGrappling)
        {
            if (enableDebugLogs) Debug.Log("G key pressed - attempting to start grapple");
            StartGrappling();
        }
        
        // Grapple key released - stop everything
        if (grapple.WasReleasedThisFrame() && isGrappling)
        {
            if (enableDebugLogs) Debug.Log("G key released - stopping grapple");
            StopGrappling();
        }
        
        // If grappling and target in range, start reaching
        if (isGrappling && isGrappleableInRange && !isHandReaching && !isPulling)
        {
            StartReachingToTarget();
        }
    }
    
    void StartGrappling()
    {
        isGrappling = true;
        
        if (enableDebugLogs) Debug.Log("Grappling started - animation triggered");
        
        // DEBUG: Show grapple joint status
        if (enableDebugLogs) 
        {
            Debug.Log($"Grapple Joint: {(grappleJoint != null ? grappleJoint.name : "NULL")}");
            Debug.Log($"Target in range: {isGrappleableInRange}");
            Debug.Log($"Current target: {(currentTarget != null ? currentTarget.name : "NULL")}");
        }
        
        // Notify target if in range
        if (isGrappleableInRange && currentTarget != null)
        {
            currentTarget.StartGrapple();
            currentTargetRigidbody = currentTarget.GetComponent<Rigidbody>();
            if (enableDebugLogs) 
            {
                Debug.Log($"Target setup: {currentTarget.name}, HasRigidbody: {currentTargetRigidbody != null}");
                if (currentTargetRigidbody != null)
                {
                    Debug.Log($"Target Rigidbody - Mass: {currentTargetRigidbody.mass}, Drag: {currentTargetRigidbody.drag}, AngularDrag: {currentTargetRigidbody.angularDrag}");
                    Debug.Log($"Target Rigidbody - IsKinematic: {currentTargetRigidbody.isKinematic}, UseGravity: {currentTargetRigidbody.useGravity}");
                }
            }
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning($"No valid target found! isGrappleableInRange: {isGrappleableInRange}, currentTarget: {(currentTarget != null ? currentTarget.name : "NULL")}");
        }
    }
    
    void StopGrappling()
    {
        if (enableDebugLogs) Debug.Log("Grappling stopped - resetting all states");
        
        // Notify current target
        if (currentTarget != null)
        {
            currentTarget.ReleaseGrapple();
        }
        
        StopAllCoroutines();
        ResetAllStates();
    }
    
    void StartReachingToTarget()
    {
        if (currentTarget == null || grappleJoint == null) 
        {
            if (enableDebugLogs) Debug.Log("Cannot start reaching - missing target or joint");
            return;
        }
        
        isHandReaching = true;
        
        // Store original hand position
        if (grappleJoint != null)
        {
            handRestPosition = grappleJoint.position;
            handRestRotation = grappleJoint.rotation;
        }
        
        // Enable hand override (LateUpdate will handle positioning)
        if (overrideHandInLateUpdate)
        {
            shouldOverrideHand = true;
            if (enableDebugLogs) Debug.Log("Enabled hand override in LateUpdate - animation will play on body");
        }
        else if (disableAnimatorDuringGrapple && nyxAnimator != null)
        {
            nyxAnimator.enabled = false;
            if (enableDebugLogs) Debug.Log("Disabled animator for manual hand control");
        }
        
        if (enableDebugLogs) Debug.Log($"Hand reaching to target: {currentTarget.name}");
        
        Transform targetPoint = currentTarget.grapplePoint != null ? currentTarget.grapplePoint : currentTarget.transform;
        StartCoroutine(MoveJointToTarget(targetPoint));
    }
    
    IEnumerator MoveJointToTarget(Transform targetPoint)
    {
        if (grappleJoint == null)
        {
            if (enableDebugLogs) Debug.LogError("GrappleJoint is NULL! Please assign the hand/wrist transform in the inspector.");
            yield break;
        }
        
        Vector3 startPosition = grappleJoint.position;
        Vector3 targetPosition = targetPoint.position;
        
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float journeyTime = journeyLength / jointMoveSpeed;
        float elapsedTime = 0;
        
        if (enableDebugLogs) 
        {
            Debug.Log($"Moving joint: '{grappleJoint.name}'");
            Debug.Log($"From: {startPosition} To: {targetPosition}");
            Debug.Log($"Distance={journeyLength:F2}, Time={journeyTime:F2}, Speed={jointMoveSpeed}");
        }
        
        // If journey time is too small, the movement might be instant
        if (journeyTime < 0.1f)
        {
            if (enableDebugLogs) Debug.LogWarning($"Journey time is very short ({journeyTime:F3}s) - movement might appear instant! Try reducing jointMoveSpeed to 5-15 in inspector.");
        }
        
        // Warn if speed is still too high
        if (jointMoveSpeed > 20f && enableDebugLogs)
        {
            Debug.LogWarning($"jointMoveSpeed is {jointMoveSpeed} - this might be too fast to see! Try 5-15 for visible movement.");
        }
        
        while (elapsedTime < journeyTime && isHandReaching)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / journeyTime;
            
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, progress);
            
            // Set target position for LateUpdate override OR direct movement
            if (overrideHandInLateUpdate)
            {
                targetHandPosition = currentPos;
                Vector3 direction = (targetPosition - currentPos).normalized;
                if (direction != Vector3.zero)
                {
                    targetHandRotation = Quaternion.LookRotation(direction);
                }
            }
            else
            {
                grappleJoint.position = currentPos;
                Vector3 direction = (targetPosition - grappleJoint.position).normalized;
                if (direction != Vector3.zero)
                {
                    grappleJoint.rotation = Quaternion.LookRotation(direction);
                }
            }
            
            // Debug every few frames to see progress
            if (enableDebugLogs && Time.frameCount % 10 == 0)
            {
                Debug.Log($"Joint moving: Progress={progress:F2}, Pos={currentPos}");
            }
            
            // Draw a debug line from original position to current position (breadcrumb trail)
            Debug.DrawLine(startPosition, currentPos, Color.cyan, 0.1f);
            Debug.DrawLine(currentPos, targetPosition, Color.red, 0.1f);
            
            yield return null;
        }
        
        if (isHandReaching)
        {
            // Set final position
            if (overrideHandInLateUpdate)
            {
                targetHandPosition = targetPosition;
                targetHandRotation = Quaternion.LookRotation((targetPosition - grappleOrigin.position).normalized);
            }
            else
            {
                grappleJoint.position = targetPosition;
            }
            
            if (enableDebugLogs) Debug.Log("Hand reached target position - triggering joint reached");
            
            // Manually trigger the next step since we might not have a proper trigger collider setup
            if (currentTarget != null)
            {
                OnJointReachedTarget(currentTarget);
            }
        }
        else
        {
            if (enableDebugLogs) Debug.Log("Hand movement was interrupted");
        }
    }
    
    // Called by GrappleJointTrigger when hand reaches target OR manually from MoveJointToTarget
    public void OnJointReachedTarget(Grappleable target)
    {
        if (!isHandReaching || target != currentTarget) 
        {
            if (enableDebugLogs) Debug.Log($"Joint reached ignored - isHandReaching:{isHandReaching}, correctTarget:{target == currentTarget}");
            return;
        }
        
        if (enableDebugLogs) Debug.Log($"Joint trigger reached target: {target.name}");
        
        isHandReaching = false;
        
        // Attach hand to the target's grapple point
        if (overrideHandInLateUpdate)
        {
            isHandAttachedToTarget = true;
            attachedGrapplePoint = target.grapplePoint != null ? target.grapplePoint : target.transform;
            if (enableDebugLogs) Debug.Log($"Hand attached to grapple point: {attachedGrapplePoint.name}");
        }
        
        // Only start pulling if target has rigidbody AND is pullable
        if (currentTargetRigidbody != null && currentTarget.IsPullable)
        {
            StartPulling();
        }
        else
        {
            if (enableDebugLogs) 
            {
                if (currentTargetRigidbody == null)
                    Debug.Log("Target has no rigidbody - no pulling required");
                else if (!currentTarget.IsPullable)
                    Debug.Log("Target is not pullable - no pulling required");
            }
            // For non-pullable objects, they handle their own logic when the joint reaches them
        }
    }
    
    void StartPulling()
    {
        if (currentTargetRigidbody == null) 
        {
            if (enableDebugLogs) Debug.Log("Cannot start pulling - no rigidbody");
            return;
        }
        
        isPulling = true;
        
        if (enableDebugLogs) Debug.Log("Started pulling target");
        
        if (useSpringJoint)
        {
            CreateSpringJoint();
        }
        
        StartCoroutine(PullTargetCoroutine());
    }
    
    IEnumerator PullTargetCoroutine()
    {
        while (isPulling && currentTargetRigidbody != null && isGrappling)
        {
            float currentDistance = Vector3.Distance(currentTarget.transform.position, grappleOrigin.position);
            
            // Check if reached minimum distance
            if (currentDistance <= minDistanceFromOrigin)
            {
                if (enableDebugLogs) Debug.Log("Target reached minimum distance - stopping pull");
                StopGrappling();
                yield break;
            }
            
            // Apply pulling force (only if not using spring joint)
            if (!useSpringJoint)
            {
                Vector3 directionToOrigin = (grappleOrigin.position - currentTarget.transform.position).normalized;
                float distanceFactor = Mathf.Clamp01((currentDistance - minDistanceFromOrigin) / grappleRange);
                
                // Use stronger force calculation
                Vector3 pullForceVector = directionToOrigin * pullForce * distanceFactor;
                
                // Try both Force and Impulse modes for stronger effect
                currentTargetRigidbody.AddForce(pullForceVector, ForceMode.Force);
                
                // Also add a small impulse for immediate effect
                if (currentTargetRigidbody.velocity.magnitude < 1f)
                {
                    currentTargetRigidbody.AddForce(pullForceVector * 0.1f, ForceMode.Impulse);
                }
                
                // Debug the forces being applied
                if (enableDebugLogs && Time.fixedTime % 0.5f < Time.fixedDeltaTime)
                {
                    Debug.Log($"Pulling: Distance={currentDistance:F2}, Force={pullForceVector.magnitude:F2}, Velocity={currentTargetRigidbody.velocity.magnitude:F2}, Mass={currentTargetRigidbody.mass}");
                }
                
                // Limit velocity
                if (currentTargetRigidbody.velocity.magnitude > maxPullSpeed)
                {
                    currentTargetRigidbody.velocity = currentTargetRigidbody.velocity.normalized * maxPullSpeed;
                }
                
                // Reduce drag factor (was 0.98f, now 0.995f for less resistance)
                currentTargetRigidbody.velocity *= 0.995f;
            }
            
            yield return new WaitForFixedUpdate();
        }
        
        if (enableDebugLogs) Debug.Log("Pull coroutine ended");
    }
    
    void CheckPullingConditions()
    {
        // Auto-stop if target gets too close while pulling
        if (isPulling && currentTarget != null)
        {
            float distance = Vector3.Distance(currentTarget.transform.position, grappleOrigin.position);
            if (distance <= minDistanceFromOrigin)
            {
                StopGrappling();
            }
        }
    }
    
    void UpdateAnimator()
    {
        if (nyxAnimator != null)
        {
            nyxAnimator.SetBool("isGrappling", isGrappling);
        }
    }
    
    void CreateSpringJoint()
    {
        if (currentTargetRigidbody == null) return;
        
        Rigidbody grappleOriginRigidbody = grappleOrigin.GetComponent<Rigidbody>();
        if (grappleOriginRigidbody == null)
        {
            grappleOriginRigidbody = grappleOrigin.gameObject.AddComponent<Rigidbody>();
            grappleOriginRigidbody.isKinematic = true;
        }
        
        currentSpringJoint = currentTargetRigidbody.gameObject.AddComponent<SpringJoint>();
        currentSpringJoint.connectedBody = grappleOriginRigidbody;
        currentSpringJoint.autoConfigureConnectedAnchor = false;
        currentSpringJoint.anchor = Vector3.zero;
        currentSpringJoint.connectedAnchor = Vector3.zero;
        currentSpringJoint.spring = springStrength;
        currentSpringJoint.damper = springDamper;
        currentSpringJoint.minDistance = 0f;
        currentSpringJoint.maxDistance = 0f;
        
        if (enableDebugLogs) Debug.Log("Spring joint created");
    }
    
    void CleanupSpringJoint()
    {
        if (currentSpringJoint != null)
        {
            Destroy(currentSpringJoint);
            currentSpringJoint = null;
        }
    }
    
    void ResetJointPosition()
    {
        if (grappleJoint != null && originalJointParent != null)
        {
            grappleJoint.SetParent(originalJointParent);
            grappleJoint.localPosition = originalJointPosition;
            grappleJoint.localRotation = originalJointRotation;
        }
    }
    
    // Public method for external force release (like from LeverGrapple)
    public void ForceReleaseGrapple()
    {
        if (enableDebugLogs) Debug.Log("Force release grapple called");
        StopGrappling();
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!useGizmos || !showDebugRay || grappleOrigin == null) return;
        
        Vector3 rayOrigin = grappleOrigin.position;
        Vector3 rayDirection = grappleOrigin.forward;
        
        Color gizmoColor = isGrappleableInRange ? debugHitColor : debugRayColor;
        Gizmos.color = gizmoColor;
        
        if (isGrappleableInRange && lastHit.collider != null)
        {
            Gizmos.DrawLine(rayOrigin, lastHit.point);
            Gizmos.DrawWireSphere(lastHit.point, 0.1f);
        }
        else
        {
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * grappleRange);
        }
        
        // Draw cone visualization
        DrawGizmoCone(rayOrigin, rayDirection, grappleRange, grappleAngle);
        
        // Draw state indicators
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(rayOrigin, 0.05f);
        
        // Draw minimum distance indicator
        if (isPulling && currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 directionToTarget = (currentTarget.transform.position - grappleOrigin.position).normalized;
            Vector3 minDistancePos = grappleOrigin.position + directionToTarget * minDistanceFromOrigin;
            Gizmos.DrawWireSphere(minDistancePos, 0.2f);
        }
        
        // Draw hand movement line when reaching
        if (isHandReaching && grappleJoint != null && currentTarget != null)
        {
            Gizmos.color = Color.cyan;
            Transform targetPoint = currentTarget.grapplePoint != null ? currentTarget.grapplePoint : currentTarget.transform;
            Gizmos.DrawLine(grappleJoint.position, targetPoint.position);
            
            // Draw hand position
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(grappleJoint.position, 0.15f);
        }
        
        // Draw hand attachment when attached to target
        if (isHandAttachedToTarget && grappleJoint != null && attachedGrapplePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(grappleJoint.position, grappleOrigin.position);
            
            // Draw attached hand position with different color
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(grappleJoint.position, 0.2f);
            
            // Draw connection line to show attachment
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(grappleJoint.position, attachedGrapplePoint.position);
        }
        
        // Draw state indicators with colors
        if (enableDebugLogs)
        {
            // Draw state spheres with different colors
            Gizmos.color = isGrappling ? Color.green : Color.gray;
            Gizmos.DrawWireSphere(rayOrigin + Vector3.up * 1f, 0.1f); // Grappling state
            
            Gizmos.color = isHandReaching ? Color.cyan : Color.gray;
            Gizmos.DrawWireSphere(rayOrigin + Vector3.up * 1.2f, 0.1f); // Reaching state
            
            Gizmos.color = isPulling ? Color.magenta : Color.gray;
            Gizmos.DrawWireSphere(rayOrigin + Vector3.up * 1.4f, 0.1f); // Pulling state
            
            Gizmos.color = isHandAttachedToTarget ? Color.yellow : Color.gray;
            Gizmos.DrawWireSphere(rayOrigin + Vector3.up * 1.6f, 0.1f); // Hand attached state
        }
    }
    
    void DrawGizmoCone(Vector3 origin, Vector3 direction, float range, float angle)
    {
        int segments = 12;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float currentAngle = i * angleStep;
            Vector3 coneDirection = GetConeDirection(direction, angle, currentAngle);
            Gizmos.DrawLine(origin, origin + coneDirection * range);
        }
        
        float[] distances = { range * 0.5f, range };
        foreach (float distance in distances)
        {
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;
                
                Vector3 point1 = origin + GetConeDirection(direction, angle, angle1) * distance;
                Vector3 point2 = origin + GetConeDirection(direction, angle, angle2) * distance;
                
                Gizmos.DrawLine(point1, point2);
            }
        }
    }
    
    Vector3 GetConeDirection(Vector3 forward, float coneAngle, float rotationAngle)
    {
        Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, forward).normalized;
        
        float rad = coneAngle * Mathf.Deg2Rad;
        float rotRad = rotationAngle * Mathf.Deg2Rad;
        
        Vector3 coneDirection = forward * Mathf.Cos(rad) + 
                               (right * Mathf.Cos(rotRad) + up * Mathf.Sin(rotRad)) * Mathf.Sin(rad);
        
        return coneDirection.normalized;
    }
}

// Helper component for joint trigger detection
public class GrappleJointTrigger : MonoBehaviour
{
    private NyxGrapple nyxGrapple;
    
    public void Initialize(NyxGrapple grapple)
    {
        nyxGrapple = grapple;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (nyxGrapple == null) return;
        
        Grappleable grappleable = other.GetComponent<Grappleable>();
        if (grappleable != null)
        {
            // Notify NyxGrapple that joint reached target
            nyxGrapple.OnJointReachedTarget(grappleable);
        }
    }
}

