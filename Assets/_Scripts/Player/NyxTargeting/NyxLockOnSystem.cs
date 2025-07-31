using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Simple lock-on system for Nyx that shows a red dot above targeted enemies
/// </summary>
public class NyxLockOnSystem : MonoBehaviour
{
    [Header("Nyx Reference")]
    [SerializeField] private Transform nyxTransform;
    [SerializeField] private bool autoFindNyx = true;
    
    [Header("Lock-On Settings")]
    [SerializeField] private float lockOnRange = 20f;
    [SerializeField] private float lockOnLostDistance = 25f;
    [SerializeField] private bool autoLockOnClosest = false; // Disabled by default for better control
    
    [Header("Input")]
    [SerializeField] private PlayerInputActions playerControls;
    [SerializeField] private float cycleInputThreshold = 0.3f;
    
    // Add cooldown to prevent immediate reactivation
    [Header("Timing")]
    [SerializeField] private float lockOnCooldown = 0.3f; // Increased for smoother feel
    [SerializeField] private float inputDebounceTime = 0.5f; // Increased to reduce message spam
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true; // Can be disabled after testing

    [Header("Rotation Settings")]
    [SerializeField] private bool enableAutoRotation = false; // Disabled to prevent camera movement
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private bool onlyRotateWhenLocked = true;
    [SerializeField] private float rotationThreshold = 5f; // Don't rotate if already facing target within this angle
    
    private InputAction cycleTargets;
    private float lastCycleInput;
    
    // Add cooldown tracking
    private float lastLockOnReleaseTime;
    private float lastCooldownWarningTime; // Prevent spam warnings
    private bool isInCooldown => Time.time - lastLockOnReleaseTime < lockOnCooldown;
    
    [Header("Simple Red Dot")]
    [SerializeField] private GameObject redDotPrefab;
    [SerializeField] private float dotHeight = 3f;
    [SerializeField] private float dotScale = 0.5f;
    
    // Dependencies
    private NyxTargetingSystem targetingSystem;
    
    // Lock-on state
    private ITargetable currentLockedTarget;
    private bool isLockOnActive;
    private List<ITargetable> lockableTargets;
    private int currentTargetIndex;
    
    // Simple red dot
    private GameObject currentRedDot;
    
    // Events
    public System.Action<ITargetable> OnTargetLocked;
    public System.Action OnLockOnReleased;
    
    // Helper method for debug logging
    private void DebugLog(string message)
    {
        if (enableDebugLogs) Debug.Log($"NyxLockOn: {message}");
    }
    
    // Properties
    public ITargetable CurrentTarget => currentLockedTarget;
    public bool IsLockOnActive => isLockOnActive;
    public bool HasTarget => currentLockedTarget != null;
    public Transform NyxTransform => nyxTransform;
    
    void Awake()
    {
        // Find Nyx transform if not assigned
        if (nyxTransform == null && autoFindNyx)
        {
            GameObject nyxObject = GameObject.FindWithTag("Nyx");
            if (nyxObject != null)
            {
                nyxTransform = nyxObject.transform;
            }
            else
            {
                Debug.LogError("NyxLockOnSystem: Could not find GameObject with 'Nyx' tag!");
            }
        }
        
        // Get PlayerInputActions
        if (playerControls == null)
        {
            var playerMovement = GetComponent<Player_Movement>();
            if (playerMovement != null && playerMovement.playerControls != null)
            {
                playerControls = playerMovement.playerControls;
            }
            else
            {
                playerControls = new PlayerInputActions();
            }
        }
        
        // Initialize collections
        lockableTargets = new List<ITargetable>();
        
        // Find targeting system
        targetingSystem = GetComponent<NyxTargetingSystem>();
        if (targetingSystem == null)
        {
            Debug.LogError("NyxLockOnSystem requires NyxTargetingSystem component!");
        }
        
        // Create simple red dot if not assigned
        if (redDotPrefab == null)
        {
            CreateSimpleRedDot();
        }
    }
    
