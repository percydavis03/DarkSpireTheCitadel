using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages foot/kick attacks with area-of-effect knockback and knockdown states.
/// All foot attack settings (range, multiplier) are configured in KnockbackManager for centralized control.
/// Provides a clean, unified knockback system with settings in one place.
/// </summary>
public class FootAttackManager : MonoBehaviour
{
    [Header("Foot Attack References")]
    [Tooltip("The foot hitbox GameObject (usually on a child of the player)")]
    public GameObject footHitbox;
    
    [Header("Foot Attack Configuration")]
    [Tooltip("Layer mask for enemies")]
    public LayerMask enemyLayerMask = -1;
    
    [Header("Info")]
    [Tooltip("Foot attack settings (range, multiplier) are now configured in KnockbackManager")]
    [SerializeField] private bool footSettingsInKnockbackManager = true;
    
    [Header("Knockdown Settings")]
    [Tooltip("Duration to wait before applying knockdown state (prevents conflicts)")]
    public float knockdownDelay = 0.05f;
    
    [Header("Debug")]
    [Tooltip("Enable debug logs for foot attacks")]
    public bool enableDebugLogs = false; // Disabled to reduce console spam
    
    [Tooltip("Show debug visualization in scene view")]
    public bool showDebugGizmos = true;
    
    private void Start()
    {
        // Validate KnockbackManager is available
        if (KnockbackManager.Instance == null)
        {
            Debug.LogWarning("⚠️ FootAttackManager: KnockbackManager not found! Foot attacks will not work properly.");
        }
        else
        {
            Debug.Log("🦵 FootAttackManager: Ready to use KnockbackManager's default knockback data");
        }
    }
    
    /// <summary>
    /// Perform a foot attack using the assigned foot hitbox position
    /// Uses KnockbackManager's centralized foot attack settings
    /// </summary>
    /// <returns>Number of enemies affected</returns>
    public int PerformFootAttack()
    {
        if (footHitbox == null)
        {
            Debug.LogWarning("🦵 FootAttackManager: footHitbox is null! Please assign it in the inspector.");
            return 0;
        }
        
        return PerformFootAttackAt(footHitbox.transform.position);
    }
    
    /// <summary>
    /// Perform a foot attack from the specified position, affecting all enemies in range
    /// Uses KnockbackManager's centralized foot attack settings
    /// </summary>
    /// <param name="attackPosition">Position where the foot attack originates</param>
    /// <returns>Number of enemies affected</returns>
    public int PerformFootAttackAt(Vector3 attackPosition)
    {
        if (KnockbackManager.Instance == null)
        {
            Debug.LogWarning("🦵 KnockbackManager not available! Foot attack cannot proceed.");
            return 0;
        }
        
        float range = KnockbackManager.Instance.FootAttackRange;
        float multiplier = KnockbackManager.Instance.FootAttackMultiplier;
        
        if (enableDebugLogs)
        {
            Debug.Log($"🦵 FOOT ATTACK initiated at {attackPosition} with range {range}");
        }
        
        // Use KnockbackManager's dedicated foot attack method
        int enemiesHit = KnockbackManager.Instance.ApplyFootAttackKnockback(attackPosition, enemyLayerMask);
        
        // Apply knockdown states manually since KnockbackManager only handles physics
        ApplyKnockdownToEnemiesInRange(attackPosition, range);
        
        if (enableDebugLogs)
        {
            Debug.Log($"🦵 FOOT ATTACK RESULT: Hit {enemiesHit} enemies with {multiplier}x multiplier in {range} unit range");
        }
        
        return enemiesHit;
    }
    
    /// <summary>
    /// Activate foot hitbox and perform foot attack
    /// This is the main method to call from animation events
    /// Uses KnockbackManager's centralized foot attack settings
    /// </summary>
    /// <returns>Number of enemies affected</returns>
    public int ActivateFootAttack()
    {
        if (footHitbox == null)
        {
            Debug.LogWarning("🦵 FootAttackManager: footHitbox is null! Please assign it in the inspector.");
            return 0;
        }
        
        // Activate the hitbox GameObject
        footHitbox.SetActive(true);
        
        if (enableDebugLogs)
        {
            Debug.Log("🦵 FootAttackManager: Foot hitbox activated and attack initiated");
        }
        
        // Perform the foot attack using centralized settings
        return PerformFootAttack();
    }
    
    /// <summary>
    /// Deactivate foot hitbox
    /// Call this when the kick animation window ends
    /// </summary>
    public void DeactivateFootAttack()
    {
        if (footHitbox != null)
        {
            footHitbox.SetActive(false);
            
            if (enableDebugLogs)
            {
                Debug.Log("🦵 FootAttackManager: Foot hitbox deactivated");
            }
        }
        else
        {
            Debug.LogWarning("🦵 FootAttackManager: footHitbox is null! Please assign it in the inspector.");
        }
    }
    
    /// <summary>
    /// Apply knockdown states to all enemies in range (for use with KnockbackManager area knockback)
    /// </summary>
    private void ApplyKnockdownToEnemiesInRange(Vector3 center, float range)
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(center, range, enemyLayerMask);
        
