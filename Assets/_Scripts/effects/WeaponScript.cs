using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public PlayerSaveState thisGameSave;
    
    [Header("Enhanced Parry System")]
    public bool parryEnabled = true;
    
    [Header("Parry Window Settings")]
    public float easyParryWindow = 0.3f;        // Beginner enemies
    public float normalParryWindow = 0.15f;     // Standard enemies  
    public float hardParryWindow = 0.08f;       // Elite enemies
    public float expertParryWindow = 0.05f;     // Boss-level timing
    
    [Header("Parry Feedback")]
    public float parryDamageMultiplier = 0.8f;      // Less damage than normal attacks
    public float parryStunDuration = 2.0f;          // Standard stun duration
    public float perfectParryBonus = 1.2f;          // Minor damage boost for perfect timing
    
    [Header("Parry Stun System (Cohesive with Grapple)")]
    public float perfectParryStunMultiplier = 2.0f; // Perfect parries = 2x stun duration
    public float comboParryStunBonus = 0.5f;        // Extra 0.5s stun during combos
    public Vector3 standardParryKnockback = new Vector3(30, 15, 30); // Simple, consistent knockback
    
    [Header("Combo Parry System")]
    public float comboParryWindow = 0.1f;       
    public float comboParryBonus = 1.1f;        // Minimal extra damage
    
    private float lastParryTime = -1f;
    
    private void Start()
    {
        if (thisGameSave == null)
        {
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
        if (Player_Movement.instance != null)
        {
            int currentCombo = Player_Movement.instance.comboCount;
            
            if (Player_Movement.instance.isComboing && currentCombo > 0)
            {
                int comboDamage = thisGameSave.GetComboDamage(currentCombo);
                Debug.Log($"Combo {currentCombo} damage: {comboDamage}");
                return comboDamage;
            }
        }
        
        return thisGameSave.mainAttackDamage;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            // Enhanced parry check with precise timing
            if (parryEnabled && CheckForEnhancedParry(other))
            {
                return; // Parry handled, don't continue with normal damage
            }
            
            int damage = GetCurrentComboDamage();
            
            bool isThirdCombo = Player_Movement.instance != null && 
                               Player_Movement.instance.isComboing && 
                               Player_Movement.instance.comboCount == 3;
            
            if (other.TryGetComponent(out Enemy_Basic enemyBasic))
            {
                enemyBasic.TakeComboDamage(damage);
                
                if (isThirdCombo)
                {
                    enemyBasic.GetKnockedBack(new Vector3(50, 20, 50));
                    Debug.Log("3rd Combo Attack - Strong Knockback Applied!");
                }
            }
            else if (other.TryGetComponent(out Worker worker))
            {
                worker.TakeComboDamage(damage);
                
                if (isThirdCombo)
                {
                    Debug.Log("3rd Combo Attack - Extra Impact on Worker!");
                }
            }
        }
        
        if (other.TryGetComponent(out IKnockbackable knockbackable))
        {
            bool isThirdCombo = Player_Movement.instance != null && 
                               Player_Movement.instance.isComboing && 
                               Player_Movement.instance.comboCount == 3;
            
            if (isThirdCombo)
            {
                knockbackable.GetKnockedBack(new Vector3(50, 20, 50));
            }
            else
            {
                knockbackable.GetKnockedBack(new Vector3(5, 5, 5));
            }
        }
    }

    private bool CheckForEnhancedParry(Collider enemyCollider)
    {
        bool playerAttacking = Player_Movement.instance != null && Player_Movement.instance.IsInAttackState();
        
        if (!playerAttacking) return false;
        
        // Check Enemy_Basic with enhanced timing
        if (enemyCollider.TryGetComponent(out Enemy_Basic enemyBasic))
        {
            if (enemyBasic.CanBeParriedNow() && !enemyBasic.isStunned)
            {
                // Get the appropriate parry window based on enemy and attack type
                float parryWindow = GetParryWindow(enemyBasic);
                
                // Check if we're in a valid parry window
                if (enemyBasic.IsInParryWindow() && Time.time - lastParryTime >= parryWindow)
                {
                    lastParryTime = Time.time;
                    
                    // Check for perfect parry timing
                    bool isPerfectParry = enemyBasic.IsInPerfectParryWindow();
                    return ExecuteEnhancedParry(enemyBasic, isPerfectParry);
                }
            }
        }
        // Check Worker with enhanced timing
        else if (enemyCollider.TryGetComponent(out Worker worker))
        {
            if (worker.CanBeParriedNow() && !worker.isStunned)
            {
                float parryWindow = GetParryWindow(worker);
                
                if (worker.IsInParryWindow() && Time.time - lastParryTime >= parryWindow)
                {
                    lastParryTime = Time.time;
                    bool isPerfectParry = worker.IsInPerfectParryWindow();
                    return ExecuteEnhancedParry(worker, isPerfectParry);
                }
            }
        }
        
        return false;
    }

    private float GetParryWindow(Enemy_Basic enemy)
    {
        // Adjust parry window based on player's combo state
        bool inCombo = Player_Movement.instance != null && Player_Movement.instance.isComboing;
        float baseWindow = inCombo ? comboParryWindow : normalParryWindow;
        
        // Further adjust based on enemy difficulty/type
        if (enemy.name.Contains("Elite") || enemy.name.Contains("Boss"))
        {
            return hardParryWindow;
        }
        else if (enemy.name.Contains("Expert"))
        {
            return expertParryWindow;
        }
        
        return baseWindow;
    }

    private float GetParryWindow(Worker worker)
    {
        bool inCombo = Player_Movement.instance != null && Player_Movement.instance.isComboing;
        return inCombo ? comboParryWindow : easyParryWindow; // Workers are easier to parry
    }

    private bool ExecuteEnhancedParry(Enemy_Basic enemy, bool isPerfectParry)
    {
        Debug.Log(isPerfectParry ? "PERFECT PARRY! Extended stun!" : "PARRY SUCCESS! Enemy stunned!");
        
        // Calculate reduced parry damage
        float totalMultiplier = parryDamageMultiplier; // 0.8f
        
        if (isPerfectParry)
        {
            totalMultiplier *= perfectParryBonus; // 1.2f
        }
        
        if (Player_Movement.instance != null && Player_Movement.instance.isComboing)
        {
            totalMultiplier *= comboParryBonus; // 1.1f
        }
        
        int parryDamage = Mathf.RoundToInt(GetCurrentComboDamage() * totalMultiplier);
        
        // MAIN REWARD: Calculate enhanced stun duration
        float stunDuration = parryStunDuration; // Base 2.0s
        
        if (isPerfectParry)
        {
            stunDuration *= perfectParryStunMultiplier; // Perfect = 4.0s stun!
        }
        
        if (Player_Movement.instance != null && Player_Movement.instance.isComboing)
        {
            stunDuration += comboParryStunBonus; // +0.5s during combos
        }
        
        // Apply the enhanced stun (this makes enemies grappable!)
        enemy.GetParried(stunDuration);
        
        // Apply reduced damage
        enemy.enemyHP -= parryDamage;
        
        // Simple, consistent knockback
        enemy.GetKnockedBack(standardParryKnockback);
        
        Debug.Log($"Parry: {parryDamage} damage, {stunDuration}s stun (Perfect: {isPerfectParry})");
        
        OnParrySuccess(isPerfectParry);
        
        return true;
    }

    private bool ExecuteEnhancedParry(Worker worker, bool isPerfectParry)
    {
        Debug.Log(isPerfectParry ? "PERFECT PARRY! Worker stunned longer!" : "PARRY SUCCESS! Worker stunned!");
        
        // Calculate reduced parry damage
        float totalMultiplier = parryDamageMultiplier; // 0.8f
        
        if (isPerfectParry)
        {
            totalMultiplier *= perfectParryBonus; // 1.2f
        }
        
        if (Player_Movement.instance != null && Player_Movement.instance.isComboing)
        {
            totalMultiplier *= comboParryBonus; // 1.1f
        }
        
        int parryDamage = Mathf.RoundToInt(GetCurrentComboDamage() * totalMultiplier);
        
        // MAIN REWARD: Calculate enhanced stun duration  
        float stunDuration = parryStunDuration; // Base 2.0s
        
        if (isPerfectParry)
        {
            stunDuration *= perfectParryStunMultiplier; // Perfect = 4.0s stun!
        }
        
        if (Player_Movement.instance != null && Player_Movement.instance.isComboing)
        {
            stunDuration += comboParryStunBonus; // +0.5s during combos
        }
        
        // Apply the enhanced stun (works with grapple system!)
        worker.GetParried(stunDuration);
        
        // Apply reduced damage
        worker.enemyHP -= parryDamage;
        
        // Simple, consistent knockback for workers
        worker.GetKnockedBack(standardParryKnockback);
        
        Debug.Log($"Worker parry: {parryDamage} damage, {stunDuration}s stun (Perfect: {isPerfectParry})");
        
        OnParrySuccess(isPerfectParry);
        
        return true;
    }
    
    private void OnParrySuccess(bool isPerfectParry)
    {
        Debug.Log($"Parry executed! Perfect: {isPerfectParry} - Focus: Extended stun for grappling!");
        
        // TODO: Simple feedback without slow motion:
        // - Brief screen shake for all parries
        // - Different sound effects for perfect vs normal
        // - UI feedback showing stun duration
        // - Particle effects or visual indicators
    }
}
