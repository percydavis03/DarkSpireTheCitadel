using FMODUnity;
using FMOD.Studio;
using UnityEngine;

/// <summary>
/// FMOD-based combat sound manager with parameter-driven audio system
/// Supports CharacterType parameter only (0=Nyx, 1=Worker, 2=Spear, 3=Boss)
/// </summary>
public class CombatSoundManager : MonoBehaviour
{
    public static CombatSoundManager Instance { get; private set; }
    
    [Header("FMOD Combat Events")]
    [SerializeField] private EventReference attackEvent;
    [SerializeField] private EventReference hitEvent;
    [SerializeField] private EventReference hurtEvent;
    [SerializeField] private EventReference knockedEvent;
    [SerializeField] private EventReference fallEvent;
    [SerializeField] private EventReference kickedEvent;
    [SerializeField] private EventReference wooshEvent;
    [SerializeField] private EventReference deadEvent;
    [SerializeField] private EventReference stunnedEvent;
    
    [Header("Nyx-Specific Events")]
    [SerializeField] private EventReference slash1Event;
    [SerializeField] private EventReference slash2Event;
    [SerializeField] private EventReference gauntletAttackEvent;
    [SerializeField] private EventReference spinAttackEvent;
    [SerializeField] private EventReference parryEvent;
    [SerializeField] private EventReference rollEvent;
    
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = false;
    
