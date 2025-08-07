using UnityEngine;

/// <summary>
/// Component that can be attached to objects to assign combat audio parameters
/// Allows easy assignment of CharacterType for FMOD events
/// </summary>
public class CombatAudioComponent : MonoBehaviour
{
    [Header("Character Configuration")]
    [CharacterType]
    [Tooltip("Character type for this object")]
    [SerializeField] private int characterType = 1;
    
    [Header("Audio Settings")]
    [SerializeField] private bool enableDebugLogs = false;
    [SerializeField] private bool use3DAudio = true;
    
    [Header("Sound Cooldowns")]
    [Tooltip("Minimum time between repeated sounds to prevent spam")]
    [SerializeField] private float soundCooldown = 0.1f;
    
    // Cooldown tracking
    private float lastSoundTime = 0f;
    
    // Cached audio position
    private Vector3 AudioPosition => use3DAudio ? transform.position : Vector3.zero;
    
    #region Public Properties
    
    public int CharacterType => characterType;
    
    #endregion
    
    #region Basic Enemy Sound Methods
    
    /// <summary>
    /// Play hit connected sound
    /// </summary>
    public void PlayHitConnected()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null)
        {
            CombatSoundManager.Instance.PlayHitConnected(AudioPosition, characterType);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing hit connected sound");
        }
    }
    
    /// <summary>
    /// Play hurt sound
    /// </summary>
    public void PlayHurt()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null)
        {
            CombatSoundManager.Instance.PlayHurt(AudioPosition, characterType);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing hurt sound");
        }
    }
    
    /// <summary>
    /// Play knocked down sound
    /// </summary>
    public void PlayKnocked()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null)
        {
            CombatSoundManager.Instance.PlayKnocked(AudioPosition, characterType);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing knocked sound");
        }
    }
    
    /// <summary>
    /// Play fall sound
    /// </summary>
    public void PlayFall()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null)
        {
            CombatSoundManager.Instance.PlayFall(AudioPosition, characterType);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing fall sound");
        }
    }
    
    /// <summary>
    /// Play kicked sound
    /// </summary>
    public void PlayKicked()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null)
        {
            CombatSoundManager.Instance.PlayKicked(AudioPosition, characterType);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing kicked sound");
        }
    }
    
    /// <summary>
    /// Play woosh sound
    /// </summary>
    public void PlayWoosh()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null)
        {
            CombatSoundManager.Instance.PlayWoosh(AudioPosition, characterType);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing woosh sound");
        }
    }
    
    /// <summary>
    /// Play dead sound
    /// </summary>
    public void PlayDead()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null)
        {
            CombatSoundManager.Instance.PlayDead(AudioPosition, characterType);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing dead sound");
        }
    }
    
    /// <summary>
    /// Play stunned sound
    /// </summary>
    public void PlayStunned()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null)
        {
            CombatSoundManager.Instance.PlayStunned(AudioPosition, characterType);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing stunned sound");
        }
    }
    
    #endregion
    
    #region Nyx-Specific Sound Methods
    
    /// <summary>
    /// Play Nyx woosh sound (only works if characterType = 0)
    /// </summary>
    public void PlayNyxWoosh()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null && characterType == 0)
        {
            CombatSoundManager.Instance.PlayNyxWoosh(AudioPosition);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing Nyx woosh sound");
        }
    }
    
    /// <summary>
    /// Play Nyx slash 1 sound (only works if characterType = 0)
    /// </summary>
    public void PlayNyxSlash1()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null && characterType == 0)
        {
            CombatSoundManager.Instance.PlayNyxSlash1(AudioPosition);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing Nyx slash 1 sound");
        }
    }
    
    /// <summary>
    /// Play Nyx slash 2 sound (only works if characterType = 0)
    /// </summary>
    public void PlayNyxSlash2()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null && characterType == 0)
        {
            CombatSoundManager.Instance.PlayNyxSlash2(AudioPosition);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing Nyx slash 2 sound");
        }
    }
    
    /// <summary>
    /// Play Nyx gauntlet attack sound (only works if characterType = 0)
    /// </summary>
    public void PlayNyxGauntletAttack()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null && characterType == 0)
        {
            CombatSoundManager.Instance.PlayNyxGauntletAttack(AudioPosition);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing Nyx gauntlet attack sound");
        }
    }
    
    /// <summary>
    /// Play Nyx spin attack sound (only works if characterType = 0)
    /// </summary>
    public void PlayNyxSpinAttack()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null && characterType == 0)
        {
            CombatSoundManager.Instance.PlayNyxSpinAttack(AudioPosition);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing Nyx spin attack sound");
        }
    }
    
    /// <summary>
    /// Play Nyx hurt sound (only works if characterType = 0)
    /// </summary>
    public void PlayNyxHurt()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null && characterType == 0)
        {
            CombatSoundManager.Instance.PlayNyxHurt(AudioPosition);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing Nyx hurt sound");
        }
    }
    
    /// <summary>
    /// Play Nyx parry sound (only works if characterType = 0)
    /// </summary>
    public void PlayNyxParry()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null && characterType == 0)
        {
            CombatSoundManager.Instance.PlayNyxParry(AudioPosition);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing Nyx parry sound");
        }
    }
    
    /// <summary>
    /// Play Nyx roll sound (only works if characterType = 0)
    /// </summary>
    public void PlayNyxRoll()
    {
        if (!CanPlaySound()) return;
        
        if (CombatSoundManager.Instance != null && characterType == 0)
        {
            CombatSoundManager.Instance.PlayNyxRoll(AudioPosition);
            
            if (enableDebugLogs)
                Debug.Log($"{gameObject.name}: Playing Nyx roll sound");
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Check if enough time has passed to play another sound
    /// </summary>
    private bool CanPlaySound()
    {
        if (Time.time - lastSoundTime < soundCooldown)
        {
            return false;
        }
        
        lastSoundTime = Time.time;
        return true;
    }
    
    /// <summary>
    /// Reset sound cooldowns
    /// </summary>
    public void ResetCooldowns()
    {
        lastSoundTime = 0f;
    }
    
    /// <summary>
    /// Set character type
    /// </summary>
    public void SetCharacterType(int type)
    {
        characterType = Mathf.Clamp(type, 0, 3);
    }
    

    
    /// <summary>
    /// Set debug logging
    /// </summary>
    public void SetDebugLogs(bool enable)
    {
        enableDebugLogs = enable;
    }
    
    /// <summary>
    /// Set 3D audio usage
    /// </summary>
    public void SetUse3DAudio(bool use3D)
    {
        use3DAudio = use3D;
    }
    
    /// <summary>
    /// Set sound cooldown
    /// </summary>
    public void SetSoundCooldown(float cooldown)
    {
        soundCooldown = Mathf.Max(0f, cooldown);
    }
    
    #endregion
    
    #region Editor Methods
    
    [ContextMenu("Test Basic Sounds")]
    private void TestBasicSounds()
    {
        if (CombatSoundManager.Instance == null)
        {
            Debug.LogWarning("CombatSoundManager not found in scene!");
            return;
        }
        
        Debug.Log($"Testing basic sounds for {gameObject.name} [Character: {characterType}]");
        
        // Test basic enemy sounds
        PlayHitConnected();
        Invoke(nameof(PlayHurt), 0.2f);
        Invoke(nameof(PlayKnocked), 0.4f);
        Invoke(nameof(PlayFall), 0.6f);
        Invoke(nameof(PlayKicked), 0.8f);
        Invoke(nameof(PlayWoosh), 1.0f);
        Invoke(nameof(PlayDead), 1.2f);
        Invoke(nameof(PlayStunned), 1.4f);
    }
    
    [ContextMenu("Test Nyx Sounds")]
    private void TestNyxSounds()
    {
        if (CombatSoundManager.Instance == null)
        {
            Debug.LogWarning("CombatSoundManager not found in scene!");
            return;
        }
        
        if (characterType != 0)
        {
            Debug.LogWarning("Nyx sounds only work when characterType = 0!");
            return;
        }
        
        Debug.Log($"Testing Nyx sounds for {gameObject.name} [Character: {characterType}]");
        
        // Test Nyx-specific sounds
        PlayNyxWoosh();
        Invoke(nameof(PlayNyxSlash1), 0.2f);
        Invoke(nameof(PlayNyxSlash2), 0.4f);
        Invoke(nameof(PlayNyxGauntletAttack), 0.6f);
        Invoke(nameof(PlayNyxSpinAttack), 0.8f);
        Invoke(nameof(PlayNyxHurt), 1.0f);
        Invoke(nameof(PlayNyxParry), 1.2f);
        Invoke(nameof(PlayNyxRoll), 1.4f);
    }
    
    void OnValidate()
    {
        // Clamp values to valid ranges
        characterType = Mathf.Clamp(characterType, 0, 3);
        soundCooldown = Mathf.Max(0f, soundCooldown);
    }
    
    #endregion
}
