using UnityEngine;

[CreateAssetMenu(fileName = "New Knockback Data", menuName = "Combat/Knockback Data")]
public class KnockbackData : ScriptableObject
{
    [Header("Basic Settings")]
    [Tooltip("How far to knockback (in units)")]
    public float knockbackDistance = 3f;
    
    [Tooltip("How long the knockback takes (in seconds)")]
    public float duration = 0.4f;
    
    [Tooltip("Time before knockback movement starts (impact pause)")]
    public float impactPause = 0.1f;
    
    [Tooltip("Time after knockback before entity can act again")]
    public float recoveryTime = 0.1f;
    
    [Tooltip("Animation curve for knockback movement")]
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Optional Effects")]
    [Tooltip("Sound to play on knockback")]
    public AudioClip knockbackSound;
    
    [Tooltip("Effect to spawn on impact")]
    public GameObject impactEffect;
    
    [Header("Advanced")]
    [Tooltip("How much object mass affects knockback distance")]
    [Range(0f, 1f)]
    public float massEffect = 0.3f;
    
    /// <summary>
    /// Get final knockback distance based on object mass
    /// </summary>
    public float GetScaledDistance(float objectMass = 1f)
    {
        if (massEffect <= 0f) return knockbackDistance;
        
        float massMultiplier = Mathf.Lerp(1f, 1f / objectMass, massEffect);
        return knockbackDistance * massMultiplier;
    }
    
    private void OnValidate()
    {
        knockbackDistance = Mathf.Max(0f, knockbackDistance);
        duration = Mathf.Max(0.1f, duration);
        impactPause = Mathf.Max(0f, impactPause);
        recoveryTime = Mathf.Max(0f, recoveryTime);
        
        // Ensure curve exists
        if (movementCurve == null)
            movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }
} 