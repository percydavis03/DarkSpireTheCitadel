using UnityEngine;
using TMPro;

public class WindowModeandRes : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown windowModeDropdown;
    
    [Header("Window Mode Index Assignment")]
    [SerializeField] private int windowedIndex = 1;
    [SerializeField] private int fullscreenIndex = 0;
    [SerializeField] private int windowedFullscreenIndex = 2;
    
    void OnEnable()
    {
        // Debug the actual variable values
        Debug.Log($"WindowModeandRes: windowedIndex = {windowedIndex}, fullscreenIndex = {fullscreenIndex}, windowedFullscreenIndex = {windowedFullscreenIndex}");
        
        if (windowModeDropdown != null)
        {
            // Set dropdown to index 0
            windowModeDropdown.value = 0;
            windowModeDropdown.onValueChanged.AddListener(ChangeWindowMode);
           // Debug.Log("WindowModeandRes: Dropdown listener added");
        }
        else
        {
           // Debug.LogWarning("WindowModeandRes: No dropdown assigned!");
        }
    }
    
    public void ChangeWindowMode(int modeIndex)
    {
        //Debug.Log($"WindowModeandRes: Changing to mode index {modeIndex}");
        
        if (modeIndex == windowedIndex)
        {
            Screen.fullScreen = false;
          //  Debug.Log("WindowModeandRes: Set to Windowed mode");
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
          //  Debug.Log("WindowModeandRes: Set to Windowed Fullscreen mode");
        }
        else
        {
          //  Debug.LogWarning($"WindowModeandRes: Unknown mode index {modeIndex}");
        }
    }
}
