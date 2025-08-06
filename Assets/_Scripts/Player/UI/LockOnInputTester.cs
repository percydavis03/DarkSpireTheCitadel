using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple script to test if lock-on input is working
/// Add this to any GameObject and check the console
/// </summary>
public class LockOnInputTester : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private bool enableTesting = true;
    [SerializeField] private bool showDebugLogs = true;
    
    private PlayerInputActions playerControls;
    private InputAction cycleTargets;
    private float lastCycleInput;
    
    void Awake()
    {
        // Try to find existing PlayerInputActions
        var playerMovement = FindObjectOfType<Player_Movement>();
        if (playerMovement != null && playerMovement.playerControls != null)
        {
            playerControls = playerMovement.playerControls;
            if (showDebugLogs) Debug.Log("✓ Found PlayerInputActions from Player_Movement");
        }
        else
        {
            playerControls = new PlayerInputActions();
            if (showDebugLogs) Debug.Log("✓ Created new PlayerInputActions for testing");
        }
    }
    
    void OnEnable()
    {
        if (playerControls != null && enableTesting)
        {
            cycleTargets = playerControls.General.CycleTargets;
            cycleTargets.Enable();
            
            if (showDebugLogs) Debug.Log("✓ CycleTargets input action enabled for testing");
        }
    }
    
    void OnDisable()
    {
        cycleTargets?.Disable();
    }
    
    void Update()
    {
        if (!enableTesting || cycleTargets == null) return;
        
        float cycleInput = cycleTargets.ReadValue<float>();
        
        // Check if input changed
        if (Mathf.Abs(cycleInput) > 0.1f && Mathf.Abs(lastCycleInput) <= 0.1f)
        {
            if (cycleInput > 0)
            {
                Debug.Log("🎮 LOCK-ON INPUT DETECTED: Next Target (RB or Scroll Up)");
            }
            else if (cycleInput < 0)
            {
                Debug.Log("🎮 LOCK-ON INPUT DETECTED: Previous Target (LB or Scroll Down)");
            }
            
            // Try to manually activate lock-on
            var lockOnSystem = FindObjectOfType<NyxLockOnSystem>();
            if (lockOnSystem != null)
            {
                if (!lockOnSystem.IsLockOnActive)
                {
                    Debug.Log("🎯 Trying to activate lock-on...");
                    lockOnSystem.ActivateLockOn();
                }
                else
                {
                    Debug.Log("🔄 Cycling to next target...");
                    if (cycleInput > 0)
                        lockOnSystem.CycleTargetRight();
                    else
                        lockOnSystem.CycleTargetLeft();
                }
            }
            else
            {
                Debug.LogWarning("❌ No NyxLockOnSystem found!");
            }
        }
        
        lastCycleInput = cycleInput;
    }
    
    void OnGUI()
    {
        if (!enableTesting) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== LOCK-ON INPUT TESTER ===");
        GUILayout.Label("Try these inputs:");
        GUILayout.Label("• RB/LB on controller");
        GUILayout.Label("• Mouse scroll wheel");
        GUILayout.Label("• Check console for messages");
        
        if (cycleTargets != null)
        {
            float input = cycleTargets.ReadValue<float>();
            GUILayout.Label($"Current Input: {input:F2}");
        }
        
        var lockOnSystem = FindObjectOfType<NyxLockOnSystem>();
        if (lockOnSystem != null)
        {
            GUILayout.Label($"Lock-On Active: {lockOnSystem.IsLockOnActive}");
            GUILayout.Label($"Has Target: {lockOnSystem.HasTarget}");
        }
        
        GUILayout.EndArea();
    }
} 