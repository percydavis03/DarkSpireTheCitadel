using UnityEngine;

[CreateAssetMenu(fileName = "New Knockback Data", menuName = "Combat/Knockback Data")]
public class KnockbackData : ScriptableObject
{
    [Header("Force Settings")]
    [Tooltip("Base knockback distance in units")]
    public float knockbackDistance = 3f;
    
    [Tooltip("Duration of the knockback effect")]
    public float duration = 0.4f;
    
    [Tooltip("Curve controlling knockback movement over time")]
    public AnimationCurve movementCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),     // Start slow
        new Keyframe(0.3f, 1f, 0f, 0f),   // Peak quickly 
        new Keyframe(1f, 1f, 0f, 0f)      // Hold at end
    );
    
    [Header("Restrictions")]
    [Tooltip("Minimum distance required to apply knockback")]
    public float minimumDistance = 0.1f;
    
    [Tooltip("Maximum distance the knockback can reach")]
    public float maximumDistance = 10f;
    
    [Header("Special Effects")]
    [Tooltip("Time to pause before knockback starts (for impact frames)")]
    public float impactPause = 0.1f;
    
    [Tooltip("Should the entity briefly pause all actions during knockback")]
    public bool pauseActions = true;
    
    [Tooltip("Recovery time after knockback ends")]
    public float recoveryTime = 0.2f;
    
    [Header("Audio/Visual")]
    [Tooltip("Sound effect to play on knockback")]
    public AudioClip knockbackSound;
    
    [Tooltip("Particle effect to spawn at impact point")]
    public GameObject impactEffect;
    
    [Header("Mass Scaling")]
    [Tooltip("How much mass affects knockback (0 = no effect, 1 = full effect)")]
    [Range(0f, 1f)]
    public float massInfluence = 0.3f;
    
    [Tooltip("Reference mass for scaling calculations")]
    public float referenceMass = 1f;
    
    /// <summary>
    /// Calculate final knockback distance based on entity mass
    /// </summary>
    public float GetScaledDistance(float entityMass)
    {
        if (massInfluence <= 0f) return knockbackDistance;
        
        float massRatio = referenceMass / Mathf.Max(entityMass, 0.1f);
        float scaledDistance = knockbackDistance * Mathf.Lerp(1f, massRatio, massInfluence);
        
        return Mathf.Clamp(scaledDistance, minimumDistance, maximumDistance);
    }
    
    /// <summary>
    /// Validate data on load
    /// </summary>
    private void OnValidate()
    {
        duration = Mathf.Max(0.1f, duration);
        knockbackDistance = Mathf.Max(0f, knockbackDistance);
        minimumDistance = Mathf.Max(0f, minimumDistance);
        maximumDistance = Mathf.Max(minimumDistance, maximumDistance);
        impactPause = Mathf.Max(0f, impactPause);
        recoveryTime = Mathf.Max(0f, recoveryTime);
        referenceMass = Mathf.Max(0.1f, referenceMass);
        
        // Ensure movement curve has proper keyframes
        if (movementCurve == null || movementCurve.keys.Length == 0)
        {
            movementCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2f),     
                new Keyframe(0.3f, 1f, 0f, 0f),   
                new Keyframe(1f, 1f, 0f, 0f)      
            );
        }
    }
} 