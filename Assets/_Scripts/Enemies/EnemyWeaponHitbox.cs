using UnityEngine;

/// <summary>
/// Script for enemy weapon hitboxes that applies knockback to the player using KnockbackManager
/// Attach this to enemy weapon hitboxes (spear_hitbox, shovelHitbox, etc.)
/// </summary>
public class EnemyWeaponHitbox : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private KnockbackData weaponKnockbackData;
    [SerializeField] private float knockbackMultiplier = 1f;
    
    [Header("Damage Settings")]
    [SerializeField] private int weaponDamage = 1;
    [SerializeField] private bool dealsDamage = true;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private void OnTriggerEnter(Collider other)
    {
        if (enableDebugLogs) 
            Debug.Log($"🗡️ EnemyWeaponHitbox collision with {other.gameObject.name}, tag: {other.gameObject.tag}");
        
        // Check if we hit the player
        if (other.gameObject.CompareTag("Player"))
        {
            if (enableDebugLogs) 
                Debug.Log($"🗡️ Player hit by {gameObject.name}!");
            
            // Apply knockback using modern system (knockback only)
            ApplyKnockbackToPlayer(other);
            
            // Apply damage separately (damage only)
            if (dealsDamage)
            {
                ApplyDamageToPlayer();
            }
        }
    }
    
    private void ApplyKnockbackToPlayer(Collider playerCollider)
    {
        if (KnockbackManager.Instance == null)
        {
            Debug.LogError("🗡️ KnockbackManager not found! Cannot apply knockback.");
            return;
        }
        
        // Try to get KnockbackReceiver from player
        if (playerCollider.TryGetComponent(out KnockbackReceiver knockbackReceiver))
        {
            if (enableDebugLogs) 
                Debug.Log($"🗡️ Applying knockback using KnockbackManager!");
            
            // Apply knockback using modern system (knockback only, no damage)
            bool success = KnockbackManager.Instance.ApplyKnockback(
                knockbackReceiver, 
                transform.position, 
                weaponKnockbackData, 
                knockbackMultiplier
            );
            
            if (enableDebugLogs)
            {
                if (success)
                    Debug.Log($"🗡️ Knockback applied successfully!");
                else
                    Debug.LogWarning($"🗡️ Knockback failed to apply!");
            }
        }
        else
        {
            if (enableDebugLogs) 
                Debug.LogWarning($"🗡️ Player doesn't have KnockbackReceiver component! No knockback applied.");
            
            // No fallback - damage will be handled separately by ApplyDamageToPlayer()
        }
    }
    
    private void ApplyDamageToPlayer()
    {
        if (Main_Player.instance != null)
        {
            // Use existing damage system
            if (enableDebugLogs) 
                Debug.Log($"🗡️ Applying damage to player via Main_Player system");
            
            // Calculate direction from weapon to player for knockback
            Vector3 knockbackDirection = (Main_Player.instance.transform.position - transform.position).normalized;
            Main_Player.instance.TakeDamageFromEnemy(transform, knockbackDirection);
        }
    }
    
    private void OnValidate()
    {
        // Auto-assign default knockback data if none is set
        if (weaponKnockbackData == null && KnockbackManager.Instance != null)
        {
            // This will be assigned in the inspector
            Debug.LogWarning($"🗡️ {gameObject.name}: No KnockbackData assigned! Please assign in inspector.");
        }
    }
}