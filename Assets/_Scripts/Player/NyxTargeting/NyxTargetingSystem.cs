using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Shared targeting system for Nyx that handles raycast and cone detection
/// Can be used by grapple, lock-on, and other targeting-based systems
/// </summary>
public class NyxTargetingSystem : MonoBehaviour
{
    [Header("Targeting Settings")]
    [SerializeField] private float targetingRange = 15f;
    [SerializeField] private float targetingAngle = 75f; // Increased from 45f to be more forgiving
    [SerializeField] private LayerMask obstacleLayerMask = -1;
    [SerializeField] private Transform targetingOrigin;
    
    [Header("Camera-Based Targeting")]
    [SerializeField] private bool useCameraDirection = true; // New option for camera-based targeting
    [SerializeField] private Camera targetingCamera; // Will auto-find Camera.main if null
    [SerializeField] private float cameraAngleTolerance = 5f; // Extra tolerance for camera-based targeting
    
    [Header("Target Filtering")]
    [SerializeField] private TargetType[] allowedTargetTypes = { TargetType.Enemy, TargetType.Grappleable };
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private bool prioritizeClosestTarget = true;
    
    [Header("Performance")]
    [SerializeField] private float updateFrequency = 0.1f; // How often to scan for targets
    [SerializeField] private int maxTargetsPerFrame = 10; // Limit for performance
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool showDebugVisualization = true;
    [SerializeField] private Color coneColor = Color.blue;
    [SerializeField] private Color targetLineColor = Color.green;
    [SerializeField] private Color blockedLineColor = Color.red;
    
    // Events
    public System.Action<ITargetable> OnTargetDetected;
    public System.Action<ITargetable> OnTargetLost;
    public System.Action<List<ITargetable>> OnTargetsUpdated;
    
    // Current state
    private List<ITargetable> availableTargets = new List<ITargetable>();
    private List<ITargetable> validTargets = new List<ITargetable>();
    private ITargetable bestTarget;
    private float lastUpdateTime;
    
    // Cached components
    private Transform cachedTransform;
    
    // Properties
    public float TargetingRange => targetingRange;
    public float TargetingAngle => targetingAngle;
    public Transform TargetingOrigin => targetingOrigin != null ? targetingOrigin : cachedTransform;
    public ITargetable BestTarget => bestTarget;
    public List<ITargetable> ValidTargets => new List<ITargetable>(validTargets);
    public bool HasValidTargets => validTargets.Count > 0;
    
    void Awake()
    {
        cachedTransform = transform;
        
        // Auto-assign targeting origin if not set
        if (targetingOrigin == null)
        {
            targetingOrigin = cachedTransform;
        }
        
        // Auto-find camera if not set
        if (targetingCamera == null)
        {
            targetingCamera = Camera.main;
            if (targetingCamera == null)
            {
                targetingCamera = FindObjectOfType<Camera>();
            }
        }
        
        // Initialize collections
        availableTargets = new List<ITargetable>();
        validTargets = new List<ITargetable>();
    }
    
