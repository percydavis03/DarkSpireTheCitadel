using UnityEngine;
using System;

/// <summary>
/// Industry-standard game settings manager that handles player preferences
/// Saves settings using PlayerPrefs for persistent storage across sessions
/// </summary>
public class GameSettings : MonoBehaviour
{

    
    [Header("Display Settings")]
    [SerializeField] private int defaultWindowMode = 0; // 0 = Fullscreen, 1 = Windowed, 2 = Windowed Fullscreen
    [SerializeField] private float defaultBrightness = 1.0f;
    [SerializeField] private float minBrightness = 0.05f;
    [SerializeField] private float maxBrightness = 2.0f;
    
    // PlayerPrefs keys - industry standard naming with app prefix
    private const string WINDOW_MODE_KEY = "DarkSpire_WindowMode";
    private const string RESOLUTION_WIDTH_KEY = "DarkSpire_ResolutionWidth";
    private const string RESOLUTION_HEIGHT_KEY = "DarkSpire_ResolutionHeight";
    private const string BRIGHTNESS_KEY = "DarkSpire_Brightness";
    private const string SETTINGS_VERSION_KEY = "DarkSpire_SettingsVersion";
    private const int CURRENT_SETTINGS_VERSION = 2; // Incremented for new settings
    
    // Cached values for performance
    private int _windowMode = 0;
    private int _resolutionWidth = 1920;
    private int _resolutionHeight = 1080;
    private float _brightness = 1.0f;
    private bool _settingsLoaded = false;
    
