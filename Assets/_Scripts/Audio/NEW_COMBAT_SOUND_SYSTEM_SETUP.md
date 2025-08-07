# New FMOD Combat Sound System Setup Guide

## 🎯 **System Overview**

This new combat sound system uses FMOD with parameter-driven audio. Each object can be assigned:
- **CharacterType** (0-3): 0=Nyx, 1=Worker, 2=Spear, 3=Boss

## 🧱 **1. FMOD Studio Setup**

### **Core Events Structure**
Create these events in your FMOD project:

```
Events/Combat/
├── Hit
├── Hurt
├── Knocked
├── Fall
├── Kicked
├── Woosh
├── Dead
├── Stunned
├── Slash1
├── Slash2
├── GauntletAttack
├── SpinAttack
├── Parry
└── Roll
```

### **FMOD Parameters for Each Event**
Add this parameter to **every** combat event:

| Parameter Name | Type | Range | Description |
|----------------|------|-------|-------------|
| CharacterType | Continuous | 0-3 | 0=Nyx, 1=Worker, 2=Spear, 3=Boss |

### **Example Event Setup: Combat/Hit**
1. Create base event `event:/Combat/Hit`
2. Add 1 parameter (CharacterType)
3. Use **Multi Instruments** with **Parameter Conditions**:
   - CharacterType = 0 → Nyx hit sounds
   - CharacterType = 1 → Worker hit sounds  
   - CharacterType = 2 → Spear hit sounds
   - CharacterType = 3 → Boss hit sounds

### **Audio File Organization**
Organize your audio files by character and action:

```
Assets/SFX/Combat/
├── Nyx/
│   ├── Hit_01.wav
│   ├── Hurt_01.wav
│   ├── Woosh_01.wav
│   ├── Slash1_01.wav
│   ├── Slash2_01.wav
│   ├── GauntletAttack_01.wav
│   ├── SpinAttack_01.wav
│   ├── Parry_01.wav
│   └── Roll_01.wav
├── Worker/
│   ├── Hit_01.wav
│   ├── Hurt_01.wav
│   ├── Knocked_01.wav
│   ├── Fall_01.wav
│   ├── Kicked_01.wav
│   ├── Woosh_01.wav
│   ├── Dead_01.wav
│   └── Stunned_01.wav
├── Spear/
│   ├── Hit_01.wav
│   ├── Hurt_01.wav
│   ├── Knocked_01.wav
│   ├── Fall_01.wav
│   ├── Kicked_01.wav
│   ├── Woosh_01.wav
│   ├── Dead_01.wav
│   └── Stunned_01.wav
└── Boss/
    ├── Hit_01.wav
    ├── Hurt_01.wav
    ├── Knocked_01.wav
    ├── Fall_01.wav
    ├── Kicked_01.wav
    ├── Woosh_01.wav
    ├── Dead_01.wav
    └── Stunned_01.wav
```

## 🎮 **2. Unity Setup**

### **Step 1: Create CombatSoundManager**
1. Create an empty GameObject named "CombatSoundManager"
2. Add the `CombatSoundManager` component
3. Assign all FMOD event paths using the EventRef dropdowns:

**Basic Enemy Events:**
- Hit Event: `event:/Combat/Hit`
- Hurt Event: `event:/Combat/Hurt`
- Knocked Event: `event:/Combat/Knocked`
- Fall Event: `event:/Combat/Fall`
- Kicked Event: `event:/Combat/Kicked`
- Woosh Event: `event:/Combat/Woosh`
- Dead Event: `event:/Combat/Dead`
- Stunned Event: `event:/Combat/Stunned`

**Nyx-Specific Events:**
- Slash1 Event: `event:/Combat/Slash1`
- Slash2 Event: `event:/Combat/Slash2`
- GauntletAttack Event: `event:/Combat/GauntletAttack`
- SpinAttack Event: `event:/Combat/SpinAttack`
- Parry Event: `event:/Combat/Parry`
- Roll Event: `event:/Combat/Roll`

### **Step 2: Add CombatAudioComponent to Objects**
1. Select any GameObject that needs combat sounds
2. Add the `CombatAudioComponent` component
3. Configure the parameters:

**For Nyx Character:**
```
Character Type: 0 (Nyx)
```

**For Worker Enemy:**
```
Character Type: 1 (Worker)
```

**For Spear Enemy:**
```
Character Type: 2 (Spear)
```

**For Boss:**
```
Character Type: 3 (Boss)
```

