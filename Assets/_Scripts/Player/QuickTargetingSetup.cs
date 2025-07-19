using UnityEngine;

/// <summary>
/// Quick setup script to add targeting components at runtime
/// Add this to any GameObject in your scene and it will auto-setup targeting
/// </summary>
public class QuickTargetingSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool enableDebugLogs = true;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupTargetingSystem();
        }
    }
    
    [ContextMenu("Setup Targeting System")]
    public void SetupTargetingSystem()
    {
        if (enableDebugLogs) Debug.Log("Setting up targeting system...");
        
        // 1. Setup Nyx targeting components
        SetupNyxComponents();
        
        // 2. Setup enemy targetable components
        SetupEnemyComponents();
        
        if (enableDebugLogs) Debug.Log("Targeting system setup complete!");
    }
    
    private void SetupNyxComponents()
    {
        // Find Nyx child object
        GameObject nyx = GameObject.FindWithTag("Nyx");
        if (nyx == null)
        {
            if (enableDebugLogs) Debug.LogError("Could not find GameObject with tag 'Nyx'");
            return;
        }
        
        // Add NyxTargetingSystem if missing
        if (nyx.GetComponent<NyxTargetingSystem>() == null)
        {
            nyx.AddComponent<NyxTargetingSystem>();
            if (enableDebugLogs) Debug.Log("Added NyxTargetingSystem to Nyx");
        }
        
        // Add NyxLockOnSystem if missing
        if (nyx.GetComponent<NyxLockOnSystem>() == null)
        {
            nyx.AddComponent<NyxLockOnSystem>();
            if (enableDebugLogs) Debug.Log("Added NyxLockOnSystem to Nyx");
        }
        
        if (enableDebugLogs) Debug.Log($"✓ Nyx components setup on: {nyx.name}");
        
        // The lock-on system will automatically create a visual indicator above enemies
        if (enableDebugLogs) Debug.Log("✓ Lock-on indicator will be created automatically when targeting enemies");
    }
    
    private void SetupEnemyComponents()
    {
        int addedCount = 0;
        
        // Setup Enemy_Basic objects
        Enemy_Basic[] enemies = FindObjectsOfType<Enemy_Basic>();
        foreach (var enemy in enemies)
        {
            if (enemy.GetComponent<TargetableEnemy>() == null)
            {
                enemy.gameObject.AddComponent<TargetableEnemy>();
                addedCount++;
            }
        }
        
        // Setup Worker objects  
        Worker[] workers = FindObjectsOfType<Worker>();
        foreach (var worker in workers)
        {
            if (worker.GetComponent<TargetableEnemy>() == null)
            {
                worker.gameObject.AddComponent<TargetableEnemy>();
                addedCount++;
            }
        }
        
        // Setup Boss objects
        Boss[] bosses = FindObjectsOfType<Boss>();
        foreach (var boss in bosses)
        {
            if (boss.GetComponent<TargetableEnemy>() == null)
            {
                boss.gameObject.AddComponent<TargetableEnemy>();
                addedCount++;
            }
        }
        
        if (enableDebugLogs) Debug.Log($"✓ Added TargetableEnemy to {addedCount} enemies");
    }
} 