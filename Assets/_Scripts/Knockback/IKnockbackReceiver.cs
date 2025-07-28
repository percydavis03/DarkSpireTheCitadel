using UnityEngine;
using System;

public interface IKnockbackReceiver
{
    /// <summary>
    /// Whether this entity can currently receive knockback
    /// </summary>
    bool CanReceiveKnockback { get; }
    
    /// <summary>
    /// The mass of this entity for knockback calculations
    /// </summary>
    float Mass { get; }
    
    /// <summary>
    /// Transform of the entity being knocked back
    /// </summary>
    Transform Transform { get; }
    
    /// <summary>
    /// Apply knockback to this entity
    /// </summary>
    /// <param name="direction">Horizontal direction of knockback (Y component ignored)</param>
    /// <param name="knockbackData">Data defining the knockback properties</param>
    /// <param name="multiplier">Multiplier for the knockback force</param>
    void ReceiveKnockback(Vector3 direction, KnockbackData knockbackData, float multiplier = 1f);
    
    /// <summary>
    /// Called when knockback starts
    /// </summary>
    void OnKnockbackStarted(KnockbackData data);
    
    /// <summary>
    /// Called when knockback ends
    /// </summary>
    void OnKnockbackEnded(KnockbackData data);
    
    /// <summary>
    /// Check if currently being knocked back
    /// </summary>
    bool IsBeingKnockedBack { get; }
}

/// <summary>
/// Event data for knockback events
/// </summary>
[System.Serializable]
public class KnockbackEventData
{
    public IKnockbackReceiver receiver;
    public Vector3 direction;
    public KnockbackData knockbackData;
    public float multiplier;
    public Vector3 impactPoint;
    
    public KnockbackEventData(IKnockbackReceiver receiver, Vector3 direction, KnockbackData data, float multiplier, Vector3 impactPoint)
    {
        this.receiver = receiver;
        this.direction = direction;
        this.knockbackData = data;
        this.multiplier = multiplier;
        this.impactPoint = impactPoint;
    }
}

/// <summary>
/// Static events for knockback system
/// </summary>
public static class KnockbackEvents
{
    /// <summary>
    /// Fired when any entity receives knockback
    /// </summary>
    public static event Action<KnockbackEventData> OnKnockbackApplied;
    
    /// <summary>
    /// Fired when knockback starts on an entity
    /// </summary>
    public static event Action<IKnockbackReceiver> OnKnockbackStarted;
    
    /// <summary>
    /// Fired when knockback ends on an entity
    /// </summary>
    public static event Action<IKnockbackReceiver> OnKnockbackEnded;
    
    internal static void TriggerKnockbackApplied(KnockbackEventData eventData)
    {
        OnKnockbackApplied?.Invoke(eventData);
    }
    
    internal static void TriggerKnockbackStarted(IKnockbackReceiver receiver)
    {
        OnKnockbackStarted?.Invoke(receiver);
    }
    
    internal static void TriggerKnockbackEnded(IKnockbackReceiver receiver)
    {
        OnKnockbackEnded?.Invoke(receiver);
    }
} 