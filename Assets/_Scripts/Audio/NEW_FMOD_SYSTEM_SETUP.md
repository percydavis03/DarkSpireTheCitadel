# New FMOD-Only Combat Audio System

## 🎯 **System Overview**

This is a complete rewrite using a proper FMOD parameter-driven architecture. The system provides:

- **Single CombatSoundManager** - Centralized audio control
- **Data-driven approach** - ScriptableObject configs per character
- **Parameter-based FMOD events** - One event handles multiple variations
- **Universal component** - One script works with any character

## 🧱 **1. FMOD Studio Setup**

### **Core Events Structure**
Create these main events in your FMOD project:

```
Events/Combat/
├── Attack
├── Hit  
├── Block
├── Parry
├── Dodge
├── Death
├── Hurt
├── FallDown
├── Footstep
└── Whoosh
```

### **FMOD Parameters for Each Event**
Add these parameters to **every** combat event:

| Parameter Name | Type | Range | Description |
|----------------|------|-------|-------------|
| CharacterType | Continuous | 0-3 | 0=Nyx, 1=Worker, 2=Spear, 3=Boss |
| WeaponType | Continuous | 0-5 | 0=Gauntlet, 1=Kick, 2=Spear, 3=Sword, etc. |
| SurfaceType | Continuous | 0-4 | 0=Flesh, 1=Metal, 2=Armor, 3=Stone, 4=Wood |
| IsSpecial | Continuous | 0-1 | 0=Normal, 1=Critical/Perfect/Special |

### **Example Event Setup: Combat/Attack**
1. Create base event `event:/Combat/Attack`
2. Add 4 parameters (CharacterType, WeaponType, SurfaceType, IsSpecial)
3. Use **Multi Instruments** with **Parameter Conditions**:
   - CharacterType = 0 → Nyx attack sounds
   - CharacterType = 1 → Worker attack sounds  
   - CharacterType = 2 → Spear attack sounds
4. Further subdivide by WeaponType:
   - WeaponType = 0 → Gauntlet sounds
   - WeaponType = 1 → Kick sounds
   - WeaponType = 2 → Spear thrust sounds

### **Audio File Organization**
Organize your audio files by character and action:

```
Assets/SFX/Combat/
├── Nyx/
│   ├── Gauntlet_Attack_01.wav
│   ├── Gauntlet_Attack_02.wav
│   ├── Kick_Attack_01.wav
│   └── Nyx_Hurt_01.wav
├── Worker/
│   ├── Worker_Attack_01.wav
│   ├── Worker_Hurt_01.wav
│   └── Worker_Death_01.wav
└── Spear/
    ├── Spear_Attack_01.wav
    ├── Spear_Thrust_01.wav
    └── Spear_Death_01.wav
```

## 🎮 **2. Unity Setup**

### **Step 1: Create CombatSoundManager**
1. Create an empty GameObject named "CombatSoundManager"
2. Add the `CombatSoundManager` component
3. Assign all FMOD event paths using the EventRef dropdowns:
   - Attack Event: `event:/Combat/Attack`
   - Hit Event: `event:/Combat/Hit`
   - Block Event: `event:/Combat/Block`
   - Parry Event: `event:/Combat/Parry`
   - Dodge Event: `event:/Combat/Dodge`
   - Death Event: `event:/Combat/Death`
   - Hurt Event: `event:/Combat/Hurt`
   - Fall Down Event: `event:/Combat/FallDown`
   - Footstep Event: `event:/Combat/Footstep`
   - Whoosh Event: `event:/Combat/Whoosh`

### **Step 2: Create Character Audio Configs**
1. Right-click in Project → Create → DarkSpire → Audio → Character Audio Config
2. Create configs for each character type:

**Nyx_AudioConfig.asset:**
```
Character Name: Nyx
Character Type: Nyx
Primary Weapon: Gauntlet
Secondary Weapon: Kick
Surface Type: Flesh
Can Critical Hit: ✅
Can Parry: ✅
Can Perfect Dodge: ✅
```

**Worker_AudioConfig.asset:**
```
Character Name: Worker
Character Type: Worker
Primary Weapon: Sword
Secondary Weapon: Shield
Surface Type: Metal
Can Critical Hit: ❌
Can Parry: ❌
Can Perfect Dodge: ❌
```

**Spear_AudioConfig.asset:**
```
Character Name: Spear Enemy
Character Type: Spear
Primary Weapon: Spear
Secondary Weapon: Spear
Surface Type: Metal
Can Critical Hit: ❌
Can Parry: ✅
Can Perfect Dodge: ❌
```

3. Place configs in `Assets/Resources/` folder for auto-loading

### **Step 3: Add Component to Character Prefabs**
1. Add `AnimationAudioEvents` component to each character prefab
2. Assign the appropriate `CharacterAudioConfig`
3. Configure settings:
   - Enable Debug Logs: ✅ (for testing)
   - Use 3D Audio: ✅
   - Enable Sound Cooldowns: ✅

### **Step 4: Set Up Animation Events**
In Unity's Animation window, add these animation events:

