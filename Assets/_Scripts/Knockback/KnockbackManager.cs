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
    
    [Header("Default Settings")]
    [SerializeField] private KnockbackData defaultKnockbackData;
    
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
    }
} 