# Quick Setup Instructions for Simple Lock-On System

## The Problem
Your console shows multiple lock-on systems running at the same time, which is causing conflicts. The main issues are:

1. **Missing "Boss" tag** causing UnityException
2. **Multiple Event Systems** in the scene
3. **Old lock-on systems** still running alongside the new one
4. **Lock-on indicator not appearing**

## Quick Fix (3 Steps)

### Step 1: Add the Setup Script
1. Create an empty GameObject in your scene
2. Name it "SimpleLockOnSetup" 
3. Add the `SimpleLockOnSetup` component to it
4. The script will automatically run and fix most issues

### Step 2: Use the Unity Menu (Alternative)
- Go to **Tools > Simple Lock-On > Setup System** in the Unity menu bar
- This will automatically disable old systems and configure the new one

### Step 3: Test the System
- Press **T** key to test lock-on cycling
- Press **Y** key to force unlock
- Use **middle mouse scroll wheel** for normal operation

## What the Setup Does

✅ **Disables old systems** (NyxLockOnSystem, NyxTargetingSystem)  
✅ **Adds SimpleNyxLockOn** to your Player object  
✅ **Auto-finds Nyx transform** (looks for "Nyx" tag)  
✅ **Auto-finds camera** (uses Camera.main)  
✅ **Handles missing tags** (removes "Boss" tag requirement)  
✅ **Creates visible indicator** (bright red glowing sphere)  
✅ **Filters dead enemies** (checks HP and death states automatically)  

## Manual Setup (if automatic fails)

### 1. Disable Old Systems
Find these components in your scene and **disable** them:
- `NyxLockOnSystem`
- `NyxTargetingSystem` 
- `ManualLockOnTest`

### 2. Add New System
- Find your Player object (the one with `Player_Movement`)
- Add the `SimpleNyxLockOn` component to it

### 3. Configure References
In the SimpleNyxLockOn component:
- **Nyx Transform**: Drag your Nyx character
- **Player Transform**: Should auto-fill (the Player object itself)
- **Player Camera**: Drag your main camera

### 4. Tag Your Objects
- Tag your Nyx character with **"Nyx"**
- Tag your enemies with **"Enemy"**
- Remove any references to "Boss" tag if you don't have it

## Troubleshooting

### "No enemies in view"
- Check that enemies are tagged with "Enemy"
- Make sure enemies are within Lock-On Range (default: 25 units)
- Verify Camera Field of View covers the enemies (default: 60°)

### "Indicator not visible"
- The new system creates a bright red glowing sphere
- Check the indicator Height Offset (default: 2)
- Look for a red sphere above locked enemies

### "Input not working"
- Make sure PlayerInputActions has a "CycleTargets" action
- Check that it's mapped to middle mouse scroll wheel
- Try the test keys (T and Y) first

### "Multiple Event Systems"
- Look for multiple EventSystem objects in your scene
- Delete extra ones (keep only one)

### "Dead enemies still targetable"
- The system automatically filters out dead enemies
- Checks: HP <= 0, "dead" boolean flags, disabled colliders, "isDead" animator parameters
- If still targeting dead enemies, enable debug logs to see filtering messages

## Success Indicators

When working correctly, you should see:
```
✓ Found Nyx object: [YourNyxName]
✓ Found camera: [YourCameraName]  
✓ Found X enemies with 'Enemy' tag
[SimpleNyxLockOn] Enemies in view: 2 [Enemy1, Enemy2], Locked target: Enemy1, Index: 0, Indicator active: true
```

## Support

If you still have issues:
1. Check the Console for `[SimpleNyxLockOn]` messages
2. Enable "Show Debug Info" on the component
3. Use the test keys (T/Y) to verify the system responds
4. Make sure only ONE lock-on system is enabled at a time 