    void OnEnable()
    {
        if (playerControls != null)
        {
            cycleTargets = playerControls.General.CycleTargets;
            cycleTargets.Enable();
        }
        
        if (targetingSystem != null)
        {
            targetingSystem.OnTargetsUpdated += OnTargetsUpdated;
            targetingSystem.ConfigureTargeting(
                lockOnRange, 
                120f,
                new TargetType[] { TargetType.Enemy, TargetType.Boss }, 
                true
            );
        }
    }
    
    void OnDisable()
    {
        cycleTargets?.Disable();
        
        if (targetingSystem != null)
        {
            targetingSystem.OnTargetsUpdated -= OnTargetsUpdated;
        }
        
        ReleaseLockOn();
    }
    
    void Update()
    {
        if (nyxTransform == null) return;
        
        UpdateLockOnState();
        HandleTargetCycling();
        UpdateRedDot();
        HandleTargetRotation();
        
        // Debug: Log lock-on state changes (reduced frequency for less spam)
        if (enableDebugLogs && Time.frameCount % 240 == 0) // Every 4 seconds at 60fps instead of 2
        {
            DebugLog($"Status - IsActive: {isLockOnActive}, Target: {(currentLockedTarget?.Transform.name ?? "None")}, TargetCount: {lockableTargets.Count}, RedDotActive: {(currentRedDot?.activeInHierarchy ?? false)}, InCooldown: {isInCooldown}");
        }
    }
    
    private void UpdateLockOnState()
    {
        if (isLockOnActive && currentLockedTarget != null)
        {
            // Check if player is hurt and release lock-on to avoid conflicts
            if (Player_Movement.instance != null && Player_Movement.instance.anim != null)
            {
                if (Player_Movement.instance.anim.GetBool("isHurt"))
                {
                    DebugLog("Player is hurt - releasing lock-on to prevent conflicts");
                    ReleaseLockOn();
                    return;
                }
            }
            
            // Check if target is still valid
            try
            {
                if (currentLockedTarget.Transform == null || !currentLockedTarget.CanBeTargeted)
                {
                    DebugLog("Target is no longer valid - releasing lock-on");
                    ReleaseLockOn();
                    return;
                }
                
                Vector3 targetPos = currentLockedTarget.TargetPoint != null ? 
                    currentLockedTarget.TargetPoint.position : 
                    currentLockedTarget.Transform.position;
                
                float distance = Vector3.Distance(nyxTransform.position, targetPos);
                
                if (distance > lockOnLostDistance)
                {
                    DebugLog($"Target too far ({distance:F1}m > {lockOnLostDistance}m) - releasing lock-on");
                    ReleaseLockOn();
                }
            }
            catch (System.Exception)
            {
                DebugLog("Target object destroyed - releasing lock-on");
                ReleaseLockOn();
            }
        }
    }
    
    private void HandleTargetRotation()
    {
        // Auto-rotation disabled by default to prevent camera movement when switching targets
        if (!enableAutoRotation || nyxTransform == null) return;
        
        // Only rotate when locked on if specified
        if (onlyRotateWhenLocked && !isLockOnActive) return;
        
        // Get the target to rotate towards
        ITargetable targetToFace = isLockOnActive ? currentLockedTarget : null;
        
        // If not locked on but we have targets and auto-rotation is enabled, face closest
        if (!isLockOnActive && lockableTargets.Count > 0 && !onlyRotateWhenLocked)
        {
            targetToFace = GetClosestTarget();
        }
        
        if (targetToFace?.Transform == null) return;
        
        // Calculate direction to target
        Vector3 targetPos = targetToFace.TargetPoint != null ? 
            targetToFace.TargetPoint.position : 
            targetToFace.Transform.position;
        
        Vector3 directionToTarget = (targetPos - nyxTransform.position).normalized;
        
        // Only rotate on Y axis (horizontal rotation)
        directionToTarget.y = 0;
        
        if (directionToTarget.magnitude < 0.1f) return; // Target too close or same position
        
        // Check if we're already facing the target
        float angleToTarget = Vector3.Angle(nyxTransform.forward, directionToTarget);
        if (angleToTarget < rotationThreshold) return;
        
        // Calculate target rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        
        // Smoothly rotate towards target
        nyxTransform.rotation = Quaternion.Slerp(
            nyxTransform.rotation, 
            targetRotation, 
            rotationSpeed * Time.deltaTime
        );
    }
    
