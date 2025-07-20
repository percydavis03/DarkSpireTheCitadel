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
            Debug.LogWarning("Animations_Nyx: No WeaponScript found - finisher attacks may not work!");
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
    // Note: Kick attacks now use dedicated FootKickScript on the foot collider
    // No need to enable/disable kick mode - the foot collider handles everything
    
    /// <summary>
    /// Activate foot/kick hitbox - call when the kick should apply knockback
    /// Enables the foot collider GameObject for knockdown detection
    /// </summary>
    public void ActivateFoot()
    {
        if (footHitbox != null)
        {
            footHitbox.SetActive(true);
            if (ShouldDebugAttacks()) Debug.Log("🎬 ActivateFoot: Foot hitbox enabled for knockback");
        }
        else if (ShouldDebugAttacks())
        {
            Debug.LogWarning("🎬 ActivateFoot: footHitbox is null! Please assign it in the inspector.");
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