    void Update()
    {
        // Update targets at specified frequency
        if (Time.time - lastUpdateTime >= updateFrequency)
        {
            UpdateTargeting();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// Main targeting update method - scans for and evaluates targets
    /// </summary>
    public void UpdateTargeting()
    {
        Vector3 origin = TargetingOrigin.position;
        Vector3 forward = GetTargetingDirection();
        
        // Step 1: Find all potential targets in range
        FindPotentialTargets(origin);
        
        // Step 2: Filter targets by angle and line of sight
        FilterValidTargets(origin, forward);
        
        // Step 3: Select best target
        SelectBestTarget();
        
        // Step 4: Notify listeners of updates
        OnTargetsUpdated?.Invoke(validTargets);
        
        if (enableDebugLogs)
        {
            Debug.Log($"Targeting Update: {availableTargets.Count} potential, {validTargets.Count} valid targets. Best: {(bestTarget != null ? bestTarget.Transform.name : "None")}");
        }
    }
    
    /// <summary>
    /// Get the targeting direction based on camera or character orientation
    /// </summary>
    private Vector3 GetTargetingDirection()
    {
        if (useCameraDirection && targetingCamera != null)
        {
            // Use camera forward direction projected onto horizontal plane
            Vector3 cameraForward = targetingCamera.transform.forward;
            cameraForward.y = 0f; // Remove vertical component for better targeting
            return cameraForward.normalized;
        }
        else
        {
            // Use character forward direction
            return TargetingOrigin.forward;
        }
    }
    
    /// <summary>
    /// Find all potential targetable objects in range
    /// </summary>
    private void FindPotentialTargets(Vector3 origin)
    {
        availableTargets.Clear();
        
        // Use OverlapSphere to find all colliders in range
        Collider[] colliders = Physics.OverlapSphere(origin, targetingRange);
        
        int processed = 0;
        foreach (var collider in colliders)
        {
            // Performance limit
            if (processed >= maxTargetsPerFrame) break;
            
            // Try to get ITargetable component
            ITargetable targetable = collider.GetComponent<ITargetable>();
            if (targetable == null)
            {
                // Also check parent objects
                targetable = collider.GetComponentInParent<ITargetable>();
            }
            
            if (targetable != null && IsAllowedTargetType(targetable.TargetType) && targetable.CanBeTargeted)
            {
                availableTargets.Add(targetable);
                processed++;
            }
        }
    }
    
    /// <summary>
    /// Filter targets by angle and line of sight
    /// </summary>
    private void FilterValidTargets(Vector3 origin, Vector3 forward)
    {
        validTargets.Clear();
        
        // Calculate effective angle tolerance
        float effectiveAngle = targetingAngle;
        if (useCameraDirection)
        {
            effectiveAngle += cameraAngleTolerance; // More forgiving for camera-based targeting
        }
        
        foreach (var target in availableTargets)
        {
            if (target?.Transform == null) continue;
            
            Vector3 targetPosition = target.TargetPoint != null ? target.TargetPoint.position : target.Transform.position;
            Vector3 directionToTarget = (targetPosition - origin).normalized;
            
            // Check angle
            float angle = Vector3.Angle(forward, directionToTarget);
            if (angle > effectiveAngle)
            {
                if (enableDebugLogs)
                    Debug.Log($"Target {target.Transform.name} outside angle: {angle:F1}° (max: {effectiveAngle}°)");
                continue;
            }
            
            // Check line of sight if required
            if (requireLineOfSight && !HasLineOfSight(origin, targetPosition))
            {
                if (enableDebugLogs)
                    Debug.Log($"Target {target.Transform.name} blocked by obstacle");
                continue;
            }
            
            validTargets.Add(target);
        }
    }
    
    /// <summary>
    /// Check if there's a clear line of sight to the target
    /// </summary>
    private bool HasLineOfSight(Vector3 origin, Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - origin).normalized;
        float distance = Vector3.Distance(origin, targetPosition);
        
        // Perform raycast
        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, obstacleLayerMask))
        {
            // Check if we hit the target or something in front of it
            ITargetable hitTargetable = hit.collider.GetComponent<ITargetable>();
            if (hitTargetable == null)
            {
                hitTargetable = hit.collider.GetComponentInParent<ITargetable>();
            }
            
            // If we hit a targetable object, that's fine (we hit our target or another valid target)
            if (hitTargetable != null)
            {
                return true;
            }
            
            // Otherwise, we hit an obstacle
            return false;
        }
        
