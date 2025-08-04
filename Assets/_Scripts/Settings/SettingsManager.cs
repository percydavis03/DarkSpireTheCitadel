using UnityEngine;
using System;
using FMODUnity;

/// <summary>
/// Singleton settings manager that handles loading, saving, and applying game settings
/// Automatically manages FMOD integration and persistence while UI works directly with ScriptableObject
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Settings Data")]
    [SerializeField] private GameSettingsData settingsData;
    [SerializeField] private string settingsFileName = "GameSettings";
    
    [Header("FMOD Bus Paths")]
    [SerializeField] private string masterBusPath = "bus:/";
    [SerializeField] private string musicBusPath = "bus:/Music";
    [SerializeField] private string sfxBusPath = "bus:/SFX";
    [SerializeField] private string environmentBusPath = "bus:/Environment";
    [SerializeField] private string voiceBusPath = "bus:/VA";
    
    // Singleton instance
    private static SettingsManager _instance;
    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SettingsManager>();
                if (_instance == null)
                {
                    // Auto-create if doesn't exist
                    GameObject settingsObject = new GameObject("SettingsManager");
                    _instance = settingsObject.AddComponent<SettingsManager>();
                    DontDestroyOnLoad(settingsObject);
                }
            }
            return _instance;
        }
    }
    
    // FMOD bus references
    private FMOD.Studio.Bus masterBus;
    private FMOD.Studio.Bus musicBus;
    private FMOD.Studio.Bus sfxBus;
    private FMOD.Studio.Bus environmentBus;
    private FMOD.Studio.Bus voiceBus;
    
    // Events for background systems (not UI)
    public static event Action<GameSettingsData> OnSettingsLoaded;
    public static event Action<GameSettingsData> OnSettingsApplied;
    
    // Property to access current settings (mainly for other systems, not UI)
    public static GameSettingsData CurrentSettings => Instance.settingsData;
    
    void Awake()
    {
        // Singleton enforcement
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // If no settings data is assigned, try to load from Resources
            if (settingsData == null)
            {
                LoadSettingsFromResources();
            }
            
            // Initialize FMOD buses
            InitializeFMODBuses();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Load settings from persistent storage
        LoadSettings();
        
        // Apply all settings
        ApplyAllSettings();
        
        // Notify listeners that settings are loaded
        OnSettingsLoaded?.Invoke(settingsData);
    }
    
    /// <summary>
    /// Initialize FMOD bus references
    /// </summary>
    private void InitializeFMODBuses()
    {
        try
        {
            masterBus = RuntimeManager.GetBus(masterBusPath);
            musicBus = RuntimeManager.GetBus(musicBusPath);
            sfxBus = RuntimeManager.GetBus(sfxBusPath);
            environmentBus = RuntimeManager.GetBus(environmentBusPath);
            voiceBus = RuntimeManager.GetBus(voiceBusPath);
            
            Debug.Log("SettingsManager: FMOD buses initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SettingsManager: Failed to initialize FMOD buses: {e.Message}");
        }
    }
    
    /// <summary>
    /// Load settings data from Resources folder
    /// </summary>
    private void LoadSettingsFromResources()
    {
        settingsData = Resources.Load<GameSettingsData>(settingsFileName);
        
        if (settingsData == null)
        {
            Debug.LogWarning($"SettingsManager: Could not find {settingsFileName} in Resources folder. Please create one using the menu: Assets > Create > DarkSpire > Settings > Game Settings Data");
        }
    }
    
    /// <summary>
    /// Load settings from PlayerPrefs into the ScriptableObject
    /// </summary>
    public void LoadSettings()
    {
        if (settingsData == null)
        {
            Debug.LogError("SettingsManager: No settings data assigned!");
            return;
        }
        
        // Load display settings
        settingsData.ScrollSensitivity = PlayerPrefs.GetFloat("DarkSpire_ScrollSensitivity", settingsData.ScrollSensitivity);
        settingsData.WindowMode = PlayerPrefs.GetInt("DarkSpire_WindowMode", settingsData.WindowMode);
        settingsData.ResolutionWidth = PlayerPrefs.GetInt("DarkSpire_ResolutionWidth", Screen.currentResolution.width);
        settingsData.ResolutionHeight = PlayerPrefs.GetInt("DarkSpire_ResolutionHeight", Screen.currentResolution.height);
        settingsData.Brightness = PlayerPrefs.GetFloat("DarkSpire_Brightness", settingsData.Brightness);
        
        // Load audio settings
        settingsData.MasterVolume = PlayerPrefs.GetFloat("DarkSpire_MasterVolume", settingsData.MasterVolume);
        settingsData.MusicVolume = PlayerPrefs.GetFloat("DarkSpire_MusicVolume", settingsData.MusicVolume);
        settingsData.SFXVolume = PlayerPrefs.GetFloat("DarkSpire_SFXVolume", settingsData.SFXVolume);
        settingsData.EnvironmentVolume = PlayerPrefs.GetFloat("DarkSpire_EnvironmentVolume", settingsData.EnvironmentVolume);
        settingsData.VoiceVolume = PlayerPrefs.GetFloat("DarkSpire_VoiceVolume", settingsData.VoiceVolume);
        
        // Load mute settings
        settingsData.MasterMuted = PlayerPrefs.GetInt("DarkSpire_MasterMuted", 0) == 1;
        settingsData.MusicMuted = PlayerPrefs.GetInt("DarkSpire_MusicMuted", 0) == 1;
        settingsData.SFXMuted = PlayerPrefs.GetInt("DarkSpire_SFXMuted", 0) == 1;
        settingsData.EnvironmentMuted = PlayerPrefs.GetInt("DarkSpire_EnvironmentMuted", 0) == 1;
        settingsData.VoiceMuted = PlayerPrefs.GetInt("DarkSpire_VoiceMuted", 0) == 1;
        
        settingsData.ValidateSettings();
        
        Debug.Log("SettingsManager: Settings loaded from PlayerPrefs");
    }
    
    /// <summary>
    /// Save current settings to PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        if (settingsData == null) return;
        
        // Save display settings
        PlayerPrefs.SetFloat("DarkSpire_ScrollSensitivity", settingsData.ScrollSensitivity);
        PlayerPrefs.SetInt("DarkSpire_WindowMode", settingsData.WindowMode);
        PlayerPrefs.SetInt("DarkSpire_ResolutionWidth", settingsData.ResolutionWidth);
        PlayerPrefs.SetInt("DarkSpire_ResolutionHeight", settingsData.ResolutionHeight);
        PlayerPrefs.SetFloat("DarkSpire_Brightness", settingsData.Brightness);
        
        // Save audio settings
        PlayerPrefs.SetFloat("DarkSpire_MasterVolume", settingsData.MasterVolume);
        PlayerPrefs.SetFloat("DarkSpire_MusicVolume", settingsData.MusicVolume);
        PlayerPrefs.SetFloat("DarkSpire_SFXVolume", settingsData.SFXVolume);
        PlayerPrefs.SetFloat("DarkSpire_EnvironmentVolume", settingsData.EnvironmentVolume);
        PlayerPrefs.SetFloat("DarkSpire_VoiceVolume", settingsData.VoiceVolume);
        
        // Save mute settings
        PlayerPrefs.SetInt("DarkSpire_MasterMuted", settingsData.MasterMuted ? 1 : 0);
        PlayerPrefs.SetInt("DarkSpire_MusicMuted", settingsData.MusicMuted ? 1 : 0);
        PlayerPrefs.SetInt("DarkSpire_SFXMuted", settingsData.SFXMuted ? 1 : 0);
        PlayerPrefs.SetInt("DarkSpire_EnvironmentMuted", settingsData.EnvironmentMuted ? 1 : 0);
        PlayerPrefs.SetInt("DarkSpire_VoiceMuted", settingsData.VoiceMuted ? 1 : 0);
        
        PlayerPrefs.Save();
        
        Debug.Log("SettingsManager: Settings saved to PlayerPrefs");
    }
    
    /// <summary>
    /// Apply all settings (display, audio, etc.)
    /// </summary>
    public void ApplyAllSettings()
    {
        ApplyDisplaySettings();
        ApplyAudioSettings();
        OnSettingsApplied?.Invoke(settingsData);
    }
    
    /// <summary>
    /// Apply display settings (window mode, resolution, brightness)
    /// </summary>
    public void ApplyDisplaySettings()
    {
        if (settingsData == null) return;
        
        // Apply window mode
        switch (settingsData.WindowMode)
        {
            case 0: // Fullscreen
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                Screen.fullScreen = true;
                break;
            case 1: // Windowed
                Screen.fullScreen = false;
                break;
            case 2: // Windowed Fullscreen
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Screen.fullScreen = true;
                break;
        }
        
        // Apply resolution
        Screen.SetResolution(settingsData.ResolutionWidth, settingsData.ResolutionHeight, Screen.fullScreen);
        
        Debug.Log($"SettingsManager: Applied display settings - Mode: {settingsData.WindowMode}, Resolution: {settingsData.ResolutionWidth}x{settingsData.ResolutionHeight}");
    }
    
    /// <summary>
    /// Apply audio settings to FMOD buses
    /// </summary>
    public void ApplyAudioSettings()
    {
        if (settingsData == null) return;
        
        try
        {
            // Apply volume and mute settings to each bus
            SetBusVolume(masterBus, settingsData.MasterVolume, settingsData.MasterMuted);
            SetBusVolume(musicBus, settingsData.MusicVolume, settingsData.MusicMuted);
            SetBusVolume(sfxBus, settingsData.SFXVolume, settingsData.SFXMuted);
            SetBusVolume(environmentBus, settingsData.EnvironmentVolume, settingsData.EnvironmentMuted);
            SetBusVolume(voiceBus, settingsData.VoiceVolume, settingsData.VoiceMuted);
            
            Debug.Log($"SettingsManager: Applied audio settings - Master: {settingsData.MasterVolume}, Music: {settingsData.MusicVolume}, SFX: {settingsData.SFXVolume}, Environment: {settingsData.EnvironmentVolume}, Voice: {settingsData.VoiceVolume}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SettingsManager: Error applying audio settings: {e.Message}");
        }
    }
    
    /// <summary>
    /// Set volume for a specific FMOD bus
    /// </summary>
    private void SetBusVolume(FMOD.Studio.Bus bus, float volume, bool muted)
    {
        if (!bus.isValid()) return;
        
        float finalVolume = muted ? 0f : volume;
        bus.setVolume(finalVolume);
    }
    
    /// <summary>
    /// Reset all settings to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        if (settingsData == null) return;
        
        settingsData.ResetToDefaults();
        ApplyAllSettings();
        SaveSettings();
        
        Debug.Log("SettingsManager: Reset all settings to defaults");
    }
    
    /// <summary>
    /// Force apply current settings (useful when settings change externally)
    /// Call this whenever the ScriptableObject values change
    /// </summary>
    public static void ApplyCurrentSettings()
    {
        if (Instance.settingsData != null)
        {
            Instance.ApplyAllSettings();
        }
    }
    
    /// <summary>
    /// Force save current settings to PlayerPrefs
    /// </summary>
    public static void SaveCurrentSettings()
    {
        Instance.SaveSettings();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveSettings();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveSettings();
        }
    }
    
    void OnDestroy()
    {
        SaveSettings();
    }
}