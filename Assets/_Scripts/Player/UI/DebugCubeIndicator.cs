using UnityEngine;

/// <summary>
/// Simple debug cube that appears above locked enemies
/// Use this to test if the positioning system works
/// </summary>
public class DebugCubeIndicator : MonoBehaviour
{
    [Header("Debug Cube")]
    [SerializeField] private bool enableDebugCube = true;
    [SerializeField] private float cubeSize = 1f;
    [SerializeField] private float heightOffset = 0.15f; // Slightly higher than main indicator
    [SerializeField] private Color cubeColor = Color.yellow;
    
    private GameObject debugCube;
    private Transform targetEnemy;
    
    public void ShowOnEnemy(Transform enemy)
    {
        if (!enableDebugCube) return;
        
        targetEnemy = enemy;
        
        // Create or reuse debug cube
        if (debugCube == null)
        {
            debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugCube.name = "DEBUG_LOCK_ON_CUBE";
            
            // Remove collider
            Collider cubeCollider = debugCube.GetComponent<Collider>();
            if (cubeCollider != null)
            {
                DestroyImmediate(cubeCollider);
            }
            
            // Set color
            Renderer cubeRenderer = debugCube.GetComponent<Renderer>();
            if (cubeRenderer != null)
            {
                cubeRenderer.material.color = cubeColor;
            }
        }
        
        // Position above enemy
        if (enemy != null)
        {
            Vector3 position = enemy.position;
            position.y += heightOffset;
            debugCube.transform.position = position;
            debugCube.transform.localScale = Vector3.one * cubeSize;
            debugCube.SetActive(true);
            
            Debug.Log($"🟡 DEBUG CUBE positioned at: {position} (Enemy: {enemy.name})");
        }
    }
    
    public void Hide()
    {
        if (debugCube != null)
        {
            debugCube.SetActive(false);
        }
        targetEnemy = null;
        
        Debug.Log("🟡 DEBUG CUBE hidden");
    }
    
    void Update()
    {
        // Follow the target enemy
        if (enableDebugCube && debugCube != null && debugCube.activeInHierarchy && targetEnemy != null)
        {
            Vector3 position = targetEnemy.position;
            position.y += heightOffset;
            debugCube.transform.position = position;
        }
    }
    
    void OnDestroy()
    {
        if (debugCube != null)
        {
            DestroyImmediate(debugCube);
        }
    }
} 