    // Character type constants for reference
    public const int CHARACTER_NYX = 0;
    public const int CHARACTER_WORKER = 1;
    public const int CHARACTER_SPEAR = 2;
    public const int CHARACTER_BOSS = 3;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (enableDebugLogs)
                Debug.Log("CombatSoundManager: Initialized successfully");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("CombatSoundManager: Duplicate instance destroyed");
            Destroy(gameObject);
        }
    }
    
    #region Basic Enemy Sounds (0-3: Worker, Spear, Boss)
    
    /// <summary>
    /// Play hit connected sound for basic enemies
    /// </summary>
    public void PlayHitConnected(Vector3 position, int characterType = 1)
    {
        PlayCombatSound(hitEvent, position, characterType);
    }
    
    /// <summary>
    /// Play hurt sound for basic enemies
    /// </summary>
    public void PlayHurt(Vector3 position, int characterType = 1)
    {
        PlayCombatSound(hurtEvent, position, characterType);
    }
    
    /// <summary>
    /// Play knocked down sound for basic enemies
    /// </summary>
    public void PlayKnocked(Vector3 position, int characterType = 1)
    {
        PlayCombatSound(knockedEvent, position, characterType);
    }
    
    /// <summary>
    /// Play fall sound for basic enemies
    /// </summary>
    public void PlayFall(Vector3 position, int characterType = 1)
    {
        PlayCombatSound(fallEvent, position, characterType);
    }
    
    /// <summary>
    /// Play kicked sound for basic enemies
    /// </summary>
    public void PlayKicked(Vector3 position, int characterType = 1)
    {
        PlayCombatSound(kickedEvent, position, characterType);
    }
    
    /// <summary>
    /// Play woosh sound for basic enemies
    /// </summary>
    public void PlayWoosh(Vector3 position, int characterType = 1)
    {
        PlayCombatSound(wooshEvent, position, characterType);
    }
    
    /// <summary>
    /// Play dead sound for basic enemies
    /// </summary>
    public void PlayDead(Vector3 position, int characterType = 1)
    {
        PlayCombatSound(deadEvent, position, characterType);
    }
    
    /// <summary>
    /// Play stunned sound for basic enemies
    /// </summary>
    public void PlayStunned(Vector3 position, int characterType = 1)
    {
        PlayCombatSound(stunnedEvent, position, characterType);
    }
    
    #endregion
    
    #region Legacy Method Support (for existing enemy scripts)
    
    /// <summary>
    /// Legacy method for hit sound (converts string parameters to int)
    /// </summary>
    public void PlayHitSound(Vector3 position, string targetType = "Enemy", string surfaceType = "Flesh", 
        string weaponType = "Gauntlet", bool isCritical = false)
    {
        int charType = GetCharacterTypeFromString(targetType);
        PlayCombatSound(hitEvent, position, charType);
    }
    
    /// <summary>
    /// Legacy method for hurt sound (converts string parameters to int)
    /// </summary>
    public void PlayHurtSound(Vector3 position, string characterType = "Worker", string damageType = "Light", 
        string weaponType = "Gauntlet")
    {
        int charType = GetCharacterTypeFromString(characterType);
        PlayCombatSound(hurtEvent, position, charType);
    }
    
    /// <summary>
    /// Legacy method for parry sound (converts string parameters to int)
    /// </summary>
    public void PlayParrySound(Vector3 position, string characterType = "Nyx", string weaponType = "Gauntlet", 
        string parriedWeapon = "Sword", bool isPerfect = false)
    {
        int charType = GetCharacterTypeFromString(characterType);
        PlayCombatSound(parryEvent, position, charType);
    }
    
    /// <summary>
    /// Legacy method for fall down sound (converts string parameters to int)
    /// </summary>
    public void PlayFallDownSound(Vector3 position, string characterType = "Worker", string fallType = "Knockdown", 
        string surfaceType = "Metal", bool isHeavyFall = false)
    {
        int charType = GetCharacterTypeFromString(characterType);
        PlayCombatSound(fallEvent, position, charType);
    }
    
    /// <summary>
    /// Legacy method for attack sound (converts string parameters to int)
    /// </summary>
    public void PlayAttackSound(Vector3 position, string characterType = "Worker", string weaponType = "Sword", 
        string attackType = "Light", bool isCritical = false)
    {
        int charType = GetCharacterTypeFromString(characterType);
        PlayCombatSound(attackEvent, position, charType);
    }
    
    /// <summary>
    /// Legacy method for death sound (converts string parameters to int)
    /// </summary>
    public void PlayDeathSound(Vector3 position, string characterType = "Worker", string deathType = "Normal", 
        string killerWeapon = "Gauntlet")
    {
        int charType = GetCharacterTypeFromString(characterType);
        PlayCombatSound(deadEvent, position, charType);
    }
    
    #endregion
    
    #region Nyx-Specific Sounds (CharacterType = 0)
    
    /// <summary>
    /// Play woosh sound for Nyx
    /// </summary>
    public void PlayNyxWoosh(Vector3 position)
    {
        PlayCombatSound(wooshEvent, position, 0);
    }
    
    /// <summary>
    /// Play slash 1 sound for Nyx
    /// </summary>
    public void PlayNyxSlash1(Vector3 position)
    {
        PlayCombatSound(slash1Event, position, 0);
    }
    
    /// <summary>
    /// Play slash 2 sound for Nyx
    /// </summary>
    public void PlayNyxSlash2(Vector3 position)
    {
        PlayCombatSound(slash2Event, position, 0);
    }
    
    /// <summary>
    /// Play gauntlet attack sound for Nyx
    /// </summary>
    public void PlayNyxGauntletAttack(Vector3 position)
    {
        PlayCombatSound(gauntletAttackEvent, position, 0);
    }
    
    /// <summary>
    /// Play spin attack sound for Nyx
    /// </summary>
    public void PlayNyxSpinAttack(Vector3 position)
    {
        PlayCombatSound(spinAttackEvent, position, 0);
    }
    
    /// <summary>
    /// Play hurt sound for Nyx
    /// </summary>
    public void PlayNyxHurt(Vector3 position)
    {
        PlayCombatSound(hurtEvent, position, 0);
    }
    
    /// <summary>
    /// Play parry sound for Nyx
    /// </summary>
    public void PlayNyxParry(Vector3 position)
    {
        PlayCombatSound(parryEvent, position, 0);
    }
    
    /// <summary>
    /// Play roll sound for Nyx
    /// </summary>
    public void PlayNyxRoll(Vector3 position)
    {
        PlayCombatSound(rollEvent, position, 0);
    }
    
    #endregion
    
    #region Core Sound Playing Method
    
    /// <summary>
    /// Core method for playing combat sounds with FMOD parameters
    /// </summary>
    private void PlayCombatSound(EventReference eventRef, Vector3 position, int characterType = 0)
    {
        if (eventRef.IsNull)
        {
            if (enableDebugLogs)
                Debug.LogWarning($"CombatSoundManager: Event reference is null!");
            return;
        }

        try
        {
            EventInstance sound = RuntimeManager.CreateInstance(eventRef);
            sound.set3DAttributes(RuntimeUtils.To3DAttributes(position));
            
            // Set FMOD parameter
            sound.setParameterByName("CharacterType", characterType);

            sound.start();
            sound.release();

            if (enableDebugLogs)
            {
                string eventName = eventRef.ToString();
                Debug.Log($"CombatSoundManager: Played {eventName} at {position} [Character: {characterType}]");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CombatSoundManager: Failed to play event '{eventRef}': {e.Message}");
        }
    }
    
    #endregion
    
    #region Parameter Conversion Methods (for legacy support)
    
    /// <summary>
    /// Convert character type string to integer
    /// </summary>
    private int GetCharacterTypeFromString(string characterType)
    {
        return characterType.ToLower() switch
        {
            "nyx" => 0,
            "worker" => 1,
            "spear" => 2,
            "boss" => 3,
            "enemy" => 1, // Default to worker
            _ => 1
        };
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Enable or disable debug logging
    /// </summary>
    public void SetDebugLogging(bool enabled)
    {
        enableDebugLogs = enabled;
        Debug.Log($"CombatSoundManager: Debug logging {(enabled ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Test if an event reference is valid
    /// </summary>
    public bool IsEventValid(EventReference eventRef)
    {
        return !eventRef.IsNull;
    }
    
    #endregion
}
