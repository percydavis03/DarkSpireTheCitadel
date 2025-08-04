# Nyx Targeting System

A shared targeting system for Nyx that provides both lock-on functionality and improved grapple targeting, separating the raycast and cone detection logic from specific systems for better performance and reusability.

## Overview

The targeting system consists of several components working together:

1. **NyxTargetingSystem** - Core targeting logic with raycast and cone detection
2. **NyxLockOnSystem** - Enemy lock-on functionality  
3. **ITargetable Interface** - Defines objects that can be targeted
4. **TargetableEnemy** - Implementation for enemy objects
5. **TargetableGrappleable** - Implementation for grappleable objects
6. **NyxGrapple** - Updated to use shared targeting system

## Features

- **Shared Detection Logic**: Both grapple and lock-on use the same efficient targeting system
- **Flexible Target Types**: Supports enemies, grappleable objects, bosses, etc.
- **Visual Feedback**: Highlights targeted objects and shows UI indicators
- **Performance Optimized**: Configurable update frequency and target limits
- **Backwards Compatible**: Existing grapple system continues to work with fallback

## Quick Setup

### Automatic Setup (Recommended)

1. Open Unity Editor
2. Go to `Tools → Nyx Targeting → Setup Targeting System`
3. Click "Add Targeting Systems to Nyx Child Object" (for child object setup)
4. Click "Setup Complete Targeting System" (adds components to all enemies/objects)
5. Ready to use! RB/LB and scroll wheel controls work immediately

### Manual Setup

1. **Add Core Components to Nyx Child Object:**
   ```
   - NyxTargetingSystem (on Nyx child, not player root)
   - NyxLockOnSystem (on Nyx child, not player root)
   - Ensure Nyx child has "Nyx" tag
   - Parent should have Player_Movement component
   ```

2. **Add Targetable Components to Enemies:**
   ```
   - TargetableEnemy (to Enemy_Basic, Worker, Boss objects)
   ```

3. **Add Targetable Components to Grappleable Objects:**
   ```
   - TargetableGrappleable (to objects with Grappleable component)
   ```

## Input Setup

The targeting system has been set up with **RB/LB on controller** and **E/R keys on keyboard** for easy target cycling!

### ✅ Already Set Up For You!

The `CycleTargets` input action has been added to your `PlayerInputActions` with these bindings:
- **Controller**: LB (previous target) / RB (next target)  
- **Keyboard**: E (previous target) / R (next target)
- **Auto Lock-On**: Automatically locks onto enemies when you start cycling

### How It Works

1. **Press E/R or RB/LB** - Automatically locks onto the nearest enemy and starts cycling
2. **Continue pressing** - Cycles through all available targets
3. **Stop input** - Maintains lock on current target  
4. **Move away or target dies** - Automatically releases lock-on

### Optional: Lock-On Toggle

If you want a dedicated lock-on toggle button:
1. Add a `LockOn` action to your `PlayerInputActions`
2. Enable "Enable Lock On Toggle" in `NyxLockOnSystem` inspector
3. The system will use both cycling and toggle functionality

## Configuration

### NyxTargetingSystem Settings

- **Targeting Range**: How far to scan for targets (15f recommended)
- **Targeting Angle**: Cone angle for detection (45° recommended)
- **Update Frequency**: How often to scan (0.1f = 10 times per second)
- **Obstacle Layer Mask**: Which layers block line of sight

### NyxLockOnSystem Settings

- **Lock-On Range**: Maximum distance for lock-on (20f recommended)
- **Lock-On Angle**: Cone angle for lock-on (60° recommended)
- **Camera Settings**: Automatic camera adjustment when locked on

## Usage

### Lock-On Controls
- **RB/LB on Controller**: Cycle through targets (auto-locks when you start cycling)
- **E/R Keys on Keyboard**: Cycle through targets (E=previous, R=next)
- **Auto-Release**: Lock-on automatically releases if target dies or moves too far
- **Visual Feedback**: Targets highlight when locked on (red for enemies, green/blue for objects)

### Grapple Integration
- The grapple system automatically uses the targeting system for improved detection
- Falls back to original detection if targeting system is unavailable
- Provides same functionality with better performance

## Target Priority System

Targets are prioritized as follows:
1. **Boss** (Priority 10)
2. **Enemy_Basic** (Priority 5)  
3. **Enemy Grappleable** (Priority 4)
4. **Worker** (Priority 3)
5. **Pullable Objects** (Priority 2)
6. **Interactive Objects** (Priority 1)

## Visual Feedback

### Enemy Targets
- **Red tint** when targeted by lock-on system
- **Material brightness** increase for visibility
- **Optional target indicator** UI element

### Grappleable Targets  
- **Green tint** for pullable objects
- **Blue tint** for interactive objects
- **Consistent highlighting** across systems

## Performance Notes

- Targeting system updates at configurable frequency (default 0.1s)
- Maximum targets processed per frame is limited (default 10)
- Shared computation between grapple and lock-on systems
- Efficient caching of target data

## Troubleshooting

### No Targets Detected
1. Ensure objects have TargetableEnemy or TargetableGrappleable components
2. Check that targets are within range and angle
3. Verify line of sight isn't blocked by obstacles
4. Check target types are allowed in targeting system configuration

### Lock-On Not Working  
1. **For Child Object Setup**: Ensure components are on Nyx child, not player root
2. Check that Nyx child object has "Nyx" tag  
3. Verify Player_Movement component exists on parent object
4. Check console for "Found PlayerInputActions on parent Player_Movement" message
5. Ensure lock-on is enabled in NyxLockOnSystem settings
6. Verify enemies have TargetableEnemy components
7. Try the "Add Targeting Systems to Nyx Child Object" button in Tools menu

### Grapple Issues
1. Check NyxGrapple has reference to NyxTargetingSystem  
2. Verify grappleable objects have TargetableGrappleable components
3. Fallback detection should still work if targeting system missing

## Extending the System

### Custom Target Types
Add new target types to the `TargetType` enum and implement `ITargetable`:

```csharp
public enum TargetType 
{
    Enemy,
    Grappleable, 
    Interactive,
    Destructible,
    Boss,
    YourCustomType  // Add here
}
```

### Custom Targeting Behavior
Implement `ITargetable` interface for custom targeting behavior:

```csharp
public class CustomTargetable : MonoBehaviour, ITargetable 
{
    // Implement required properties and methods
}
```

## Performance Impact

The new system should provide **better performance** than the original because:
- Single targeting scan shared between multiple systems
- Configurable update frequency reduces CPU overhead  
- Early rejection of invalid targets
- Cached target data prevents redundant calculations

## Migration Notes

- Existing grapple functionality is preserved
- No breaking changes to existing Grappleable objects
- Targeting components can be added incrementally
- System gracefully degrades if components are missing 