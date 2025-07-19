using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Debug script to help fix target sprite visibility issues
/// Add this to any GameObject to test the lock-on indicator system
/// </summary>
public class TargetSpriteDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugging = true;
    [SerializeField] private KeyCode testKey = KeyCode.T;
    [SerializeField] private KeyCode createIndicatorKey = KeyCode.I;
    [SerializeField] private KeyCode hideIndicatorKey = KeyCode.H;
    
    [Header("Test Target")]
    [SerializeField] private Transform testTarget; // Assign an enemy to test with
    [SerializeField] private bool autoFindTestTarget = true;
    
    [Header("Indicator Creation")]
    [SerializeField] private GameObject lockOnIndicatorPrefab; // Assign the SimpleLockOnIndicator prefab
    [SerializeField] private bool createSimpleIndicator = true;
    
    private LockOnIndicator currentIndicator;
    private NyxLockOnSystem lockOnSystem;
    private NyxTargetingSystem targetingSystem;
    
    void Start()
    {
        if (enableDebugging)
        {
            // Find systems
            lockOnSystem = FindObjectOfType<NyxLockOnSystem>();
            targetingSystem = FindObjectOfType<NyxTargetingSystem>();
            
            if (lockOnSystem == null)
                Debug.LogError("TargetSpriteDebugger: No NyxLockOnSystem found!");
            if (targetingSystem == null)
                Debug.LogError("TargetSpriteDebugger: No NyxTargetingSystem found!");
            
            // Auto-find test target
            if (autoFindTestTarget && testTarget == null)
            {
                Enemy_Basic[] enemies = FindObjectsOfType<Enemy_Basic>();
                if (enemies.Length > 0)
                {
                    testTarget = enemies[0].transform;
                }
            }
            
            // Auto-find indicator prefab
            if (lockOnIndicatorPrefab == null)
            {
                lockOnIndicatorPrefab = Resources.Load<GameObject>("SimpleLockOnIndicator");
                if (lockOnIndicatorPrefab == null)
                {
                    lockOnIndicatorPrefab = FindAsset<GameObject>("SimpleLockOnIndicator");
                }
            }
        }
    }
    
    void Update()
    {
        if (!enableDebugging) return;
        
        // Test key - test full lock-on system
        if (Input.GetKeyDown(testKey))
        {
            TestFullLockOnSystem();
        }
        
        // Create indicator key - manually create and show indicator
        if (Input.GetKeyDown(createIndicatorKey))
        {
            CreateAndShowIndicator();
        }
        
        // Hide indicator key
        if (Input.GetKeyDown(hideIndicatorKey))
        {
            HideIndicator();
        }
    }
    
    void TestFullLockOnSystem()
    {
        if (lockOnSystem == null)
        {
            Debug.LogError("TargetSpriteDebugger: No lock-on system available!");
            return;
        }
        
        if (targetingSystem == null)
        {
            Debug.LogError("TargetSpriteDebugger: No targeting system available!");
            return;
        }
        
        // Force update targeting
        targetingSystem.ForceUpdate();
        
        if (targetingSystem.HasValidTargets)
        {
            lockOnSystem.ActivateLockOn();
            Debug.Log($"TargetSpriteDebugger: Activated lock-on with {targetingSystem.ValidTargets.Count} targets");
        }
        else
        {
            Debug.LogWarning("TargetSpriteDebugger: No valid targets found!");
        }
    }
    
    void CreateAndShowIndicator()
    {
        if (testTarget == null)
        {
            Debug.LogError("TargetSpriteDebugger: No test target assigned!");
            return;
        }
        
        // Create indicator
        GameObject indicatorObj = null;
        
        if (lockOnIndicatorPrefab != null)
        {
            indicatorObj = Instantiate(lockOnIndicatorPrefab);
        }
        else if (createSimpleIndicator)
        {
            indicatorObj = CreateSimpleIndicator();
        }
        
        if (indicatorObj != null)
        {
            currentIndicator = indicatorObj.GetComponent<LockOnIndicator>();
            if (currentIndicator != null)
            {
                currentIndicator.ShowOnTarget(testTarget);
                Debug.Log($"TargetSpriteDebugger: Created indicator for {testTarget.name}");
                
                // Debug info
                SpriteRenderer sr = indicatorObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Debug.Log($"TargetSpriteDebugger: SpriteRenderer - Enabled: {sr.enabled}, Sprite: {sr.sprite != null}, Color: {sr.color}, Scale: {indicatorObj.transform.localScale}");
                }
            }
            else
            {
                Debug.LogError("TargetSpriteDebugger: No LockOnIndicator component found!");
            }
        }
        else
        {
            Debug.LogError("TargetSpriteDebugger: Failed to create indicator!");
        }
    }
    
    GameObject CreateSimpleIndicator()
    {
        GameObject obj = new GameObject("DebugLockOnIndicator");
        
        // Add SpriteRenderer
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        
        // Create a simple red circle sprite
        Texture2D texture = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        Vector2 center = new Vector2(32, 32);
        float radius = 28f;
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius && distance >= radius - 4f)
                {
                    pixels[y * 64 + x] = Color.red;
                }
                else
                {
                    pixels[y * 64 + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        sr.sprite = sprite;
        sr.color = Color.red;
        
        // Set scale
        obj.transform.localScale = Vector3.one * 0.5f;
        
        // Add LockOnIndicator component
        LockOnIndicator indicator = obj.AddComponent<LockOnIndicator>();
        
        return obj;
    }
    
    void HideIndicator()
    {
        if (currentIndicator != null)
        {
            currentIndicator.Hide();
        }
    }
    
    // Helper method to find assets
    T FindAsset<T>(string name) where T : Object
    {
        T[] assets = Resources.FindObjectsOfTypeAll<T>();
        foreach (T asset in assets)
        {
            if (asset.name == name)
                return asset;
        }
        return null;
    }
    
    void OnGUI()
    {
        if (!enableDebugging) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== TARGET SPRITE DEBUGGER ===");
        GUILayout.Space(10);
        
        GUILayout.Label($"Press {testKey} to test full lock-on system");
        GUILayout.Label($"Press {createIndicatorKey} to create indicator manually");
        GUILayout.Label($"Press {hideIndicatorKey} to hide indicator");
        GUILayout.Space(10);
        
        if (testTarget != null)
        {
            GUILayout.Label($"Test Target: {testTarget.name}");
        }
        else
        {
            GUILayout.Label("Test Target: None assigned");
        }
        
        if (lockOnIndicatorPrefab != null)
        {
            GUILayout.Label("Indicator Prefab: Found");
        }
        else
        {
            GUILayout.Label("Indicator Prefab: Missing");
        }
        
        if (currentIndicator != null)
        {
            GUILayout.Label($"Current Indicator: {currentIndicator.name}");
            GUILayout.Label($"Active: {currentIndicator.gameObject.activeInHierarchy}");
        }
        else
        {
            GUILayout.Label("Current Indicator: None");
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Test Full System"))
        {
            TestFullLockOnSystem();
        }
        
        if (GUILayout.Button("Create Indicator"))
        {
            CreateAndShowIndicator();
        }
        
        if (GUILayout.Button("Hide Indicator"))
        {
            HideIndicator();
        }
        
        GUILayout.EndArea();
    }
} 