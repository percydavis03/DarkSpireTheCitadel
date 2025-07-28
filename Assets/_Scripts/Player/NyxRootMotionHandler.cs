using UnityEngine;

/// <summary>
/// Handles root motion for Nyx player character animations.
/// Industry standard implementation for managing animation-driven movement.
/// This script should be attached to the same GameObject as the Animator component.
/// </summary>
public class NyxRootMotionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private Player_Movement playerMovement;
    
    [Header("Root Motion Settings")]
    [SerializeField] private bool enableRootMotion = true;
    [SerializeField] private bool applyPositionY = false; // Usually false to prevent floating
    [SerializeField] private bool applyRotation = true;
    [SerializeField] private float rootMotionMultiplier = 1f;
    
    [Header("State-Based Root Motion")]
    [SerializeField] private bool enableDuringRoll = true;
    [SerializeField] private bool enableDuringAttacks = false; // Usually handled by code
    [SerializeField] private bool enableDuringMovement = false; // Movement handled by script
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    // Cached components
    private Transform cachedTransform;
    private Vector3 rootMotionPositionDelta;
    private Quaternion rootMotionRotationDelta;
    
    // Animation state tracking
    private bool isRolling = false;
    private bool isAttacking = false;
    
    void Awake()
    {
        // Auto-find components if not assigned
        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();
            
        if (animator == null)
            animator = GetComponent<Animator>();
            
        if (playerMovement == null)
            playerMovement = GetComponentInParent<Player_Movement>();
            
        cachedTransform = transform;
        
        // Validate setup
        if (characterController == null)
        {
            Debug.LogError($"NyxRootMotionHandler: CharacterController not found on {gameObject.name} or its parent!");
        }
        
        if (animator == null)
        {
            Debug.LogError($"NyxRootMotionHandler: Animator not found on {gameObject.name}!");
        }
    }
    
    void Start()
    {
        // Ensure root motion is enabled on the Animator
        if (animator != null)
        {
            animator.applyRootMotion = true;
        }
    }
    
    void Update()
    {
        // Track animation states
        UpdateAnimationStates();
    }
    
    /// <summary>
    /// Unity's root motion callback - called before each frame's movement is applied
    /// </summary>
    void OnAnimatorMove()
    {
        if (!enableRootMotion || animator == null || characterController == null)
            return;
            
        // Get root motion deltas from animator
        rootMotionPositionDelta = animator.deltaPosition;
        rootMotionRotationDelta = animator.deltaRotation;
        
        // Determine if we should apply root motion based on current state
        bool shouldApplyRootMotion = ShouldApplyRootMotion();
        
        if (shouldApplyRootMotion)
        {
            ApplyRootMotion();
        }
        else
        {
            // When not using root motion, we still need to reset the animator's delta
            // to prevent accumulation of movement
            animator.ApplyBuiltinRootMotion();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Root Motion - Rolling: {isRolling}, Applying: {shouldApplyRootMotion}, Delta: {rootMotionPositionDelta}");
        }
    }
    
    /// <summary>
    /// Determines whether root motion should be applied based on current animation state
    /// </summary>
    private bool ShouldApplyRootMotion()
    {
        // Apply root motion during roll animations
        if (isRolling && enableDuringRoll)
            return true;
            
        // Apply root motion during attack animations if enabled
        if (isAttacking && enableDuringAttacks)
            return true;
            
        // Apply root motion during movement if enabled (usually false for player)
        if (enableDuringMovement)
            return true;
            
        return false;
    }
    
    /// <summary>
    /// Applies root motion to the character controller
    /// </summary>
    private void ApplyRootMotion()
    {
        // Apply position movement
        Vector3 movement = rootMotionPositionDelta * rootMotionMultiplier;
        
        // Usually we don't want Y movement from root motion (to prevent floating)
        if (!applyPositionY)
        {
            movement.y = 0;
        }
        
        // Apply movement through CharacterController to respect collisions
        characterController.Move(movement);
        
        // Apply rotation if enabled
        if (applyRotation)
        {
            cachedTransform.rotation = cachedTransform.rotation * rootMotionRotationDelta;
        }
    }
    
    /// <summary>
    /// Updates animation state tracking
    /// </summary>
    private void UpdateAnimationStates()
    {
        if (animator == null) return;
        
        // Track rolling state
        isRolling = animator.GetBool("isRolling");
        
        // Track attacking state
        isAttacking = animator.GetBool("isAttacking");
    }
    
    /// <summary>
    /// Called by animation events when roll starts
    /// </summary>
    public void OnRollStart()
    {
        if (showDebugInfo)
            Debug.Log("Root Motion: Roll Started");
            
        // Disable script-based roll movement when using root motion
        if (playerMovement != null && enableDuringRoll)
        {
            playerMovement.SetUseRootMotionForRoll(true);
        }
    }
    
    /// <summary>
    /// Called by animation events when roll ends
    /// </summary>
    public void OnRollEnd()
    {
        if (showDebugInfo)
            Debug.Log("Root Motion: Roll Ended");
            
        // Re-enable script-based movement
        if (playerMovement != null)
        {
            playerMovement.SetUseRootMotionForRoll(false);
            playerMovement.EndRoll(); // Ensure roll state is properly ended
        }
    }
    
    /// <summary>
    /// Public method to enable/disable root motion at runtime
    /// </summary>
    public void SetRootMotionEnabled(bool enabled)
    {
        enableRootMotion = enabled;
        
        if (animator != null)
        {
            animator.applyRootMotion = enabled;
        }
    }
    
    /// <summary>
    /// Get current root motion delta for debugging
    /// </summary>
    public Vector3 GetCurrentRootMotionDelta()
    {
        return rootMotionPositionDelta;
    }
} 