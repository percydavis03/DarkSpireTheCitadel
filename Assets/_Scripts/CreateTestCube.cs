using UnityEngine;

public class CreateTestCube : MonoBehaviour
{
    [Header("Test Cube Creation")]
    [Tooltip("Click to create a test cube for grappling")]
    public bool createTestCube = false;
    
    void Update()
    {
        if (createTestCube)
        {
            createTestCube = false;
            CreateGrappleTestCube();
        }
    }
    
    [ContextMenu("Create Grapple Test Cube")]
    public void CreateGrappleTestCube()
    {
        // Find player position
        Player_Movement player = FindObjectOfType<Player_Movement>();
        Vector3 spawnPosition = Vector3.zero + Vector3.forward * 5f + Vector3.up * 1f;
        
        if (player != null)
        {
            spawnPosition = player.transform.position + player.transform.forward * 3f + Vector3.up * 1f;
        }
        
        // Create test cube
        GameObject testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        testCube.name = "GRAPPLE TEST CUBE";
        testCube.transform.position = spawnPosition;
        testCube.transform.localScale = Vector3.one * 0.5f;
        
        // Set tag
        testCube.tag = "CanBeGrappled";
        
        // Add components
        Grappleable grappleable = testCube.AddComponent<Grappleable>();
        grappleable.canBeGrappled = true;
        grappleable.pullable = true;
        
        Rigidbody rb = testCube.AddComponent<Rigidbody>();
        rb.mass = 1f;
        
        // Make it bright green so it's visible
        Renderer renderer = testCube.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.green;
            renderer.material = mat;
        }
        
        Debug.Log($"✅ Created grapple test cube at {spawnPosition}");
        Debug.Log("🎯 Stand close to it (within 5 units), look at it, and hold G!");
        
        // Log distance to player for reference
        if (player != null)
        {
            float distance = Vector3.Distance(spawnPosition, player.transform.position);
            Debug.Log($"📏 Distance from player: {distance:F1} units (should be under 10)");
        }
    }
} 