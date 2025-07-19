#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Utility script to automatically set up the targeting system
/// Adds ITargetable components to existing enemies and grappleable objects
/// </summary>
public class TargetingSystemSetup : EditorWindow
{
    [MenuItem("Tools/Nyx Targeting/Setup Targeting System")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TargetingSystemSetup), false, "Targeting System Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Nyx Targeting System Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("This tool will automatically add ITargetable components to existing enemies and grappleable objects in the scene.");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Add TargetableEnemy to all Enemy_Basic components"))
        {
            AddTargetableEnemyToEnemyBasic();
        }
        
        if (GUILayout.Button("Add TargetableEnemy to all Worker components"))
        {
            AddTargetableEnemyToWorkers();
        }
        
        if (GUILayout.Button("Add TargetableEnemy to all Boss components"))
        {
            AddTargetableEnemyToBosses();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Verify Grappleable Components (Now Built-in)"))
        {
            VerifyGrappleableComponents();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Setup Complete Targeting System (All of the above)"))
        {
            SetupCompleteTargetingSystem();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Add Targeting Systems to Nyx Child Object"))
        {
            AddTargetingSystemsToNyx();
        }
        
        GUILayout.Space(20);
        
        GUILayout.Label("Setup Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("For Nyx as child object (recommended):", EditorStyles.miniBoldLabel);
        GUILayout.Label("1. Use 'Add Targeting Systems to Nyx Child Object' button");
        GUILayout.Label("2. Run 'Setup Complete Targeting System' for enemies/objects");
        GUILayout.Label("3. RB/LB and scroll wheel controls are ready to use!");
        GUILayout.Space(5);
        GUILayout.Label("Manual Setup:", EditorStyles.miniBoldLabel);
        GUILayout.Label("1. Add NyxTargetingSystem component to Nyx child object");
        GUILayout.Label("2. Add NyxLockOnSystem component to Nyx child object");
        GUILayout.Label("3. Ensure Nyx child object has 'Nyx' tag");
        GUILayout.Label("4. Parent object should have Player_Movement component");
    }
    
    private static void AddTargetableEnemyToEnemyBasic()
    {
        Enemy_Basic[] enemies = FindObjectsOfType<Enemy_Basic>();
        int addedCount = 0;
        
        foreach (var enemy in enemies)
        {
            if (enemy.GetComponent<TargetableEnemy>() == null)
            {
                enemy.gameObject.AddComponent<TargetableEnemy>();
                addedCount++;
                
                // Mark the object as dirty so changes are saved
                EditorUtility.SetDirty(enemy.gameObject);
            }
        }
        
        Debug.Log($"Added TargetableEnemy to {addedCount} Enemy_Basic objects");
    }
    
    private static void AddTargetableEnemyToWorkers()
    {
        Worker[] workers = FindObjectsOfType<Worker>();
        int addedCount = 0;
        
        foreach (var worker in workers)
        {
            if (worker.GetComponent<TargetableEnemy>() == null)
            {
                worker.gameObject.AddComponent<TargetableEnemy>();
                addedCount++;
                
                // Mark the object as dirty so changes are saved
                EditorUtility.SetDirty(worker.gameObject);
            }
        }
        
        Debug.Log($"Added TargetableEnemy to {addedCount} Worker objects");
    }
    
    private static void AddTargetableEnemyToBosses()
    {
        Boss[] bosses = FindObjectsOfType<Boss>();
        int addedCount = 0;
        
        foreach (var boss in bosses)
        {
            if (boss.GetComponent<TargetableEnemy>() == null)
            {
                boss.gameObject.AddComponent<TargetableEnemy>();
                addedCount++;
                
                // Mark the object as dirty so changes are saved
                EditorUtility.SetDirty(boss.gameObject);
            }
        }
        
        Debug.Log($"Added TargetableEnemy to {addedCount} Boss objects");
    }
    
    private static void VerifyGrappleableComponents()
    {
        Grappleable[] grapplebales = FindObjectsOfType<Grappleable>();
        int validCount = 0;
        int needsTagCount = 0;
        
        foreach (var grappleable in grapplebales)
        {
            validCount++;
            
            // Check if it has the correct tag
            if (!grappleable.CompareTag("CanBeGrappled"))
            {
                grappleable.tag = "CanBeGrappled";
                needsTagCount++;
                
                // Mark the object as dirty so changes are saved
                EditorUtility.SetDirty(grappleable.gameObject);
            }
        }
        
        Debug.Log($"Found {validCount} Grappleable objects (now with built-in targeting). Added 'CanBeGrappled' tag to {needsTagCount} objects.");
    }
    
    private static void AddTargetingSystemsToNyx()
    {
        GameObject nyx = GameObject.FindWithTag("Nyx");
        if (nyx == null)
        {
            Debug.LogError("Could not find GameObject with tag 'Nyx'. Please ensure the Nyx child object is tagged correctly.");
            return;
        }
        
        bool addedSomething = false;
        
        // Add NyxTargetingSystem if missing
        if (nyx.GetComponent<NyxTargetingSystem>() == null)
        {
            nyx.AddComponent<NyxTargetingSystem>();
            Debug.Log("Added NyxTargetingSystem to Nyx child object");
            addedSomething = true;
            EditorUtility.SetDirty(nyx);
        }
        
        // Add NyxLockOnSystem if missing
        if (nyx.GetComponent<NyxLockOnSystem>() == null)
        {
            var lockOnSystem = nyx.AddComponent<NyxLockOnSystem>();
            // Disable camera adjustments by default for child setups
            var serializedObject = new UnityEditor.SerializedObject(lockOnSystem);
            var adjustCameraProp = serializedObject.FindProperty("adjustCameraOnLockOn");
            if (adjustCameraProp != null)
            {
                adjustCameraProp.boolValue = false;
                serializedObject.ApplyModifiedProperties();
            }
            Debug.Log("Added NyxLockOnSystem to Nyx child object (camera adjustments disabled)");
            addedSomething = true;
            EditorUtility.SetDirty(nyx);
        }
        
        if (!addedSomething)
        {
            Debug.Log("Nyx child object already has all required targeting components!");
        }
        
        // Check parent setup
        var playerMovement = nyx.GetComponentInParent<Player_Movement>();
        if (playerMovement != null)
        {
            Debug.Log($"✓ Found Player_Movement on parent '{playerMovement.name}' - input actions should work correctly");
        }
        else
        {
            Debug.LogWarning("Player_Movement component not found on parent objects - input actions may not work correctly!");
        }
    }
    
    private static void SetupCompleteTargetingSystem()
    {
        Debug.Log("Setting up complete targeting system...");
        
        AddTargetableEnemyToEnemyBasic();
        AddTargetableEnemyToWorkers();
        AddTargetableEnemyToBosses();
        VerifyGrappleableComponents();
        AddTargetingSystemsToNyx(); // Add this to complete setup
        
        Debug.Log("Complete targeting system setup finished!");
    }
}

/// <summary>
/// Component that automatically adds targeting components when added to Nyx
/// </summary>
[System.Serializable]
public class NyxTargetingAutoSetup : MonoBehaviour
{
    [Header("Auto-Setup")]
    [SerializeField] private bool autoSetupOnAwake = true;
    [SerializeField] private bool setupTargetingSystem = true;
    [SerializeField] private bool setupLockOnSystem = true;
    
    void Awake()
    {
        if (autoSetupOnAwake)
        {
            SetupTargetingSystems();
        }
    }
    
    [ContextMenu("Setup Targeting Systems")]
    public void SetupTargetingSystems()
    {
        if (setupTargetingSystem && GetComponent<NyxTargetingSystem>() == null)
        {
            gameObject.AddComponent<NyxTargetingSystem>();
            Debug.Log("Added NyxTargetingSystem to " + gameObject.name);
        }
        
        if (setupLockOnSystem && GetComponent<NyxLockOnSystem>() == null)
        {
            gameObject.AddComponent<NyxLockOnSystem>();
            Debug.Log("Added NyxLockOnSystem to " + gameObject.name);
        }
        
        // Ensure NyxGrapple has reference to targeting system
        var grappleSystem = GetComponent<NyxGrapple>();
        if (grappleSystem != null)
        {
            Debug.Log("NyxGrapple found - targeting integration ready");
        }
    }
}
#endif 