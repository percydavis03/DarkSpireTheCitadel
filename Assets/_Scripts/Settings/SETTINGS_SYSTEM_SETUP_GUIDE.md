# DarkSpire Settings System Setup Guide

## Overview
This system replaces the old singleton-based `GameSettings` with a clean, industry-standard ScriptableObject-based approach that includes full FMOD audio support.

## Components Created

### 1. GameSettingsData (ScriptableObject)
- Stores all settings data in a reusable, inspector-friendly format
- Includes display settings, audio volumes, and mute states
- Auto-validates settings ranges

### 2. SettingsManager (Non-Singleton)
- Handles loading/saving settings to PlayerPrefs
- Manages FMOD bus volumes and muting
- Provides events for UI updates
- Can be placed on any GameObject (typically your options menu)

### 3. AudioVolumeSlider (UI Component)
- Reusable volume slider for any FMOD audio bus
- Automatically syncs with SettingsManager
- Supports mute buttons and percentage display

### 4. Updated SimpleSettingsMenu
- Now uses the ScriptableObject system
- Integrates with SettingsManager instead of singleton

## Setup Instructions

### Step 1: Create the Settings Data Asset
1. Right-click in your `Assets/Resources/` folder
2. Go to `Create > DarkSpire > Settings > Game Settings Data`
3. Name it "GameSettings" (this will be auto-loaded by SettingsManager)
4. Configure default values in the inspector

### Step 2: Add SettingsManager to Your Options Menu
1. Add the `SettingsManager` component to your options menu GameObject
2. Assign the GameSettingsData asset you created
3. Verify the FMOD bus paths match your project:
   - Master: `bus:/`
   - Music: `bus:/Music`
   - SFX: `bus:/SFX`
   - Environment: `bus:/Environment`
   - Voice: `bus:/VA`

### Step 3: Set Up Audio Volume Sliders
For each audio type you want to control:

1. Create a UI setup with:
   - Slider component
   - Text label (optional)
   - Value display text (optional)
   - Mute button (optional)

2. Add the `AudioVolumeSlider` component
3. Configure the bus type (Master, Music, SFX, Environment, Voice)
4. Assign UI references

### Step 4: Update Your Options Menu
1. Add a reference to your `SettingsManager` in the `SimpleSettingsMenu`
2. The system will automatically handle loading/saving and FMOD integration

### Step 5: Remove Old System (Optional)
You can safely delete:
- `Assets/_Scripts/GameSettings.cs` (old singleton)
- Any references to `GameSettings.Instance`

## FMOD Bus Structure
The system expects these FMOD buses:
```
Master (bus:/)
├── Music (bus:/Music)
├── SFX (bus:/SFX)
├── Environment (bus:/Environment)
└── VA (bus:/VA)
```

## Key Features

### Industry Standard Practices
- ✅ ScriptableObject data separation
- ✅ Non-singleton pattern for menu components
- ✅ Event-driven UI updates
- ✅ Persistent storage with PlayerPrefs
- ✅ Validation and error handling

### FMOD Integration
- ✅ Direct bus volume control
- ✅ Individual muting per bus
- ✅ Real-time audio changes
- ✅ Automatic bus initialization

### Developer Friendly
- ✅ Minimal scripts (only 4 total)
- ✅ Reusable components
- ✅ Inspector-configurable
- ✅ Clear separation of concerns

## Usage Examples

### Setting Volume from Code
```csharp
// Get the settings manager
SettingsManager settings = FindObjectOfType<SettingsManager>();

// Update volumes (will automatically apply to FMOD)
settings.UpdateMasterVolume(0.8f);
settings.UpdateMusicVolume(0.6f);
settings.ToggleSFXMute();
```

### Accessing Current Settings
```csharp
SettingsManager settings = FindObjectOfType<SettingsManager>();
GameSettingsData currentSettings = settings.CurrentSettings;

float currentMusicVolume = currentSettings.MusicVolume;
bool isMasterMuted = currentSettings.MasterMuted;
```

### Listening to Settings Changes
```csharp
void Start()
{
    SettingsManager.OnSettingsChanged += OnSettingsUpdated;
}

void OnSettingsUpdated(GameSettingsData settings)
{
    // React to settings changes
    Debug.Log($"New master volume: {settings.MasterVolume}");
}
```

## Migration Notes
- Old `GameSettings.Instance` calls should be replaced with `SettingsManager` references
- The ScriptableObject approach allows for better asset management and version control
- Settings are now stored both in PlayerPrefs (persistence) and ScriptableObject (runtime)
- The system maintains the same PlayerPrefs keys for backward compatibility

## Troubleshooting
- If FMOD buses aren't found, check the bus paths in SettingsManager
- Make sure the GameSettingsData asset is in the Resources folder for auto-loading
- Verify that SettingsManager is present in scenes where you need settings access