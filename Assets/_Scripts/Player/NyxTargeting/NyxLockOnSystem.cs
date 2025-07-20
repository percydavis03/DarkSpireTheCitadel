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
    [SerializeField] private bool autoLockOnClosest = false;
    
    [Header("Input")]
    [SerializeField] private PlayerInputActions playerControls;
    [SerializeField] private float cycleInputThreshold = 0.3f;

    [Header("Rotation Settings")]
    [SerializeField] private bool enableAutoRotation = false;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private bool onlyRotateWhenLocked = true;
    [SerializeField] private float rotationThreshold = 5f; // Don't rotate if already facing target within this angle
    
    private InputAction cycleTargets;
    private float lastCycleInput;
    
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
    }
    
    private void UpdateLockOnState()
    {
        if (isLockOnActive && currentLockedTarget != null)
        {
            Vector3 targetPos = currentLockedTarget.TargetPoint != null ? 
                currentLockedTarget.TargetPoint.position : 
                currentLockedTarget.Transform.position;
            
            float distance = Vector3.Distance(nyxTransform.position, targetPos);
            
            if (distance > lockOnLostDistance || !currentLockedTarget.CanBeTargeted)
            {
                ReleaseLockOn();
            }
        }
    }
    
    private void HandleTargetRotation()
    {
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
        
        if (autoLockOnClosest && !isLockOnActive && lockableTargets.Count > 0)
        {
            var closestTarget = targetingSystem.GetClosestTargetTo(nyxTransform.position);
            if (closestTarget != null && (closestTarget.TargetType == TargetType.Enemy || closestTarget.TargetType == TargetType.Boss))
            {
                LockOnToTarget(closestTarget);
            }
        }
    }
    
    private void HandleTargetCycling()
    {
        if (cycleTargets == null) return;
        
        float cycleInput = cycleTargets.ReadValue<float>();
        
        if (Mathf.Abs(cycleInput) > cycleInputThreshold && Mathf.Abs(lastCycleInput) <= cycleInputThreshold)
        {
            if (!isLockOnActive && lockableTargets.Count > 0)
            {
                ActivateLockOn();
            }
            else if (isLockOnActive && lockableTargets.Count > 1)
            {
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
    
    public void ActivateLockOn()
    {
        if (lockableTargets.Count == 0) return;
        
        ITargetable bestTarget = null;
        if (targetingSystem != null)
        {
            targetingSystem.ForceUpdate();
            bestTarget = targetingSystem.BestTarget;
            
            if (bestTarget != null && bestTarget.TargetType != TargetType.Enemy && bestTarget.TargetType != TargetType.Boss)
            {
                bestTarget = null;
            }
        }
        
        if (bestTarget == null && lockableTargets.Count > 0)
        {
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
        }
        
        if (bestTarget != null)
        {
            LockOnToTarget(bestTarget);
        }
    }
    
    public void LockOnToTarget(ITargetable target)
    {
        if (target == null || !target.CanBeTargeted) return;
        
        if (currentLockedTarget != null)
        {
            currentLockedTarget.OnTargetDeselected();
        }
        
        currentLockedTarget = target;
        isLockOnActive = true;
        
        currentTargetIndex = lockableTargets.IndexOf(target);
        if (currentTargetIndex == -1)
        {
            currentTargetIndex = 0;
        }
        
        target.OnTargetSelected();
        ShowRedDot(target);
        OnTargetLocked?.Invoke(target);
    }
    
    public void ReleaseLockOn()
    {
        if (currentLockedTarget != null)
        {
            currentLockedTarget.OnTargetDeselected();
            currentLockedTarget = null;
        }
        
        isLockOnActive = false;
        HideRedDot();
        OnLockOnReleased?.Invoke();
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
            currentRedDot.SetActive(false);
        }
    }
    
    private void UpdateRedDot()
    {
        if (isLockOnActive && currentLockedTarget != null && currentRedDot != null)
        {
            UpdateRedDotPosition(currentLockedTarget.Transform);
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