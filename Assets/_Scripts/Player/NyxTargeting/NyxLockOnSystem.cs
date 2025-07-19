using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Lock-on system for Nyx that uses the shared targeting system
/// Allows cycling through enemies and maintaining camera focus
/// </summary>
public class NyxLockOnSystem : MonoBehaviour
{
    [Header("Lock-On Settings")]
    [SerializeField] private bool enableLockOn = true;
    [SerializeField] private float lockOnRange = 20f;
    [SerializeField] private float lockOnAngle = 80f; // Increased from 75f to match new targeting system tolerance
    [SerializeField] private bool autoLockOnClosest = true; // Changed to true for better UX
    [SerializeField] private float lockOnLostDistance = 25f; // Distance at which lock-on is automatically lost
    
    [Header("Camera Settings")]
    [SerializeField] private bool adjustCameraOnLockOn = false; // Disabled by default for child setups
    [SerializeField] private Transform cameraTarget; // Camera will look between Nyx and locked target
    [SerializeField] private float cameraLerpSpeed = 2f;
    [SerializeField] private Vector3 cameraOffset = Vector3.up * 2f;
    
    [Header("Input")]
    [SerializeField] private PlayerInputActions playerControls;
    [SerializeField] private bool enableLockOnToggle = false; // Set to true if you want a lock-on toggle button
    [SerializeField] private float cycleInputThreshold = 0.3f; // How much input needed to trigger cycling
    
    private InputAction lockOnToggle;
    private InputAction cycleTargets;
    private float lastCycleInput;
    
    [Header("UI")]
    [SerializeField] private GameObject lockOnIndicatorPrefab;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private bool showLockOnIndicator = true;
    
    [Header("Combat Feel")]
    [SerializeField] private bool autoFaceTarget = true; // Auto-face locked target for better combat feel
    [SerializeField] private float faceTargetSpeed = 8f; // How fast to turn toward target
    [SerializeField] private bool onlyFaceWhenNotMoving = false; // Only face target when player isn't moving
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool showDebugGizmos = true;
    
    // Dependencies
    private NyxTargetingSystem targetingSystem;
    private Camera mainCamera;
    
    // Lock-on state
    private ITargetable currentLockedTarget;
    private bool isLockOnActive;
    private List<ITargetable> lockableTargets;
    private int currentTargetIndex;
    
    // UI elements
    private GameObject lockOnIndicatorInstance;
    
    // Events
    public System.Action<ITargetable> OnTargetLocked;
    public System.Action OnLockOnReleased;
    public System.Action<ITargetable> OnTargetCycled;
    
    // Properties
    public ITargetable CurrentTarget => currentLockedTarget;
    public bool IsLockOnActive => isLockOnActive;
    public bool HasTarget => currentLockedTarget != null;
    
