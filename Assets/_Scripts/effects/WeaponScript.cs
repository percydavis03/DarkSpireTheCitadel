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
    public float parryDamageMultiplier = 1.5f;
    public float parryStunDuration = 2.0f;
    public float perfectParryBonus = 2.0f;      // Extra damage for frame-perfect parries
    
    [Header("Combo Parry System")]
    public float comboParryWindow = 0.1f;       // Shorter window during combos
    public float comboParryBonus = 1.2f;        // Extra multiplier for combo parries
    
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
        Debug.Log(isPerfectParry ? "PERFECT PARRY! Maximum damage!" : "PARRY SUCCESS! Enemy stunned!");
        
        // Calculate enhanced parry damage
        float totalMultiplier = parryDamageMultiplier;
        
        if (isPerfectParry)
        {
            totalMultiplier *= perfectParryBonus;
        }
        
        if (Player_Movement.instance != null && Player_Movement.instance.isComboing)
        {
            totalMultiplier *= comboParryBonus;
        }
        
        int parryDamage = Mathf.RoundToInt(GetCurrentComboDamage() * totalMultiplier);
        
        // Apply enhanced parry effects
        float stunDuration = isPerfectParry ? parryStunDuration * 1.5f : parryStunDuration;
        enemy.GetParried(stunDuration);
        
        enemy.enemyHP -= parryDamage;
        Debug.Log($"Enhanced parry damage: {parryDamage} (Perfect: {isPerfectParry})");
        
        OnParrySuccess(isPerfectParry);
        
        return true;
    }

    private bool ExecuteEnhancedParry(Worker worker, bool isPerfectParry)
    {
        Debug.Log(isPerfectParry ? "PERFECT PARRY! Maximum damage!" : "PARRY SUCCESS! Worker stunned!");
        
        float totalMultiplier = parryDamageMultiplier;
        
        if (isPerfectParry)
        {
            totalMultiplier *= perfectParryBonus;
        }
        
        if (Player_Movement.instance != null && Player_Movement.instance.isComboing)
        {
            totalMultiplier *= comboParryBonus;
        }
        
        int parryDamage = Mathf.RoundToInt(GetCurrentComboDamage() * totalMultiplier);
        
        float stunDuration = isPerfectParry ? parryStunDuration * 1.5f : parryStunDuration;
        worker.GetParried(stunDuration);
        
        worker.enemyHP -= parryDamage;
        Debug.Log($"Enhanced parry damage: {parryDamage} (Perfect: {isPerfectParry})");
        
        OnParrySuccess(isPerfectParry);
        
        return true;
    }

    private void OnParrySuccess(bool isPerfectParry)
    {
        Debug.Log($"Enhanced parry executed! Perfect: {isPerfectParry}");
        
        if (Player_Movement.instance != null)
        {
            StartCoroutine(EnhancedParryFeedback(isPerfectParry));
        }
        
        // TODO: Enhanced feedback based on parry quality:
        // - Different particle effects for perfect vs normal parries
        // - Different screen shake intensity
        // - Different sound effects
        // - UI feedback showing parry quality
        // - Different slow motion effects
    }

    private IEnumerator EnhancedParryFeedback(bool isPerfectParry)
    {
        // Enhanced feedback for perfect parries
        float timeScale = isPerfectParry ? 0.05f : 0.1f;
        float duration = isPerfectParry ? 0.2f : 0.1f;
        
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}
