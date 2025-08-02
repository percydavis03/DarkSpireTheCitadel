/*
 * ========================================
 * UNIFIED KNOCKBACK SYSTEM - SETUP GUIDE
 * ========================================
 * 
 * INDUSTRY-STANDARD FEATURES:
 * ✅ Preserves Y position (no unwanted vertical movement)
 * ✅ NavMesh integration with safety checks
 * ✅ Data-driven configuration (ScriptableObjects)
 * ✅ Event system for effects/animation
 * ✅ Mass-based knockback scaling
 * ✅ Collision detection and stuck prevention
 * 
 * SYSTEM FILES:
 * =============
 * - KnockbackData.cs        (Configuration assets)
 * - IKnockbackReceiver.cs   (Interface + events)
 * - KnockbackManager.cs     (Main controller)
 * - KnockbackReceiver.cs    (Entity component)
 * 
 * 🚨 LEGACY DEPRECATION:
 * Scripts in Assets/_Scripts/Sam/ are deprecated.
 * Use this unified system instead.
 * 
 * QUICK SETUP:
 * ==================
 * 
 * 1. CREATE KNOCKBACK DATA
 *    Right-click > Create > Combat > Knockback Data
 *    Create: LightKnockback (distance: 2), HeavyKnockback (distance: 4)
 * 
 * 2. SETUP MANAGER
 *    Add KnockbackManager to scene GameObject
 *    Assign LightKnockback as default data
 * 
 * 3. ADD TO ENEMIES
 *    Add KnockbackReceiver component to enemy prefabs
 *    Set Mass (1.0 normal, 2.0+ large enemies, 5.0+ bosses)
 *    Enable NavMesh options if using NavMesh
 * 
 * 4. USAGE IN CODE
 *    Replace old knockback calls with:
 *    KnockbackManager.Instance.ApplyKnockback(target, sourcePos, data, force);
 * 
 * MIGRATION FROM LEGACY SYSTEM:
 * ==============================
 * 
 * OLD (DEPRECATED):                    NEW (RECOMMENDED):
 * =================                    ==================
 * GetComponent<Knock>()        ->      GetComponent<KnockbackReceiver>()
 * knock.KnockBack(transform)   ->      KnockbackManager.Instance.ApplyKnockback(target, sourcePos)
 * KnockbackTrigger component   ->      Use OnTriggerEnter + ApplyKnockback call
 * 
 * COMMON INTEGRATIONS:
 * ====================
 * 
 * // Weapon hit
 * KnockbackManager.Instance.ApplyKnockback(enemy, weaponPos, lightData);
 * 
 * // Heavy attack
 * KnockbackManager.Instance.ApplyKnockback(enemy, pos, heavyData, 1.5f);
 * 
 * // Grapple release
 * KnockbackManager.Instance.ApplyKnockback(target, pos, data, 2f, direction);
 * 
 * // Event handling
 * KnockbackEvents.OnKnockbackStarted += (receiver) => {
 *     // Camera shake, effects, etc.
 * };
 * 
 * CONFIGURATION REFERENCE:
 * =========================
 * 
 * RECOMMENDED DISTANCES:    RECOMMENDED DURATIONS:    MASS GUIDELINES:
 * Light: 1-2 units         Quick: 0.2-0.3s          Small: 0.5-1.0
 * Normal: 2-3 units        Normal: 0.3-0.5s         Normal: 1.0-2.0  
 * Heavy: 3-5 units         Heavy: 0.5-0.8s          Large: 2.0-5.0
 * Special: 5-8 units       —                        Boss: 5.0-15.0
 * 
 * ANIMATION INTEGRATION:
 * ======================
 * 1. Add Animator parameters: "isKnockedBack" (Bool), "knockbackTrigger" (Trigger)
 * 2. Set these in KnockbackReceiver component
 * 3. Create transitions: Normal ↔ KnockedBack states
 * 
 * TROUBLESHOOTING:
 * ================
 * 
 * ❌ Knockback not working?
 *    → Check target has KnockbackReceiver component
 *    → Verify CanReceiveKnockback is true
 *    → Ensure KnockbackManager has default data
 * 
 * ❌ NavMesh issues?
 *    → Enable "Warp NavMesh After Knockback"
 *    → Check NavMesh Agent is properly assigned
 * 
 * ❌ Position/rotation problems?
 *    → System preserves Y/rotation automatically
 *    → Look for conflicting scripts modifying transform
 * 
 * PERFORMANCE NOTES:
 * ==================
 * ✅ Coroutine-based (efficient)
 * ✅ NavMesh disabled during knockback (saves CPU)
 * ✅ No physics simulation overhead
 * ✅ Handles dozens of simultaneous knockbacks
 * 
 * 🎯 SYSTEM READY FOR PRODUCTION USE!
 */

// This is a documentation file - no code implementation needed
using UnityEngine; 