using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple, reusable volume slider component for FMOD audio buses
/// Can be configured for any audio type and integrates with SettingsManager
/// </summary>
public class AudioVolumeSlider : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeLabel;
    [SerializeField] private TextMeshProUGUI volumeValueText;
    [SerializeField] private Button muteButton;
    [SerializeField] private TextMeshProUGUI muteButtonText;
    
    [Header("Audio Configuration")]
    [SerializeField] private AudioBusType busType = AudioBusType.Master;
    [SerializeField] private string customLabel = "";
    [SerializeField] private bool showPercentage = true;
    [SerializeField] private bool showMuteButton = true;
    
    [Header("Settings Data")]
    [SerializeField] private GameSettingsData gameSettings;
    
    // Audio bus types matching FMOD setup
    public enum AudioBusType
    {
        Master,
        Music,
        SFX,
        Environment,
        Voice
    }
    
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
        if (volumeSlider == null)
        {
            volumeSlider = GetComponent<Slider>();
        }
        
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        if (muteButton != null)
        {
            muteButton.onClick.AddListener(OnMuteToggled);
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
                Debug.LogWarning($"AudioVolumeSlider ({busType}): No GameSettings asset found in Resources folder!");
            }
        }
    }
    
    /// <summary>
    /// Set up UI elements based on configuration
    /// </summary>
    private void SetupUI()
    {
        // Set label text
        if (volumeLabel != null)
        {
            string labelText = string.IsNullOrEmpty(customLabel) ? busType.ToString() : customLabel;
            volumeLabel.text = labelText;
        }
        
        // Show/hide mute button
        if (muteButton != null)
        {
            muteButton.gameObject.SetActive(showMuteButton);
        }
    }
    

    
    /// <summary>
    /// Update slider value from current settings
    /// </summary>
    private void UpdateFromSettings()
    {
        if (gameSettings == null || !isInitialized) return;
        
        isUpdatingFromSettings = true;
        float volume = 0f;
        bool muted = false;
        
        switch (busType)
        {
            case AudioBusType.Master:
                volume = gameSettings.MasterVolume;
                muted = gameSettings.MasterMuted;
                break;
            case AudioBusType.Music:
                volume = gameSettings.MusicVolume;
                muted = gameSettings.MusicMuted;
                break;
            case AudioBusType.SFX:
                volume = gameSettings.SFXVolume;
                muted = gameSettings.SFXMuted;
                break;
            case AudioBusType.Environment:
                volume = gameSettings.EnvironmentVolume;
                muted = gameSettings.EnvironmentMuted;
                break;
            case AudioBusType.Voice:
                volume = gameSettings.VoiceVolume;
                muted = gameSettings.VoiceMuted;
                break;
        }
        
        // Update slider
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
        }
        
        // Update volume display
        UpdateVolumeDisplay(volume);
        
        // Update mute button
        UpdateMuteButton(muted);
        
        isUpdatingFromSettings = false;
    }
    
    /// <summary>
    /// Handle volume slider changes
    /// </summary>
    private void OnVolumeChanged(float value)
    {
        if (isUpdatingFromSettings || gameSettings == null) return;
        
        // Update the appropriate volume setting
        switch (busType)
        {
            case AudioBusType.Master:
                gameSettings.MasterVolume = value;
                break;
            case AudioBusType.Music:
                gameSettings.MusicVolume = value;
                break;
            case AudioBusType.SFX:
                gameSettings.SFXVolume = value;
                break;
            case AudioBusType.Environment:
                gameSettings.EnvironmentVolume = value;
                break;
            case AudioBusType.Voice:
                gameSettings.VoiceVolume = value;
                break;
        }
        
        // Apply the changes to FMOD
        SettingsManager.ApplyCurrentSettings();
        
        UpdateVolumeDisplay(value);
    }
    
    /// <summary>
    /// Handle mute button toggle
    /// </summary>
    private void OnMuteToggled()
    {
        if (gameSettings == null) return;
        
        // Toggle the appropriate mute setting
        switch (busType)
        {
            case AudioBusType.Master:
                gameSettings.MasterMuted = !gameSettings.MasterMuted;
                UpdateMuteButton(gameSettings.MasterMuted);
                break;
            case AudioBusType.Music:
                gameSettings.MusicMuted = !gameSettings.MusicMuted;
                UpdateMuteButton(gameSettings.MusicMuted);
                break;
            case AudioBusType.SFX:
                gameSettings.SFXMuted = !gameSettings.SFXMuted;
                UpdateMuteButton(gameSettings.SFXMuted);
                break;
            case AudioBusType.Environment:
                gameSettings.EnvironmentMuted = !gameSettings.EnvironmentMuted;
                UpdateMuteButton(gameSettings.EnvironmentMuted);
                break;
            case AudioBusType.Voice:
                gameSettings.VoiceMuted = !gameSettings.VoiceMuted;
                UpdateMuteButton(gameSettings.VoiceMuted);
                break;
        }
        
        // Apply the changes to FMOD
        SettingsManager.ApplyCurrentSettings();
    }
    
    /// <summary>
    /// Update volume display text
    /// </summary>
    private void UpdateVolumeDisplay(float volume)
    {
        if (volumeValueText != null)
        {
            if (showPercentage)
            {
                volumeValueText.text = $"{(volume * 100):F0}%";
            }
            else
            {
                volumeValueText.text = volume.ToString("F2");
            }
        }
    }
    
    /// <summary>
    /// Update mute button appearance
    /// </summary>
    private void UpdateMuteButton(bool muted)
    {
        if (muteButtonText != null)
        {
            muteButtonText.text = muted ? "Unmute" : "Mute";
        }
        
        // Optional: Change button color or sprite based on mute state
        if (muteButton != null)
        {
            var colors = muteButton.colors;
            colors.normalColor = muted ? Color.red : Color.white;
            muteButton.colors = colors;
        }
    }
    

    
    /// <summary>
    /// Public method to set the bus type (useful for dynamic setup)
    /// </summary>
    public void SetBusType(AudioBusType newBusType)
    {
        busType = newBusType;
        SetupUI();
        UpdateFromSettings();
    }
    
    /// <summary>
    /// Public method to set custom label
    /// </summary>
    public void SetCustomLabel(string label)
    {
        customLabel = label;
        SetupUI();
    }
}