        // No obstacles hit
        return true;
    }
    
    /// <summary>
    /// Select the best target from valid targets
    /// </summary>
    private void SelectBestTarget()
    {
        ITargetable previousBestTarget = bestTarget;
        bestTarget = null;
        
        if (validTargets.Count == 0)
        {
            if (previousBestTarget != null)
            {
                OnTargetLost?.Invoke(previousBestTarget);
            }
            return;
        }
        
        if (prioritizeClosestTarget)
        {
            // Find closest target
            float closestDistance = float.MaxValue;
            Vector3 origin = TargetingOrigin.position;
            
            foreach (var target in validTargets)
            {
                Vector3 targetPos = target.TargetPoint != null ? target.TargetPoint.position : target.Transform.position;
                float distance = Vector3.Distance(origin, targetPos);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = target;
                }
            }
        }
        else
        {
            // Use priority-based selection
            bestTarget = validTargets.OrderByDescending(t => t.TargetPriority).First();
        }
        
        // Notify if target changed
        if (bestTarget != previousBestTarget)
        {
            if (previousBestTarget != null)
            {
                OnTargetLost?.Invoke(previousBestTarget);
            }
            
            if (bestTarget != null)
            {
                OnTargetDetected?.Invoke(bestTarget);
            }
        }
    }
    
    /// <summary>
    /// Check if a target type is allowed
    /// </summary>
    private bool IsAllowedTargetType(TargetType targetType)
    {
        return allowedTargetTypes.Contains(targetType);
    }
    
    /// <summary>
    /// Force an immediate targeting update (useful for other systems)
    /// </summary>
    public void ForceUpdate()
    {
        UpdateTargeting();
    }
    
    /// <summary>
    /// Check if a specific target is currently valid
    /// </summary>
    public bool IsTargetValid(ITargetable target)
    {
        return validTargets.Contains(target);
    }
    
    /// <summary>
    /// Get the closest valid target to a specific position
    /// </summary>
    public ITargetable GetClosestTargetTo(Vector3 position)
    {
        if (validTargets.Count == 0) return null;
        
        float closestDistance = float.MaxValue;
        ITargetable closestTarget = null;
        
        foreach (var target in validTargets)
        {
            Vector3 targetPos = target.TargetPoint != null ? target.TargetPoint.position : target.Transform.position;
            float distance = Vector3.Distance(position, targetPos);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }
        
        return closestTarget;
    }
    
    /// <summary>
    /// Configure targeting parameters (useful for different systems with different needs)
    /// </summary>
    public void ConfigureTargeting(float range, float angle, TargetType[] types, bool requireLoS = true)
    {
        targetingRange = range;
        targetingAngle = angle;
        allowedTargetTypes = types;
        requireLineOfSight = requireLoS;
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugVisualization) return;
        
        Vector3 origin = TargetingOrigin.position;
        Vector3 forward = GetTargetingDirection();
        
        // Draw targeting cone
        DrawTargetingCone(origin, forward, targetingRange, targetingAngle);
        
        // Draw lines to valid targets
        foreach (var target in validTargets)
        {
            if (target?.Transform == null) continue;
            
            Vector3 targetPos = target.TargetPoint != null ? target.TargetPoint.position : target.Transform.position;
            Color lineColor = target == bestTarget ? Color.yellow : targetLineColor;
            
            Gizmos.color = lineColor;
            Gizmos.DrawLine(origin, targetPos);
            
            // Draw target indicator
            Gizmos.DrawWireSphere(targetPos, 0.5f);
        }
        
        // Draw lines to blocked targets
        if (enableDebugLogs)
        {
            foreach (var target in availableTargets)
            {
                if (validTargets.Contains(target) || target?.Transform == null) continue;
                
                Vector3 targetPos = target.TargetPoint != null ? target.TargetPoint.position : target.Transform.position;
                Gizmos.color = blockedLineColor;
                Gizmos.DrawLine(origin, targetPos);
            }
        }
    }
    
    /// <summary>
    /// Draw a targeting cone for visualization
    /// </summary>
    private void DrawTargetingCone(Vector3 origin, Vector3 direction, float range, float angle)
    {
        Gizmos.color = coneColor;
        
        // Draw center line
        Gizmos.DrawRay(origin, direction * range);
        
        // Draw cone edges
        Vector3 rightEdge = Quaternion.Euler(0, angle, 0) * direction;
        Vector3 leftEdge = Quaternion.Euler(0, -angle, 0) * direction;
        
        Gizmos.DrawRay(origin, rightEdge * range);
        Gizmos.DrawRay(origin, leftEdge * range);
        
        // Draw range circle
        Gizmos.color = new Color(coneColor.r, coneColor.g, coneColor.b, 0.1f);
        Gizmos.DrawWireSphere(origin, range);
    }
} 