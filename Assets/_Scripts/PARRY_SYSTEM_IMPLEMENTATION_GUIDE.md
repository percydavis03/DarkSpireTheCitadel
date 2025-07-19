# Enhanced Parry System Implementation Guide

## Overview
This enhanced parry system uses **animation events** to create precise, frame-specific timing windows that dramatically increase parry difficulty and skill requirements.

## Key Improvements Over Original System
- **Precise Timing**: Instead of a static 0.5s window, enemies have specific vulnerable frames
- **Perfect Parry System**: Frame-perfect timing grants bonus damage and effects
- **Difficulty Scaling**: Different enemy types have different parry window sizes
- **Multi-Phase Attacks**: Support for complex attack patterns with multiple parry opportunities
- **Visual Feedback**: Enhanced feedback for different parry qualities

## Animation Event Setup in Unity

### Step 1: Open Enemy Attack Animations
1. Navigate to `Assets/Animations/Enemy/` (or your enemy animation folder)
2. Select an enemy attack animation (e.g., "SpearAttack", "WorkerSwing")
3. Open the Animation window (Window → Animation → Animation)

### Step 2: Add Animation Events
For each enemy attack animation, add these events at specific frames:

#### Basic Attack Pattern (Single-Phase):
```
Frame 10:  AnimAttackStartup()      // Attack begins, no parry window yet
Frame 25:  AnimParryWindowOpen()    // Parry window opens - vulnerable!
Frame 30:  AnimAttackActive()       // Damage hitbox activates
Frame 35:  AnimParryWindowClose()   // Parry window closes
Frame 50:  AnimAttackRecovery()     // Attack recovery phase
Frame 60:  AnimAttackEnd()          // Attack completely finished
```

#### Advanced Attack Pattern (Multi-Phase):
```
Frame 10:  AnimAttackStartup()      // First phase startup
Frame 20:  AnimParryWindowOpen()    // First parry window
Frame 25:  AnimNextAttackPhase()    // Advance to phase 2
Frame 40:  AnimParryWindowOpen()    // Second parry window
Frame 45:  AnimAttackActive()       // Final damage phase
Frame 55:  AnimAttackRecovery()     // Recovery
Frame 65:  AnimAttackEnd()          // Complete
```

### Step 3: Configure Enemy Difficulty Settings

#### Enemy_Basic Configuration:
```csharp
// In the inspector, set these values:
parryDifficulty = 3;  // 1=Easy, 5=Expert
maxAttackPhases = 1;  // Single-phase attack
difficultyParryWindows = {0.4f, 0.3f, 0.2f, 0.1f, 0.05f};
difficultyPerfectWindows = {0.1f, 0.08f, 0.06f, 0.04f, 0.02f};
```

#### Worker Configuration:
```csharp
// Workers are easier to parry:
workerParryWindow = 0.3f;     // Generous window
workerPerfectWindow = 0.1f;   // Reasonable perfect timing
```

### Step 4: Testing Different Difficulty Levels

#### Easy Enemies (parryDifficulty = 1-2):
- Large parry windows (0.3-0.4s)
- Generous perfect parry timing (0.08-0.1s)
- Clear visual/audio telegraphs

#### Normal Enemies (parryDifficulty = 3):
- Medium parry windows (0.2s)
- Moderate perfect timing (0.06s)
- Standard telegraphs

#### Hard Enemies (parryDifficulty = 4):
- Small parry windows (0.1s)
- Tight perfect timing (0.04s)
- Subtle telegraphs

#### Expert/Boss Enemies (parryDifficulty = 5):
- Tiny parry windows (0.05s)
- Frame-perfect timing (0.02s)
- Minimal telegraphs, requires memorization

## Advanced Implementation Strategies

### 1. Feint Attacks
Create fake parry windows to mislead players:
```
Frame 15:  AnimParryWindowOpen()    // Fake window opens
Frame 18:  AnimParryWindowClose()   // Quickly closes (feint!)
Frame 30:  AnimParryWindowOpen()    // Real parry window
Frame 35:  AnimAttackActive()       // Actual damage
```

### 2. Combo-Dependent Difficulty
```csharp
// In WeaponScript.cs, parry windows get smaller during combos:
float baseWindow = inCombo ? comboParryWindow : normalParryWindow;
```

### 3. Rhythm-Based Parrying
For multiple enemies, create rhythmic attack patterns:
```
Enemy 1: Attack at beats 1, 5, 9
Enemy 2: Attack at beats 3, 7, 11
Result: Player must parry on musical rhythm
```

### 4. Conditional Parry Windows
```csharp
public void AnimConditionalParryWindow()
{
    // Only open parry window if player is in certain state
    if (Player_Movement.instance.isComboing)
    {
        AnimParryWindowOpen();
    }
    else
    {
        // Attack is unparryable if player not in combo
        AnimAttackActive();
    }
}
```

## Visual Feedback Implementation

### 1. Parry Window Indicators
Add visual cues when parry windows open:
```csharp
public void AnimParryWindowOpen()
{
    // Flash enemy weapon or add particle effect
    weaponRenderer.material.color = Color.yellow;
    parryWindowEffect.Play();
    
    // Existing logic...
}
```

### 2. Perfect Parry Feedback
```csharp
private void OnParrySuccess(bool isPerfectParry)
{
    if (isPerfectParry)
    {
        // Golden particle effect, longer slow-mo, special sound
        perfectParryEffect.Play();
        // Screen flash, camera shake, etc.
    }
    else
    {
        // Standard blue effect, brief slow-mo
        normalParryEffect.Play();
    }
}
```

## Debugging and Tuning

### Debug Console Outputs
The system includes extensive debug logging:
- `"Parry window OPENED - Difficulty X"`
- `"PERFECT PARRY WINDOW ACTIVE!"`
- `"Attack Phase X/Y"`

### Testing Checklist
1. **Timing Verification**: Use Debug.Log timestamps to verify window durations
2. **Difficulty Scaling**: Test each difficulty level feels appropriately challenging
3. **Perfect Parry Rate**: Aim for 5-15% perfect parry success rate for skilled players
4. **Multi-Enemy Scenarios**: Test parrying with multiple enemies attacking simultaneously

### Common Issues and Solutions

#### Issue: Parry windows too difficult
**Solution**: Increase `difficultyParryWindows` values or add more visual telegraphs

#### Issue: Perfect parries too easy/hard
**Solution**: Adjust `difficultyPerfectWindows` or timing of `AnimPerfectParryWindow()` calls

#### Issue: Animation events not firing
**Solution**: Ensure animation events are placed on keyframes, not between them

#### Issue: Parry states not resetting
**Solution**: Ensure `ResetParryStates()` is called in all attack end scenarios

## Enemy Naming Convention for Auto-Difficulty
The system automatically adjusts difficulty based on enemy names:
- `Enemy_Elite_Spear` → Hard difficulty
- `Boss_FinalSpear` → Expert difficulty  
- `Worker_Basic` → Easy difficulty
- Default → Normal difficulty

## Conclusion
This animation-event-driven parry system transforms parrying from a simple timing check into a precise, skill-based mechanic that rewards mastery and provides scalable difficulty. The frame-perfect timing windows create moments of high tension and satisfaction when successfully executed. 