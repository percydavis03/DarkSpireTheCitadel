using UnityEngine;

/// <summary>
/// Simple example showing how to use the unified knockback system.
/// Attach to any GameObject and press T to test knockback on enemies.
/// </summary>
public class KnockbackExample : MonoBehaviour
{
    [Header("Example Knockback Data")]
    [Tooltip("Assign different knockback data assets to test various types")]
    public KnockbackData lightKnockback;
    public KnockbackData heavyKnockback;
    
    [Header("Testing")]
    [Tooltip("Enable to show debug rays and logs")]
    public bool enableDebug = true;
    
    private void Update()
    {
        // Press T to test knockback on nearest enemy
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestKnockbackOnNearestEnemy();
        }
        
        // Press Y for heavy knockback test
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TestHeavyKnockbackOnNearestEnemy();
        }
    }
    
    /// <summary>
    /// Example: Basic knockback usage
    /// </summary>
    private void TestKnockbackOnNearestEnemy()
    {
        GameObject enemy = GameObject.FindWithTag("Enemy");
        if (enemy != null)
        {
            // Basic knockback - uses default data if lightKnockback not assigned
            KnockbackManager.Instance.ApplyKnockback(
                enemy,                      // Target to knockback
                transform.position,         // Source position (where attack came from)
                lightKnockback,            // Knockback configuration (null uses default)
                1.0f                       // Force multiplier
            );
            
            if (enableDebug)
                Debug.Log($"🎯 Applied light knockback to {enemy.name}");
        }
        else
        {
            if (enableDebug)
                Debug.LogWarning("No enemy found with 'Enemy' tag");
        }
    }
    
    /// <summary>
    /// Example: Heavy knockback with increased force
    /// </summary>
    private void TestHeavyKnockbackOnNearestEnemy()
    {
        GameObject enemy = GameObject.FindWithTag("Enemy");
        if (enemy != null)
        {
            // Heavy knockback with force multiplier
            KnockbackManager.Instance.ApplyKnockback(
                enemy,
                transform.position,
                heavyKnockback ?? lightKnockback, // Fallback to light if heavy not assigned
                1.5f                              // 50% more force
            );
            
            if (enableDebug)
                Debug.Log($"💥 Applied heavy knockback to {enemy.name}");
        }
    }
    
    /// <summary>
    /// Example: Weapon collision integration
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Example of how to integrate with weapon systems
        if (other.CompareTag("Enemy") && lightKnockback != null)
        {
            // Apply knockback first
            bool knockbackApplied = KnockbackManager.Instance.ApplyKnockback(
                other.gameObject,
                transform.position,
                lightKnockback
            );
            
            if (knockbackApplied && enableDebug)
            {
                Debug.Log($"⚔️ Weapon hit: Applied knockback to {other.name}");
            }
            
            // Then apply damage or other effects
            // var enemy = other.GetComponent<Enemy_Basic>();
            // enemy?.TakeDamage();
        }
    }
    
    /// <summary>
    /// Example: Custom direction knockback (for special attacks)
    /// </summary>
    public void ApplyDirectionalKnockback(GameObject target, Vector3 direction)
    {
        KnockbackManager.Instance.ApplyKnockback(
            target,
            transform.position,
            heavyKnockback,
            1.0f,
            direction.normalized // Override direction
        );
    }
    
    private void Start()
    {
        // Example: Subscribe to knockback events for effects
        KnockbackEvents.OnKnockbackStarted += OnKnockbackStarted;
        KnockbackEvents.OnKnockbackEnded += OnKnockbackEnded;
        
        if (enableDebug)
        {
            Debug.Log("🎮 Knockback Example ready! Press T for light knockback, Y for heavy knockback");
        }
    }
    
    private void OnDestroy()
    {
        // Always unsubscribe from events
        KnockbackEvents.OnKnockbackStarted -= OnKnockbackStarted;
        KnockbackEvents.OnKnockbackEnded -= OnKnockbackEnded;
    }
    
    private void OnKnockbackStarted(IKnockbackReceiver receiver)
    {
        if (enableDebug)
            Debug.Log($"🔄 Knockback started on {receiver.Transform.name}");
        
        // Add your effects here:
        // - Camera shake
        // - Particle effects
        // - Sound effects
        // - UI feedback
    }
    
    private void OnKnockbackEnded(IKnockbackReceiver receiver)
    {
        if (enableDebug)
            Debug.Log($"✅ Knockback ended on {receiver.Transform.name}");
        
        // Add cleanup or follow-up effects here
    }
}