**Attack Animations:**
- Frame of impact: `PlayAttack()`
- For heavy attacks: `PlayHeavyAttack()`
- For secondary attacks: `PlaySecondaryAttack()`
- For critical hits: `PlayCriticalAttack()`

**Damage Animations:**
- Start of hurt anim: `PlayHurt()`
- Start of death anim: `PlayDeath()`

**Combat Actions:**
- Block contact: `PlayBlock()`
- Perfect parry: `PlayParry()`
- Dodge start: `PlayDodge()`
- Perfect dodge: `PlayPerfectDodge()`
- Knockdown/fall: `PlayFallDown()`
- Heavy fall: `PlayHeavyFallDown()`

**Movement:**
- Foot contact: `PlayFootstep()`
- Weapon swing start: `PlayWhoosh()`

**Hit Feedback:**
- When attack connects: `PlaySuccessfulHit()`

## 🔧 **3. System Usage**

### **From Animation Events (Recommended)**
```csharp
// Simply call these methods from animation events
PlayAttack();           // Basic attack
PlayHeavyAttack();      // Heavy attack
PlaySecondaryAttack();  // Secondary weapon (kick, etc.)
PlayCriticalAttack();   // Critical hit
PlayHurt();             // Taking damage
PlayDeath();            // Character death
PlayBlock();            // Blocking attack
PlayParry();            // Perfect parry
PlayDodge();            // Dodging
PlayPerfectDodge();     // Perfect dodge
PlayFallDown();         // Knockdown/fall
PlayHeavyFallDown();    // Heavy/brutal fall
PlayFootstep();         // Footstep
PlayWhoosh();           // Weapon whoosh
PlaySuccessfulHit();    // Hit connects with target
```

### **From Code (Advanced)**
```csharp
// Direct access to CombatSoundManager
CombatSoundManager.Instance.PlayAttackSound(
    transform.position,     // 3D position
    "Nyx",                 // Character type
    "Gauntlet",            // Weapon type  
    "Heavy",               // Attack type
    true                   // Is critical
);

CombatSoundManager.Instance.PlayHitSound(
    hitPosition,           // Where the hit occurred
    "Worker",              // What was hit
    "Metal",               // Surface type
    "Gauntlet",            // Weapon that hit
    false                  // Not critical
);

CombatSoundManager.Instance.PlayParrySound(
    transform.position,    // 3D position
    "Nyx",                 // Character parrying
    "Gauntlet",            // Weapon used to parry
    "Sword",               // Weapon being parried
    true                   // Perfect parry
);

CombatSoundManager.Instance.PlayFallDownSound(
    transform.position,    // 3D position
    "Worker",              // Character falling
    "Knockdown",           // Type of fall
    "Metal",               // Surface landed on
    false                  // Not a heavy fall
);
```

## 🎛️ **4. FMOD Parameter Mapping**

The system automatically converts strings to FMOD parameter values:

### **Character Types:**
- "Nyx" → 0
- "Worker" → 1  
- "Spear" → 2
- "Boss" → 3

### **Weapon Types:**
- "Gauntlet", "Fist" → 0
- "Kick" → 1
- "Spear" → 2
- "Sword" → 3
- "Axe" → 4
- "Shield" → 5

### **Surface Types:**
- "Flesh" → 0
- "Metal" → 1
- "Armor" → 2
- "Stone" → 3
- "Wood" → 4

### **Attack/Damage Types:**
- "Light", "Normal" → 0
- "Heavy", "Brutal" → 1
- "Special" → 2

## 🧪 **5. Testing & Debugging**

### **Test Individual Components**
1. Select character prefab with `AnimationAudioEvents`
2. Right-click → Context Menu → "Test Audio"
3. Check console for debug messages

### **Test CombatSoundManager**
1. Select CombatSoundManager GameObject
2. Right-click → Context Menu → "Validate All Events"
3. Check that all events show ✅ in console

### **Debug Settings**
- Enable Debug Logs on `CombatSoundManager`
- Enable Debug Logs on `AnimationAudioEvents`
- Use FMOD Studio Live Update for real-time tweaking

## 📋 **6. Setup Checklist**

- [ ] FMOD events created with correct parameter names
- [ ] Audio files organized and imported to FMOD
- [ ] Multi-instruments set up with parameter conditions
- [ ] Banks built and in Unity StreamingAssets folder
- [ ] CombatSoundManager prefab created and configured
- [ ] Character audio configs created for each character type
- [ ] Configs placed in Resources folder
- [ ] AnimationAudioEvents added to character prefabs
- [ ] Animation events added to character animations
- [ ] System tested with debug logging enabled

## 🚀 **7. Benefits of This System**

✅ **Centralized Control** - One manager handles all combat audio  
✅ **Data-Driven** - Easy to modify without code changes  
✅ **Scalable** - Add new characters by creating configs  
✅ **Flexible** - One FMOD event handles many variations  
✅ **Performance** - Efficient parameter-based playback  
✅ **Designer-Friendly** - Non-programmers can configure audio  
✅ **Maintainable** - Clean separation of concerns  

Your combat audio system is now fully FMOD-driven and ready for production!
