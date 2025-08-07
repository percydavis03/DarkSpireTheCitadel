using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers;

/// <summary>
/// Camera-relative lock-on system for Nyx that shows a red dot above targeted enemies.
/// E = cycle left (camera-relative), R = cycle right (camera-relative)
/// Automatically unlocks when no more targets exist in the selected direction.
/// </summary>
public class NyxLockOnSystem : MonoBehaviour
{
    [Header("Nyx Reference")]
    [SerializeField] private Transform nyxTransform;
    [SerializeField] private bool autoFindNyx = true;
    [SerializeField] private Player_Movement playerMovement;
    
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool autoFindCamera = true;
    
    [Header("Lock-On Settings")]
    [SerializeField] private bool useCameraOnlyTargeting = true; // Pure camera-based targeting (ignore distance)
    [SerializeField] private float lockOnRange = 35f; // Only used if useCameraOnlyTargeting is false
    [SerializeField] private float lockOnLostDistance = 40f;
    [SerializeField] private bool autoLockOnClosest = false; // Disabled by default for better control
    
    [Header("Input")]
    [SerializeField] private PlayerInputActions playerControls;
    
    // Add cooldown to prevent immediate reactivation
    [Header("Timing")]
    [SerializeField] private float lockOnCooldown = 0.3f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool enableRotationDebugLogs = false;

    [Header("Rotation Settings")]
    [SerializeField] private bool enableAutoRotation = true; // Enable rotation to face locked-on targets
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private bool onlyRotateWhenLocked = true;
    [SerializeField] private float rotationThreshold = 5f; // Don't rotate if already facing target within this angle
    
    private InputAction cycleTargets;
    private float lastCycleInput;
    
    // Add cooldown tracking
    private float lastLockOnReleaseTime;
    private bool isInCooldown => Time.time - lastLockOnReleaseTime < lockOnCooldown;
    
    [Header("Simple Red Dot")]
    [SerializeField] private GameObject redDotPrefab;
    [SerializeField] private float dotHeight = 3f;
    [SerializeField] private float dotScale = 0.5f;
    [SerializeField] private bool useRedDotIndicator = true; // Disable this to use only the new mesh highlight system
    
    // Dependencies (removed - now using simple camera-based targeting)
    
    // Lock-on state
    private Transform currentLockedTarget;
    private bool isLockOnActive;
    
    // Simple red dot
    private GameObject currentRedDot;
    
    // Events
    public System.Action<Transform> OnTargetLocked;
    public System.Action OnLockOnReleased;
    
    // Helper method for debug logging
    private void DebugLog(string message)
    {
        if (enableDebugLogs) Debug.Log($"NyxLockOn: {message}");
    }
    
    // Properties
    public Transform CurrentTarget => currentLockedTarget;
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
        