    void Awake()
    {
        // Get or create input actions - check parent objects first since Nyx is a child
        if (playerControls == null)
        {
            // Try to find PlayerInputActions on parent objects first
            var playerMovement = GetComponentInParent<Player_Movement>();
            if (playerMovement != null && playerMovement.playerControls != null)
            {
                playerControls = playerMovement.playerControls;
                if (enableDebugLogs) Debug.Log("NyxLockOnSystem: Found PlayerInputActions on parent Player_Movement");
            }
            else
            {
                // Fallback: create new instance
                playerControls = new PlayerInputActions();
                if (enableDebugLogs) Debug.Log("NyxLockOnSystem: Created new PlayerInputActions instance");
            }
        }
        
        // Initialize collections
        lockableTargets = new List<ITargetable>();
        
        // Find dependencies
        targetingSystem = GetComponent<NyxTargetingSystem>();
        if (targetingSystem == null)
        {
            Debug.LogError("NyxLockOnSystem requires NyxTargetingSystem component on the same GameObject (Nyx)!");
        }
        
        // Don't look for camera since we're not adjusting it
        if (adjustCameraOnLockOn)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
        }
    }
    
    void OnEnable()
    {
        if (playerControls != null)
        {
            // Set up the cycle targets input action (RB/LB + scroll wheel)
            cycleTargets = playerControls.General.CycleTargets;
            cycleTargets.Enable();
            
            // Optional: Set up lock-on toggle if enabled
            if (enableLockOnToggle)
            {
                // You can add a LockOn action to your PlayerInputActions if you want toggle functionality
                // For now, we'll auto-lock when targets are available
            }
        }
        
        // Subscribe to targeting system events
        if (targetingSystem != null)
        {
            targetingSystem.OnTargetsUpdated += OnTargetsUpdated;
            
            // Configure targeting system for lock-on with wider angle to account for camera tolerance
            // The targeting system will add its own camera tolerance on top of this
            targetingSystem.ConfigureTargeting(
                lockOnRange, 
                lockOnAngle + 15f, // Add extra tolerance for lock-on (80° + 15° = 95° effective)
                new TargetType[] { TargetType.Enemy, TargetType.Boss }, 
                true
            );
        }
    }
    
    void OnDisable()
    {
        // Safely disable input actions
        cycleTargets?.Disable();
        lockOnToggle?.Disable();
        
        if (targetingSystem != null)
        {
            targetingSystem.OnTargetsUpdated -= OnTargetsUpdated;
        }
        
        ReleaseLockOn();
    }
    
    void Update()
    {
        if (!enableLockOn) return;
        
        UpdateLockOnState();
        HandleTargetCycling();
        UpdateUI();
        
        // Handle automatic facing when locked on
        if (autoFaceTarget && isLockOnActive && currentLockedTarget != null)
        {
            HandleAutoFacing();
        }
    }
    
    void LateUpdate()
    {
        // Handle camera adjustments if needed
        if (adjustCameraOnLockOn && isLockOnActive && currentLockedTarget != null)
        {
            UpdateCamera();
        }
    }
    
    /// <summary>
    /// Update lock-on state and validate current target
    /// </summary>
    private void UpdateLockOnState()
    {
        // Check if current target is still valid
        if (isLockOnActive && currentLockedTarget != null)
        {
            // Check distance
            Vector3 targetPos = currentLockedTarget.TargetPoint != null ? 
                currentLockedTarget.TargetPoint.position : 
                currentLockedTarget.Transform.position;
            
            float distance = Vector3.Distance(transform.position, targetPos);
            
            if (distance > lockOnLostDistance || !currentLockedTarget.CanBeTargeted)
            {
                if (enableDebugLogs)
                    Debug.Log($"Lock-on lost: distance={distance:F1}, canTarget={currentLockedTarget.CanBeTargeted}");
                
                ReleaseLockOn();
                return;
            }
            
            // Check if target is still in valid targets
            if (targetingSystem != null && !targetingSystem.IsTargetValid(currentLockedTarget))
            {
                if (enableDebugLogs)
                    Debug.Log("Lock-on lost: target no longer valid");
                
                ReleaseLockOn();
            }
        }
    }
    
    /// <summary>
    /// Called when targeting system updates available targets
    /// </summary>
    private void OnTargetsUpdated(List<ITargetable> targets)
    {
        lockableTargets.Clear();
        
        // Filter targets that can be locked onto (enemies and bosses)
        foreach (var target in targets)
        {
            if (target.TargetType == TargetType.Enemy || target.TargetType == TargetType.Boss)
            {
                lockableTargets.Add(target);
            }
        }
        
        // Auto-lock if enabled and no current target
        if (autoLockOnClosest && !isLockOnActive && lockableTargets.Count > 0)
        {
            var closestTarget = targetingSystem.GetClosestTargetTo(transform.position);
            if (closestTarget != null && (closestTarget.TargetType == TargetType.Enemy || closestTarget.TargetType == TargetType.Boss))
            {
                LockOnToTarget(closestTarget);
            }
        }
    }
    
    /// <summary>
    /// Handle target cycling with RB/LB and mouse scroll wheel
    /// </summary>
    private void HandleTargetCycling()
    {
        if (cycleTargets == null) return;
        
        float cycleInput = cycleTargets.ReadValue<float>();
        
        // Check if input crossed threshold (to prevent rapid cycling)
        if (Mathf.Abs(cycleInput) > cycleInputThreshold && Mathf.Abs(lastCycleInput) <= cycleInputThreshold)
        {
            // Auto-activate lock-on if not already active and we have targets
            if (!isLockOnActive && lockableTargets.Count > 0)
            {
                ActivateLockOn();
                if (enableDebugLogs)
                    Debug.Log("Auto-activated lock-on via cycling input");
            }
            else if (isLockOnActive && lockableTargets.Count > 1)
            {
                // Cycle targets based on input direction
                if (cycleInput > 0)
                {
                    CycleNextTarget();
                }
                else if (cycleInput < 0)
                {
                    CyclePreviousTarget();
                }
            }
        }
        
        lastCycleInput = cycleInput;
    }
    
    /// <summary>
    /// Activate lock-on to the best available target
    /// </summary>
    public void ActivateLockOn()
    {
        if (lockableTargets.Count == 0)
        {
            if (enableDebugLogs)
                Debug.Log("No lockable targets available");
            return;
        }
        
        // Get the best target from targeting system
        ITargetable bestTarget = null;
        if (targetingSystem != null)
        {
            // Force an immediate update to get fresh targets
            targetingSystem.ForceUpdate();
            bestTarget = targetingSystem.BestTarget;
            
            // Make sure it's a lockable type
            if (bestTarget != null && bestTarget.TargetType != TargetType.Enemy && bestTarget.TargetType != TargetType.Boss)
            {
                bestTarget = null;
            }
        }
        
        // Fallback to closest lockable target if targeting system doesn't have a good one
        if (bestTarget == null && lockableTargets.Count > 0)
        {
            float closestDistance = float.MaxValue;
            foreach (var target in lockableTargets)
            {
                Vector3 targetPos = target.TargetPoint != null ? target.TargetPoint.position : target.Transform.position;
                float distance = Vector3.Distance(transform.position, targetPos);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = target;
                }
            }
        }
        
        if (bestTarget != null)
        {
            LockOnToTarget(bestTarget);
            if (enableDebugLogs)
                Debug.Log($"Activated lock-on to: {bestTarget.Transform.name}");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("No suitable target found for lock-on");
        }
    }
    
    /// <summary>
    /// Lock onto a specific target
    /// </summary>
    public void LockOnToTarget(ITargetable target)
    {
        if (target == null || !target.CanBeTargeted) return;
        
        // Release previous lock-on
        if (currentLockedTarget != null)
        {
            currentLockedTarget.OnTargetDeselected();
        }
        
        currentLockedTarget = target;
        isLockOnActive = true;
        
        // Find target index in list for cycling
        currentTargetIndex = lockableTargets.IndexOf(target);
        if (currentTargetIndex == -1)
        {
            currentTargetIndex = 0;
        }
        
        // Notify target
        target.OnTargetSelected();
        
        // Fire event
        OnTargetLocked?.Invoke(target);
        
        if (enableDebugLogs)
            Debug.Log($"Locked onto: {target.Transform.name}");
    }
    
    /// <summary>
    /// Release current lock-on
    /// </summary>
    public void ReleaseLockOn()
    {
        if (currentLockedTarget != null)
        {
            currentLockedTarget.OnTargetDeselected();
            currentLockedTarget = null;
        }
        
        isLockOnActive = false;
        
        // Fire event
        OnLockOnReleased?.Invoke();
        
        if (enableDebugLogs)
            Debug.Log("Lock-on released");
    }
    
    /// <summary>
    /// Cycle to the next target
    /// </summary>
    public void CycleNextTarget()
    {
        if (lockableTargets.Count <= 1) return;
        
        currentTargetIndex = (currentTargetIndex + 1) % lockableTargets.Count;
        var newTarget = lockableTargets[currentTargetIndex];
        
        LockOnToTarget(newTarget);
        OnTargetCycled?.Invoke(newTarget);
        
        if (enableDebugLogs)
            Debug.Log($"Cycled to next target: {newTarget.Transform.name}");
    }
    
    /// <summary>
    /// Cycle to the previous target
    /// </summary>
    public void CyclePreviousTarget()
    {
        if (lockableTargets.Count <= 1) return;
        
        currentTargetIndex = (currentTargetIndex - 1 + lockableTargets.Count) % lockableTargets.Count;
        var newTarget = lockableTargets[currentTargetIndex];
        
        LockOnToTarget(newTarget);
        OnTargetCycled?.Invoke(newTarget);
        
        if (enableDebugLogs)
            Debug.Log($"Cycled to previous target: {newTarget.Transform.name}");
    }
    
    /// <summary>
    /// Update camera to look between Nyx and locked target
    /// </summary>
    private void UpdateCamera()
    {
        if (mainCamera == null || currentLockedTarget == null) return;
        
        Vector3 targetPos = currentLockedTarget.TargetPoint != null ? 
            currentLockedTarget.TargetPoint.position : 
            currentLockedTarget.Transform.position;
        
        // Calculate midpoint between Nyx and target
        Vector3 midpoint = (transform.position + targetPos) / 2f;
        midpoint += cameraOffset;
        
        // Smoothly move camera to look at midpoint
        if (cameraTarget != null)
        {
            Vector3 newPosition = Vector3.Lerp(cameraTarget.position, midpoint, Time.deltaTime * cameraLerpSpeed);
            cameraTarget.position = newPosition;
        }
    }
    
    /// <summary>
    /// Update UI elements
    /// </summary>
    private void UpdateUI()
    {
        if (!showLockOnIndicator) return;
        
        if (isLockOnActive && currentLockedTarget != null)
        {
            // Create indicator if it doesn't exist
            if (lockOnIndicatorInstance == null && lockOnIndicatorPrefab != null && uiCanvas != null)
            {
                lockOnIndicatorInstance = Instantiate(lockOnIndicatorPrefab, uiCanvas.transform);
            }
            
            // Update indicator position
            if (lockOnIndicatorInstance != null && mainCamera != null)
            {
                Vector3 targetPos = currentLockedTarget.TargetPoint != null ? 
                    currentLockedTarget.TargetPoint.position : 
                    currentLockedTarget.Transform.position;
                
                Vector3 screenPos = mainCamera.WorldToScreenPoint(targetPos);
                lockOnIndicatorInstance.transform.position = screenPos;
                lockOnIndicatorInstance.SetActive(screenPos.z > 0); // Hide if behind camera
            }
        }
        else
        {
            // Hide indicator
            if (lockOnIndicatorInstance != null)
            {
                lockOnIndicatorInstance.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Handle automatic facing toward locked target
    /// </summary>
    private void HandleAutoFacing()
    {
        if (currentLockedTarget?.Transform == null) return;
        
        // Check if we should face the target (respect movement setting)
        bool shouldFace = true;
        if (onlyFaceWhenNotMoving)
        {
            // Check if player is moving (you might need to adjust this based on your input system)
            Vector3 inputVector = Vector3.zero;
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                shouldFace = false;
            }
        }
        
        if (!shouldFace) return;
        
        // Calculate direction to target
        Vector3 targetPosition = currentLockedTarget.TargetPoint != null ? 
            currentLockedTarget.TargetPoint.position : 
            currentLockedTarget.Transform.position;
        
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        
        // Remove Y component to keep rotation on horizontal plane
        directionToTarget.y = 0f;
        
        if (directionToTarget.magnitude < 0.1f) return; // Too close to calculate direction
        
        // Smoothly rotate toward target
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * faceTargetSpeed);
        
        if (enableDebugLogs && Time.frameCount % 60 == 0) // Log every 60 frames to avoid spam
        {
            Debug.Log($"Auto-facing target: {currentLockedTarget.Transform.name}");
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw lock-on range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, lockOnRange);
        
        // Draw line to locked target
        if (isLockOnActive && currentLockedTarget != null)
        {
            Vector3 targetPos = currentLockedTarget.TargetPoint != null ? 
                currentLockedTarget.TargetPoint.position : 
                currentLockedTarget.Transform.position;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPos);
            
            // Draw target indicator
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 1f);
        }
    }
} 