    private ITargetable GetClosestTarget()
    {
        if (lockableTargets.Count == 0) return null;
        
        ITargetable closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (var target in lockableTargets)
        {
            if (!target.CanBeTargeted) continue;
            
            Vector3 targetPos = target.TargetPoint != null ? target.TargetPoint.position : target.Transform.position;
            float distance = Vector3.Distance(nyxTransform.position, targetPos);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = target;
            }
        }
        
        return closest;
    }
    
    private void OnTargetsUpdated(List<ITargetable> targets)
    {
        lockableTargets.Clear();
        
        foreach (var target in targets)
        {
            if (target.TargetType == TargetType.Enemy || target.TargetType == TargetType.Boss)
            {
                lockableTargets.Add(target);
            }
        }
        
        // Check if current locked target is still in the list
        if (isLockOnActive && currentLockedTarget != null)
        {
            if (!lockableTargets.Contains(currentLockedTarget))
            {
                DebugLog("Current target no longer in target list - releasing lock-on");
                ReleaseLockOn();
                return;
            }
            else
            {
                // Update the current target index
                currentTargetIndex = lockableTargets.IndexOf(currentLockedTarget);
            }
        }
        
        if (autoLockOnClosest && !isLockOnActive && lockableTargets.Count > 0)
        {
            // Respect cooldown even for automatic lock-on
            if (!isInCooldown)
            {
                var closestTarget = targetingSystem.GetClosestTargetTo(nyxTransform.position);
                if (closestTarget != null && (closestTarget.TargetType == TargetType.Enemy || closestTarget.TargetType == TargetType.Boss))
                {
                    LockOnToTarget(closestTarget);
                }
            }
        }
    }
    
    private void HandleTargetCycling()
    {
        if (cycleTargets == null) return;
        
        float cycleInput = cycleTargets.ReadValue<float>();
        
        // Only process input on input edge (when input changes from low to high threshold)
        if (Mathf.Abs(cycleInput) > cycleInputThreshold && Mathf.Abs(lastCycleInput) <= cycleInputThreshold)
        {
            if (!isLockOnActive && lockableTargets.Count > 0)
            {
                // Check cooldown before reactivating
                if (isInCooldown)
                {
                    // Only log cooldown warning once per debounce period to prevent spam
                    if (Time.time - lastCooldownWarningTime > inputDebounceTime)
                    {
                        float remainingCooldown = lockOnCooldown - (Time.time - lastLockOnReleaseTime);
                        DebugLog($"Lock-on reactivation blocked by cooldown ({remainingCooldown:F2}s remaining)");
                        lastCooldownWarningTime = Time.time;
                    }
                    lastCycleInput = cycleInput;
                    return;
                }
                
                DebugLog($"Reactivating lock-on. Available targets: {lockableTargets.Count}");
                ActivateLockOn();
            }
            else if (isLockOnActive)
            {
                if (cycleInput > 0)
                {
                    CycleNextTargetOrCancel();
                }
                else if (cycleInput < 0)
                {
                    CyclePreviousTargetOrCancel();
                }
            }
            else if (!isLockOnActive && lockableTargets.Count == 0)
            {
                // Only log this message once per debounce period
                if (Time.time - lastCooldownWarningTime > inputDebounceTime)
                {
                    DebugLog($"Cannot reactivate - no targets available. Target count: {lockableTargets.Count}");
                    lastCooldownWarningTime = Time.time;
                }
            }
        }
        
        lastCycleInput = cycleInput;
    }
    
    public void ActivateLockOn()
    {
        DebugLog($"ActivateLockOn called. Available targets: {lockableTargets.Count}");
        
        // Force update targeting system to ensure we have latest targets
        if (targetingSystem != null)
        {
            targetingSystem.ForceUpdate();
        }
        
        if (lockableTargets.Count == 0) 
        {
            DebugLog("No targets available for lock-on");
            return;
        }
        
        ITargetable bestTarget = null;
        if (targetingSystem != null)
        {
            targetingSystem.ForceUpdate();
            bestTarget = targetingSystem.BestTarget;
            
            if (bestTarget != null && bestTarget.TargetType != TargetType.Enemy && bestTarget.TargetType != TargetType.Boss)
            {
                DebugLog($"Best target {bestTarget.Transform.name} is not Enemy/Boss type, ignoring");
                bestTarget = null;
            }
        }
        
        if (bestTarget == null && lockableTargets.Count > 0)
        {
            DebugLog("No best target from targeting system, finding closest manually");
            float closestDistance = float.MaxValue;
            foreach (var target in lockableTargets)
            {
                Vector3 targetPos = target.TargetPoint != null ? target.TargetPoint.position : target.Transform.position;
                float distance = Vector3.Distance(nyxTransform.position, targetPos);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = target;
                }
            }
            DebugLog($"Found closest target: {(bestTarget != null ? bestTarget.Transform.name : "None")} at distance {closestDistance:F1}");
        }
        
        if (bestTarget != null)
        {
            DebugLog($"Activating lock-on to target: {bestTarget.Transform.name}");
            LockOnToTarget(bestTarget);
        }
        else
        {
            DebugLog("No suitable target found for lock-on activation");
        }
    }
    
    public void LockOnToTarget(ITargetable target)
    {
        if (target == null || !target.CanBeTargeted) 
        {
            Debug.Log("NyxLockOn: Cannot lock onto invalid target");
            return;
        }
        
        // Release previous target cleanly
        if (currentLockedTarget != null)
        {
            currentLockedTarget.OnTargetDeselected();
        }
        
        currentLockedTarget = target;
        isLockOnActive = true;
        
        currentTargetIndex = lockableTargets.IndexOf(target);
        if (currentTargetIndex == -1)
        {
            // Target not in list, add it and use index 0
            Debug.Log("NyxLockOn: Target not in lockable list, adding it");
            lockableTargets.Insert(0, target);
            currentTargetIndex = 0;
        }
        
        Debug.Log($"NyxLockOn: Locked onto target: {target.Transform.name} (index: {currentTargetIndex})");
        target.OnTargetSelected();
        ShowRedDot(target);
        OnTargetLocked?.Invoke(target);
        
        // Reset cooldown warning timer for smooth experience
        lastCooldownWarningTime = 0f;
    }
    
    public void ReleaseLockOn()
    {
        if (currentLockedTarget != null)
        {
            // Safely check if the target still exists before accessing its properties
            try
            {
                if (currentLockedTarget.Transform != null)
                {
                    DebugLog($"Releasing lock-on from target: {currentLockedTarget.Transform.name}");
                }
                else
                {
                    DebugLog("Releasing lock-on from target (Transform is null)");
                }
                currentLockedTarget.OnTargetDeselected();
            }
            catch (System.Exception)
            {
                DebugLog("Releasing lock-on from target (object destroyed)");
            }
            currentLockedTarget = null;
        }
        else
        {
            DebugLog("Releasing lock-on (no current target)");
        }
        
        isLockOnActive = false;
        currentTargetIndex = -1;
        
        // Force red dot to disappear immediately
        HideRedDot();
        
        // Double-check that red dot is actually hidden
        if (currentRedDot != null && currentRedDot.activeInHierarchy)
        {
            DebugLog("Force hiding red dot - was still active");
            currentRedDot.SetActive(false);
        }
        
        // Record the release time for cooldown and reset warning timer
        lastLockOnReleaseTime = Time.time;
        lastCooldownWarningTime = 0f; // Reset to allow immediate feedback next time
        
        DebugLog($"Lock-on released. IsActive: {isLockOnActive}, TargetIndex: {currentTargetIndex}, RedDotActive: {(currentRedDot?.activeInHierarchy ?? false)}");
        OnLockOnReleased?.Invoke();
    }
    
    public void CycleNextTargetOrCancel()
    {
        if (lockableTargets.Count == 0) 
        {
            Debug.Log("NyxLockOn: No targets available - releasing lock-on");
            ReleaseLockOn();
            return;
        }
        
        if (lockableTargets.Count == 1)
        {
            // Only one target - cancel lock-on on next scroll
            Debug.Log("NyxLockOn: Only one target - cancelling lock-on");
            ReleaseLockOn();
            return;
        }
        
        // Validate current target index
        if (currentTargetIndex < 0 || currentTargetIndex >= lockableTargets.Count)
        {
            Debug.Log("NyxLockOn: Invalid target index - resetting to 0");
            currentTargetIndex = 0;
        }
        
        // Check if we're at the last target
        if (currentTargetIndex >= lockableTargets.Count - 1)
        {
            // We're at the last target, cancel lock-on instead of cycling
            Debug.Log("NyxLockOn: At last target - cancelling lock-on");
            ReleaseLockOn();
            return;
        }
        
        // Cycle to next target
        currentTargetIndex++;
        
        // Validate the new target
        if (currentTargetIndex < lockableTargets.Count && lockableTargets[currentTargetIndex] != null)
        {
            var newTarget = lockableTargets[currentTargetIndex];
            Debug.Log($"NyxLockOn: Cycling to next target: {newTarget.Transform.name}");
            LockOnToTarget(newTarget);
        }
        else
        {
            Debug.Log("NyxLockOn: Next target is invalid - releasing lock-on");
            ReleaseLockOn();
        }
    }
    
    public void CyclePreviousTargetOrCancel()
    {
        if (lockableTargets.Count == 0) 
        {
            Debug.Log("NyxLockOn: No targets available - releasing lock-on");
            ReleaseLockOn();
            return;
        }
        
        if (lockableTargets.Count == 1)
        {
            // Only one target - cancel lock-on on scroll
            Debug.Log("NyxLockOn: Only one target - cancelling lock-on");
            ReleaseLockOn();
            return;
        }
        
        // Validate current target index
        if (currentTargetIndex < 0 || currentTargetIndex >= lockableTargets.Count)
        {
            Debug.Log("NyxLockOn: Invalid target index - resetting to last");
            currentTargetIndex = lockableTargets.Count - 1;
        }
        
        // Check if we're at the first target
        if (currentTargetIndex <= 0)
        {
            // We're at the first target, cancel lock-on instead of cycling
            Debug.Log("NyxLockOn: At first target - cancelling lock-on");
            ReleaseLockOn();
            return;
        }
        
        // Cycle to previous target
        currentTargetIndex--;
        
        // Validate the new target
        if (currentTargetIndex >= 0 && currentTargetIndex < lockableTargets.Count && lockableTargets[currentTargetIndex] != null)
        {
            var newTarget = lockableTargets[currentTargetIndex];
            Debug.Log($"NyxLockOn: Cycling to previous target: {newTarget.Transform.name}");
            LockOnToTarget(newTarget);
        }
        else
        {
            Debug.Log("NyxLockOn: Previous target is invalid - releasing lock-on");
            ReleaseLockOn();
        }
    }
    
    public void CycleNextTarget()
    {
        if (lockableTargets.Count <= 1) return;
        
        currentTargetIndex = (currentTargetIndex + 1) % lockableTargets.Count;
        var newTarget = lockableTargets[currentTargetIndex];
        
        LockOnToTarget(newTarget);
    }
    
    public void CyclePreviousTarget()
    {
        if (lockableTargets.Count <= 1) return;
        
        currentTargetIndex = (currentTargetIndex - 1 + lockableTargets.Count) % lockableTargets.Count;
        var newTarget = lockableTargets[currentTargetIndex];
        
        LockOnToTarget(newTarget);
    }
    
    private void ShowRedDot(ITargetable target)
    {
        if (target?.Transform == null) return;
        
        if (currentRedDot == null)
        {
            if (redDotPrefab != null)
            {
                currentRedDot = Instantiate(redDotPrefab);
            }
            else
            {
                currentRedDot = CreateSimpleRedDot();
            }
        }
        
        if (currentRedDot != null)
        {
            currentRedDot.SetActive(true);
            UpdateRedDotPosition(target.Transform);
        }
    }
    
    private void HideRedDot()
    {
        if (currentRedDot != null)
        {
            bool wasVisible = currentRedDot.activeInHierarchy;
            currentRedDot.SetActive(false);
            if (wasVisible)
            {
                DebugLog("Red dot manually hidden");
            }
        }
    }
    
    private void UpdateRedDot()
    {
        if (currentRedDot == null) return;
        
        bool shouldShow = isLockOnActive && currentLockedTarget != null && currentLockedTarget.Transform != null;
        bool isCurrentlyVisible = currentRedDot.activeInHierarchy;
        
        if (shouldShow && !isCurrentlyVisible)
        {
            // Should show but currently hidden - show it
            currentRedDot.SetActive(true);
            UpdateRedDotPosition(currentLockedTarget.Transform);
            DebugLog("Red dot activated");
        }
        else if (!shouldShow && isCurrentlyVisible)
        {
            // Should hide but currently visible - hide it
            currentRedDot.SetActive(false);
            DebugLog("Red dot deactivated");
        }
        else if (shouldShow && isCurrentlyVisible)
        {
            // Should show and is showing - just update position
            try
            {
                if (currentLockedTarget?.Transform != null)
                {
                    UpdateRedDotPosition(currentLockedTarget.Transform);
                }
                else
                {
                    // Target is null, force hide
                    currentRedDot.SetActive(false);
                    DebugLog("Red dot hidden - target is null");
                }
            }
            catch (System.Exception)
            {
                // Target was destroyed, force hide
                currentRedDot.SetActive(false);
                DebugLog("Red dot hidden - target destroyed");
            }
        }
        // If should hide and is hidden, do nothing
        
        // Extra safety check - if not locked on, force hide
        if (!isLockOnActive && currentRedDot.activeInHierarchy)
        {
            currentRedDot.SetActive(false);
            DebugLog("Red dot force hidden - not locked on");
        }
    }
    
    private void UpdateRedDotPosition(Transform target)
    {
        if (currentRedDot == null || target == null) return;
        
        Vector3 targetPos = target.position;
        
        // Try to get the enemy's collider to position above their head
        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider != null)
        {
            targetPos.y = targetCollider.bounds.max.y + dotHeight;
        }
        else
        {
            targetPos.y += dotHeight;
        }
        
        currentRedDot.transform.position = targetPos;
    }
    
    private GameObject CreateSimpleRedDot()
    {
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.name = "RedDot";
        
        // Remove collider
        DestroyImmediate(dot.GetComponent<Collider>());
        
        // Set material to red
        Renderer renderer = dot.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material redMaterial = new Material(Shader.Find("Standard"));
            redMaterial.color = Color.red;
            renderer.material = redMaterial;
        }
        
        // Set scale
        dot.transform.localScale = Vector3.one * dotScale;
        
        // Initially hide
        dot.SetActive(false);
        
        return dot;
    }
    
    void OnDrawGizmos()
    {
        if (nyxTransform == null) return;
        
        // Draw lock-on range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(nyxTransform.position, lockOnRange);
        
        // Draw line to locked target
        if (isLockOnActive && currentLockedTarget != null)
        {
            Vector3 targetPos = currentLockedTarget.TargetPoint != null ? 
                currentLockedTarget.TargetPoint.position : 
                currentLockedTarget.Transform.position;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(nyxTransform.position, targetPos);
        }
    }
} 