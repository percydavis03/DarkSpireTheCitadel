using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class WindowModeandRes : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    
    [Header("Window Mode Index Assignment")]
    [SerializeField] private int windowedIndex = 1;
    [SerializeField] private int fullscreenIndex = 0;
    [SerializeField] private int windowedFullscreenIndex = 2;
    
    private Resolution[] resolutions;
    private List<Resolution> filteredResolutions;
    
    void OnEnable()
    {
        // Debug the actual variable values
        Debug.Log($"WindowModeandRes: windowedIndex = {windowedIndex}, fullscreenIndex = {fullscreenIndex}, windowedFullscreenIndex = {windowedFullscreenIndex}");
        
        SetupWindowModeDropdown();
        SetupResolutionDropdown();
    }
    
    void SetupWindowModeDropdown()
    {
        if (windowModeDropdown != null)
        {
            // Set dropdown to index 0
            windowModeDropdown.value = 0;
            windowModeDropdown.onValueChanged.AddListener(ChangeWindowMode);
            // Debug.Log("WindowModeandRes: Window mode dropdown listener added");
        }
        else
        {
            // Debug.LogWarning("WindowModeandRes: No window mode dropdown assigned!");
        }
    }
    
    void SetupResolutionDropdown()
    {
        if (resolutionDropdown != null)
        {
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
            
            int currentResolutionIndex = 0;
            for (int i = 0; i < filteredResolutions.Count; i++)
            {
                string option = filteredResolutions[i].width + "x" + filteredResolutions[i].height;
                options.Add(option);
                
                // Check if this is the current resolution
                if (filteredResolutions[i].width == Screen.currentResolution.width &&
                    filteredResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }
            
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
            
            // Debug.Log($"WindowModeandRes: Resolution dropdown setup complete. Found {filteredResolutions.Count} unique resolutions");
        }
        else
        {
            // Debug.LogWarning("WindowModeandRes: No resolution dropdown assigned!");
        }
    }
    
    public void ChangeWindowMode(int modeIndex)
    {
        //Debug.Log($"WindowModeandRes: Changing to mode index {modeIndex}");
        
        if (modeIndex == windowedIndex)
        {
            Screen.fullScreen = false;
            // Debug.Log("WindowModeandRes: Set to Windowed mode");
        }
        else if (modeIndex == fullscreenIndex)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
            Screen.fullScreen = true;
            // Debug.Log("WindowModeandRes: Set to Fullscreen mode");
        }
        else if (modeIndex == windowedFullscreenIndex)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
            // Debug.Log("WindowModeandRes: Set to Windowed Fullscreen mode");
        }
        else
        {
            // Debug.LogWarning($"WindowModeandRes: Unknown mode index {modeIndex}");
        }
    }
    
    public void ChangeResolution(int resolutionIndex)
    {
        if (resolutionIndex >= 0 && resolutionIndex < filteredResolutions.Count)
        {
            Resolution resolution = filteredResolutions[resolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            // Debug.Log($"WindowModeandRes: Changed resolution to {resolution.width}x{resolution.height}");
        }
        else
        {
            // Debug.LogWarning($"WindowModeandRes: Invalid resolution index {resolutionIndex}");
        }
    }
    
    void OnDisable()
    {
        // Clean up listeners
        if (windowModeDropdown != null)
        {
            windowModeDropdown.onValueChanged.RemoveListener(ChangeWindowMode);
        }
        
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(ChangeResolution);
        }
    }
}
