using UnityEngine;
using UnityEditor;

/// <summary>
/// Setup script for the new SimpleNyxLockOn system
/// This will disable old systems and configure the new one
/// </summary>
public class SimpleLockOnSetup : MonoBehaviour
{
    [Header("Setup Actions")]
    [SerializeField] private bool disableOldSystems = true;
    [SerializeField] private bool setupNewSystem = true;
    [SerializeField] private bool testMode = true;
    
    [Header("Test Controls")]
    [SerializeField] private KeyCode testLockOnKey = KeyCode.T;
    [SerializeField] private KeyCode forceUnlockKey = KeyCode.Y;
    
    private SimpleNyxLockOn newLockOnSystem;
    
    void Start()
    {
        SetupLockOnSystem();
    }
    
    void Update()
    {
        // Test controls
        if (testMode)
        {
            if (Input.GetKeyDown(testLockOnKey))
            {
                TestLockOn();
            }
            
            if (Input.GetKeyDown(forceUnlockKey))
            {
                ForceUnlock();
            }
        }
    }
    
    [ContextMenu("Setup Lock-On System")]
    public void SetupLockOnSystem()
    {
        Debug.Log("=== Setting up Simple Lock-On System ===");
        
        if (disableOldSystems)
        {
            DisableOldSystems();
        }
        
        if (setupNewSystem)
        {
            SetupNewSystem();
        }
        
        Debug.Log("=== Simple Lock-On System Setup Complete ===");
    }
    
    private void DisableOldSystems()
    {
        Debug.Log("Disabling old lock-on systems...");
        
        // Find and disable old systems
        NyxLockOnSystem oldLockOn = FindObjectOfType<NyxLockOnSystem>();
        if (oldLockOn != null)
        {
            oldLockOn.enabled = false;
            Debug.Log("✓ Disabled NyxLockOnSystem");
        }
        
        NyxTargetingSystem oldTargeting = FindObjectOfType<NyxTargetingSystem>();
        if (oldTargeting != null)
        {
            oldTargeting.enabled = false;
            Debug.Log("✓ Disabled NyxTargetingSystem");
        }
        
        // Find and disable manual test scripts
        ManualLockOnTest manualTest = FindObjectOfType<ManualLockOnTest>();
        if (manualTest != null)
        {
            manualTest.enabled = false;
            Debug.Log("✓ Disabled ManualLockOnTest");
        }
    }
    
    private void SetupNewSystem()
    {
        Debug.Log("Setting up new SimpleNyxLockOn system...");
        
        // Find player object
        Player_Movement playerMovement = FindObjectOfType<Player_Movement>();
        if (playerMovement == null)
        {
            Debug.LogError("❌ Player_Movement not found! Cannot setup lock-on system.");
            return;
        }
        
        // Add SimpleNyxLockOn to player if not present
        newLockOnSystem = playerMovement.GetComponent<SimpleNyxLockOn>();
        if (newLockOnSystem == null)
        {
            newLockOnSystem = playerMovement.gameObject.AddComponent<SimpleNyxLockOn>();
            Debug.Log("✓ Added SimpleNyxLockOn component to player");
        }
        else
        {
            Debug.Log("✓ SimpleNyxLockOn already exists");
        }
        
        // Configure the system
        ConfigureNewSystem();
        
        // Test the setup
        TestSystemSetup();
    }
    
    private void ConfigureNewSystem()
    {
        if (newLockOnSystem == null) return;
        
        Debug.Log("Configuring SimpleNyxLockOn system...");
        
        // Find Nyx if not assigned
        if (newLockOnSystem.NyxTransform == null)
        {
            GameObject nyxObj = GameObject.FindWithTag("Nyx");
            if (nyxObj != null)
            {
                // Use reflection to set the private field
                var field = typeof(SimpleNyxLockOn).GetField("nyxTransform", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(newLockOnSystem, nyxObj.transform);
                Debug.Log("✓ Found and assigned Nyx transform");
            }
            else
            {
                Debug.LogWarning("⚠️ No GameObject with 'Nyx' tag found. Please tag your Nyx object.");
            }
        }
        
        // Find camera
        Camera playerCamera = Camera.main ?? FindObjectOfType<Camera>();
        if (playerCamera != null)
        {
            var field = typeof(SimpleNyxLockOn).GetField("playerCamera", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(newLockOnSystem, playerCamera);
            Debug.Log("✓ Found and assigned player camera");
        }
        
        Debug.Log("✓ SimpleNyxLockOn configured");
    }
    
    private void TestSystemSetup()
    {
        Debug.Log("Testing system setup...");
        
        // Check for enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"Found {enemies.Length} enemies with 'Enemy' tag");
        
        if (enemies.Length == 0)
        {
            Debug.LogWarning("⚠️ No enemies found! Make sure your enemies are tagged with 'Enemy'");
        }
        
        // Check for Nyx
        GameObject nyxObj = GameObject.FindWithTag("Nyx");
        if (nyxObj == null)
        {
            Debug.LogWarning("⚠️ No Nyx object found! Make sure to tag your Nyx character with 'Nyx'");
        }
        else
        {
            Debug.Log($"✓ Found Nyx object: {nyxObj.name}");
        }
        
        // Check camera
        Camera cam = Camera.main ?? FindObjectOfType<Camera>();
        if (cam == null)
        {
            Debug.LogWarning("⚠️ No camera found!");
        }
        else
        {
            Debug.Log($"✓ Found camera: {cam.name}");
        }
        
        Debug.Log("=== Setup Test Complete ===");
        Debug.Log($"Use {testLockOnKey} to test lock-on and {forceUnlockKey} to force unlock");
    }
    
    private void TestLockOn()
    {
        if (newLockOnSystem == null)
        {
            Debug.LogError("No SimpleNyxLockOn system found!");
            return;
        }
        
        Debug.Log($"🔴 TEST: Attempting to cycle lock-on (Enemies in scene: {GameObject.FindGameObjectsWithTag("Enemy").Length})");
        
        // Simulate scroll wheel input
        newLockOnSystem.GetType()
            .GetMethod("CycleToNextTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(newLockOnSystem, null);
    }
    
    private void ForceUnlock()
    {
        if (newLockOnSystem == null)
        {
            Debug.LogError("No SimpleNyxLockOn system found!");
            return;
        }
        
        Debug.Log("🔴 TEST: Force unlocking");
        newLockOnSystem.ForceUnlock();
    }
    
#if UNITY_EDITOR
    [MenuItem("Tools/Simple Lock-On/Setup System")]
    static void SetupFromMenu()
    {
        SimpleLockOnSetup setup = FindObjectOfType<SimpleLockOnSetup>();
        if (setup == null)
        {
            GameObject setupObj = new GameObject("SimpleLockOnSetup");
            setup = setupObj.AddComponent<SimpleLockOnSetup>();
        }
        setup.SetupLockOnSystem();
    }
    
    [MenuItem("Tools/Simple Lock-On/Create Boss Tag")]
    static void CreateBossTag()
    {
        // This will help create the Boss tag if needed
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        
        // Check if Boss tag already exists
        bool bossTagExists = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Boss")
            {
                bossTagExists = true;
                break;
            }
        }
        
        if (!bossTagExists)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = "Boss";
            tagManager.ApplyModifiedProperties();
            Debug.Log("✓ Created 'Boss' tag");
        }
        else
        {
            Debug.Log("'Boss' tag already exists");
        }
    }
#endif
} 