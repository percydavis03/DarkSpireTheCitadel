using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations_Nyx : MonoBehaviour
{
    [Header("Performance")]
    [Tooltip("Enable debug logs for animation events only during attacks")]
    public bool enableAttackDebugLogs = true;
    
    [Header("Combat System References")]
    [Tooltip("Foot attack manager handles all foot/kick attack logic")]
    public FootAttackManager footAttackManager;

    [Header("Visual Effects")]
    [Tooltip("Sword particle effects to activate during attacks")]
    public GameObject swordParticleEffects;
    
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
        
        // Auto-find FootAttackManager if not assigned
        if (footAttackManager == null)
        {
            footAttackManager = FindObjectOfType<FootAttackManager>();
            if (footAttackManager == null)
            {
                Debug.LogWarning("⚠️ FootAttackManager not found! Foot attacks will not work. Add FootAttackManager component to scene.");
            }
            else
            {
                Debug.Log("🦵 Auto-found FootAttackManager for foot attacks");
            }
        }
    }
    
    /// <summary>
    /// Debug visualization for foot attack range
    /// Note: FootAttackManager handles its own gizmo visualization
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // FootAttackManager handles its own debug visualization
        // This method kept for potential future animation-specific debug draws
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
        
        // Activate sword particle effects for regular attack
        if (swordParticleEffects != null)
        {
            swordParticleEffects.SetActive(true);
            if (ShouldDebugAttacks()) Debug.Log("🎬 ActivateAttack: Sword particle effects activated");
        }
    }
    
    // Main animation event for ending attacks
    public void AttackAnimEnd()
    {
        if (playerMovement != null)
        {
            if (ShouldDebugAttacks()) Debug.Log("🎬 AttackAnimEnd: Animation event triggered");
            playerMovement.EndAttack();
        }
        
        // Deactivate sword particle effects when attack ends
        if (swordParticleEffects != null)
        {
            swordParticleEffects.SetActive(false);
            if (ShouldDebugAttacks()) Debug.Log("🎬 AttackAnimEnd: Sword particle effects deactivated");
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
        
        // Deactivate sword particle effects when spin attack ends
        if (swordParticleEffects != null)
        {
            swordParticleEffects.SetActive(false);
            if (ShouldDebugAttacks()) Debug.Log("🎬 SpinAttackEnd: Sword particle effects deactivated");
        }
    }
    
    // New: Enable combo window during attack animation
    public void EnableComboWindow()
    {
        if (playerMovement != null)
        {
            if (ShouldDebugAttacks()) Debug.Log("🎬 EnableComboWindow: Combo window opened via animation event");
            
            // DON'T increment combo count here - only open the window
            // Combo count will be incremented when player actually continues the combo
            
            // Open the combo window
            playerMovement.EnableComboNext();
            
            if (ShouldDebugAttacks()) Debug.Log($"🎬 EnableComboWindow: Combo window opened, waiting for player input");
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
    
    // Reset attack state when rolling starts
    public void RollStart()
    {
        if (playerMovement != null)
        {
            if (ShouldDebugAttacks()) Debug.Log("🎬 RollStart: Resetting attack state for roll");
            
            // Reset attack state
            playerMovement.anim.SetBool("isAttacking", false);
            playerMovement.anim.SetInteger("AttackInt", 0);
            
            // Also reset combo state
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
        
        // Activate sword particle effects for spin attack
        if (swordParticleEffects != null)
        {
            swordParticleEffects.SetActive(true);
            if (ShouldDebugAttacks()) Debug.Log("🎬 StartSpinAttack: Sword particle effects activated");
        }
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
    // ORGANIZED: Foot attacks now use the dedicated FootAttackManager system
    // Animation events trigger the FootAttackManager for clean separation of concerns
    // All knockback logic, range detection, and knockdown states handled by FootAttackManager
    
    /// <summary>
    /// Activate foot/kick attack - applies knockback to enemies in range
    /// Now uses the organized FootAttackManager system
    /// </summary>
    public void ActivateFoot()
    {
        if (footAttackManager != null)
        {
          ///  if (ShouldDebugAttacks()) Debug.Log("🎬 ActivateFoot: Triggering FootAttackManager");
            
            int enemiesHit = footAttackManager.ActivateFootAttack();
            
           // if (ShouldDebugAttacks()) Debug.Log($"🎬 ActivateFoot: FootAttackManager hit {enemiesHit} enemies");
        }
        else
        {
          //  Debug.LogWarning("🎬 ActivateFoot: FootAttackManager not available! Foot attack will not work. Please assign FootAttackManager in the inspector.");
        }
    }

    
    /// <summary>
    /// Deactivate foot/kick hitbox - call when the kick knockback window ends
    /// Now uses the FootAttackManager system
    /// </summary>
    public void DeactivateFoot()
    {
        if (footAttackManager != null)
        {
            footAttackManager.DeactivateFootAttack();
            if (ShouldDebugAttacks()) Debug.Log("🎬 DeactivateFoot: FootAttackManager deactivated foot hitbox");
        }
        else
        {
            Debug.LogWarning("🎬 DeactivateFoot: FootAttackManager not available! Please assign FootAttackManager in the inspector.");
        }
    }
}