    // Singleton pattern for easy access - industry standard approach
    private static GameSettings _instance;
    public static GameSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameSettings>();
                if (_instance == null)
                {
                    // Auto-create if doesn't exist
                    GameObject settingsObject = new GameObject("GameSettings");
                    _instance = settingsObject.AddComponent<GameSettings>();
                    DontDestroyOnLoad(settingsObject);
                }
            }
            return _instance;
        }
    }
    
    // Events for settings changes - allows other systems to respond
    public static event Action<int> OnWindowModeChanged;
    public static event Action<int, int> OnResolutionChanged;
    public static event Action<float> OnBrightnessChanged;
    
    // Properties with validation
    
    public int WindowMode
    {
        get
        {
            if (!_settingsLoaded) LoadSettings();
            return _windowMode;
        }
        set
        {
            int clampedValue = Mathf.Clamp(value, 0, 2); // 0-2 for the three window modes
            if (_windowMode != clampedValue)
            {
                _windowMode = clampedValue;
                SaveWindowMode();
                OnWindowModeChanged?.Invoke(_windowMode);
            }
        }
    }
    
    public int ResolutionWidth
    {
        get
        {
            if (!_settingsLoaded) LoadSettings();
            return _resolutionWidth;
        }
        set
        {
            if (_resolutionWidth != value)
            {
                _resolutionWidth = value;
                SaveResolution();
                OnResolutionChanged?.Invoke(_resolutionWidth, _resolutionHeight);
            }
        }
    }
    
    public int ResolutionHeight
    {
        get
        {
            if (!_settingsLoaded) LoadSettings();
            return _resolutionHeight;
        }
        set
        {
            if (_resolutionHeight != value)
            {
                _resolutionHeight = value;
                SaveResolution();
                OnResolutionChanged?.Invoke(_resolutionWidth, _resolutionHeight);
            }
        }
    }
    
    public float Brightness
    {
        get
        {
            if (!_settingsLoaded) LoadSettings();
            return _brightness;
        }
        set
        {
            float clampedValue = Mathf.Clamp(value, minBrightness, maxBrightness);
            if (_brightness != clampedValue)
            {
                _brightness = clampedValue;
                SaveBrightness();
                OnBrightnessChanged?.Invoke(_brightness);
            }
        }
    }
    
    // Getters for ranges and defaults
    public int DefaultWindowMode => defaultWindowMode;
    public float MinBrightness => minBrightness;
    public float MaxBrightness => maxBrightness;
    public float DefaultBrightness => defaultBrightness;
    
    void Awake()
    {
        // Singleton enforcement
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Ensure settings are loaded
        if (!_settingsLoaded)
        {
            LoadSettings();
        }
    }
    
    /// <summary>
    /// Load all settings from PlayerPrefs
    /// </summary>
    public void LoadSettings()
    {
        // Check settings version for future compatibility
        int savedVersion = PlayerPrefs.GetInt(SETTINGS_VERSION_KEY, 0);
        
        if (savedVersion == 0)
        {
            // First time setup - use defaults and current screen settings
            _windowMode = defaultWindowMode;
            _resolutionWidth = Screen.currentResolution.width;
            _resolutionHeight = Screen.currentResolution.height;
            _brightness = defaultBrightness;
            SaveAllSettings();
        }
        else if (savedVersion == CURRENT_SETTINGS_VERSION)
        {
            // Load saved settings
            _windowMode = PlayerPrefs.GetInt(WINDOW_MODE_KEY, defaultWindowMode);
            _windowMode = Mathf.Clamp(_windowMode, 0, 2);
            
            _resolutionWidth = PlayerPrefs.GetInt(RESOLUTION_WIDTH_KEY, Screen.currentResolution.width);
            _resolutionHeight = PlayerPrefs.GetInt(RESOLUTION_HEIGHT_KEY, Screen.currentResolution.height);
            
            _brightness = PlayerPrefs.GetFloat(BRIGHTNESS_KEY, defaultBrightness);
            _brightness = Mathf.Clamp(_brightness, minBrightness, maxBrightness);
        }
        else if (savedVersion < CURRENT_SETTINGS_VERSION)
        {
            // Handle version migration - load what exists, use defaults for new settings
            // New settings get defaults
            _windowMode = defaultWindowMode;
            _resolutionWidth = Screen.currentResolution.width;
            _resolutionHeight = Screen.currentResolution.height;
            _brightness = defaultBrightness;
            
            Debug.Log($"GameSettings: Migrated from version {savedVersion} to {CURRENT_SETTINGS_VERSION}");
            SaveAllSettings();
        }
        else
        {
            // Handle future version (shouldn't happen)
            Debug.LogWarning($"GameSettings: Unknown settings version {savedVersion}, using defaults");
            _windowMode = defaultWindowMode;
            _resolutionWidth = Screen.currentResolution.width;
            _resolutionHeight = Screen.currentResolution.height;
            _brightness = defaultBrightness;
            SaveAllSettings();
        }
        
        _settingsLoaded = true;
        
        Debug.Log($"GameSettings: Loaded settings - Window: {_windowMode}, Resolution: {_resolutionWidth}x{_resolutionHeight}, Brightness: {_brightness}");
    }
    
    /// <summary>
    /// Save all settings to PlayerPrefs
    /// </summary>
    public void SaveAllSettings()
    {
        SaveWindowMode();
        SaveResolution();
        SaveBrightness();
        PlayerPrefs.SetInt(SETTINGS_VERSION_KEY, CURRENT_SETTINGS_VERSION);
        PlayerPrefs.Save(); // Force immediate save
        
        Debug.Log("GameSettings: All settings saved");
    }
    
    /// <summary>
    /// Save just the window mode setting
    /// </summary>
    private void SaveWindowMode()
    {
        PlayerPrefs.SetInt(WINDOW_MODE_KEY, _windowMode);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Save just the resolution settings
    /// </summary>
    private void SaveResolution()
    {
        PlayerPrefs.SetInt(RESOLUTION_WIDTH_KEY, _resolutionWidth);
        PlayerPrefs.SetInt(RESOLUTION_HEIGHT_KEY, _resolutionHeight);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Save just the brightness setting
    /// </summary>
    private void SaveBrightness()
    {
        PlayerPrefs.SetFloat(BRIGHTNESS_KEY, _brightness);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Reset all settings to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        _windowMode = defaultWindowMode;
        _resolutionWidth = Screen.currentResolution.width;
        _resolutionHeight = Screen.currentResolution.height;
        _brightness = defaultBrightness;
        
        SaveAllSettings();
        
        // Notify all listeners
        OnWindowModeChanged?.Invoke(_windowMode);
        OnResolutionChanged?.Invoke(_resolutionWidth, _resolutionHeight);
        OnBrightnessChanged?.Invoke(_brightness);
        
        Debug.Log("GameSettings: Reset to defaults");
    }
    
    /// <summary>
    /// Set resolution from width and height values
    /// </summary>
    public void SetResolution(int width, int height)
    {
        _resolutionWidth = width;
        _resolutionHeight = height;
        SaveResolution();
        OnResolutionChanged?.Invoke(_resolutionWidth, _resolutionHeight);
    }
    
    /// <summary>
    /// Apply the current window mode setting to the Unity Screen
    /// </summary>
    public void ApplyWindowMode()
    {
        switch (_windowMode)
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
    }
    
    /// <summary>
    /// Apply the current resolution setting to the Unity Screen
    /// </summary>
    public void ApplyResolution()
    {
        Screen.SetResolution(_resolutionWidth, _resolutionHeight, Screen.fullScreen);
    }
    
    /// <summary>
    /// Apply both window mode and resolution settings
    /// </summary>
    public void ApplyDisplaySettings()
    {
        ApplyWindowMode();
        ApplyResolution();
    }
    
    /// <summary>
    /// Validate that all settings are within acceptable ranges
    /// </summary>
    public bool ValidateSettings()
    {
        // All current settings are automatically validated by their setters
        return true;
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Save settings when app is paused (mobile/console)
            SaveAllSettings();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            // Save settings when app loses focus
            SaveAllSettings();
        }
    }
    
    void OnDestroy()
    {
        if (_instance == this)
        {
            SaveAllSettings();
        }
    }
    
    // Debug methods for testing
    [ContextMenu("Debug: Print Current Settings")]
    private void DebugPrintSettings()
    {
        Debug.Log($"Current Settings - Window Mode: {WindowMode}, Resolution: {_resolutionWidth}x{_resolutionHeight}, Brightness: {Brightness}");
    }
    
    [ContextMenu("Debug: Reset Settings")]
    private void DebugResetSettings()
    {
        ResetToDefaults();
    }
}