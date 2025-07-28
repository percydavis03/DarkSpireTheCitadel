# Root Motion Setup Guide for Nyx Roll Animations

This guide explains how to properly set up root motion for Nyx's roll animations to fix the position desynchronization issue.

## Problem Description
When roll animations have root transform movement but transition back to idle without proper root motion handling, the character position gets desynchronized. The idle animation plays at a different position than where the roll animation ended.

## Industry Standard Solution

### 1. Add NyxRootMotionHandler Component

1. In Unity, select the **animationSource** GameObject (the child object with the Animator component)
2. Add the `NyxRootMotionHandler` script component
3. The script will automatically find the required references (CharacterController, Animator, Player_Movement)
4. Configure the settings:
   - **Enable During Roll**: `true`
   - **Enable During Attacks**: `false` (attacks are handled by script)
   - **Enable During Movement**: `false` (movement is handled by script)
   - **Apply Position Y**: `false` (prevents floating)
   - **Apply Rotation**: `true`
   - **Root Motion Multiplier**: `1.0`

### 2. Animation Events Setup

For each roll animation that uses root motion, add animation events:

#### For "nyx stand 2 roll.anim":
1. Open the Animation window (Window → Animation → Animation)
2. Select the Nyx GameObject with the animation
3. Select the "nyx stand 2 roll" animation clip
4. Add events at these timing points:

**Roll Start Event** (at frame 0 or when roll movement begins):
- Function: `OnRollStart`
- Target: NyxRootMotionHandler component

**Roll End Event** (at the frame where roll movement ends, before transitioning to idle):
- Function: `OnRollEnd`
- Target: NyxRootMotionHandler component

#### For other roll animations:
Repeat the same process for:
- "s2r.anim"
- "str2.anim" 
- "nyx roll.anim"
- Any other roll animations

### 3. Animator Controller Setup

Ensure your animator controller has proper transitions:

1. **Roll Entry Transition**:
   - Condition: `isRolling` = `true`
   - Transition Duration: `0.1-0.25` seconds
   - Has Exit Time: `false`
   - Can Transition To Self: `false`

2. **Roll Exit Transition**:
   - Condition: `isRolling` = `false`
   - Transition Duration: `0.1-0.25` seconds
   - Has Exit Time: `true` (use animation events instead)
   - Exit Time: `0.9-1.0` (near end of animation)

### 4. Animation Import Settings

For roll animations with root motion:

1. Select the roll animation FBX files
2. In the Inspector, go to the **Rig** tab
3. Ensure **Animation Type** is set to **Humanoid**
4. Go to the **Animation** tab
5. For each roll animation clip:
   - **Root Transform Rotation**: Based Upon → Body Orientation
   - **Root Transform Position (Y)**: Based Upon → Original
   - **Root Transform Position (XZ)**: Based Upon → Root Node Position
   - Check **Loop Time** if the animation should loop (usually false for rolls)

### 5. Testing the Setup

1. Play the game and trigger a roll
2. Watch the console for debug messages:
   - "Root Motion: Roll Started"
   - "Root Motion: Roll Ended"
   - "Roll ended - returning to normal movement"
3. Verify that the character ends up at the correct position after the roll
4. If issues persist, enable **Show Debug Info** in the NyxRootMotionHandler

### 6. Troubleshooting

**Character still teleports after roll:**
- Check that animation events are properly placed
- Verify that the animation has root motion in its curves
- Ensure the Animator has `Apply Root Motion` enabled

**Character doesn't move during roll:**
- Check that `Enable During Roll` is true in NyxRootMotionHandler
- Verify the animation has MotionT.x and MotionT.z curves
- Check the Root Motion Multiplier value

**Roll feels too fast/slow:**
- Adjust the **Root Motion Multiplier** in NyxRootMotionHandler
- Modify the animation speed in the Animator Controller

**Character floats during roll:**
- Ensure `Apply Position Y` is set to `false`
- Check that gravity is still being applied by the CharacterController

## Technical Details

The solution works by:

1. **OnAnimatorMove()** captures root motion deltas from the Animator
2. **State-based filtering** only applies root motion during appropriate animations
3. **CharacterController integration** ensures collisions are respected
4. **Animation events** provide precise timing for state transitions
5. **Fallback support** maintains the existing script-based roll system

This approach follows Unity's recommended practices for handling root motion in character controllers and provides smooth, collision-aware movement during animations. 