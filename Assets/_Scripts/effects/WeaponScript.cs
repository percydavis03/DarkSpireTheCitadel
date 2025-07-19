using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public PlayerSaveState thisGameSave;
    
    private void Start()
    {
        // Find PlayerSaveState if not assigned
        if (thisGameSave == null)
        {
            // Try to find it from a player object or manager
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var playerMovement = player.GetComponent<Player_Movement>();
                if (playerMovement != null)
                {
                    thisGameSave = playerMovement.thisGameSave;
                }
            }
        }
    }
    
    public int GetCurrentComboDamage()
    {
        // Get current combo count from Player_Movement
        if (Player_Movement.instance != null)
        {
            int currentCombo = Player_Movement.instance.comboCount;
            
            // Use combo-specific damage if in combo, otherwise use main attack damage
            if (Player_Movement.instance.isComboing && currentCombo > 0)
            {
                int comboDamage = thisGameSave.GetComboDamage(currentCombo);
                Debug.Log($"Combo {currentCombo} damage: {comboDamage}");
                return comboDamage;
            }
        }
        
        // Fallback to main attack damage
        return thisGameSave.mainAttackDamage;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Handle enemy damage
        if (other.gameObject.CompareTag("Enemy"))
        {
            // Get the appropriate damage for current combo
            int damage = GetCurrentComboDamage();
            
            // Check if this is the 3rd combo attack for knockback
            bool isThirdCombo = Player_Movement.instance != null && 
                               Player_Movement.instance.isComboing && 
                               Player_Movement.instance.comboCount == 3;
            
            // Apply damage to different enemy types
            if (other.TryGetComponent(out Enemy_Basic enemyBasic))
            {
                enemyBasic.TakeComboDamage(damage);
                
                // Apply special knockback for 3rd combo attack
                if (isThirdCombo)
                {
                    enemyBasic.GetKnockedBack(new Vector3(50, 20, 50)); // Reduced knockback (was 200,50,200)
                    Debug.Log("3rd Combo Attack - Strong Knockback Applied!");
                }
            }
            else if (other.TryGetComponent(out Worker worker))
            {
                worker.TakeComboDamage(damage);
                
                // Workers don't have knockback system, so just apply normal damage
                if (isThirdCombo)
                {
                    Debug.Log("3rd Combo Attack - Extra Impact on Worker!");
                }
            }
        }
        
        // Handle knockback for objects that support it (normal knockback for other attacks)
        if (other.TryGetComponent(out IKnockbackable knockbackable))
        {
            // Check if this is 3rd combo for stronger knockback
            bool isThirdCombo = Player_Movement.instance != null && 
                               Player_Movement.instance.isComboing && 
                               Player_Movement.instance.comboCount == 3;
            
            if (isThirdCombo)
            {
                knockbackable.GetKnockedBack(new Vector3(50, 20, 50)); // Reduced strong knockback (was 200,50,200)
            }
            else
            {
                knockbackable.GetKnockedBack(new Vector3(5, 5, 5)); // Reduced normal knockback (was 10,10,10)
            }
        }
    }
}
