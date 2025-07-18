using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallInvisRay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask wallLayerMask = -1; // Which layers count as walls (set to WALL layer in inspector)
    [SerializeField] private float fadeSpeed = 5f; // How fast walls fade in/out
    [SerializeField] private float raycastDistance = 100f; // Max distance to check for walls
    [SerializeField] private bool disableRendererWhenInvisible = true; // Disable renderer for performance when fully faded
    
    [Header("Debug")]
    [SerializeField] private bool showDebugRay = false;
    
    private Transform playerTransform;
    private Camera cameraComponent;
    private Dictionary<Transform, WallGroupData> affectedWallGroups = new Dictionary<Transform, WallGroupData>();
    private List<Transform> wallGroupsToRemove = new List<Transform>();
    
    // Data structure to track wall group state
    private class WallGroupData
    {
        public List<Renderer> renderers = new List<Renderer>();
        public List<WallRendererData> rendererData = new List<WallRendererData>();
        public float currentAlpha = 1f;
        public Coroutine fadeCoroutine;
    }
    
    private class WallRendererData
    {
        public Material[] originalMaterials;
        public Material[] fadeMaterials;
        public bool isOriginallyActive = true;
    }
    
    void Start()
    {
        // Find the camera component
        cameraComponent = GetComponent<Camera>();
        if (cameraComponent == null)
            cameraComponent = Camera.main;
        
        // Find player tagged "Nyx"
        FindPlayer();
    }
    
    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Nyx");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log("WallInvisRay: Found player tagged 'Nyx'");
        }
        else
        {
            Debug.LogWarning("WallInvisRay: No GameObject found with tag 'Nyx'!");
        }
    }
    
    void Update()
    {
        if (playerTransform == null || cameraComponent == null)
        {
            // Try to find player again if it wasn't found initially
            if (playerTransform == null)
                FindPlayer();
            return;
        }
        
        CheckWallsBlockingPlayer();
    }
    
    void CheckWallsBlockingPlayer()
    {
        Vector3 cameraPosition = cameraComponent.transform.position;
        Vector3 playerPosition = playerTransform.position;
        Vector3 direction = (playerPosition - cameraPosition).normalized;
        float distance = Vector3.Distance(cameraPosition, playerPosition);
        
        // Raycast from camera to player
        RaycastHit[] hits = Physics.RaycastAll(cameraPosition, direction, Mathf.Min(distance, raycastDistance), wallLayerMask);
        
        // Debug ray
        if (showDebugRay)
            Debug.DrawRay(cameraPosition, direction * distance, Color.red);
        
        // Track which walls are currently blocking
        HashSet<Renderer> currentlyBlockingWalls = new HashSet<Renderer>();
        
        foreach (RaycastHit hit in hits)
        {
            Renderer wallRenderer = hit.collider.GetComponent<Renderer>();
            if (wallRenderer != null)
            {
                currentlyBlockingWalls.Add(wallRenderer);
                
                // If this wall isn't being tracked yet, start tracking it
                if (!affectedWalls.ContainsKey(wallRenderer))
                {
                    StartTrackingWall(wallRenderer);
                }
                
                // Fade out this wall
                FadeWall(wallRenderer, 0f);
            }
        }
        
        // Check for walls that are no longer blocking and should fade back in
        wallsToRemove.Clear();
        foreach (var kvp in affectedWalls)
        {
            Renderer wallRenderer = kvp.Key;
            if (wallRenderer == null)
            {
                wallsToRemove.Add(wallRenderer);
                continue;
            }
            
            if (!currentlyBlockingWalls.Contains(wallRenderer))
            {
                // This wall is no longer blocking, fade it back in
                FadeWall(wallRenderer, 1f);
            }
        }
        
        // Clean up null references
        foreach (Renderer renderer in wallsToRemove)
        {
            if (affectedWalls.ContainsKey(renderer))
                affectedWalls.Remove(renderer);
        }
    }
    
    void StartTrackingWall(Renderer wallRenderer)
    {
        WallData wallData = new WallData();
        wallData.originalMaterials = wallRenderer.materials;
        wallData.isOriginallyActive = wallRenderer.enabled;
        
        // Create fade materials (copies that support transparency)
        wallData.fadeMaterials = new Material[wallData.originalMaterials.Length];
        for (int i = 0; i < wallData.originalMaterials.Length; i++)
        {
            wallData.fadeMaterials[i] = new Material(wallData.originalMaterials[i]);
            
            // Force transparency settings for URP Lit Shader
            Material mat = wallData.fadeMaterials[i];
            Material originalMat = wallData.originalMaterials[i];
            
            // Preserve all original properties first
            if (originalMat.HasProperty("_BaseMap") && mat.HasProperty("_BaseMap"))
            {
                mat.SetTexture("_BaseMap", originalMat.GetTexture("_BaseMap"));
            }
            if (originalMat.HasProperty("_MainTex") && mat.HasProperty("_MainTex"))
            {
                mat.SetTexture("_MainTex", originalMat.GetTexture("_MainTex"));
            }
            if (originalMat.HasProperty("_BaseColor") && mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", originalMat.GetColor("_BaseColor"));
            }
            if (originalMat.HasProperty("_Color") && mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", originalMat.GetColor("_Color"));
            }
            
            // URP Lit specific transparency setup
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
            }
            
            if (mat.HasProperty("_Blend"))
            {
                mat.SetFloat("_Blend", 0); // 0 = Alpha, 1 = Premultiply, 2 = Additive, 3 = Multiply
            }
            
            // Set blend modes for transparency
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            
            // URP keywords
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_OFF");
            mat.DisableKeyword("_ALPHATEST_ON");
            
            // Legacy Standard Shader fallback
            if (mat.HasProperty("_Mode"))
            {
                mat.SetFloat("_Mode", 2); // Fade mode for Standard Shader
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
        }
        
        affectedWalls[wallRenderer] = wallData;
    }
    
    void FadeWall(Renderer wallRenderer, float targetAlpha)
    {
        if (!affectedWalls.ContainsKey(wallRenderer))
            return;
        
        WallData wallData = affectedWalls[wallRenderer];
        
        // Stop any existing fade coroutine
        if (wallData.fadeCoroutine != null)
        {
            StopCoroutine(wallData.fadeCoroutine);
        }
        
        wallData.fadeCoroutine = StartCoroutine(FadeWallCoroutine(wallRenderer, wallData, targetAlpha));
    }
    
    IEnumerator FadeWallCoroutine(Renderer wallRenderer, WallData wallData, float targetAlpha)
    {
        float startAlpha = wallData.currentAlpha;
        float elapsed = 0f;
        
        while (elapsed < 1f / fadeSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed * fadeSpeed);
            wallData.currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            
            // Apply alpha to materials
            for (int i = 0; i < wallData.fadeMaterials.Length; i++)
            {
                Material mat = wallData.fadeMaterials[i];
                
                // Try different common alpha properties
                if (mat.HasProperty("_Color"))
                {
                    Color color = mat.color;
                    color.a = wallData.currentAlpha;
                    mat.color = color;
                }
                else if (mat.HasProperty("_BaseColor"))
                {
                    Color color = mat.GetColor("_BaseColor");
                    color.a = wallData.currentAlpha;
                    mat.SetColor("_BaseColor", color);
                }
                else if (mat.HasProperty("_MainColor"))
                {
                    Color color = mat.GetColor("_MainColor");
                    color.a = wallData.currentAlpha;
                    mat.SetColor("_MainColor", color);
                }
                
                // Also try setting alpha directly if available
                if (mat.HasProperty("_Alpha"))
                {
                    mat.SetFloat("_Alpha", wallData.currentAlpha);
                }
            }
            
            // Update renderer materials
            if (wallData.currentAlpha < 1f)
            {
                wallRenderer.materials = wallData.fadeMaterials;
                wallRenderer.enabled = true;
            }
            else
            {
                wallRenderer.materials = wallData.originalMaterials;
            }
            
            // Optionally disable renderer when fully invisible for performance
            if (disableRendererWhenInvisible && wallData.currentAlpha <= 0.01f)
            {
                wallRenderer.enabled = false;
            }
            else if (!wallRenderer.enabled && wallData.currentAlpha > 0.01f)
            {
                wallRenderer.enabled = true;
            }
            
            yield return null;
        }
        
        // Ensure final state
        wallData.currentAlpha = targetAlpha;
        
        if (targetAlpha >= 1f)
        {
            // Fully visible - restore original materials
            wallRenderer.materials = wallData.originalMaterials;
            wallRenderer.enabled = wallData.isOriginallyActive;
            
            // Clean up this wall from tracking if it's back to normal
            if (wallData.currentAlpha >= 1f)
            {
                // Clean up fade materials
                for (int i = 0; i < wallData.fadeMaterials.Length; i++)
                {
                    if (wallData.fadeMaterials[i] != null)
                        DestroyImmediate(wallData.fadeMaterials[i]);
                }
                affectedWalls.Remove(wallRenderer);
            }
        }
        else if (disableRendererWhenInvisible && targetAlpha <= 0.01f)
        {
            wallRenderer.enabled = false;
        }
        
        wallData.fadeCoroutine = null;
    }
    
    void OnDestroy()
    {
        // Clean up created materials
        foreach (var kvp in affectedWalls)
        {
            WallData wallData = kvp.Value;
            if (wallData.fadeMaterials != null)
            {
                for (int i = 0; i < wallData.fadeMaterials.Length; i++)
                {
                    if (wallData.fadeMaterials[i] != null)
                        DestroyImmediate(wallData.fadeMaterials[i]);
                }
            }
        }
        affectedWalls.Clear();
    }
}
