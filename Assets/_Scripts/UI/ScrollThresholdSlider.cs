using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI component for adjusting scroll threshold in game settings
/// Attach this to a UI slider to control scroll threshold
/// </summary>
public class ScrollThresholdSlider : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider thresholdSlider;
    [SerializeField] private TextMeshProUGUI valueDisplayText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private string titleString = "Scroll Threshold";
    [SerializeField] private bool showPercentage = false;
    [SerializeField] private string valueFormat = "F2";
    
    [Header("Settings Data")]
    [SerializeField] private GameSettingsData gameSettings;
    
    private bool isInitialized = false;
    private bool isUpdatingFromSettings = false;
    
    void Start()
    {
        InitializeSlider();
        FindGameSettings();
        SetupUI();
        UpdateFromSettings();
    }
    
    void OnDestroy()
    {
        // Clean up UI event listeners - handled automatically by Unity
    }
    
    /// <summary>
    /// Initialize slider configuration
    /// </summary>
    private void InitializeSlider()
    {
        if (thresholdSlider == null)
        {
            thresholdSlider = GetComponent<Slider>();
        }
        
        if (thresholdSlider != null)
        {
            thresholdSlider.onValueChanged.AddListener(OnThresholdChanged);
        }
        
        isInitialized = true;
    }
    
    /// <summary>
    /// Find the GameSettingsData asset
    /// </summary>
    private void FindGameSettings()
    {
        if (gameSettings == null)
        {
            gameSettings = Resources.Load<GameSettingsData>("GameSettings");
            if (gameSettings == null)
            {
                Debug.LogWarning("ScrollThresholdSlider: No GameSettings asset found in Resources folder!");
            }
        }
    }
    
    /// <summary>
    /// Set up UI elements based on configuration
    /// </summary>
    private void SetupUI()
    {
        // Set title text
        if (titleText != null)
        {
            titleText.text = titleString;
        }
        
        // Set up slider range
        if (thresholdSlider != null && gameSettings != null)
        {
            thresholdSlider.minValue = gameSettings.MinScrollThreshold;
            thresholdSlider.maxValue = gameSettings.MaxScrollThreshold;
        }
    }
    
    /// <summary>
    /// Update UI from current settings
    /// </summary>
    private void UpdateFromSettings()
    {
        if (gameSettings == null || thresholdSlider == null) return;
        
        isUpdatingFromSettings = true;
        thresholdSlider.value = gameSettings.ScrollThreshold;
        UpdateValueDisplay(gameSettings.ScrollThreshold);
        isUpdatingFromSettings = false;
    }
    
    /// <summary>
    /// Handle slider value changes
    /// </summary>
    private void OnThresholdChanged(float newValue)
    {
        if (isUpdatingFromSettings || gameSettings == null) return;
        
        // Update the setting
        gameSettings.ScrollThreshold = newValue;
        
        // Update display
        UpdateValueDisplay(newValue);
        
        // Apply changes and save
        if (SettingsManager.Instance != null)
        {
            SettingsManager.ApplyCurrentSettings();
        }
    }
    
    /// <summary>
    /// Update value display text
    /// </summary>
    private void UpdateValueDisplay(float value)
    {
        if (valueDisplayText == null) return;
        
        if (showPercentage)
        {
            valueDisplayText.text = $"{(value * 100):F0}%";
        }
        else
        {
            valueDisplayText.text = value.ToString(valueFormat);
        }
    }
}
