using UnityEngine;

/// <summary>
/// Manual lock-on test - press T key to test lock-on
/// Add this to any GameObject for testing
/// </summary>
public class ManualLockOnTest : MonoBehaviour
{
    [Header("Manual Testing")]
    [SerializeField] private KeyCode testKey = KeyCode.T;
    [SerializeField] private bool enableTesting = true;
    
    private NyxLockOnSystem lockOnSystem;
    private NyxTargetingSystem targetingSystem;
    private DebugCubeIndicator debugCube;
    
    void Start()
    {
        // Find the lock-on system
        lockOnSystem = FindObjectOfType<NyxLockOnSystem>();
        targetingSystem = FindObjectOfType<NyxTargetingSystem>();
        
        // Add debug cube component
        debugCube = gameObject.AddComponent<DebugCubeIndicator>();
        
        if (lockOnSystem == null)
        {
            Debug.LogError("❌ No NyxLockOnSystem found! Make sure it's on your Nyx child object.");
        }
        else
        {
            Debug.Log("✅ Found NyxLockOnSystem - ready for testing");
        }
        
        if (targetingSystem == null)
        {
            Debug.LogError("❌ No NyxTargetingSystem found!");
        }
        else
        {
            Debug.Log("✅ Found NyxTargetingSystem - ready for testing");
        }
    }
    
    void Update()
    {
        if (!enableTesting) return;
        
        // Manual lock-on test with T key
        if (Input.GetKeyDown(testKey))
        {
            Debug.Log($"🔴 MANUAL TEST: {testKey} key pressed - testing lock-on...");
            TestLockOn();
        }
        
        // Manual unlock test with Y key
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log("🔴 MANUAL TEST: Y key pressed - releasing lock-on...");
            if (lockOnSystem != null)
            {
                lockOnSystem.ReleaseLockOn();
            }
            if (debugCube != null)
            {
                debugCube.Hide();
            }
        }
    }
    
    void TestLockOn()
    {
        if (lockOnSystem == null)
        {
            Debug.LogError("❌ No lock-on system available");
            return;
        }
        
        if (targetingSystem == null)
        {
            Debug.LogError("❌ No targeting system available");
            return;
        }
        
        // Force update targeting to get fresh targets
        targetingSystem.ForceUpdate();
        
        Debug.Log($"🎯 Available targets: {targetingSystem.ValidTargets.Count}");
        Debug.Log($"🎯 Best target: {(targetingSystem.BestTarget?.Transform.name ?? "None")}");
        
        if (targetingSystem.HasValidTargets)
        {
            Debug.Log("🎯 Activating lock-on manually...");
            lockOnSystem.ActivateLockOn();
            
            // Also show debug cube
            if (debugCube != null && targetingSystem.BestTarget != null)
            {
                debugCube.ShowOnEnemy(targetingSystem.BestTarget.Transform);
            }
        }
        else
        {
            Debug.LogWarning("❌ No valid targets found for lock-on");
            
            // Try to find any enemy manually
            Enemy_Basic[] enemies = FindObjectsOfType<Enemy_Basic>();
            Debug.Log($"🔍 Found {enemies.Length} Enemy_Basic objects in scene");
            
            foreach (var enemy in enemies)
            {
                var targetable = enemy.GetComponent<TargetableEnemy>();
                if (targetable != null)
                {
                    Debug.Log($"✅ Enemy {enemy.name} has TargetableEnemy component");
                }
                else
                {
                    Debug.LogWarning($"❌ Enemy {enemy.name} missing TargetableEnemy component");
                }
            }
        }
    }
    
    void OnGUI()
    {
        if (!enableTesting) return;
        
        GUILayout.BeginArea(new Rect(10, 250, 350, 300));
        GUILayout.Label("=== MANUAL LOCK-ON TEST ===");
        GUILayout.Label($"Press {testKey} to test lock-on");
        GUILayout.Label("Press Y to release lock-on");
        GUILayout.Label("🟡 YELLOW CUBE = Debug indicator");
        GUILayout.Space(10);
        
        if (lockOnSystem != null)
        {
            GUILayout.Label($"Lock-On Active: {lockOnSystem.IsLockOnActive}");
            GUILayout.Label($"Has Target: {lockOnSystem.HasTarget}");
            if (lockOnSystem.HasTarget)
            {
                GUILayout.Label($"Target: {lockOnSystem.CurrentTarget?.Transform.name}");
            }
        }
        
        if (targetingSystem != null)
        {
            GUILayout.Label($"Valid Targets: {targetingSystem.ValidTargets.Count}");
            GUILayout.Label($"Best Target: {targetingSystem.BestTarget?.Transform.name ?? "None"}");
        }
        
        GUILayout.Space(10);
        if (GUILayout.Button("Test Lock-On Now"))
        {
            TestLockOn();
        }
        
        if (GUILayout.Button("Release Lock-On"))
        {
            lockOnSystem?.ReleaseLockOn();
            debugCube?.Hide();
        }
        
        GUILayout.EndArea();
    }
} 