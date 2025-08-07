using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class KnockbackManager : MonoBehaviour
{
    private static KnockbackManager _instance;
    public static KnockbackManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<KnockbackManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("KnockbackManager");
                    _instance = go.AddComponent<KnockbackManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Knockback Data References")]
    [SerializeField] private KnockbackData defaultKnockbackData;
    
    [Header("Attack Type Knockback Data")]
    [SerializeField] [Tooltip("Knockback data for foot/kick attacks")]
    private KnockbackData footAttackKnockbackData;
    
    [SerializeField] [Tooltip("Knockback data for weapon attacks")]
    private KnockbackData weaponAttackKnockbackData;
    
    [SerializeField] [Tooltip("Knockback data for parry counterattacks")]
    private KnockbackData parryKnockbackData;
    
    [SerializeField] [Tooltip("Knockback data for enemy weapon attacks")]
    private KnockbackData enemyWeaponKnockbackData;
    
    [Header("Foot Attack Settings")]
    [SerializeField] [Tooltip("Force multiplier for foot attacks")]
    private float footAttackMultiplier = 2f;
    
    [SerializeField] [Tooltip("Range for foot attacks")]
    private float footAttackRange = 3f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool showDebugRays = false;
    [SerializeField] private float debugRayDuration = 2f;
    
    private HashSet<IKnockbackReceiver> activeKnockbacks = new HashSet<IKnockbackReceiver>();
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Apply knockback to a target
    /// </summary>
    /// <param name="target">Target to knockback</param>
    /// <param name="sourcePosition">Position where knockback originates from</param>
    /// <param name="knockbackData">Knockback configuration (null uses default)</param>
    /// <param name="multiplier">Force multiplier</param>
    /// <param name="overrideDirection">Optional override for knockback direction</param>
    public bool ApplyKnockback(IKnockbackReceiver target, Vector3 sourcePosition, KnockbackData knockbackData = null, float multiplier = 1f, Vector3? overrideDirection = null)
    {
        if (target == null || !target.CanReceiveKnockback)
        {
            if (enableDebugLogs) Debug.Log($"Cannot apply knockback - target invalid or cannot receive knockback");
            return false;
        }
        
        // Use default data if none provided
        if (knockbackData == null)
        {
            knockbackData = defaultKnockbackData;
            if (knockbackData == null)
            {
                Debug.LogError("No knockback data provided and no default data set!");
                return false;
            }
        }
        
        // Calculate direction (horizontal only)
        Vector3 direction;
        if (overrideDirection.HasValue)
        {
            direction = overrideDirection.Value;
        }
        else
        {
            direction = target.Transform.position - sourcePosition;
        }
        
        // Force horizontal direction only
        direction.y = 0f;
        direction = direction.normalized;
        
        if (direction.magnitude < 0.1f)
        {
            // Fallback to forward direction if positions are too close
            direction = target.Transform.forward;
            direction.y = 0f;
            direction = direction.normalized;
        }
        
        // Debug visualization
        if (showDebugRays)
        {
            Debug.DrawRay(target.Transform.position, direction * knockbackData.knockbackDistance, Color.red, debugRayDuration);
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"Applying knockback to {target.Transform.name} - Direction: {direction}, Distance: {knockbackData.GetScaledDistance(target.Mass)}, Multiplier: {multiplier}");
        }
        
        // Create event data
        var eventData = new KnockbackEventData(target, direction, knockbackData, multiplier, sourcePosition);
        
        // Apply the knockback
        target.ReceiveKnockback(direction, knockbackData, multiplier);
        
        // Fire event
        KnockbackEvents.TriggerKnockbackApplied(eventData);
        
        return true;
    }
    
    /// <summary>
    /// Apply knockback using GameObject (convenience method)
    /// </summary>
    public bool ApplyKnockback(GameObject target, Vector3 sourcePosition, KnockbackData knockbackData = null, float multiplier = 1f, Vector3? overrideDirection = null)
    {
        var knockbackReceiver = target.GetComponent<KnockbackReceiver>();
        if (knockbackReceiver == null)
        {
            if (enableDebugLogs) Debug.LogWarning($"GameObject {target.name} does not have KnockbackReceiver component");
            return false;
        }
        
        return ApplyKnockback(knockbackReceiver, sourcePosition, knockbackData, multiplier, overrideDirection);
    }
    
    /// <summary>
    /// Apply knockback using Collider (convenience method)
    /// </summary>
    public bool ApplyKnockback(Collider target, Vector3 sourcePosition, KnockbackData knockbackData = null, float multiplier = 1f, Vector3? overrideDirection = null)
    {
        return ApplyKnockback(target.gameObject, sourcePosition, knockbackData, multiplier, overrideDirection);
    }
    
    /// <summary>
    /// Apply knockback to all targets within a spherical area
    /// </summary>
    /// <param name="center">Center of the area effect</param>
    /// <param name="radius">Radius of effect</param>
    /// <param name="layerMask">Layer mask to filter targets</param>
    /// <param name="knockbackData">Knockback configuration</param>
    /// <param name="multiplier">Force multiplier</param>
    /// <param name="requireTag">Optional tag requirement for targets</param>
    /// <returns>Number of targets successfully knocked back</returns>
    public int ApplyAreaKnockback(Vector3 center, float radius, LayerMask layerMask, KnockbackData knockbackData = null, float multiplier = 1f, string requireTag = null)
    {
        Collider[] targets = Physics.OverlapSphere(center, radius, layerMask);
        int successCount = 0;
        
        foreach (Collider target in targets)
        {
            // Skip if tag requirement not met
            if (!string.IsNullOrEmpty(requireTag) && !target.gameObject.CompareTag(requireTag))
                continue;
            
            // Apply knockback
            if (ApplyKnockback(target, center, knockbackData, multiplier))
            {
                successCount++;
            }
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"Area knockback: Affected {successCount}/{targets.Length} targets within {radius} units");
        }
        
        return successCount;
    }
    
    /// <summary>
    /// Apply foot attack knockback using the configured foot attack settings
    /// </summary>
    /// <param name="center">Center of the foot attack</param>
    /// <param name="customData">Optional override knockback data (uses footAttackKnockbackData if null)</param>
    /// <returns>Number of targets successfully knocked back</returns>
    public int ApplyFootAttackKnockback(Vector3 center, KnockbackData customData = null)
    {
        KnockbackData dataToUse = customData ?? footAttackKnockbackData ?? defaultKnockbackData;
        
        if (dataToUse == null)
        {
            Debug.LogError("No knockback data available for foot attack!");
            return 0;
        }
        
        string targetTag = string.IsNullOrEmpty(dataToUse.requiredTag) ? "Enemy" : dataToUse.requiredTag;
        return ApplyAreaKnockback(center, dataToUse.areaRadius, dataToUse.targetLayers, dataToUse, dataToUse.forceMultiplier, targetTag);
    }
    
    /// <summary>
    /// Register a knockback as active (called by KnockbackReceiver)
    /// </summary>
    internal void RegisterActiveKnockback(IKnockbackReceiver receiver)
    {
        activeKnockbacks.Add(receiver);
        KnockbackEvents.TriggerKnockbackStarted(receiver);
        
        if (enableDebugLogs) Debug.Log($"Knockback started on {receiver.Transform.name}. Active knockbacks: {activeKnockbacks.Count}");
    }
    
    /// <summary>
    /// Unregister a knockback (called by KnockbackReceiver)
    /// </summary>
    internal void UnregisterActiveKnockback(IKnockbackReceiver receiver)
    {
        activeKnockbacks.Remove(receiver);
        KnockbackEvents.TriggerKnockbackEnded(receiver);
        
        if (enableDebugLogs) Debug.Log($"Knockback ended on {receiver.Transform.name}. Active knockbacks: {activeKnockbacks.Count}");
    }
    
    /// <summary>
    /// Get count of currently active knockbacks
    /// </summary>
    public int ActiveKnockbackCount => activeKnockbacks.Count;
    
    /// <summary>
    /// Get the configured foot attack multiplier
    /// </summary>
    public float FootAttackMultiplier => footAttackMultiplier;
    
    /// <summary>
    /// Get the configured foot attack range
    /// </summary>
    public float FootAttackRange => footAttackRange;
    
    /// <summary>
    /// Check if a specific receiver is currently being knocked back
    /// </summary>
    public bool IsKnockingBack(IKnockbackReceiver receiver)
    {
        return activeKnockbacks.Contains(receiver);
    }
    
    /// <summary>
    /// Stop all active knockbacks (emergency stop)
    /// </summary>
    public void StopAllKnockbacks()
    {
        var receivers = new List<IKnockbackReceiver>(activeKnockbacks);
        foreach (var receiver in receivers)
        {
            if (receiver != null && receiver.IsBeingKnockedBack)
            {
                // This should trigger the receiver to stop its knockback
                receiver.OnKnockbackEnded(null);
            }
        }
        activeKnockbacks.Clear();
        
        if (enableDebugLogs) Debug.Log("Stopped all active knockbacks");
    }
    
    /// <summary>
    /// Apply weapon attack knockback
    /// </summary>
    public bool ApplyWeaponKnockback(IKnockbackReceiver target, Vector3 sourcePosition, KnockbackData customData = null)
    {
        KnockbackData dataToUse = customData ?? weaponAttackKnockbackData ?? defaultKnockbackData;
        return ApplyKnockback(target, sourcePosition, dataToUse, dataToUse?.forceMultiplier ?? 1f);
    }
    
    /// <summary>
    /// Apply parry counterattack knockback
    /// </summary>
    public bool ApplyParryKnockback(IKnockbackReceiver target, Vector3 sourcePosition, KnockbackData customData = null)
    {
        KnockbackData dataToUse = customData ?? parryKnockbackData ?? defaultKnockbackData;
        return ApplyKnockback(target, sourcePosition, dataToUse, dataToUse?.forceMultiplier ?? 1f);
    }
    
    /// <summary>
    /// Apply enemy weapon attack knockback
    /// </summary>
    public bool ApplyEnemyWeaponKnockback(IKnockbackReceiver target, Vector3 sourcePosition, KnockbackData customData = null)
    {
        KnockbackData dataToUse = customData ?? enemyWeaponKnockbackData ?? defaultKnockbackData;
        return ApplyKnockback(target, sourcePosition, dataToUse, dataToUse?.forceMultiplier ?? 1f);
    }
    
    /// <summary>
    /// Create default knockback data if none exists
    /// </summary>
    [ContextMenu("Create Default Knockback Data")]
    private void CreateDefaultKnockbackData()
    {
        if (defaultKnockbackData == null)
        {
            defaultKnockbackData = ScriptableObject.CreateInstance<KnockbackData>();
            Debug.Log("Created default knockback data. Consider saving it as an asset for persistence.");
        }
    }
    
    private void OnValidate()
    {
        if (defaultKnockbackData == null)
        {
            Debug.LogWarning("KnockbackManager: No default knockback data assigned. Some operations may fail.");
        }
        
        // Validate foot attack settings
        footAttackMultiplier = Mathf.Max(0.1f, footAttackMultiplier);
        footAttackRange = Mathf.Max(0.1f, footAttackRange);
    }
} 