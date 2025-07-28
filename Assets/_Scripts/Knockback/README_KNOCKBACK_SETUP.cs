/*
 * ========================================
 * INDUSTRY-STANDARD KNOCKBACK SYSTEM
 * Complete Setup Guide
 * ========================================
 * 
 * This system provides professional-grade knockback that:
 * ✅ Preserves Y position (no vertical movement)
 * ✅ Preserves X/Z rotations (no tilting)
 * ✅ Compatible with NavMesh, CharacterController, and Rigidbody
 * ✅ Data-driven configuration with ScriptableObjects
 * ✅ Event-based architecture for extensibility
 * ✅ Easy to modify and extend
 * ✅ Does NOT use any scripts from Sam folder
 * 
 * FILES CREATED:
 * ==============
 * - KnockbackData.cs           (ScriptableObject configurations)
 * - IKnockbackReceiver.cs      (Interface + events)
 * - KnockbackManager.cs        (Singleton controller)
 * - KnockbackReceiver.cs       (Component for entities)
 * - KnockbackIntegrationExamples.cs (Usage examples)
 * 
 * QUICK START SETUP:
 * ==================
 * 
 * STEP 1: Create Knockback Data Assets
 * ------------------------------------
 * 1. Right-click in Project window
 * 2. Create > Combat > Knockback Data
 * 3. Create these recommended types:
 * 
 *    LIGHT KNOCKBACK:
 *    - Knockback Distance: 2
 *    - Duration: 0.3
 *    - Impact Pause: 0.1
 *    - Recovery Time: 0.1
 * 
 *    HEAVY KNOCKBACK:
 *    - Knockback Distance: 4
 *    - Duration: 0.5
 *    - Impact Pause: 0.15
 *    - Recovery Time: 0.2
 * 
 *    GRAPPLE RELEASE:
 *    - Knockback Distance: 3
 *    - Duration: 0.4
 *    - Impact Pause: 0.05
 *    - Recovery Time: 0.3
 * 
 * STEP 2: Setup KnockbackManager
 * ------------------------------
 * 1. Create empty GameObject in scene named "KnockbackManager"
 * 2. Add KnockbackManager script to it
 * 3. Assign your Light Knockback data as the default
 * 4. Enable debug options if needed for testing
 * 
 * STEP 3: Add KnockbackReceiver to Enemies
 * -----------------------------------------
 * 1. Select your enemy prefab
 * 2. Add "KnockbackReceiver" component
 * 3. Configure settings:
 *    - Mass: 1.0 (normal enemies), 2-5 (large enemies), 5+ (bosses)
 *    - Has NavMesh Agent: ✓ (if using NavMesh)
 *    - Disable NavMesh During Knockback: ✓
 *    - Warp NavMesh After Knockback: ✓
 *    - Animator: Assign if you want animation integration
 * 
 * STEP 4: Test the System
 * -----------------------
 * Add this test code to any script to try it out:
 * 
 * void Update()
 * {
 *     if (Input.GetKeyDown(KeyCode.T))
 *     {
 *         // Find first enemy and knock it back
 *         GameObject enemy = GameObject.FindWithTag("Enemy");
 *         if (enemy != null)
 *         {
 *             KnockbackManager.Instance.ApplyKnockback(
 *                 enemy,
 *                 transform.position,
 *                 null, // Uses default data
 *                 1.0f  // Normal force
 *             );
 *         }
 *     }
 * }
 * 
 * INTEGRATION WITH EXISTING SYSTEMS:
 * ==================================
 * 
 * FOR YOUR ENEMY_BASIC.CS:
 * ------------------------
 * Replace your existing GetKnockedBack method with this:
 * 
 * public void GetKnockedBack(Vector3 sourcePosition, float force = 1f)
 * {
 *     KnockbackManager.Instance.ApplyKnockback(
 *         this.gameObject, 
 *         sourcePosition, 
 *         null, // Uses default knockback data
 *         force
 *     );
 * }
 * 
 * FOR WEAPON COLLISIONS:
 * ----------------------
 * In your weapon OnTriggerEnter:
 * 
 * private void OnTriggerEnter(Collider other)
 * {
 *     if (other.CompareTag("Enemy"))
 *     {
 *         // Apply knockback first
 *         KnockbackManager.Instance.ApplyKnockback(
 *             other.gameObject,
 *             transform.position,
 *             weaponKnockbackData // Assign in inspector
 *         );
 *         
 *         // Then your existing damage code
 *         var enemy = other.GetComponent<Enemy_Basic>();
 *         enemy?.TakeDamage();
 *     }
 * }
 * 
 * FOR GRAPPLE SYSTEM ENHANCEMENT:
 * -------------------------------
 * Add this to your NyxGrapple.cs StopGrappling method:
 * 
 * if (currentTarget != null)
 * {
 *     Vector3 releaseDirection = (currentTarget.transform.position - grappleOrigin.position).normalized;
 *     KnockbackManager.Instance.ApplyKnockback(
 *         currentTarget.gameObject,
 *         grappleOrigin.position,
 *         grappleReleaseKnockbackData, // Create this asset
 *         1.5f, // Extra force for grapple releases
 *         releaseDirection
 *     );
 * }
 * 
 * ANIMATION INTEGRATION (OPTIONAL):
 * =================================
 * 
 * 1. Add these parameters to your enemy Animator:
 *    - "isKnockedBack" (Bool)
 *    - "knockbackTrigger" (Trigger)
 * 
 * 2. Create animation states:
 *    - Idle/Moving -> KnockedBack (when isKnockedBack = true)
 *    - KnockedBack -> Idle/Moving (when isKnockedBack = false)
 * 
 * 3. In KnockbackReceiver component, set:
 *    - Knockback Bool Parameter: "isKnockedBack"
 *    - Knockback Trigger Parameter: "knockbackTrigger"
 * 
 * ADVANCED FEATURES:
 * ==================
 * 
 * EVENT SYSTEM FOR EFFECTS:
 * -------------------------
 * // Add to any MonoBehaviour
 * void Start()
 * {
 *     KnockbackEvents.OnKnockbackStarted += OnEnemyKnockedBack;
 * }
 * 
 * void OnEnemyKnockedBack(IKnockbackReceiver receiver)
 * {
 *     // Screen shake, particles, sound effects, etc.
 *     CameraShake.Instance?.Shake(0.3f, 0.2f);
 * }
 * 
 * MULTIPLE KNOCKBACK TYPES:
 * -------------------------
 * // Different attacks use different data
 * KnockbackManager.Instance.ApplyKnockback(target, sourcePos, lightKnockback, 1f);
 * KnockbackManager.Instance.ApplyKnockback(target, sourcePos, heavyKnockback, 1.2f);
 * KnockbackManager.Instance.ApplyKnockback(target, sourcePos, grappleRelease, 2f);
 * 
 * CONDITIONAL KNOCKBACK:
 * ----------------------
 * if (isCriticalHit)
 *     KnockbackManager.Instance.ApplyKnockback(target, pos, heavyKnockback, 1.5f);
 * else if (isBlocked)
 *     KnockbackManager.Instance.ApplyKnockback(target, pos, lightKnockback, 0.3f);
 * else
 *     KnockbackManager.Instance.ApplyKnockback(target, pos, lightKnockback, 1f);
 * 
 * CONFIGURATION TIPS:
 * ===================
 * 
 * KNOCKBACK DISTANCES:
 * - Light attacks: 1-2 units
 * - Normal attacks: 2-3 units  
 * - Heavy attacks: 3-5 units
 * - Special/Ultimate: 5-8 units
 * 
 * DURATIONS:
 * - Quick hits: 0.2-0.3 seconds
 * - Normal hits: 0.3-0.5 seconds
 * - Heavy hits: 0.5-0.8 seconds
 * 
 * MASS VALUES:
 * - Small enemies (rats, etc.): 0.5-1.0
 * - Normal enemies: 1.0-2.0
 * - Large enemies: 2.0-5.0
 * - Mini-bosses: 5.0-8.0
 * - Main bosses: 8.0-15.0
 * 
 * ANIMATION CURVES:
 * - Fast start, slow end: Good for impact feeling
 * - Constant speed: Good for consistent movement
 * - Slow start, fast end: Good for "wind up" effects
 * 
 * TROUBLESHOOTING:
 * ===============
 * 
 * KNOCKBACK NOT WORKING:
 * - Ensure target has KnockbackReceiver component
 * - Check that CanReceiveKnockback is true
 * - Verify KnockbackManager has default data assigned
 * 
 * NAVMESH ISSUES:
 * - Enable "Warp NavMesh After Knockback" in KnockbackReceiver
 * - Ensure NavMesh Agent is assigned properly
 * 
 * Y POSITION CHANGING:
 * - System forces Y preservation, check for other scripts
 * - Disable any physics or other movement during knockback
 * 
 * ROTATION ISSUES:
 * - System preserves rotations automatically
 * - Check for other scripts modifying rotation
 * 
 * PERFORMANCE CONSIDERATIONS:
 * ==========================
 * 
 * ✅ System uses coroutines (efficient)
 * ✅ No physics simulation during knockback
 * ✅ NavMesh temporarily disabled (saves pathfinding)
 * ✅ Events are optional (unsubscribe when not needed)
 * ✅ ScriptableObjects reduce memory allocation
 * 
 * The system is designed for production use and should handle
 * dozens of simultaneous knockbacks without performance issues.
 * 
 * EXTENDING THE SYSTEM:
 * =====================
 * 
 * CREATE SPECIALIZED KNOCKBACK TYPES:
 * - Environmental (explosions, traps)
 * - Elemental (fire vs ice knockback)
 * - Weapon-specific (each weapon feels different)
 * - Status-based (poisoned enemies react differently)
 * 
 * ADD CUSTOM EFFECTS:
 * - Screen shake integration
 * - Particle system spawning
 * - Dynamic audio based on knockback strength
 * - UI feedback (damage numbers, etc.)
 * 
 * THAT'S IT! Your knockback system is ready to use! 🎯
 */

// This is a documentation file - no code implementation needed
using UnityEngine; 