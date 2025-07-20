using UnityEngine;

/// <summary>
/// Handles root motion for enemy animations and applies movement to parent GameObject
/// This script should be attached to the animationSource child object that has the Animator
/// </summary>
public class EnemyRootMotionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform parentTransform; // The main enemy GameObject with colliders
    [SerializeField] private Animator animator;
    
    [Header("Settings")]
    [SerializeField] private bool enableRootMotion = true;
    [SerializeField] private bool applyPosition = true;
    [SerializeField] private bool applyRotation = false; // Usually we don't want animation to control Y rotation
    [SerializeField] private float positionMultiplier = 1f; // Can scale the movement if needed
    
    [Header("State-Aware Root Motion")]
    [SerializeField] private bool enableDuringAttacks = true; // Apply root motion during attack animations
    [SerializeField] private bool enableDuringMovement = true; // Apply root motion during movement animations
    [SerializeField] private bool disableDuringHurt = true; // Disable during hurt/stun states
    [SerializeField] private bool disableWhenNavStopped = true; // Disable when NavMesh agent is stopped
    
    private Enemy_Basic enemyScript;
    private Worker workerScript; // Support for Worker enemies too
    private UnityEngine.AI.NavMeshAgent navAgent;
    
    void Awake()
    {
        // Auto-find components if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (parentTransform == null)
            parentTransform = transform.parent;
            
        if (parentTransform != null)
        {
            enemyScript = parentTransform.GetComponent<Enemy_Basic>();
            workerScript = parentTransform.GetComponent<Worker>();
            navAgent = parentTransform.GetComponent<UnityEngine.AI.NavMeshAgent>();
        }
        
        if (animator == null)
        {
            Debug.LogError($"EnemyRootMotionHandler on {gameObject.name}: No Animator component found!");
            enabled = false;
        }
        
        if (parentTransform == null)
        {
            Debug.LogError($"EnemyRootMotionHandler on {gameObject.name}: No parent transform found!");
            enabled = false;
        }
    }
    
    void OnAnimatorMove()
    {
        if (!enableRootMotion || animator == null || parentTransform == null)
            return;
            
        // Check if we should apply root motion based on current enemy state
        if (!ShouldApplyRootMotion())
            return;
            
        // Get the root motion from the animator
        Vector3 deltaPosition = animator.deltaPosition;
        Quaternion deltaRotation = animator.deltaRotation;
        
        // Apply position movement to parent GameObject (where colliders are)
        if (applyPosition && deltaPosition.magnitude > 0.001f)
        {
            Vector3 scaledMovement = deltaPosition * positionMultiplier;
            parentTransform.position += scaledMovement;
            
            // Update NavMeshAgent to keep it in sync
            if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
            {
                navAgent.Warp(parentTransform.position);
            }
        }
        
        // Apply rotation to parent GameObject if enabled
        if (applyRotation)
        {
            parentTransform.rotation *= deltaRotation;
        }
    }
    
    /// <summary>
    /// Determines if root motion should be applied based on current enemy state
    /// </summary>
    private bool ShouldApplyRootMotion()
    {
        // Check NavMesh agent state first
        if (disableWhenNavStopped && navAgent != null && navAgent.enabled && navAgent.isStopped)
        {
            return false;
        }
        
        // Check Enemy_Basic states
        if (enemyScript != null)
        {
            // Don't apply root motion if enemy is dead
            if (enemyScript.dead)
                return false;
                
            // Don't apply root motion if stunned and we're configured to disable it
            if (disableDuringHurt && enemyScript.isStunned)
                return false;
                
            // Don't apply root motion if hurt and we're configured to disable it
            if (disableDuringHurt && (enemyScript.isHit || animator.GetBool("IsHurting")))
                return false;
                
            // Don't apply root motion during attack recovery (this is key for your issue!)
            if (enemyScript.isInAttackRecovery)
                return false;
                
            // Check if we should apply during attacks
            if (!enableDuringAttacks && (animator.GetBool("IsAttacking") || animator.GetBool("IsAttacking2")))
                return false;
                
            // Check if we should apply during movement
            if (!enableDuringMovement && animator.GetBool("IsRunning"))
                return false;
        }
        
        // Check Worker states (if this is a Worker enemy)
        if (workerScript != null)
        {
            // Don't apply root motion if worker is dead
            if (workerScript.dead)
                return false;
                
            // Don't apply root motion if stunned
            if (disableDuringHurt && workerScript.isStunned)
                return false;
                
            // Don't apply root motion if hurt
            if (disableDuringHurt && (workerScript.isHit || animator.GetBool("IsHurting")))
                return false;
                
            // Don't apply root motion during attack recovery
            if (workerScript.isInAttackRecovery)
                return false;
                
            // Check attack states for worker
            if (!enableDuringAttacks && (animator.GetBool("IsAttacking") || animator.GetBool("isAttacking")))
                return false;
                
            // Check movement states for worker (note: Worker uses "isRunning" vs "IsRunning")
            if (!enableDuringMovement && (animator.GetBool("IsRunning") || animator.GetBool("isRunning")))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Enable or disable root motion handling
    /// </summary>
    public void SetRootMotionEnabled(bool enabled)
    {
        enableRootMotion = enabled;
    }
    
    /// <summary>
    /// Set the position multiplier to scale movement
    /// </summary>
    public void SetPositionMultiplier(float multiplier)
    {
        positionMultiplier = multiplier;
    }
    
    /// <summary>
    /// Temporarily disable root motion (useful for external scripts)
    /// </summary>
    public void DisableRootMotionTemporarily()
    {
        enableRootMotion = false;
    }
    
    /// <summary>
    /// Re-enable root motion after temporary disable
    /// </summary>
    public void EnableRootMotion()
    {
        enableRootMotion = true;
    }
} 