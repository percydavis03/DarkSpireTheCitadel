using UnityEngine;

/// <summary>
/// Example integrations for the new Knockback System
/// This file shows how to integrate knockback with your existing systems
/// </summary>
public class KnockbackIntegrationExamples : MonoBehaviour
{
    [Header("Example Knockback Data Assets")]
    public KnockbackData lightKnockback;
    public KnockbackData heavyKnockback;
    public KnockbackData grappleReleaseKnockback;
    
    // ========================================
    // EXAMPLE 1: Weapon Integration
    // ========================================
    public void ExampleWeaponHit(Collider target, Vector3 weaponPosition)
    {
        // Simple weapon knockback
        KnockbackManager.Instance.ApplyKnockback(
            target, 
            weaponPosition, 
            lightKnockback, 
            multiplier: 1.0f
        );
    }
    
    public void ExampleHeavyWeaponHit(Collider target, Vector3 weaponPosition, float chargeLevel)
    {
        // Heavy weapon with charge multiplier
        float knockbackMultiplier = 1f + (chargeLevel * 0.5f); // 1x to 1.5x based on charge
        
        KnockbackManager.Instance.ApplyKnockback(
            target,
            weaponPosition,
            heavyKnockback,
            multiplier: knockbackMultiplier
        );
    }
    
    // ========================================
    // EXAMPLE 2: Grapple Release Integration
    // ========================================
    public void ExampleGrappleRelease(GameObject grappledTarget, Vector3 playerPosition)
    {
        // Enhanced grapple release with directional knockback
        Vector3 releaseDirection = (grappledTarget.transform.position - playerPosition).normalized;
        
        KnockbackManager.Instance.ApplyKnockback(
            grappledTarget,
            playerPosition,
            grappleReleaseKnockback,
            multiplier: 1.5f, // Extra force for grapple release
            overrideDirection: releaseDirection
        );
    }
    
    // ========================================
    // EXAMPLE 3: Enemy AI Integration
    // ========================================
    public void ExampleEnemyKnockbackIntegration()
    {
        // This would go in your Enemy_Basic.cs or similar
        
        // In OnTriggerEnter or weapon collision detection:
        /*
        if (other.CompareTag("Weapon"))
        {
            // Get the weapon's position
            Vector3 weaponPosition = other.transform.position;
            
            // Apply knockback using the new system
            KnockbackManager.Instance.ApplyKnockback(
                this.gameObject, 
                weaponPosition, 
                lightKnockback
            );
            
            // Your existing damage logic here
            TakeDamage();
        }
        */
    }
    
    // ========================================
    // EXAMPLE 4: Event-Based Integration
    // ========================================
    private void Start()
    {
        // Subscribe to knockback events for additional effects
        KnockbackEvents.OnKnockbackStarted += OnEnemyKnockedBack;
        KnockbackEvents.OnKnockbackEnded += OnEnemyKnockbackEnded;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        KnockbackEvents.OnKnockbackStarted -= OnEnemyKnockedBack;
        KnockbackEvents.OnKnockbackEnded -= OnEnemyKnockbackEnded;
    }
    
    private void OnEnemyKnockedBack(IKnockbackReceiver receiver)
    {
        // Example: Disable enemy attacks during knockback
        var enemy = receiver.Transform.GetComponent<Enemy_Basic>();
        if (enemy != null)
        {
            enemy.StopAttacking();
        }
        
        // Example: Screen shake on knockback
        // CameraShake.Instance?.Shake(0.3f, 0.2f);
    }
    
    private void OnEnemyKnockbackEnded(IKnockbackReceiver receiver)
    {
        // Example: Re-enable enemy behavior after knockback
        var enemy = receiver.Transform.GetComponent<Enemy_Basic>();
        if (enemy != null)
        {
            // Enemy will naturally resume AI behavior
        }
    }
    
    // ========================================
    // EXAMPLE 5: Multiple Knockback Types
    // ========================================
    public void ExampleMultipleKnockbackTypes(GameObject target, string attackType, Vector3 sourcePosition)
    {
        KnockbackData knockbackToUse = lightKnockback; // Default
        float multiplier = 1f;
        
        switch (attackType)
        {
            case "light_attack":
                knockbackToUse = lightKnockback;
                multiplier = 0.8f;
                break;
                
            case "heavy_attack":
                knockbackToUse = heavyKnockback;
                multiplier = 1.2f;
                break;
                
            case "grapple_slam":
                knockbackToUse = grappleReleaseKnockback;
                multiplier = 2f;
                break;
                
            case "combo_finisher":
                knockbackToUse = heavyKnockback;
                multiplier = 1.8f;
                break;
        }
        
        KnockbackManager.Instance.ApplyKnockback(target, sourcePosition, knockbackToUse, multiplier);
    }
    
    // ========================================
    // EXAMPLE 6: Conditional Knockback
    // ========================================
    public void ExampleConditionalKnockback(GameObject target, Vector3 sourcePosition, bool isCriticalHit, bool isTargetShielded)
    {
        // Only apply knockback under certain conditions
        if (isTargetShielded)
        {
            // Reduced knockback for shielded enemies
            KnockbackManager.Instance.ApplyKnockback(target, sourcePosition, lightKnockback, 0.3f);
        }
        else if (isCriticalHit)
        {
            // Enhanced knockback for critical hits
            KnockbackManager.Instance.ApplyKnockback(target, sourcePosition, heavyKnockback, 1.5f);
        }
        else
        {
            // Normal knockback
            KnockbackManager.Instance.ApplyKnockback(target, sourcePosition, lightKnockback);
        }
    }
    
    // ========================================
    // EXAMPLE 7: Boss/Special Enemy Integration
    // ========================================
    public void ExampleBossKnockbackResistance(GameObject bossTarget, Vector3 sourcePosition)
    {
        // Check if boss has special resistance
        var knockbackReceiver = bossTarget.GetComponent<KnockbackReceiver>();
        if (knockbackReceiver != null)
        {
            // Bosses might have high mass or special conditions
            if (knockbackReceiver.Mass > 5f) // Heavy boss
            {
                // Use special boss knockback data (shorter distance, longer duration)
                KnockbackManager.Instance.ApplyKnockback(bossTarget, sourcePosition, lightKnockback, 0.2f);
            }
            else
            {
                // Normal knockback
                KnockbackManager.Instance.ApplyKnockback(bossTarget, sourcePosition, lightKnockback);
            }
        }
    }
    
    // ========================================
    // EXAMPLE 8: Easy integration with your current Enemy_Basic.cs
    // ========================================
    
    /// <summary>
    /// Replace your existing GetKnockedBack method in Enemy_Basic with this
    /// </summary>
    public void ReplacementGetKnockedBack(Vector3 sourcePosition, float forceMultiplier = 1f)
    {
        // Simple one-line replacement for your existing system
        KnockbackManager.Instance.ApplyKnockback(
            this.gameObject,
            sourcePosition,
            null, // Uses default knockback data
            forceMultiplier
        );
    }
} 