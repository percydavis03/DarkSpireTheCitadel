# Settings System Migration Guide

This guide explains how to migrate from the old individual settings scripts to the new consolidated settings system.

## Overview

The new system consolidates all settings management into a unified, industry-standard implementation:

- **Old System**: `WindowModeandRes.cs`, `SettingsAdjustor.cs`, individual scripts
- **New System**: `GameSettings.cs` + `SimpleSettingsMenu.cs` unified approach

## Migration Steps

### 1. Remove Old Components

**From GameObjects that had these components:**
- `WindowModeandRes` component → Replace with `SimpleSettingsMenu`
- `SettingsAdjustor` component → Replace with `SimpleSettingsMenu`

**⚠️ Important**: Do NOT delete the old script files yet - keep them as reference during migration.

### 2. Set Up New Settings Menu

**Add SimpleSettingsMenu Component:**
```csharp
// On your settings menu GameObject
public class YourSettingsMenu : MonoBehaviour 
{
    // Replace individual components with SimpleSettingsMenu
}
```

**Assign UI References in Inspector:**
- **Settings Panel**: The main settings panel GameObject
- **Buttons**: Open/Close/Apply buttons
- **Window Mode Dropdown**: TMP_Dropdown for fullscreen/windowed selection
- **Resolution Dropdown**: TMP_Dropdown for resolution selection  
- **Scroll Sensitivity Slider**: The ScrollSensitivitySlider component
- **Brightness Slider**: Standard Unity Slider
- **Brightness Value Text**: TextMeshPro component to show brightness value
- **Post Process Profile**: Your brightness post-processing profile
- **Post Process Layer**: Your post-processing layer

### 3. Update UI Prefabs/Scenes

**Window Mode Dropdown Options:**
```
Index 0: "Fullscreen"
Index 1: "Windowed" 
Index 2: "Windowed Fullscreen"
```

**Brightness Slider:**
- Min Value: 0.05
- Max Value: 2.0
- Default Value: 1.0

### 4. Code Migration

**Old WindowModeandRes.cs usage:**
```csharp
// OLD
public WindowModeandRes windowModeScript;
windowModeScript.ChangeWindowMode(0);
windowModeScript.ChangeResolution(5);
```

**New GameSettings.cs usage:**
```csharp
// NEW
GameSettings.Instance.WindowMode = 0;
GameSettings.Instance.SetResolution(1920, 1080);
GameSettings.Instance.ApplyDisplaySettings();
```

**Old SettingsAdjustor.cs usage:**
```csharp
// OLD  
public SettingsAdjustor settingsScript;
settingsScript.AdjustBrightness(1.5f);
```

**New GameSettings.cs usage:**
```csharp
// NEW
GameSettings.Instance.Brightness = 1.5f;
```

### 5. Event Subscriptions

**Subscribe to settings changes:**
```csharp
void Start()
{
    // Subscribe to setting changes
    GameSettings.OnWindowModeChanged += OnWindowModeChanged;
    GameSettings.OnResolutionChanged += OnResolutionChanged; 
    GameSettings.OnBrightnessChanged += OnBrightnessChanged;
    GameSettings.OnScrollSensitivityChanged += OnScrollSensitivityChanged;
}

void OnDestroy()
{
    // Always unsubscribe!
    GameSettings.OnWindowModeChanged -= OnWindowModeChanged;
    GameSettings.OnResolutionChanged -= OnResolutionChanged;
    GameSettings.OnBrightnessChanged -= OnBrightnessChanged;
    GameSettings.OnScrollSensitivityChanged -= OnScrollSensitivityChanged;
}
```

## Configuration Mapping

### Window Mode Indices
- **0**: Fullscreen (ExclusiveFullScreen)
- **1**: Windowed 
- **2**: Windowed Fullscreen (FullScreenWindow)

### PlayerPrefs Keys
The new system uses these PlayerPrefs keys:
- `DarkSpire_ScrollSensitivity` - Scroll sensitivity value
- `DarkSpire_WindowMode` - Window mode (0-2)
- `DarkSpire_ResolutionWidth` - Screen width
- `DarkSpire_ResolutionHeight` - Screen height  
- `DarkSpire_Brightness` - Brightness value
- `DarkSpire_SettingsVersion` - Settings version for migration

## Benefits of New System

### ✅ Advantages:
- **Unified Management**: All settings in one place
- **Persistent Storage**: Automatic save/load with PlayerPrefs
- **Event System**: Real-time updates across the game
- **Industry Standard**: Follows Unity best practices
- **Performance**: Cached values and optimized saves
- **Extensible**: Easy to add new settings

### 🔄 Migration Benefits:
- **Better Organization**: All settings code in one location
- **Reduced Complexity**: No more scattered settings scripts
- **Version Control**: Automatic settings migration support
- **Error Handling**: Robust validation and fallbacks

## Testing Your Migration

### 1. Functionality Test:
- [ ] Window mode changes work (fullscreen, windowed, windowed fullscreen)
- [ ] Resolution changes apply correctly
- [ ] Brightness adjustment affects post-processing
- [ ] Scroll sensitivity affects lock-on targeting (mouse/keyboard only)
- [ ] Settings persist between game sessions

### 2. UI Test:
- [ ] Dropdowns populate correctly
- [ ] Sliders show current values
- [ ] Changes update in real-time
- [ ] Apply button works for display settings

### 3. Edge Cases:
- [ ] Invalid resolutions are handled gracefully
- [ ] First-time setup uses sensible defaults
- [ ] Input device switching works properly
- [ ] Settings validation prevents invalid values

## Cleanup Steps

**After successful migration:**

1. **Keep old scripts temporarily** - useful for reference
2. **Remove old components** from GameObjects via Inspector
3. **Test thoroughly** with the new system
4. **Update documentation** to reference new system
5. **Eventually delete old scripts** (optional, after confidence is high)

## Common Issues & Solutions

### Issue: Settings don't persist
**Solution**: Ensure PlayerPrefs.Save() is called and permissions are correct

### Issue: UI doesn't update
**Solution**: Check event subscriptions and `isUpdatingFromSettings` flags

### Issue: Resolution dropdown is empty
**Solution**: Verify Screen.resolutions is accessible and filtering works

### Issue: Brightness doesn't work
**Solution**: Check PostProcessProfile and AutoExposure component setup

### Issue: Scroll sensitivity not working
**Solution**: Verify InputDeviceManager is in scene and mouse/keyboard detected

## Advanced Configuration

### Custom Settings:
```csharp
// Add new settings to GameSettings.cs
public class GameSettings : MonoBehaviour
{
    // Add new setting
    private float _customSetting = 1.0f;
    public float CustomSetting 
    { 
        get { /* ... */ } 
        set { /* save and notify */ } 
    }
}
```

### Custom UI Components:
```csharp
// Create specialized UI components following ScrollSensitivitySlider pattern
public class CustomSettingSlider : MonoBehaviour
{
    // Subscribe to GameSettings.OnCustomSettingChanged
    // Handle UI updates and prevent feedback loops
}
```

This migration provides a solid foundation for all future settings additions and follows Unity industry standards for settings management.