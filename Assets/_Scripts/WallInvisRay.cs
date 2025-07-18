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
    private Dictionary<GameObject, WallGroupData> affectedWallGroups = new Dictionary<GameObject, WallGroupData>();
    private List<GameObject> wallsToRemove = new List<GameObject>();
    
    // Data structure to track wall group (parent + all children renderers) state
    private class WallGroupData
    {
        public List<Renderer> renderers = new List<Renderer>();
        public List<Material[]> originalMaterials = new List<Material[]>();
        public List<Material[]> fadeMaterials = new List<Material[]>();
        public List<bool> originallyActive = new List<bool>();
        public float currentAlpha = 1f;
        public Coroutine fadeCoroutine;
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
        
        // Track which wall groups are currently blocking
        HashSet<GameObject> currentlyBlockingWallGroups = new HashSet<GameObject>();
        
        foreach (RaycastHit hit in hits)
        {
            // Simply use the hit object as the wall group (it will include all its children)
            GameObject wallGroup = hit.collider.gameObject;
            
            currentlyBlockingWallGroups.Add(wallGroup);
            
            // If this wall group isn't being tracked yet, start tracking it
            if (!affectedWallGroups.ContainsKey(wallGroup))
            {
                StartTrackingWallGroup(wallGroup);
            }
            
            // Fade out this wall group
            FadeWallGroup(wallGroup, 0f);
        }
        
        // Check for wall groups that are no longer blocking and should fade back in
        wallsToRemove.Clear();
        foreach (var kvp in affectedWallGroups)
        {
            GameObject wallGroup = kvp.Key;
            if (wallGroup == null)
            {
                wallsToRemove.Add(wallGroup);
                continue;
            }
            
            if (!currentlyBlockingWallGroups.Contains(wallGroup))
            {
                // This wall group is no longer blocking, fade it back in
                FadeWallGroup(wallGroup, 1f);
            }
        }
        
        // Clean up null references
        foreach (GameObject wallGroup in wallsToRemove)
        {
            if (affectedWallGroups.ContainsKey(wallGroup))
                affectedWallGroups.Remove(wallGroup);
        }
    }
    

    
    void StartTrackingWallGroup(GameObject wallGroup)
    {
        WallGroupData wallGroupData = new WallGroupData();
        
        // Get all renderers in this wall group (including children)
        Renderer[] renderers = wallGroup.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            wallGroupData.renderers.Add(renderer);
            wallGroupData.originalMaterials.Add(renderer.materials);
            wallGroupData.originallyActive.Add(renderer.enabled);
            
            // Create fade materials (copies that support transparency)
            Material[] fadeMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                fadeMaterials[i] = new Material(renderer.materials[i]);
                SetupTransparencyMaterial(fadeMaterials[i], renderer.materials[i]);
            }
            wallGroupData.fadeMaterials.Add(fadeMaterials);
        }
        
        affectedWallGroups[wallGroup] = wallGroupData;
    }
    
    void SetupTransparencyMaterial(Material fadeMaterial, Material originalMaterial)
    {
        // Preserve all original properties first
        if (originalMaterial.HasProperty("_BaseMap") && fadeMaterial.HasProperty("_BaseMap"))
        {
            fadeMaterial.SetTexture("_BaseMap", originalMaterial.GetTexture("_BaseMap"));
        }
        if (originalMaterial.HasProperty("_MainTex") && fadeMaterial.HasProperty("_MainTex"))
        {
            fadeMaterial.SetTexture("_MainTex", originalMaterial.GetTexture("_MainTex"));
        }
        if (originalMaterial.HasProperty("_BaseColor") && fadeMaterial.HasProperty("_BaseColor"))
        {
            fadeMaterial.SetColor("_BaseColor", originalMaterial.GetColor("_BaseColor"));
        }
        if (originalMaterial.HasProperty("_Color") && fadeMaterial.HasProperty("_Color"))
        {
            fadeMaterial.SetColor("_Color", originalMaterial.GetColor("_Color"));
        }
        
        // URP Lit specific transparency setup
        if (fadeMaterial.HasProperty("_Surface"))
        {
            fadeMaterial.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
        }
        
        if (fadeMaterial.HasProperty("_Blend"))
        {
            fadeMaterial.SetFloat("_Blend", 0); // 0 = Alpha, 1 = Premultiply, 2 = Additive, 3 = Multiply
        }
        
        // Set blend modes for transparency
        fadeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        fadeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        fadeMaterial.SetInt("_ZWrite", 0);
        fadeMaterial.renderQueue = 3000;
        
        // URP keywords
        fadeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        fadeMaterial.EnableKeyword("_ALPHAPREMULTIPLY_OFF");
        fadeMaterial.DisableKeyword("_ALPHATEST_ON");
        
        // Legacy Standard Shader fallback
        if (fadeMaterial.HasProperty("_Mode"))
        {
            fadeMaterial.SetFloat("_Mode", 2); // Fade mode for Standard Shader
            fadeMaterial.EnableKeyword("_ALPHABLEND_ON");
            fadeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
    }
    
    void FadeWallGroup(GameObject wallGroup, float targetAlpha)
    {
        if (!affectedWallGroups.ContainsKey(wallGroup))
            return;
        
        WallGroupData wallGroupData = affectedWallGroups[wallGroup];
        
        // Stop any existing fade coroutine
        if (wallGroupData.fadeCoroutine != null)
        {
            StopCoroutine(wallGroupData.fadeCoroutine);
        }
        
        wallGroupData.fadeCoroutine = StartCoroutine(FadeWallGroupCoroutine(wallGroup, wallGroupData, targetAlpha));
    }
    
    IEnumerator FadeWallGroupCoroutine(GameObject wallGroup, WallGroupData wallGroupData, float targetAlpha)
    {
        float startAlpha = wallGroupData.currentAlpha;
        float elapsed = 0f;
        
        while (elapsed < 1f / fadeSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed * fadeSpeed);
            wallGroupData.currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            
            // Apply alpha to all renderers in the group
            for (int rendererIndex = 0; rendererIndex < wallGroupData.renderers.Count; rendererIndex++)
            {
                Renderer renderer = wallGroupData.renderers[rendererIndex];
                if (renderer == null) continue;
                
                Material[] fadeMaterials = wallGroupData.fadeMaterials[rendererIndex];
                
                // Apply alpha to materials
                for (int i = 0; i < fadeMaterials.Length; i++)
                {
                    Material mat = fadeMaterials[i];
                    
                    // Try different common alpha properties
                    if (mat.HasProperty("_Color"))
                    {
                        Color color = mat.color;
                        color.a = wallGroupData.currentAlpha;
                        mat.color = color;
                    }
                    else if (mat.HasProperty("_BaseColor"))
                    {
                        Color color = mat.GetColor("_BaseColor");
                        color.a = wallGroupData.currentAlpha;
                        mat.SetColor("_BaseColor", color);
                    }
                    else if (mat.HasProperty("_MainColor"))
                    {
                        Color color = mat.GetColor("_MainColor");
                        color.a = wallGroupData.currentAlpha;
                        mat.SetColor("_MainColor", color);
                    }
                    
                    // Also try setting alpha directly if available
                    if (mat.HasProperty("_Alpha"))
                    {
                        mat.SetFloat("_Alpha", wallGroupData.currentAlpha);
                    }
                }
                
                // Update renderer materials
                if (wallGroupData.currentAlpha < 1f)
                {
                    renderer.materials = fadeMaterials;
                    renderer.enabled = true;
                }
                else
                {
                    renderer.materials = wallGroupData.originalMaterials[rendererIndex];
                }
                
                // Optionally disable renderer when fully invisible for performance
                if (disableRendererWhenInvisible && wallGroupData.currentAlpha <= 0.01f)
                {
                    renderer.enabled = false;
                }
                else if (!renderer.enabled && wallGroupData.currentAlpha > 0.01f)
                {
                    renderer.enabled = true;
                }
            }
            
            yield return null;
        }
        
        // Ensure final state
        wallGroupData.currentAlpha = targetAlpha;
        
        if (targetAlpha >= 1f)
        {
            // Fully visible - restore original materials for all renderers
            for (int rendererIndex = 0; rendererIndex < wallGroupData.renderers.Count; rendererIndex++)
            {
                Renderer renderer = wallGroupData.renderers[rendererIndex];
                if (renderer != null)
                {
                    renderer.materials = wallGroupData.originalMaterials[rendererIndex];
                    renderer.enabled = wallGroupData.originallyActive[rendererIndex];
                }
            }
            
            // Clean up this wall group from tracking if it's back to normal
            if (wallGroupData.currentAlpha >= 1f)
            {
                // Clean up fade materials
                for (int rendererIndex = 0; rendererIndex < wallGroupData.fadeMaterials.Count; rendererIndex++)
                {
                    Material[] fadeMaterials = wallGroupData.fadeMaterials[rendererIndex];
                    for (int i = 0; i < fadeMaterials.Length; i++)
                    {
                        if (fadeMaterials[i] != null)
                            DestroyImmediate(fadeMaterials[i]);
                    }
                }
                affectedWallGroups.Remove(wallGroup);
            }
        }
        else if (disableRendererWhenInvisible && targetAlpha <= 0.01f)
        {
            for (int rendererIndex = 0; rendererIndex < wallGroupData.renderers.Count; rendererIndex++)
            {
                Renderer renderer = wallGroupData.renderers[rendererIndex];
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }
        }
        
        wallGroupData.fadeCoroutine = null;
    }
    
    void OnDestroy()
    {
        // Clean up created materials
        foreach (var kvp in affectedWallGroups)
        {
            WallGroupData wallGroupData = kvp.Value;
            if (wallGroupData.fadeMaterials != null)
            {
                for (int rendererIndex = 0; rendererIndex < wallGroupData.fadeMaterials.Count; rendererIndex++)
                {
                    Material[] fadeMaterials = wallGroupData.fadeMaterials[rendererIndex];
                    for (int i = 0; i < fadeMaterials.Length; i++)
                    {
                        if (fadeMaterials[i] != null)
                            DestroyImmediate(fadeMaterials[i]);
                    }
                }
            }
        }
        affectedWallGroups.Clear();
    }
}
