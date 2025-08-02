using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations_Nyx : MonoBehaviour
{
    [Header("Performance")]
    [Tooltip("Enable debug logs for animation events only during attacks")]
    public bool enableAttackDebugLogs = true;
    
    [Header("Hitboxes")]
    public GameObject footHitbox; // Drag FootHitbox GameObject here
    
    [Header("Foot Knockback Settings")]
    [Tooltip("Knockback data for foot attacks (stronger than normal attacks)")]
    public KnockbackData footKnockbackData;
    
    [Tooltip("Multiplier for foot knockback force (default: 2x stronger than normal)")]
    public float footKnockbackMultiplier = 2f;
    
    [Tooltip("Range to detect enemies when foot attack is activated")]
    public float footAttackRange = 3f;
    
    [Tooltip("Layer mask for enemies")]
    public LayerMask enemyLayerMask = -1; // Default to all layers
    
    [Tooltip("Enable debug logs for foot attacks")]
    public bool enableFootDebugLogs = true;
    
    // Performance optimization: Cache the Player_Movement instance
    private Player_Movement playerMovement;
    private WeaponScript weaponScript;
    
    void Start()
    {
        // Cache the instance for performance
        playerMovement = Player_Movement.instance;
        
        // Cache the weapon script for finisher attacks
        weaponScript = FindObjectOfType<WeaponScript>();
        if (weaponScript == null)
        {
            // WeaponScript is optional - only needed for advanced finisher attacks
            Debug.Log("Animations_Nyx: No WeaponScript found. This is normal if using basic combat system.");
        }
        
        // Auto-load FootKnockbackData if not assigned
        if (footKnockbackData == null)
        {
            #if UNITY_EDITOR
            footKnockbackData = UnityEditor.AssetDatabase.LoadAssetAtPath<KnockbackData>("Assets/ScriptableObjects/Knockback/FootKnockbackData.asset");
            #endif
            
            if (footKnockbackData != null)
            {
                Debug.Log($"🦵 Auto-loaded FootKnockbackData: Distance={footKnockbackData.knockbackDistance}, Duration={footKnockbackData.duration}");
            }
            else
            {
                Debug.LogWarning($"⚠️ FootKnockbackData not found! Create it via Tools > Combat > Create Foot Knockback Data");
            }
        }
    }
    
    /// <summary>
    /// Debug visualization for foot attack range
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (footHitbox != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(footHitbox.transform.position, footAttackRange);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(footHitbox.transform.position, 0.1f);
        }
    }
    
    // Helper method to check if we should debug (only during attacks)
    private bool ShouldDebugAttacks()
    {
        return enableAttackDebugLogs && playerMovement != null && 
               (playerMovement.isAttacking || playerMovement.isSlashing || playerMovement.rolling);
    }
    
    public void Recover()
    {
        // Empty - can be removed if not used
    }
    
    public void ActivateAttack()
    {
        // Deprecated - was causing issues
        // Player_Movement.instance.AttackFailSafe();
    }
    
    // Main animation event for ending attacks
    public void AttackAnimEnd()
    {
        if (playerMovement != null)
        {
            if (ShouldDebugAttacks()) Debug.Log("🎬 AttackAnimEnd: Animation event triggered");
            playerMovement.EndAttack();
        }
    }

    // Specific event for spin attacks
    public void SpinAttackEnd()
    {
        if (playerMovement != null)
        {
            if (ShouldDebugAttacks()) Debug.Log("🎬 SpinAttackEnd: Spin attack animation event triggered");
            playerMovement.EndAttack();
            playerMovement.isSpinAttack = false;
        }
    }
    
    // New: Enable combo window during attack animation
    public void EnableComboWindow()
    {
        if (playerMovement != null)
        {
            if (ShouldDebugAttacks()) Debug.Log("🎬 EnableComboWindow: Combo window opened via animation event");
            playerMovement.EnableComboNext();
        }
    }
    
    // New: Force combo reset (for animation states that should break combos)
    public void ResetCombo()
    {
        if (playerMovement != null)
        {
            if (ShouldDebugAttacks()) Debug.Log("🎬 ResetCombo: Combo reset via animation event");
            playerMovement.ResetCombo();
        }
    }
    
    // End hurt state animation event
    public void HurtEnd()
    {
        if (playerMovement != null)
        {
            Debug.Log("🎬 HurtEnd: Ending hurt state via animation event");
            playerMovement.EndHurt();
        }
    }
    
    // End arm/gauntlet attack animation event
    public void EndSlash()
    {
        if (playerMovement != null)
        {
            if (ShouldDebugAttacks()) Debug.Log("🎬 EndSlash: Arm/gauntlet attack animation event triggered");
            playerMovement.EndSlash(); // Call the proper EndSlash function in Player_Movement
        }
    }
    
    public void StoppedMoving()
    {
        if (playerMovement != null)
        {
            playerMovement.StopMoving();
        }
    }
    
    public void StartSpinAttack()
    {
        // This can be used for special spin attack setup if needed
        // Player_Movement.instance.isSpinAttack = true;
    }

    // Hitbox control events
    public void ActivateSword()
    {
        if (playerMovement != null)
        {
            if (ShouldDebugAttacks()) Debug.Log("🎬 ActivateSword: Sword hitbox enabled");
            playerMovement.SwordOn();
        }
    }

    public void DeactivateSword()
    {
        if (playerMovement != null)
        {
            if (ShouldDebugAttacks()) Debug.Log("🎬 DeactivateSword: Sword hitbox disabled");
            playerMovement.SwordOff();
        }
    }
    
    // ===== KICK ATTACK ANIMATION EVENTS =====
    // ENHANCED: Foot attacks now handled directly by animation events
    // No need for separate FootKickScript - all logic is integrated here
    // Uses enhanced knockback system with greater force and proper timing
    
    /// <summary>
    /// Activate foot/kick hitbox - call when the kick should apply knockback
    /// Now handles knockback directly instead of relying on collision detection
    /// </summary>
    public void ActivateFoot()
    {
        if (footHitbox != null)
        {
            footHitbox.SetActive(true);
            if (ShouldDebugAttacks()) Debug.Log("🎬 ActivateFoot: Foot hitbox enabled for knockback");
            
            // ENHANCED: Apply foot knockback to enemies in range
            ApplyFootKnockbackToNearbyEnemies();
        }
        else if (ShouldDebugAttacks())
        {
            Debug.LogWarning("🎬 ActivateFoot: footHitbox is null! Please assign it in the inspector.");
        }
    }
    
    /// <summary>
    /// Apply enhanced foot knockback to all enemies within range
    /// </summary>
    private void ApplyFootKnockbackToNearbyEnemies()
    {
        if (footHitbox == null) return;
        
        Vector3 footPosition = footHitbox.transform.position;
        
        // Find all colliders within range
        Collider[] enemiesInRange = Physics.OverlapSphere(footPosition, footAttackRange, enemyLayerMask);
        
        int enemiesHit = 0;
        
        foreach (Collider enemyCollider in enemiesInRange)
        {
            // Only affect enemies
            if (!enemyCollider.gameObject.CompareTag("Enemy")) continue;
            
            // Apply enhanced foot knockback
            bool success = ApplyFootKnockbackToEnemy(enemyCollider);
            if (success) enemiesHit++;
        }
        
        if (enableFootDebugLogs)
        {
            Debug.Log($"🦵 FOOT ATTACK: Hit {enemiesHit} enemies within {footAttackRange} units");
        }
    }
    
    /// <summary>
    /// Apply enhanced knockback to a specific enemy
    /// </summary>
    private bool ApplyFootKnockbackToEnemy(Collider enemyCollider)
    {
        if (enableFootDebugLogs) 
            Debug.Log($"🦵 APPLYING FOOT KNOCKBACK to {enemyCollider.name}");
        
        bool knockbackApplied = false;
        
        // Try to apply knockback using the KnockbackManager first (preferred method)
        if (KnockbackManager.Instance != null)
        {
            bool success = KnockbackManager.Instance.ApplyKnockback(
                enemyCollider.gameObject,
                footHitbox.transform.position,
                footKnockbackData, // Uses special foot knockback data
                footKnockbackMultiplier // Enhanced multiplier for foot attacks
            );
            
            if (success)
            {
                knockbackApplied = true;
                if (enableFootDebugLogs) 
                    Debug.Log($"🦵 Applied ENHANCED KNOCKBACK to {enemyCollider.name} using KnockbackManager!");
                
                // Also apply knockdown state for animation
                ApplyKnockdownState(enemyCollider);
            }
        }
        
        // Fallback to legacy system if KnockbackManager failed
        if (!knockbackApplied)
        {
            if (enableFootDebugLogs) 
                Debug.LogWarning($"🦵 KnockbackManager not available, using legacy knockdown for {enemyCollider.name}");
            ApplyLegacyKnockdown(enemyCollider);
            knockbackApplied = true;
        }
        
        return knockbackApplied;
    }
    
    /// <summary>
    /// Apply knockdown animation state (works with the new knockback system)
    /// Only applies knockdown if knockback was successful to avoid timing conflicts
    /// </summary>
    private void ApplyKnockdownState(Collider enemyCollider)
    {
        // Use a slight delay to ensure knockback system has started
        StartCoroutine(ApplyKnockdownStateDelayed(enemyCollider));
    }
    
    /// <summary>
    /// Apply knockdown state with proper timing to avoid conflicts
    /// ENHANCED: Clears hurt states to prevent animation conflicts
    /// </summary>
    private IEnumerator ApplyKnockdownStateDelayed(Collider enemyCollider)
    {
        // Wait a frame to ensure knockback system has initialized
        yield return null;
        
        // Handle Enemy_Basic
        if (enemyCollider.TryGetComponent(out Enemy_Basic enemyBasic))
        {
            if (enemyBasic.anim != null)
            {
                // Only apply knockdown if enemy isn't dead or already in another state
                if (!enemyBasic.dead && enemyBasic.enemyHP > 0)
                {
                    // CRITICAL: Clear hurt states first to prevent animation conflicts
                    enemyBasic.anim.SetBool("IsHurting", false);
                    enemyBasic.anim.SetBool("IsRunning", false);
                    enemyBasic.anim.SetBool("IsAttacking", false);
                    enemyBasic.isHit = false; // Clear hit flag
                    
                    // Now apply knockdown state
                    enemyBasic.anim.SetBool("isKnockedDown", true);
                    enemyBasic.StopMoving();
                    
                    if (enableFootDebugLogs) 
                        Debug.Log($"🦵 Enemy_Basic {enemyBasic.name} KNOCKED DOWN! Cleared hurt states to prevent conflicts. (HP: {enemyBasic.enemyHP})");
                }
                else
                {
                    if (enableFootDebugLogs) 
                        Debug.Log($"🦵 Skipping knockdown for {enemyBasic.name} - enemy is dead or has no HP");
                }
            }
        }
        // Handle Worker
        else if (enemyCollider.TryGetComponent(out Worker worker))
        {
            if (worker.anim != null)
            {
                // CRITICAL: Clear hurt states for Worker too
                worker.anim.SetBool("IsHurting", false);
                worker.anim.SetBool("IsRunning", false);
                worker.anim.SetBool("IsAttacking", false);
                
                // Now apply knockdown state
                worker.anim.SetBool("isKnockedDown", true);
                worker.StopMoving();
                
                if (enableFootDebugLogs) 
                    Debug.Log($"🦵 Worker {worker.name} KNOCKED DOWN! Cleared hurt states to prevent conflicts.");
            }
        }
    }
    
    /// <summary>
    /// Legacy knockdown system (fallback only)
    /// ENHANCED: Also clears hurt states to prevent conflicts
    /// FIXED: No longer calls GetKnockedBack to prevent double knockback with KnockbackReceiver
    /// </summary>
    private void ApplyLegacyKnockdown(Collider enemyCollider)
    {
        // Handle Enemy_Basic
        if (enemyCollider.TryGetComponent(out Enemy_Basic enemyBasic))
        {
            if (enemyBasic.anim != null)
            {
                // CRITICAL: Clear hurt states first to prevent animation conflicts
                enemyBasic.anim.SetBool("IsHurting", false);
                enemyBasic.anim.SetBool("IsRunning", false);
                enemyBasic.anim.SetBool("IsAttacking", false);
                enemyBasic.isHit = false; // Clear hit flag
                
                // Now apply knockdown (animation only - knockback handled by KnockbackManager)
                enemyBasic.anim.SetBool("isKnockedDown", true);
                enemyBasic.StopMoving();
                
                // REMOVED: No longer calling GetKnockedBack to prevent double knockback
                // The KnockbackManager should handle the actual movement
                
                if (enableFootDebugLogs) 
                    Debug.Log($"🦵 Enemy_Basic {enemyBasic.name} KNOCKED DOWN (legacy animation only)! Cleared hurt states.");
            }
        }
        // Handle Worker
        else if (enemyCollider.TryGetComponent(out Worker worker))
        {
            if (worker.anim != null)
            {
                // CRITICAL: Clear hurt states for Worker too
                worker.anim.SetBool("IsHurting", false);
                worker.anim.SetBool("IsRunning", false);
                worker.anim.SetBool("IsAttacking", false);
                
                // Now apply knockdown (animation only - knockback handled by KnockbackManager)
                worker.anim.SetBool("isKnockedDown", true);
                worker.StopMoving();
                
                if (enableFootDebugLogs) 
                    Debug.Log($"🦵 Worker {worker.name} KNOCKED DOWN (legacy animation only)! Cleared hurt states.");
            }
        }
    }
    
    /// <summary>
    /// Deactivate foot/kick hitbox - call when the kick knockback window ends
    /// Disables the foot collider GameObject
    /// </summary>
    public void DeactivateFoot()
    {
        if (footHitbox != null)
        {
            footHitbox.SetActive(false);
            if (ShouldDebugAttacks()) Debug.Log("🎬 DeactivateFoot: Foot hitbox disabled");
        }
        else if (ShouldDebugAttacks())
        {
            Debug.LogWarning("🎬 DeactivateFoot: footHitbox is null! Please assign it in the inspector.");
        }
    }
}
