using UnityEngine;

/// <summary>
/// ScriptableObject that stores all game settings data
/// This approach is industry-standard for persistent game configuration
/// </summary>
[CreateAssetMenu(fileName = "GameSettingsData", menuName = "DarkSpire/Settings/Game Settings Data")]
public class GameSettingsData : ScriptableObject
{
    [Header("Scroll Sensitivity Settings")]
    [SerializeField] private float scrollSensitivity = 1.0f;
    [SerializeField] private float minScrollSensitivity = 0.1f;
    [SerializeField] private float maxScrollSensitivity = 3.0f;
    [SerializeField] private float scrollThreshold = 0.1f;
    [SerializeField] private float minScrollThreshold = 0.01f;
    [SerializeField] private float maxScrollThreshold = 1.0f;
    
    [Header("Display Settings")]
    [SerializeField] private int windowMode = 0; // 0 = Fullscreen, 1 = Windowed, 2 = Windowed Fullscreen
    [SerializeField] private int resolutionWidth = 1920;
    [SerializeField] private int resolutionHeight = 1080;
    [SerializeField] private float brightness = 1.0f;
    [SerializeField] private float minBrightness = 0.05f;
    [SerializeField] private float maxBrightness = 2.0f;
    
    [Header("Audio Settings (FMOD)")]
    [SerializeField] private float masterVolume = 1.0f;
    [SerializeField] private float musicVolume = 0.8f;
    [SerializeField] private float sfxVolume = 1.0f;
    [SerializeField] private float environmentVolume = 0.7f;
    [SerializeField] private float voiceVolume = 1.0f;
    [SerializeField] private bool masterMuted = false;
    [SerializeField] private bool musicMuted = false;
    [SerializeField] private bool sfxMuted = false;
    [SerializeField] private bool environmentMuted = false;
    [SerializeField] private bool voiceMuted = false;
    
    // Public properties with validation
    public float ScrollSensitivity
    {
        get => scrollSensitivity;
        set => scrollSensitivity = Mathf.Clamp(value, minScrollSensitivity, maxScrollSensitivity);
    }
    
    public float ScrollThreshold
    {
        get => scrollThreshold;
        set => scrollThreshold = Mathf.Clamp(value, minScrollThreshold, maxScrollThreshold);
    }
    
    public int WindowMode
    {
        get => windowMode;
        set => windowMode = Mathf.Clamp(value, 0, 2);
    }
    
    public int ResolutionWidth
    {
        get => resolutionWidth;
        set => resolutionWidth = Mathf.Max(640, value);
    }
    
    public int ResolutionHeight
    {
        get => resolutionHeight;
        set => resolutionHeight = Mathf.Max(480, value);
    }
    
    public float Brightness
    {
        get => brightness;
        set => brightness = Mathf.Clamp(value, minBrightness, maxBrightness);
    }
    
    // Audio properties with validation (0-1 range)
    public float MasterVolume
    {
        get => masterVolume;
        set => masterVolume = Mathf.Clamp01(value);
    }
    
    public float MusicVolume
    {
        get => musicVolume;
        set => musicVolume = Mathf.Clamp01(value);
    }
    
    public float SFXVolume
    {
        get => sfxVolume;
        set => sfxVolume = Mathf.Clamp01(value);
    }
    
    public float EnvironmentVolume
    {
        get => environmentVolume;
        set => environmentVolume = Mathf.Clamp01(value);
    }
    
    public float VoiceVolume
    {
        get => voiceVolume;
        set => voiceVolume = Mathf.Clamp01(value);
    }
    
    // Mute properties
    public bool MasterMuted
    {
        get => masterMuted;
        set => masterMuted = value;
    }
    
    public bool MusicMuted
    {
        get => musicMuted;
        set => musicMuted = value;
    }
    
    public bool SFXMuted
    {
        get => sfxMuted;
        set => sfxMuted = value;
    }
    
    public bool EnvironmentMuted
    {
        get => environmentMuted;
        set => environmentMuted = value;
    }
    
    public bool VoiceMuted
    {
        get => voiceMuted;
        set => voiceMuted = value;
    }
    
    // Range getters for UI setup
    public float MinScrollSensitivity => minScrollSensitivity;
    public float MaxScrollSensitivity => maxScrollSensitivity;
    public float MinScrollThreshold => minScrollThreshold;
    public float MaxScrollThreshold => maxScrollThreshold;
    public float MinBrightness => minBrightness;
    public float MaxBrightness => maxBrightness;
    
    /// <summary>
    /// Reset all settings to default values
    /// </summary>
    public void ResetToDefaults()
    {
        scrollSensitivity = 1.0f;
        scrollThreshold = 0.1f;
        windowMode = 0;
        resolutionWidth = Screen.currentResolution.width;
        resolutionHeight = Screen.currentResolution.height;
        brightness = 1.0f;
        
        masterVolume = 1.0f;
        musicVolume = 0.8f;
        sfxVolume = 1.0f;
        environmentVolume = 0.7f;
        voiceVolume = 1.0f;
        
        masterMuted = false;
        musicMuted = false;
        sfxMuted = false;
        environmentMuted = false;
        voiceMuted = false;
        
        // Mark the asset as dirty so Unity saves the changes
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// Validate all settings are within acceptable ranges
    /// </summary>
    public void ValidateSettings()
    {
        scrollSensitivity = Mathf.Clamp(scrollSensitivity, minScrollSensitivity, maxScrollSensitivity);
        scrollThreshold = Mathf.Clamp(scrollThreshold, minScrollThreshold, maxScrollThreshold);
        windowMode = Mathf.Clamp(windowMode, 0, 2);
        resolutionWidth = Mathf.Max(640, resolutionWidth);
        resolutionHeight = Mathf.Max(480, resolutionHeight);
        brightness = Mathf.Clamp(brightness, minBrightness, maxBrightness);
        
        masterVolume = Mathf.Clamp01(masterVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        environmentVolume = Mathf.Clamp01(environmentVolume);
        voiceVolume = Mathf.Clamp01(voiceVolume);
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    void OnValidate()
    {
        ValidateSettings();
    }
}