        foreach (Collider enemyCollider in enemiesInRange)
        {
            // Only affect enemies
            if (!enemyCollider.gameObject.CompareTag("Enemy")) continue;
            
            // Apply knockdown state
            StartCoroutine(ApplyKnockdownStateDelayed(enemyCollider));
        }
    }
    
    /// <summary>
    /// Apply foot-specific knockback to a single enemy
    /// </summary>
    /// <param name="enemyCollider">Enemy to knockback</param>
    /// <param name="sourcePosition">Position of the foot attack</param>
    /// <param name="multiplier">Knockback force multiplier</param>
    /// <returns>True if knockback was successfully applied</returns>
    private bool ApplyFootKnockback(Collider enemyCollider, Vector3 sourcePosition, float multiplier)
    {
        if (enableDebugLogs) 
            Debug.Log($"🦵 Applying foot knockback to {enemyCollider.name}");
        
        // Apply knockback using the KnockbackManager
        bool knockbackSuccess = false;
        if (KnockbackManager.Instance != null)
        {
            knockbackSuccess = KnockbackManager.Instance.ApplyKnockback(
                enemyCollider.gameObject,
                sourcePosition,
                null, // Use KnockbackManager's default knockback data
                multiplier
            );
        }
        else
        {
            Debug.LogWarning("🦵 KnockbackManager not available! Foot attack will not apply knockback.");
            return false;
        }
        
        // Apply knockdown state if knockback was successful
        if (knockbackSuccess)
        {
            StartCoroutine(ApplyKnockdownStateDelayed(enemyCollider));
            
            if (enableDebugLogs) 
                Debug.Log($"🦵 Successfully applied foot knockback to {enemyCollider.name} with {multiplier}x multiplier!");
        }
        
        return knockbackSuccess;
    }
    
    /// <summary>
    /// Apply knockdown animation state with proper timing to avoid conflicts
    /// </summary>
    private IEnumerator ApplyKnockdownStateDelayed(Collider enemyCollider)
    {
        // Wait to ensure knockback system has started
        yield return new WaitForSeconds(knockdownDelay);
        
        // Handle Enemy_Basic
        if (enemyCollider.TryGetComponent(out Enemy_Basic enemyBasic))
        {
            ApplyKnockdownToEnemyBasic(enemyBasic);
        }
        // Handle Worker
        else if (enemyCollider.TryGetComponent(out Worker worker))
        {
            ApplyKnockdownToWorker(worker);
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning($"🦵 {enemyCollider.name} doesn't have a supported enemy component for knockdown");
        }
    }
    
    /// <summary>
    /// Apply knockdown state to Enemy_Basic
    /// </summary>
    private void ApplyKnockdownToEnemyBasic(Enemy_Basic enemyBasic)
    {
        if (enemyBasic.anim == null) return;
        
        // Only apply knockdown if enemy isn't dead
        if (!enemyBasic.dead && enemyBasic.enemyHP > 0)
        {
            // Clear conflicting animation states
            enemyBasic.anim.SetBool("IsHurting", false);
            enemyBasic.anim.SetBool("IsRunning", false);
            enemyBasic.anim.SetBool("IsAttacking", false);
            enemyBasic.isHit = false;
            
            // Apply knockdown state
            enemyBasic.anim.SetBool("isKnockedDown", true);
            enemyBasic.StopMoving();
            
            if (enableDebugLogs) 
                Debug.Log($"🦵 Enemy_Basic {enemyBasic.name} KNOCKED DOWN! (HP: {enemyBasic.enemyHP})");
        }
        else
        {
            if (enableDebugLogs) 
                Debug.Log($"🦵 Skipping knockdown for {enemyBasic.name} - enemy is dead or has no HP");
        }
    }
    
    /// <summary>
    /// Apply knockdown state to Worker
    /// </summary>
    private void ApplyKnockdownToWorker(Worker worker)
    {
        if (worker.anim == null) return;
        
        // Clear conflicting animation states
        worker.anim.SetBool("IsHurting", false);
        worker.anim.SetBool("IsRunning", false);
        worker.anim.SetBool("IsAttacking", false);
        
        // Apply knockdown state
        worker.anim.SetBool("isKnockedDown", true);
        worker.StopMoving();
        
        if (enableDebugLogs) 
            Debug.Log($"🦵 Worker {worker.name} KNOCKED DOWN!");
    }
    
    /// <summary>
    /// Debug visualization for foot attack range using KnockbackManager settings
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Vector3 gizmoPosition = footHitbox != null ? footHitbox.transform.position : transform.position;
        
        // Get range from KnockbackManager if available
        float rangeToShow = 3f; // fallback
        float multiplierToShow = 2f; // fallback
        
        if (KnockbackManager.Instance != null)
        {
            rangeToShow = KnockbackManager.Instance.FootAttackRange;
            multiplierToShow = KnockbackManager.Instance.FootAttackMultiplier;
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gizmoPosition, rangeToShow);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(gizmoPosition, 0.1f);
        
        // Draw range label
        #if UNITY_EDITOR
        string hitboxStatus = footHitbox != null ? "" : " (No Hitbox!)";
        string label = $"Foot Range: {rangeToShow}m (x{multiplierToShow}){hitboxStatus}";
        UnityEditor.Handles.Label(gizmoPosition + Vector3.up * (rangeToShow + 0.5f), label);
        #endif
    }
    
    /// <summary>
    /// Validate settings in the inspector
    /// </summary>
    private void OnValidate()
    {
        knockdownDelay = Mathf.Max(0f, knockdownDelay);
        
        // Warn if critical components are missing
        if (footHitbox == null)
        {
            Debug.LogWarning("FootAttackManager: footHitbox is not assigned! Please assign the foot hitbox GameObject.", this);
        }
        
        // Note: Foot attack settings (range, multiplier) are now configured in KnockbackManager
    }
}