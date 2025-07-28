/*
 * INDUSTRY-STANDARD KNOCKBACK SYSTEM
 * Setup and Usage Guide
 * 
 * This system provides professional-grade knockback that:
 * ✅ Preserves Y position (no vertical movement)
 * ✅ Preserves X/Z rotations (no tilting)
 * ✅ Compatible with NavMesh, CharacterController, and Rigidbody
 * ✅ Data-driven configuration
 * ✅ Event-based architecture
 * ✅ Easy to modify and extend
 * ✅ Does NOT use any scripts from Sam folder
 * 
 * SETUP INSTRUCTIONS:
 * ===================
 * 
 * STEP 1: Create Knockback Data Assets
 * ------------------------------------
 * 1. Right-click in Project window
 * 2. Create > Combat > Knockback Data
 * 3. Create these common types:
 *    - "Light Knockback" (distance: 2, duration: 0.3)
 *    - "Heavy Knockback" (distance: 4, duration: 0.5)
 *    - "Grapple Release" (distance: 3, duration: 0.4, recovery: 0.3)
 * 
 * STEP 2: Setup KnockbackManager
 * ------------------------------
 * 1. Add KnockbackManager script to a GameObject in your scene
 *    (It will auto-create if none exists)
 * 2. Assign your default knockback data asset
 * 3. Enable debug options if needed
 * 
 * STEP 3: Add KnockbackReceiver to Entities
 * -----------------------------------------
 * 1. Add KnockbackReceiver component to enemies/objects
 * 2. Configure settings:
 *    - Mass: Controls knockback distance (higher = less knockback)
 *    - HasNavMeshAgent: Check if using NavMesh
 *    - Animator: Assign for animation integration
 * 3. Set animation parameters if using:
 *    - "isKnockedBack" (bool)
 *    - "knockbackTrigger" (trigger)
 * 
 * STEP 4: Integrate with Existing Systems
 * ---------------------------------------
 * See KnockbackIntegrationExamples.cs for detailed examples
 * 
 * USAGE EXAMPLES:
 * ===============
 * 
 * Basic Usage:
 * -----------
 * KnockbackManager.Instance.ApplyKnockback(
 *     targetGameObject,      // What to knockback
 *     attackerPosition,      // Where attack came from
 *     knockbackDataAsset,    // Configuration (optional)
 *     multiplier            // Force multiplier (optional)
 * );
 * 
 * Advanced Usage:
 * --------------
 * // With direction override
 * Vector3 customDirection = Vector3.right;
 * KnockbackManager.Instance.ApplyKnockback(
 *     target, sourcePos, knockbackData, 1.5f, customDirection
 * );
 * 
 * // Using events
 * KnockbackEvents.OnKnockbackStarted += (receiver) => {
 *     // Handle knockback start
 * };
 * 
 * INTEGRATION WITH EXISTING SYSTEMS:
 * ==================================
 * 
 * For Enemy_Basic.cs:
 * ------------------
 * // Replace your existing GetKnockedBack method with:
 * public void GetKnockedBack(Vector3 sourcePosition, float force = 1f)
 * {
 *     KnockbackManager.Instance.ApplyKnockback(
 *         this.gameObject, 
 *         sourcePosition, 
 *         null, // Uses default data
 *         force
 *     );
 * }
 * 
 * For Weapon Systems:
 * ------------------
 * // In weapon collision/trigger:
 * private void OnTriggerEnter(Collider other)
 * {
 *     if (other.CompareTag("Enemy"))
 *     {
 *         // Apply knockback
 *         KnockbackManager.Instance.ApplyKnockback(
 *             other.gameObject,
 *             transform.position,
 *             weaponKnockbackData
 *         );
 *         
 *         // Your existing damage logic
 *         var enemy = other.GetComponent<Enemy_Basic>();
 *         enemy?.TakeDamage();
 *     }
 * }
 * 
 * For Grapple System Enhancement:
 * ------------------------------
 * // Add this to NyxGrapple.cs StopGrappling method:
 * if (currentTarget != null)
 * {
 *     // Enhanced grapple release
 *     Vector3 releaseDirection = (currentTarget.transform.position - grappleOrigin.position).normalized;
 *     KnockbackManager.Instance.ApplyKnockback(
 *         currentTarget.gameObject,
 *         grappleOrigin.position,
 *         grappleReleaseKnockbackData,
 *         1.5f, // Extra force for releases
 *         releaseDirection
 *     );
 * }
 * 
 * ANIMATION INTEGRATION:
 * =====================
 * 
 * Animator Parameters (optional):
 * - "isKnockedBack" (Bool): True during knockback
 * - "knockbackTrigger" (Trigger): Fires when knockback starts
 * 
 * Create animation states:
 * - Normal State -> Knockback State (when isKnockedBack = true)
 * - Knockback State -> Normal State (when isKnockedBack = false)
 * 
 * CONFIGURATION TIPS:
 * ==================
 * 
 * Knockback Data Settings:
 * - Light attacks: 1-2 units, 0.2-0.3 duration
 * - Heavy attacks: 3-5 units, 0.4-0.6 duration
 * - Special attacks: 4-8 units, 0.5-0.8 duration
 * 
 * Mass Settings:
 * - Small enemies: 0.5-1.0
 * - Normal enemies: 1.0-2.0
 * - Large enemies: 2.0-5.0
 * - Bosses: 5.0-10.0
 * 
 * Animation Curves:
 * - Fast start, slow end: EaseOut
 * - Slow start, fast end: EaseIn
 * - Bouncy: Custom curve with overshoot
 * 
 * TROUBLESHOOTING:
 * ===============
 * 
 * Issue: Knockback not working
 * Solution: Ensure target has KnockbackReceiver component
 * 
 * Issue: NavMesh issues after knockback
 * Solution: Enable "Warp NavMesh After Knockback" in KnockbackReceiver
 * 
 * Issue: Y position changing
 * Solution: System forces Y preservation - check for other scripts modifying position
 * 
 * Issue: Entity tilting/rotating
 * Solution: System preserves rotations - check for physics or other rotation scripts
 * 
 * PERFORMANCE NOTES:
 * =================
 * 
 * - System uses coroutines (good performance)
 * - No physics simulation during knockback
 * - NavMesh temporarily disabled (prevents pathfinding overhead)
 * - Events are optional (unsubscribe when not needed)
 * 
 * EXTENDING THE SYSTEM:
 * ====================
 * 
 * Add custom effects by subscribing to events:
 * KnockbackEvents.OnKnockbackStarted += (receiver) => {
 *     // Screen shake, particles, sound effects, etc.
 * };
 * 
 * Create specialized knockback data:
 * - Environmental knockback (for traps, explosions)
 * - Element-specific knockback (fire pushes differently than ice)
 * - Weapon-specific knockback (each weapon type has unique feel)
 */

// This file serves as documentation - no actual code implementation needed
using UnityEngine; 