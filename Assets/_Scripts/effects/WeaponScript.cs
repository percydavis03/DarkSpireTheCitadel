using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public PlayerSaveState thisGameSave;
    
    [Header("Parry System")]
    public bool parryEnabled = true;
    public float parryWindow = 0.5f; // Time window for successful parry
    public float parryDamageMultiplier = 1.5f; // Extra damage multiplier on parry
    public float parryStunDuration = 2.0f; // How long enemies stay stunned
    private float lastParryTime = -1f; // Track last parry to prevent spam
    
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
            // Check for parry conditions first
            if (parryEnabled && CheckForParry(other))
            {
                return; // Parry handled, don't continue with normal damage
            }
            
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

    private bool CheckForParry(Collider enemyCollider)
    {
        // Check if player is currently attacking
        bool playerAttacking = Player_Movement.instance != null && Player_Movement.instance.IsInAttackState();
        
        if (!playerAttacking) return false;
        
        // Check parry cooldown to prevent spam
        if (Time.time - lastParryTime < parryWindow)
        {
            return false;
        }
        
        // Check Enemy_Basic
        if (enemyCollider.TryGetComponent(out Enemy_Basic enemyBasic))
        {
            if (enemyBasic.IsAttacking() && enemyBasic.canBeParried && !enemyBasic.isStunned)
            {
                lastParryTime = Time.time;
                return ExecuteParry(enemyBasic);
            }
        }
        // Check Worker
        else if (enemyCollider.TryGetComponent(out Worker worker))
        {
            if (worker.IsAttacking() && worker.canBeParried && !worker.isStunned)
            {
                lastParryTime = Time.time;
                return ExecuteParry(worker);
            }
        }
        
        return false;
    }

    private bool ExecuteParry(Enemy_Basic enemy)
    {
        Debug.Log("PARRY SUCCESS! Enemy stunned!");
        
        // Calculate parry damage
        int parryDamage = Mathf.RoundToInt(GetCurrentComboDamage() * parryDamageMultiplier);
        
        // Apply parry effects with custom duration
        enemy.GetParried(parryStunDuration);
        
        // Deal parry damage
        enemy.enemyHP -= parryDamage;
        Debug.Log($"Parry damage: {parryDamage}");
        
        // Add visual/audio effects for parry
        OnParrySuccess();
        
        return true;
    }

    private bool ExecuteParry(Worker worker)
    {
        Debug.Log("PARRY SUCCESS! Worker stunned!");
        
        // Calculate parry damage
        int parryDamage = Mathf.RoundToInt(GetCurrentComboDamage() * parryDamageMultiplier);
        
        // Apply parry effects with custom duration
        worker.GetParried(parryStunDuration);
        
        // Deal parry damage
        worker.enemyHP -= parryDamage;
        Debug.Log($"Parry damage: {parryDamage}");
        
        // Add visual/audio effects for parry
        OnParrySuccess();
        
        return true;
    }

    private void OnParrySuccess()
    {
        Debug.Log("Parry executed successfully!");
        
        // Basic feedback - can be expanded with more effects
        // Stop player briefly for impact feel
        if (Player_Movement.instance != null)
        {
            StartCoroutine(ParryFeedback());
        }
        
        // TODO: Future enhancements could include:
        // - Screen shake
        // - Particle effects  
        // - Sound effects
        // - UI feedback
        // - Slow motion effect
    }

    private IEnumerator ParryFeedback()
    {
        // Brief pause for impact feel
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1f;
    }
}
