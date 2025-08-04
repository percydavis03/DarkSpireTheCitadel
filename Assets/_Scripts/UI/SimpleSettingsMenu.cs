using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.PostProcessing;
using System.Collections.Generic;

/// <summary>
/// Comprehensive settings menu that handles all game settings using ScriptableObject system
/// Includes display settings, brightness, and FMOD audio controls
/// </summary>
public class SimpleSettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button openSettingsButton;
    [SerializeField] private Button closeSettingsButton;
    
    [Header("Settings Data")]
    [SerializeField] private GameSettingsData gameSettings;
    
    [Header("Display Settings")]
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private bool autoApplyDisplayChanges = true; // Automatically apply display changes
    
    [Header("Brightness Settings")]
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TextMeshProUGUI brightnessValueText;
    [SerializeField] private PostProcessProfile brightnessProfile;
    [SerializeField] private PostProcessLayer postProcessLayer;
    
    [Header("Window Mode Index Assignment")]
    [SerializeField] private int fullscreenIndex = 0;
    [SerializeField] private int windowedIndex = 1;
    [SerializeField] private int windowedFullscreenIndex = 2;
    
    [Header("Input Device Display (Optional)")]
    [SerializeField] private TextMeshProUGUI inputDeviceText;
    [SerializeField] private bool showInputDeviceInfo = true;
    [SerializeField] private float updateInputDeviceInterval = 0.5f;
    
    // Private variables
    private float lastInputDeviceUpdateTime;
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    private AutoExposure autoExposure;
    private bool isUpdatingFromSettings = false;
    
    void Start()
    {
        // Find GameSettingsData if not assigned
        if (gameSettings == null)
        {
            gameSettings = Resources.Load<GameSettingsData>("GameSettings");
            if (gameSettings == null)
            {
                Debug.LogError("SimpleSettingsMenu: No GameSettings asset found in Resources folder! Please create one using: Create > DarkSpire > Settings > Game Settings Data");
                return;
            }
        }
        
        // Set up button events
        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.AddListener(OpenSettings);
        }
        
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.AddListener(CloseSettings);
        }
        
        // Initialize settings panel state
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Set up display settings
        SetupWindowModeDropdown();
        SetupResolutionDropdown();
        SetupBrightnessSlider();
        
        // No need to subscribe to events since we work directly with ScriptableObject
        
        // Load current values
        LoadCurrentSettings();
    }
    
    void Update()
    {
        // Update input device display periodically
        if (showInputDeviceInfo && inputDeviceText != null && 
            Time.time - lastInputDeviceUpdateTime > updateInputDeviceInterval)
        {
            UpdateInputDeviceDisplay();
            lastInputDeviceUpdateTime = Time.time;
        }
        
        // Handle ESC key to close settings (common UX pattern)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeInHierarchy)
            {
                CloseSettings();
            }
        }
    }
    
    /// <summary>
    /// Open the settings menu
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            
            // Update input device display immediately when opening
            if (showInputDeviceInfo)
            {
                UpdateInputDeviceDisplay();
            }
            
            Debug.Log("Settings menu opened");
        }
    }
    
    /// <summary>
    /// Close the settings menu
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            
            // Save settings when closing (good UX practice)
            SettingsManager.SaveCurrentSettings();
            
            Debug.Log("Settings menu closed and settings saved");
        }
    }
    
    /// <summary>
    /// Toggle the settings menu open/closed
    /// </summary>
    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            if (settingsPanel.activeInHierarchy)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
    }
    
    /// <summary>
    /// Update the input device display text
    /// </summary>
    private void UpdateInputDeviceDisplay()
    {
        if (inputDeviceText == null) return;
        
        string deviceInfo = "Input Device: ";
        
        // Get current input device from InputDeviceManager
        if (PixelCrushers.InputDeviceManager.instance != null)
        {
            var currentDevice = PixelCrushers.InputDeviceManager.currentInputDevice;
            deviceInfo += currentDevice.ToString();
        }
        else
        {
            deviceInfo += "Unknown";
        }
        
        inputDeviceText.text = deviceInfo;
    }
    
    /// <summary>
    /// Reset all settings to defaults
    /// </summary>
    public void ResetAllSettings()
    {
        if (gameSettings != null)
        {
            gameSettings.ResetToDefaults();
            SettingsManager.ApplyCurrentSettings();
            SettingsManager.SaveCurrentSettings();
            LoadCurrentSettings(); // Refresh UI
            Debug.Log("All settings reset to defaults");
        }
    }
    
    void OnDestroy()
    {
        // Clean up UI event listeners
        
        // Clean up button events
        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.RemoveListener(OpenSettings);
        }
        
        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveListener(CloseSettings);
        }
        
        // No apply button needed - auto-apply is enabled
        
        // Clean up UI events
        if (windowModeDropdown != null)
        {
            windowModeDropdown.onValueChanged.RemoveListener(OnWindowModeChanged);
        }
        
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        }
        
        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
        }
    }
    
    #region Display Settings Setup
    
    private void SetupWindowModeDropdown()
    {
        if (windowModeDropdown == null) return;
        
        windowModeDropdown.onValueChanged.AddListener(OnWindowModeChanged);
        Debug.Log("SimpleSettingsMenu: Window mode dropdown setup complete");
    }
    
    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        
        resolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();
        
        // Filter resolutions to avoid duplicates (same resolution with different refresh rates)
        List<string> resolutionStrings = new List<string>();
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            string resolutionString = resolutions[i].width + "x" + resolutions[i].height;
            if (!resolutionStrings.Contains(resolutionString))
            {
                resolutionStrings.Add(resolutionString);
                filteredResolutions.Add(resolutions[i]);
            }
        }
        
        // Clear existing options and add filtered resolutions
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            string option = filteredResolutions[i].width + "x" + filteredResolutions[i].height;
            options.Add(option);
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        
        Debug.Log($"SimpleSettingsMenu: Resolution dropdown setup complete. Found {filteredResolutions.Count} unique resolutions");
    }
    
    private void SetupBrightnessSlider()
    {
        if (brightnessSlider == null) return;
        
        // Set up brightness post-processing
        if (brightnessProfile != null)
        {
            brightnessProfile.TryGetSettings(out autoExposure);
        }
        
        // Configure slider range
        if (gameSettings != null)
        {
            brightnessSlider.minValue = gameSettings.MinBrightness;
            brightnessSlider.maxValue = gameSettings.MaxBrightness;
        }
        else
        {
            brightnessSlider.minValue = 0.05f;
            brightnessSlider.maxValue = 2.0f;
        }
        
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        Debug.Log("SimpleSettingsMenu: Brightness slider setup complete");
    }
    
    #endregion
    

    
    #region Load Current Settings
    
    private void LoadCurrentSettings()
    {
        if (gameSettings == null) return;
        
        isUpdatingFromSettings = true;
        
        // Load window mode
        if (windowModeDropdown != null)
        {
            windowModeDropdown.value = gameSettings.WindowMode;
        }
        
        // Load resolution
        LoadCurrentResolution();
        
        // Load brightness
        if (brightnessSlider != null)
        {
            brightnessSlider.value = gameSettings.Brightness;
            UpdateBrightnessDisplay(gameSettings.Brightness);
            ApplyBrightness(gameSettings.Brightness);
        }
        
        isUpdatingFromSettings = false;
    }
    
    private void LoadCurrentResolution()
    {
        if (resolutionDropdown == null || filteredResolutions == null || gameSettings == null) return;
        
        int currentResolutionIndex = 0;
        int targetWidth = gameSettings.ResolutionWidth;
        int targetHeight = gameSettings.ResolutionHeight;
        
        for (int i = 0; i < filteredResolutions.Count; i++)
        {
            if (filteredResolutions[i].width == targetWidth && filteredResolutions[i].height == targetHeight)
            {
                currentResolutionIndex = i;
                break;
            }
        }
        
        resolutionDropdown.value = currentResolutionIndex;
    }
    
    #endregion
    
    #region UI Event Handlers
    
    private void OnWindowModeChanged(int modeIndex)
    {
        if (isUpdatingFromSettings || gameSettings == null) return;
        
        gameSettings.WindowMode = modeIndex;
        SettingsManager.ApplyCurrentSettings();
        
        Debug.Log($"Window mode changed to: {modeIndex}");
    }
    
    private void OnResolutionChanged(int resolutionIndex)
    {
        if (isUpdatingFromSettings || gameSettings == null) return;
        
        if (resolutionIndex >= 0 && resolutionIndex < filteredResolutions.Count)
        {
            Resolution resolution = filteredResolutions[resolutionIndex];
            gameSettings.ResolutionWidth = resolution.width;
            gameSettings.ResolutionHeight = resolution.height;
            SettingsManager.ApplyCurrentSettings();
            
            Debug.Log($"Resolution changed to: {resolution.width}x{resolution.height}");
        }
    }
    
    private void OnBrightnessChanged(float value)
    {
        if (isUpdatingFromSettings || gameSettings == null) return;
        
        gameSettings.Brightness = value;
        
        UpdateBrightnessDisplay(value);
        ApplyBrightness(value);
    }
    
    #endregion
    
    
    #region Brightness Management
    
    private void UpdateBrightnessDisplay(float value)
    {
        if (brightnessValueText != null)
        {
            brightnessValueText.text = value.ToString("F2");
        }
    }
    
    private void ApplyBrightness(float value)
    {
        if (autoExposure != null)
        {
            if (value > 0)
            {
                autoExposure.keyValue.value = value;
            }
            else
            {
                autoExposure.keyValue.value = 0.05f;
            }
        }
    }
    
    #endregion
    

    
    // Public methods for UI button integration
    [System.Serializable]
    public class SettingsMenuEvents
    {
        public UnityEngine.Events.UnityEvent OnSettingsOpened;
        public UnityEngine.Events.UnityEvent OnSettingsClosed;
        public UnityEngine.Events.UnityEvent OnSettingsReset;
    }
    
    [Header("Events")]
    public SettingsMenuEvents events;
    
    // Context menu for testing
    [ContextMenu("Debug: Print Current Settings")]
    private void DebugPrintCurrentSettings()
    {
        UpdateInputDeviceDisplay();
    }
}