        // Find main camera if not assigned
        if (mainCamera == null && autoFindCamera)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
            if (mainCamera == null)
            {
                Debug.LogError("NyxLockOnSystem: Could not find main camera!");
            }
        }
        
        // Find player movement if not assigned
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<Player_Movement>();
            if (playerMovement == null)
            {
                Debug.LogError("NyxLockOnSystem: Could not find Player_Movement component!");
            }
        }
        
        // Get PlayerInputActions
        if (playerControls == null)
        {
            var playerMovement = GetComponent<Player_Movement>() ?? GetComponentInParent<Player_Movement>();
            if (playerMovement?.playerControls != null)
            {
                playerControls = playerMovement.playerControls;
            }
            else
            {
                playerControls = new PlayerInputActions();
                Debug.LogWarning("NyxLockOnSystem: Could not find existing PlayerInputActions, created new instance.");
            }
        }
        
        // Simplified targeting - no targeting system dependency needed
        
        // Check for conflicting SimpleNyxLockOn system
        var simpleNyxLockOn = GetComponent<SimpleNyxLockOn>();
        if (simpleNyxLockOn != null)
        {
            Debug.LogWarning("NyxLockOnSystem: Found SimpleNyxLockOn component on the same GameObject! This will cause highlighting conflicts. Please disable SimpleNyxLockOn component to use the new system.");
        }
        
        // Red dot creation commented out
        // if (redDotPrefab == null)
        // {
        //     CreateSimpleRedDot();
        // }
    }
    
    void OnEnable()
    {
        if (playerControls != null)
        {
            cycleTargets = playerControls.General.CycleTargets;
            cycleTargets.Enable();
        }
        else
        {
            Debug.LogError("NyxLockOnSystem: playerControls is null! Input won't work.");
        }
        
        // No targeting system setup needed - using camera-based targeting
    }
    
    void OnDisable()
    {
        cycleTargets?.Disable();
        
        ReleaseLockOn();
    }
    
    void Update()
    {
        if (nyxTransform == null) return;
        
        UpdateLockOnState();
        HandleTargetCycling();
        // UpdateRedDot(); // Commented out
        HandleTargetRotation();
        

    }
    
    private void UpdateLockOnState()
    {
        if (isLockOnActive && currentLockedTarget != null)
        {
            // Check if target is still valid
            try
            {
                if (currentLockedTarget == null || !currentLockedTarget.gameObject.activeInHierarchy || !IsEnemyAlive(currentLockedTarget.gameObject))
                {
                    DebugLog("Target is no longer valid - releasing lock-on");
                    ReleaseLockOn();
                    return;
                }
                
                Vector3 targetPos = currentLockedTarget.position;
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
        
        // Respect player movement's canRotate state (important during knockback, etc.)
        if (playerMovement != null && !playerMovement.canRotate) return;
        
        // Only rotate when locked on if specified
        if (onlyRotateWhenLocked && !isLockOnActive) return;
        
        // Get the target to rotate towards
        Transform targetToFace = isLockOnActive ? currentLockedTarget : null;
        
        // If not locked on but we have targets and auto-rotation is enabled, face closest
        if (!isLockOnActive && targetToFace == null && !onlyRotateWhenLocked)
        {
            targetToFace = GetClosestTarget();
        }
        
        if (targetToFace == null) return;
        
        // Calculate direction to target
        Vector3 targetPos = targetToFace.position;
        
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
    
    private Transform GetClosestTarget()
    {
        var sortedTargets = GetTargetsSortedByScreenPosition();
        if (sortedTargets.Count == 0) return null;
        
        Transform closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (var target in sortedTargets)
        {
            if (target == null || !IsEnemyAlive(target.gameObject)) continue;
            
            float distance = Vector3.Distance(nyxTransform.position, target.position);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = target;
            }
        }
        
        return closest;
    }
    
    /// <summary>
    /// Get all enemies visible on screen, sorted by their screen X position (left to right).
    /// Simple camera-based targeting using "Enemy" tag only.
    /// </summary>
    private List<Transform> GetTargetsSortedByScreenPosition()
    {
        if (mainCamera == null)
            return new List<Transform>();
        
        var validTargets = new List<Transform>();
        var targetsWithScreenPos = new List<(Transform target, float screenX)>();
        
        // Find all enemies by tag - simple and direct
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (var enemyObj in allEnemies)
        {
            if (enemyObj == null || !enemyObj.activeInHierarchy) continue;
            
            // Check if enemy is alive
            if (!IsEnemyAlive(enemyObj)) continue;
            
            Vector3 worldPos = enemyObj.transform.position;
            
            // Optional distance check (can be disabled for pure camera-based targeting)
            if (!useCameraOnlyTargeting && nyxTransform != null)
            {
                float distance = Vector3.Distance(nyxTransform.position, worldPos);
                if (distance > lockOnRange) continue;
            }
            
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            
            // Only include targets that are in front of the camera and within screen bounds
            if (screenPos.z > 0 && 
                screenPos.x >= 0 && screenPos.x <= Screen.width &&
                screenPos.y >= 0 && screenPos.y <= Screen.height)
            {
                targetsWithScreenPos.Add((enemyObj.transform, screenPos.x));
                if (enableDebugLogs)
                    DebugLog($"Screen target: {enemyObj.name} at X:{screenPos.x:F0} Y:{screenPos.y:F0}");
            }
        }
        
        // Sort by screen X position (left to right)
        targetsWithScreenPos.Sort((a, b) => a.screenX.CompareTo(b.screenX));
        
        // Extract just the targets in sorted order
        foreach (var item in targetsWithScreenPos)
        {
            validTargets.Add(item.target);
        }
        
        if (enableDebugLogs)
        {
            DebugLog($"Total enemies found: {allEnemies.Length}, Screen visible: {validTargets.Count}");
            if (validTargets.Count > 0)
            {
                string sortedOrder = $"Screen targets: ";
                for (int i = 0; i < targetsWithScreenPos.Count; i++)
                {
                    sortedOrder += $"{targetsWithScreenPos[i].target.name}";
                    if (i < targetsWithScreenPos.Count - 1) sortedOrder += " → ";
                }
                DebugLog(sortedOrder);
            }
        }
        
        return validTargets;
    }
    
    /// <summary>
    /// Check if an enemy GameObject is alive
    /// </summary>
    private bool IsEnemyAlive(GameObject enemyObj)
    {
        if (enemyObj == null) return false;
        
        // Check different enemy types
        var enemyBasic = enemyObj.GetComponent<Enemy_Basic>();
        if (enemyBasic != null && enemyBasic.dead) return false;
        
        var worker = enemyObj.GetComponent<Worker>();
        if (worker != null && worker.dead) return false;
        
        var boss = enemyObj.GetComponent<Boss>();
        if (boss != null && boss.currentHealth <= 0) return false;
        
        return true;
    }
    
    /// <summary>
    /// Find the next target to the right (higher screen X) of the current target
    /// </summary>
    private Transform GetNextTargetRight()
    {
        var sortedTargets = GetTargetsSortedByScreenPosition();
        if (sortedTargets.Count == 0 || currentLockedTarget == null) return null;
        
        int currentIndex = sortedTargets.IndexOf(currentLockedTarget);
        if (currentIndex == -1 || currentIndex >= sortedTargets.Count - 1)
        {
            // No target to the right, return null to trigger unlock
            return null;
        }
        
        return sortedTargets[currentIndex + 1];
    }
    
    /// <summary>
    /// Find the next target to the left (lower screen X) of the current target
    /// </summary>
    private Transform GetNextTargetLeft()
    {
        var sortedTargets = GetTargetsSortedByScreenPosition();
        if (sortedTargets.Count == 0 || currentLockedTarget == null) return null;
        
        int currentIndex = sortedTargets.IndexOf(currentLockedTarget);
        if (currentIndex <= 0)
        {
            // No target to the left, return null to trigger unlock
            return null;
        }
        
        return sortedTargets[currentIndex - 1];
    }
    
    // OnTargetsUpdated method removed - now using direct camera-based targeting
    
    private void HandleTargetCycling()
    {
        if (cycleTargets == null) return;
        
        float cycleInput = cycleTargets.ReadValue<float>();
        
        // Simple edge detection
        if (Mathf.Abs(cycleInput) > 0.5f && Mathf.Abs(lastCycleInput) <= 0.5f)
        {
            if (!isLockOnActive)
            {
                if (!isInCooldown)
                {
                    ActivateLockOn();
                }
            }
            else if (isLockOnActive)
            {
                if (cycleInput > 0)
                {
                    CycleTargetRight(); // R key = move right
                }
                else if (cycleInput < 0)
                {
                    CycleTargetLeft(); // E key = move left
                }
            }
        }
        
        lastCycleInput = cycleInput;
    }
    
    public void ActivateLockOn()
    {
        // ActivateLockOn called
        
        Transform bestTarget = null;
        
        // Try to get the closest target to screen center for initial lock-on
        var sortedTargets = GetTargetsSortedByScreenPosition();
        if (sortedTargets.Count > 0 && mainCamera != null)
        {
            float screenCenterX = Screen.width * 0.5f;
            float closestToCenter = float.MaxValue;
            
            foreach (var target in sortedTargets)
            {
                Vector3 worldPos = target.position;
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
                
                float distanceFromCenter = Mathf.Abs(screenPos.x - screenCenterX);
                if (distanceFromCenter < closestToCenter)
                {
                    closestToCenter = distanceFromCenter;
                    bestTarget = target;
                }
            }
            if (enableDebugLogs && bestTarget != null)
                DebugLog($"Initial target: {bestTarget.name} (closest to screen center)");
        }
        
        // Fallback to traditional closest target if camera method fails
        if (bestTarget == null)
        {
            bestTarget = GetClosestTarget();
            if (enableDebugLogs && bestTarget != null)
                DebugLog($"Fallback target: {bestTarget.name} (closest by distance)");
        }
        
        if (bestTarget != null)
        {
            DebugLog($"Activating lock-on to target: {bestTarget.name}");
            LockOnToTarget(bestTarget);
        }
        else
        {
            DebugLog("No suitable target found for lock-on activation");
        }
    }
    
    public void LockOnToTarget(Transform target)
    {
        if (target == null || !IsEnemyAlive(target.gameObject)) 
        {
            DebugLog("Cannot lock onto invalid target");
            return;
        }
        
        // Release previous target cleanly
        if (currentLockedTarget != null)
        {
            // Disable previous target's highlight
            SetEnemyHighlight(currentLockedTarget, false);
            // Hide red dot BEFORE deselecting to prevent multiple highlights
            // HideRedDot(); // Commented out
            DebugLog($"Releasing previous target: {currentLockedTarget.name}");
        }
        
        currentLockedTarget = target;
        isLockOnActive = true;
        
        DebugLog($"Locked onto target: {target.name}");
        
        // Only show red dot if enabled (disable this to use new mesh highlight system)
        // Red dot code commented out
        // if (useRedDotIndicator)
        // {
        //     ShowRedDot(target);
        // }
        
        // Enable enemy highlight
        SetEnemyHighlight(target, true);
        
        OnTargetLocked?.Invoke(target);
    }
    
    public void ReleaseLockOn()
    {
        if (currentLockedTarget != null)
        {
            // Safely check if the target still exists before accessing its properties
            try
            {
                if (currentLockedTarget != null)
                {
                    DebugLog($"Releasing lock-on from target: {currentLockedTarget.name}");
                }
                else
                {
                    DebugLog("Releasing lock-on from target (Transform is null)");
                }
                // Disable enemy highlight
                SetEnemyHighlight(currentLockedTarget, false);
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
        
        // Red dot code commented out
        // Force red dot to disappear immediately
        // HideRedDot();
        
        // Double-check that red dot is actually hidden
        // if (currentRedDot != null && currentRedDot.activeInHierarchy)
        // {
        //     DebugLog("Force hiding red dot - was still active");
        //     currentRedDot.SetActive(false);
        // }
        
        // Record the release time for cooldown
        lastLockOnReleaseTime = Time.time;
        
        DebugLog($"Lock-on released. IsActive: {isLockOnActive}, RedDotActive: {(currentRedDot?.activeInHierarchy ?? false)}");
        OnLockOnReleased?.Invoke();
    }
    
    /// <summary>
    /// Cycle to the target on the right (camera-relative). 
    /// If no target exists to the right, unlock.
    /// </summary>
    public void CycleTargetRight()
    {
        if (!isLockOnActive || currentLockedTarget == null)
        {
            ReleaseLockOn();
            return;
        }
        
        var nextTarget = GetNextTargetRight();
        if (nextTarget != null)
        {
            LockOnToTarget(nextTarget);
            DebugLog($"Cycled right to: {nextTarget.name}");
        }
        else
        {
            // No target to the right - unlock
            DebugLog("No target to the right - unlocking");
            ReleaseLockOn();
        }
    }
    
    /// <summary>
    /// Cycle to the target on the left (camera-relative). 
    /// If no target exists to the left, unlock.
    /// </summary>
    public void CycleTargetLeft()
    {
        if (!isLockOnActive || currentLockedTarget == null)
        {
            ReleaseLockOn();
            return;
        }
        
        var nextTarget = GetNextTargetLeft();
        if (nextTarget != null)
        {
            LockOnToTarget(nextTarget);
            DebugLog($"Cycled left to: {nextTarget.name}");
        }
        else
        {
            // No target to the left - unlock
            DebugLog("No target to the left - unlocking");
            ReleaseLockOn();
        }
    }
    
    // Legacy methods kept for backward compatibility (now just redirect to camera-relative methods)
    public void CycleNextTargetOrCancel()
    {
        CycleTargetRight();
    }
    
    public void CyclePreviousTargetOrCancel()
    {
        CycleTargetLeft();
    }
    
    // Old cycling methods removed - now using camera-relative CycleTargetRight() and CycleTargetLeft()
    
    // Red dot methods commented out
    /*
    private void ShowRedDot(Transform target)
    {
        if (target == null) return;
        
        // Always hide any existing red dot first to prevent multiple highlights
        HideRedDot();
        
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
            UpdateRedDotPosition(target);
            DebugLog($"Red dot shown for target: {target.name}");
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
        
        bool shouldShow = isLockOnActive && currentLockedTarget != null;
        bool isCurrentlyVisible = currentRedDot.activeInHierarchy;
        
        if (shouldShow && !isCurrentlyVisible)
        {
            // Should show but currently hidden - show it
            currentRedDot.SetActive(true);
            UpdateRedDotPosition(currentLockedTarget);
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
                if (currentLockedTarget != null)
                {
                    UpdateRedDotPosition(currentLockedTarget);
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
    */
    // End of commented red dot methods

    
    void OnDrawGizmos()
    {
        if (nyxTransform == null) return;
        
        // Draw lock-on range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(nyxTransform.position, lockOnRange);
        
        // Draw line to locked target
        if (isLockOnActive && currentLockedTarget != null)
        {
            Vector3 targetPos = currentLockedTarget.position;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(nyxTransform.position, targetPos);
        }
    }
    
    /// <summary>
    /// Sets enemy highlight on/off by finding and enabling/disabling the Enemy Highlight renderer
    /// </summary>
    private void SetEnemyHighlight(Transform enemyTransform, bool enabled)
    {
        if (enemyTransform == null) return;
        
        // Try to find TargetableEnemy component first (preferred method)
        TargetableEnemy targetableEnemy = enemyTransform.GetComponent<TargetableEnemy>();
        if (targetableEnemy != null)
        {
            if (enabled)
            {
                targetableEnemy.OnTargetSelected();
            }
            else
            {
                targetableEnemy.OnTargetDeselected();
            }
            return;
        }
        
        // Fallback: directly look for Enemy Highlight renderer in children
        SkinnedMeshRenderer[] renderers = enemyTransform.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if (renderer.gameObject.name.Contains("Enemy Highlight") || 
                renderer.gameObject.name.Contains("Highlight"))
            {
                renderer.enabled = enabled;
                DebugLog($"Set enemy highlight {(enabled ? "ON" : "OFF")} for {enemyTransform.name}");
                return;
            }
        }
        
        DebugLog($"No highlight renderer found for enemy: {enemyTransform.name}");
    }
} 