## 🎛️ **3. Usage Examples**

### **Basic Enemy Usage**
```csharp
// Get the combat audio component
CombatAudioComponent audio = GetComponent<CombatAudioComponent>();

// Play basic enemy sounds
audio.PlayHitConnected();  // When attack hits
audio.PlayHurt();          // When taking damage
audio.PlayKnocked();       // When knocked down
audio.PlayFall();          // When falling
audio.PlayKicked();        // When kicked
audio.PlayWoosh();         // When weapon swings
audio.PlayDead();          // When dying
audio.PlayStunned();       // When stunned
```

### **Nyx-Specific Usage**
```csharp
// Get the combat audio component (must have characterType = 0)
CombatAudioComponent audio = GetComponent<CombatAudioComponent>();

// Play Nyx-specific sounds
audio.PlayNyxWoosh();           // Weapon woosh
audio.PlayNyxSlash1();          // First slash
audio.PlayNyxSlash2();          // Second slash
audio.PlayNyxGauntletAttack();  // Gauntlet attack
audio.PlayNyxSpinAttack();      // Spin attack
audio.PlayNyxHurt();            // When hurt
audio.PlayNyxParry();           // When parrying
audio.PlayNyxRoll();            // When rolling
```

### **Animation Event Integration**
1. In your animation clips, add animation events
2. Set the function to call the appropriate sound method
3. Example: `PlayHitConnected()` for attack hit events

## 🧪 **4. Testing & Debugging**

### **Test Individual Components**
1. Select GameObject with `CombatAudioComponent`
2. Right-click → Context Menu → "Test Basic Sounds" or "Test Nyx Sounds"
3. Check console for debug messages

### **Test CombatSoundManager**
1. Select CombatSoundManager GameObject
2. Enable "Enable Debug Logs" in inspector
3. Use test methods in code or context menu

### **FMOD Studio Testing**
1. Open FMOD Studio
2. Test events with different parameter values
3. Verify parameter ranges and audio routing

## 📋 **5. Parameter Reference**

### **Character Types:**
- 0 = Nyx
- 1 = Worker
- 2 = Spear
- 3 = Boss



## 🔧 **6. Advanced Configuration**

### **Sound Cooldowns**
- Set `Sound Cooldown` in `CombatAudioComponent` to prevent sound spam
- Default is 0.1 seconds between sounds

### **3D Audio**
- Enable `Use 3D Audio` for positional sound
- Disable for 2D audio (UI sounds, etc.)

### **Debug Logging**
- Enable `Enable Debug Logs` to see sound calls in console
- Useful for troubleshooting and testing

## 🚀 **7. Migration from Old System**

### **Replace Old Components**
1. Remove old `AnimationAudioEvents` components
2. Remove old `CharacterAudioConfig` ScriptableObjects
3. Add `CombatAudioComponent` to objects
4. Configure character/weapon/surface types

### **Update Animation Events**
1. Change animation event function calls
2. Use new method names:
   - `PlayHitConnected()` instead of `PlaySuccessfulHit()`
   - `PlayHurt()` instead of `PlayHurt()`
   - `PlayKnocked()` instead of `PlayKnockedDown()`

### **Update Code References**
1. Replace `CombatSoundManager.Instance.PlayAttackSound()` calls
2. Use new parameter-based methods
3. Update any custom audio scripts

## ✅ **8. Checklist**

- [ ] FMOD events created with correct parameters
- [ ] Audio files organized and assigned
- [ ] CombatSoundManager GameObject created
- [ ] Event references assigned in CombatSoundManager
- [ ] CombatAudioComponent added to character objects
- [ ] Character/Weapon/Surface types configured
- [ ] Animation events updated
- [ ] Code references updated
- [ ] Testing completed
- [ ] Debug logging verified

## 🆘 **9. Troubleshooting**

### **No Sound Playing**
1. Check FMOD event references are assigned
2. Verify FMOD banks are built and loaded
3. Check audio device settings
4. Enable debug logs to see error messages

### **Wrong Sounds Playing**
1. Verify parameter values are correct
2. Check FMOD event parameter conditions
3. Test individual events in FMOD Studio

### **Performance Issues**
1. Reduce sound cooldown values
2. Check for multiple audio components
3. Verify FMOD memory usage

This new system provides a clean, parameter-driven approach to combat audio using FMOD, making it easy to assign different character types, weapons, and surfaces to any object in your game.
