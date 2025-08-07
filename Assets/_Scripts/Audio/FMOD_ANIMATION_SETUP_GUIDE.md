# FMOD Animation Audio System Setup Guide

## Overview
This guide explains how to set up the animation audio system with FMOD for DarkSpire. The system uses character-specific scripts that can play sounds through both FMOD and Unity AudioSource as fallback.

## Prerequisites
✅ FMOD Unity Integration installed  
✅ FMOD Studio project (`FMOD_System/DarkspireAudio/DarkspireAudio.fspro`)  
✅ Banks built and located in `Assets/StreamingAssets/`  
✅ CombatSounds ScriptableObject configured  

## Current FMOD Bank Structure
Your project has the following banks:
- **SFX.bank** - Combat and action sounds
- **Music.bank** - Background music
- **Environment.bank** - Environmental sounds  
- **VA.bank** - Voice acting
- **Ambience.bank** - Ambient sounds
- **Master.bank** - Master bank (always required)

## Step 1: FMOD Studio Setup

### 1.1 Create Combat Events in FMOD Studio
Open `FMOD_System/DarkspireAudio/DarkspireAudio.fspro` and create events in the **SFX bank**:

```
Events/Combat/Nyx/
├── Gauntlet_Attack
├── Kick_Attack  
├── Hurt
├── Parry_Success
├── Knocked_Down
└── Hit_Landed

Events/Combat/Worker/
├── Attack
├── Damaged
├── Death
├── Block
└── Knocked_Down

Events/Combat/Spear/
├── Attack
├── Thrust
├── Damaged
├── Death
├── Block
├── Ground_Hit
└── Knocked_Down

Events/Combat/General/
├── Hit_Impact
├── Metal_Clang
├── Weapon_Whoosh
├── Body_Fall
├── Footsteps
├── Critical_Hit
├── Combo_Hit
└── Perfect_Dodge
```

### 1.2 Event Path Naming Convention
Use this naming pattern for consistency:
```
event:/Combat/[Character]/[Action]
```

Examples:
- `event:/Combat/Nyx/Gauntlet_Attack`
- `event:/Combat/Worker/Attack`  
- `event:/Combat/Spear/Thrust`
- `event:/Combat/General/Hit_Impact`

### 1.3 Configure Events for 3D Audio
For positional combat sounds:
1. Set event to **3D** mode
2. Add **3D Spatializer** effect
3. Configure **Min/Max Distance** (recommended: Min=1, Max=20)
4. Set **Rolloff** to **Linear** or **Inverse**

### 1.4 Add Audio Files
1. Drag your combat audio files into the **Assets** folder in FMOD Studio
2. Create **Multi Instruments** for sound variations
3. Use **Random** or **Sequential** playback for variety

### 1.5 Build Banks
1. Go to **File > Build All**
2. Banks will be built to `FMOD_System/DarkspireAudio/Build/Desktop/`
3. Unity integration automatically copies them to `Assets/StreamingAssets/`

## Step 2: Unity CombatSounds Setup

### 2.1 Configure FMOD Event Paths
In your `CombatSounds.asset`, add the FMOD event paths:

```csharp
// Nyx Sounds
fmodGauntletAttack = "event:/Combat/Nyx/Gauntlet_Attack";
fmodKickSound = "event:/Combat/Nyx/Kick_Attack";
fmodNyxHurt = "event:/Combat/Nyx/Hurt";
fmodParrySuccess = "event:/Combat/Nyx/Parry_Success";

// Worker Sounds  
fmodWorkerAttack = "event:/Combat/Worker/Attack";
fmodWorkerDamaged = "event:/Combat/Worker/Damaged";
fmodWorkerDeath = "event:/Combat/Worker/Death";

// Spear Sounds
fmodSpearAttack = "event:/Combat/Spear/Attack";
fmodSpearDamaged = "event:/Combat/Spear/Damaged";
fmodSpearDeath = "event:/Combat/Spear/Death";

// General Sounds
fmodOnHit = "event:/Combat/General/Hit_Impact";
fmodMetalClang = "event:/Combat/General/Metal_Clang";
fmodWeaponWhoosh = "event:/Combat/General/Weapon_Whoosh";
```

### 2.2 Place CombatSounds in Resources
1. Place your `CombatSounds.asset` in `Assets/Resources/`
2. Name it exactly `CombatSounds.asset`
3. The animation scripts will auto-load it

## Step 3: Character Prefab Setup

### 3.1 Add Animation Audio Component

For **Nyx Character**:
1. Add `NyxAnimationAudio` component to the Nyx prefab
2. Assign `CombatSounds` asset (optional - auto-loads from Resources)
3. Configure settings:
   - ✅ Prefer FMOD = true
   - ✅ Use 3D Audio = true  
   - Volume Multiplier = 1.0
   - Pitch Variation = 0.1

For **Worker Enemy**:
1. Add `WorkerAnimationAudio` component
2. Same configuration as above

For **Spear Enemy**:
1. Add `SpearAnimationAudio` component  
2. Same configuration as above

