using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

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
    public Vector3 standardParryKnockback = new Vector3(80, 30, 80); // Increased for more impact
    
    [Header("Simple Combat Knockback")]
    public Vector3 lightKnockback = new Vector3(5, 1, 5);      // Very light knockback
    public Vector3 mediumKnockback = new Vector3(8, 2, 8);     // Medium knockback  
    public Vector3 heavyKnockback = new Vector3(12, 3, 12);    // Heavy knockback
    
    [Header("Combo Parry System")]
    public float comboParryWindow = 0.1f;       
    public float comboParryBonus = 1.1f;        // Minimal extra damage
    
    [Header("Debug Settings")]
    [Tooltip("Enable debug logs for weapon collisions - disable for performance")]
    public bool enableDebugLogs = true; // ENABLED for parry debugging
    
    [Header("Knockback System Integration")]
    [Tooltip("This script now uses KnockbackManager for unified knockback handling.\n" +
             "• Regular attacks use KnockbackManager.ApplyWeaponKnockback()\n" +
             "• Parries use KnockbackManager.ApplyParryKnockback()\n" +
             "• Configure weapon/parry knockback data in KnockbackManager component\n" +
             "• Falls back to legacy system if enemy has no KnockbackReceiver")]
    [SerializeField] private bool _knockbackSystemInfo = true; // Info field only
    
    [Header("Parry Debug Sound System")]
    [Tooltip("Enable debug sounds when parry is successfully executed")]
    public bool enableParryDebugSounds = false;
    
    [Tooltip("Sound to play when a normal parry is executed")]
    public StudioEventEmitter normalParryDebugSound;
    
    [Tooltip("Sound to play when a perfect parry is executed")]
    public StudioEventEmitter perfectParryDebugSound;
    
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
        int baseDamage = thisGameSave.mainAttackDamage;
        
        if (Player_Movement.instance != null)
        {
            int currentCombo = Player_Movement.instance.comboCount;
            
            if (Player_Movement.instance.isComboing && currentCombo > 0)
            {
                baseDamage = thisGameSave.GetComboDamage(currentCombo);
                if (enableDebugLogs) Debug.Log($"Combo {currentCombo} damage: {baseDamage}");
            }
        }
        
        return baseDamage;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (enableDebugLogs) Debug.Log($"WeaponScript collision with: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        
        if (other.gameObject.CompareTag("Enemy"))
        {
            if (enableDebugLogs) Debug.Log($"Enemy collision detected with {other.gameObject.name}");
            
            // Enhanced parry check with precise timing
            if (parryEnabled && CheckForEnhancedParry(other))
            {
                if (enableDebugLogs) Debug.Log("Parry was executed, skipping normal damage");
                return; // Parry handled, don't continue with normal damage
            }
            
            int damage = GetCurrentComboDamage();
            if (enableDebugLogs) Debug.Log($"Calculated damage: {damage}");
            
            // Determine knockback based on attack type
            Vector3 knockbackForce = lightKnockback; // Default
            string comboType = "Basic Attack";
            
            if (Player_Movement.instance != null && Player_Movement.instance.isComboing)
            {
                // Use combo count for appropriate knockback
                int currentCombo = Player_Movement.instance.comboCount;
                switch (currentCombo)
                {
                    case 1:
                        knockbackForce = lightKnockback;
                        comboType = "1st Combo";
                        break;
                    case 2:
                        knockbackForce = mediumKnockback;
                        comboType = "2nd Combo";
                        break;
                    case 3:
                        knockbackForce = heavyKnockback;
                        comboType = "3rd Combo - HEAVY HIT!";
                        break;
                    default:
                        knockbackForce = mediumKnockback;
                        comboType = "Combo Attack";
                        break;
                }
            }
            
            if (other.TryGetComponent(out Enemy_Basic enemyBasic))
            {
                if (enableDebugLogs) Debug.Log($"Found Enemy_Basic component, calling TakeComboDamage({damage})");
                enemyBasic.TakeComboDamage(damage);
                
                // Check if enemy has KnockbackReceiver - if so, use KnockbackManager, otherwise use legacy
                if (other.TryGetComponent(out KnockbackReceiver knockbackReceiver))
                {
                    // Use new knockback system
                    if (KnockbackManager.Instance != null)
                    {
                        bool knockbackApplied = KnockbackManager.Instance.ApplyWeaponKnockback(
                            knockbackReceiver, 
                            transform.position
                        );
                        if (enableDebugLogs) Debug.Log($"{comboType} - Using KnockbackManager for knockback: {(knockbackApplied ? "Success" : "Failed")}");
                    }
                    else
                    {
                        if (enableDebugLogs) Debug.LogWarning($"KnockbackManager not found, skipping knockback for {other.name}");
                    }
                }
                else
                {
                    // Fallback to legacy system only if no KnockbackReceiver
                    enemyBasic.GetKnockedBack(knockbackForce);
                    if (enableDebugLogs) Debug.Log($"{comboType} - Legacy Knockback: {knockbackForce}");
                }
            }
            else if (other.TryGetComponent(out Worker worker))
            {
                if (enableDebugLogs) Debug.Log($"Found Worker component, calling TakeComboDamage({damage})");
                worker.TakeComboDamage(damage);
                
                // Check if worker has KnockbackReceiver - if so, use KnockbackManager, otherwise use legacy
                if (other.TryGetComponent(out KnockbackReceiver knockbackReceiver))
                {
                    // Use new knockback system
                    if (KnockbackManager.Instance != null)
                    {
                        bool knockbackApplied = KnockbackManager.Instance.ApplyWeaponKnockback(
                            knockbackReceiver, 
                            transform.position
                        );
                        if (enableDebugLogs) Debug.Log($"{comboType} on Worker - Using KnockbackManager for knockback: {(knockbackApplied ? "Success" : "Failed")}");
                    }
                    else
                    {
                        if (enableDebugLogs) Debug.LogWarning($"KnockbackManager not found, skipping knockback for {other.name}");
                    }
                }
                else
                {
                    // Fallback to legacy system only if no KnockbackReceiver
                    Vector3 workerKnockback = new Vector3(
                        knockbackForce.x * 0.8f, // Workers get slightly less knockback
                        knockbackForce.y * 0.8f,
                        knockbackForce.z * 0.8f
                    );
                    worker.GetKnockedBack(workerKnockback);
                    if (enableDebugLogs) Debug.Log($"{comboType} on Worker - Legacy Knockback: {workerKnockback}");
                }
            }
            else
            {
                if (enableDebugLogs) Debug.Log($"No Enemy_Basic or Worker component found on {other.gameObject.name}");
            }
            // Removed the duplicate IKnockbackable check that was causing double knockback
        }
        else
        {
            if (enableDebugLogs) Debug.Log($"Object {other.gameObject.name} does not have Enemy tag");
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
        if (enableDebugLogs) Debug.Log(isPerfectParry ? "PERFECT PARRY! Extended stun!" : "PARRY SUCCESS! Enemy stunned!");
        
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
        
        // Simple, consistent knockback - check for KnockbackReceiver to prevent double knockback
        if (enemy.TryGetComponent(out KnockbackReceiver knockbackReceiver))
        {
            // Use new knockback system for parries
            if (KnockbackManager.Instance != null)
            {
                bool knockbackApplied = KnockbackManager.Instance.ApplyParryKnockback(
                    knockbackReceiver, 
                    transform.position
                );
                if (enableDebugLogs) Debug.Log($"Parry - Using KnockbackManager for knockback: {(knockbackApplied ? "Success" : "Failed")}");
            }
        }
        else
        {
            // Fallback to legacy system only if no KnockbackReceiver
            enemy.GetKnockedBack(standardParryKnockback);
        }
        
        if (enableDebugLogs) Debug.Log($"Parry: {parryDamage} damage, {stunDuration}s stun (Perfect: {isPerfectParry})");
        
        OnParrySuccess(isPerfectParry);
        
        return true;
    }

    private bool ExecuteEnhancedParry(Worker worker, bool isPerfectParry)
    {
        if (enableDebugLogs) Debug.Log(isPerfectParry ? "PERFECT PARRY! Worker stunned longer!" : "PARRY SUCCESS! Worker stunned!");
        
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
        
        // Simple, consistent knockback for workers - check for KnockbackReceiver to prevent double knockback
        if (worker.TryGetComponent(out KnockbackReceiver knockbackReceiver))
        {
            // Use new knockback system for worker parries
            if (KnockbackManager.Instance != null)
            {
                bool knockbackApplied = KnockbackManager.Instance.ApplyParryKnockback(
                    knockbackReceiver, 
                    transform.position
                );
                if (enableDebugLogs) Debug.Log($"Worker Parry - Using KnockbackManager for knockback: {(knockbackApplied ? "Success" : "Failed")}");
            }
        }
        else
        {
            // Fallback to legacy system only if no KnockbackReceiver
            worker.GetKnockedBack(standardParryKnockback);
        }
        
        if (enableDebugLogs) Debug.Log($"Worker parry: {parryDamage} damage, {stunDuration}s stun (Perfect: {isPerfectParry})");
        
        OnParrySuccess(isPerfectParry);
        
        return true;
    }
    
    private void OnParrySuccess(bool isPerfectParry)
    {
        if (enableDebugLogs) Debug.Log($"Parry executed! Perfect: {isPerfectParry} - Focus: Extended stun for grappling!");
        
        // Play debug sounds for parry testing
        if (enableParryDebugSounds)
        {
            if (isPerfectParry && perfectParryDebugSound != null)
            {
                perfectParryDebugSound.Play();
                if (enableDebugLogs) Debug.Log("PERFECT PARRY DEBUG SOUND PLAYED!");
            }
            else if (!isPerfectParry && normalParryDebugSound != null)
            {
                normalParryDebugSound.Play();
                if (enableDebugLogs) Debug.Log("Normal parry debug sound played.");
            }
        }
        
        // TODO: Simple feedback without slow motion:
        // - Brief screen shake for all parries
        // - UI feedback showing stun duration
        // - Particle effects or visual indicators
    }
    

}

