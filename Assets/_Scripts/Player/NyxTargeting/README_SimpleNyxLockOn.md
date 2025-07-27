# Simple Nyx Lock-On System

A lightweight lock-on system for Nyx that allows cycling through enemies in camera view.

## Features

- **Camera-based targeting**: Only enemies visible to the camera can be locked onto
- **Middle mouse button cycling**: Scroll wheel to cycle through targets
- **"None" state**: Cycling past the last enemy turns off lock-on
- **Auto-rotation**: Player automatically rotates towards locked target
- **Visual indicator**: Red sphere appears above locked enemy
- **Lightweight**: Optimized for performance with minimal overhead

## Setup Instructions

1. Add the `SimpleNyxLockOn` component to your Player object
2. Configure the settings in the inspector:
   - **Nyx Transform**: The Nyx character transform (auto-detected if tagged "Nyx")
   - **Player Transform**: The player object that rotates (defaults to this object)
   - **Player Camera**: Main camera (auto-detected if not set)
   - **Lock-On Range**: How far enemies can be detected (default: 25)
   - **Camera Field of View**: Detection angle (default: 60°)
   - **Enemy Tags**: Tags to look for (default: "Enemy", "Boss")

## How It Works

### Cycling Logic
- **Scroll Up**: Next enemy in the list
- **Scroll Down**: Previous enemy in the list
- **Past Last Enemy**: Lock-on turns OFF
- **Before First Enemy**: Lock-on turns OFF

### Example Cycle Order
```
No Lock → Enemy 1 → Enemy 2 → Enemy 3 → No Lock → Enemy 1...
```

### Detection System
- Finds all enemies with specified tags
- Filters by range and camera view
- Sorts by distance (closest first)
- Updates every frame to handle moving enemies

## Configuration Options

### Player Rotation
- **Enable Auto Rotation**: Toggle automatic player rotation
- **Rotation Speed**: How fast the player rotates (default: 5)
- **Smooth Rotation**: Use smooth or instant rotation

### Visual Indicator
- **Lock-On Indicator Prefab**: Custom indicator prefab (optional)
- **Indicator Height Offset**: How high above enemy to place indicator

### Input
- **Player Controls**: PlayerInputActions reference (auto-detected)
- **Use Scroll Wheel**: Toggle between scroll wheel and middle mouse button

## Public Methods

```csharp
// Check if currently locked onto a target
bool IsLockedOn { get; }

// Get the current locked target
Transform CurrentTarget { get; }

// Force unlock (useful for cutscenes, etc.)
void ForceUnlock()

// Enable/disable auto-rotation
void SetRotationEnabled(bool enabled)
```

## Performance Notes

- Updates enemy list every frame but only processes visible enemies
- Uses efficient distance sorting
- Minimal memory allocation
- Debug logs can be disabled in production

## Troubleshooting

### Lock-on not working?
1. Check that enemies have the correct tags ("Enemy", "Boss")
2. Ensure enemies are within the lock-on range
3. Verify the camera field of view setting
4. Make sure PlayerInputActions has a "CycleTargets" action

### Player not rotating?
1. Check "Enable Auto Rotation" is enabled
2. Verify the Player Transform is assigned correctly
3. Increase rotation speed if it seems too slow

### No visual indicator?
1. Check if Lock-On Indicator Prefab is assigned
2. If using default indicator, ensure it's not hidden behind enemy
3. Adjust Indicator Height Offset if needed

## Integration with Existing Systems

This system is designed to work alongside the existing player movement and input systems. It automatically detects PlayerInputActions from the Player_Movement component and uses the "CycleTargets" input action.

The system respects the player's current state and won't interfere with other gameplay mechanics like attacks or menus. 