### 3.2 AudioSource Configuration (Fallback)
Each script auto-creates an AudioSource component with these settings:
- **Spatial Blend**: 1.0 (3D)
- **Volume Rolloff**: Linear
- **Min Distance**: 1
- **Max Distance**: 500
- **Doppler Level**: 1

## Step 4: Animation Events Setup

### 4.1 Add Animation Events in Unity
1. Open your character's **Animator** window
2. Select the **Animation** tab
3. Find the frame where you want sound to play
4. Right-click timeline → **Add Animation Event**
5. Select the appropriate method:

**Nyx Methods:**
- `PlayGauntletAttack()`
- `PlayKick()`
- `PlayHurt()`
- `PlayParrySuccess()`
- `PlayKnockedDown()`
- `PlayHitLanded()`
- `PlayWeaponWhoosh()`
- `PlayFootsteps()`

**Worker Methods:**
- `PlayAttack()`
- `PlayDamaged()`
- `PlayDeath()`
- `PlayBlock()`
- `PlayKnockedDown()`
- `PlayGeneralHit()`
- `PlayMetalClang()`

**Spear Methods:**
- `PlayAttack()`
- `PlayThrust()`
- `PlayDamaged()`
- `PlayDeath()`
- `PlayBlock()`
- `PlayGroundHit()`

### 4.2 Common Animation Event Timing
- **Attack Sounds**: Frame of impact contact
- **Whoosh Sounds**: Start of swing motion
- **Footsteps**: Foot contact with ground
- **Hurt Sounds**: Moment of damage taken
- **Death Sounds**: Start of death animation

## Step 5: Testing and Debugging

### 5.1 Enable Debug Logs
```csharp
// In inspector or code
nyxAudio.SetDebugLogs(true);
```

### 5.2 Test Audio in Editor
1. Use **Context Menu** → **Test [Character] Audio**
2. Check Console for debug messages
3. Verify FMOD events are found

### 5.3 FMOD Live Update
1. Enable **Live Update** in FMOD Studio
2. Connect to Unity while playing
3. Adjust audio parameters in real-time

### 5.4 Verify Bank Loading
Check that banks are loaded properly:
```csharp
// In console or script
FMODUnity.RuntimeManager.HasBankLoaded("SFX");
```

## Step 6: Bus Routing and Mixing

### 6.1 Audio Bus Structure
Your buses are already configured:
```
Master Bus (bus:/)
├── SFX (bus:/SFX)           ← Combat sounds go here
├── Music (bus:/Music)
├── Environment (bus:/Environment)  
├── VA (bus:/VA)             ← Character voices
└── Ambience (bus:/Ambience)
```

### 6.2 Route Combat Events
In FMOD Studio:
1. Select combat events
2. Route to **SFX** bus
3. Combat sounds will respect SFX volume settings

## Step 7: Performance Optimization

### 7.1 FMOD Settings
- **Max Virtual Voices**: 512
- **Max Real Voices**: 128  
- **Sample Rate**: 48000 Hz
- **Speaker Mode**: Stereo or 5.1

### 7.2 Audio Loading
- **Load Banks**: At scene start
- **Preload Sample Data**: For frequently used sounds
- **Stream From Disk**: For long music tracks

### 7.3 Memory Management
- Use **Compressed** formats (Vorbis/FADPCM)
- Set appropriate **Quality** settings
- Enable **Loop Points** for looping sounds

## Troubleshooting

### Common Issues:

**FMOD Events Not Playing:**
1. Check event path spelling
2. Verify bank is loaded
3. Check Unity Console for FMOD errors
4. Test event in FMOD Studio first

**Fallback to AudioSource:**
1. Normal behavior when FMOD events are missing
2. Check CombatSounds ScriptableObject paths
3. Verify banks in StreamingAssets folder

**No Audio on Build:**
1. Ensure banks are in StreamingAssets
2. Check FMOD platform settings
3. Verify InitSettings are correct

**3D Audio Not Working:**
1. Check AudioListener on Camera
2. Verify character AudioSource is 3D
3. Check FMOD event 3D settings

## Best Practices

1. **Test Early**: Set up audio pipeline early in development
2. **Consistent Naming**: Use clear, consistent event names
3. **Version Control**: Include .fspro and banks in version control
4. **Fallback Strategy**: Always provide Unity AudioClip fallbacks
5. **Performance Monitoring**: Monitor voice count and memory usage
6. **Iterative Tuning**: Use Live Update for real-time audio tuning

## Quick Setup Checklist

- [ ] FMOD Studio project open
- [ ] Combat events created in SFX bank
- [ ] Events configured for 3D audio
- [ ] Banks built to StreamingAssets
- [ ] CombatSounds ScriptableObject updated with FMOD paths
- [ ] Animation audio components added to prefabs
- [ ] Animation events added to character animations
- [ ] Audio tested in play mode
- [ ] FMOD buses properly configured
- [ ] Performance settings optimized

Your animation audio system is now ready! The scripts will automatically prefer FMOD when available and fallback to Unity AudioSource when needed, providing a robust and flexible audio solution for your game.
