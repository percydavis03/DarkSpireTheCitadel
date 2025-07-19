using UnityEngine;

/// <summary>
/// Wrapper component that makes Grappleable objects implement ITargetable
/// This allows existing Grappleable objects to work with the new targeting system
/// </summary>
[RequireComponent(typeof(Grappleable))]
public class TargetableGrappleable : MonoBehaviour, ITargetable
{
    [Header("Target Settings")]
    [SerializeField] private int priority = 2; // Lower than enemies by default
    [SerializeField] private bool showTargetEffects = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject targetIndicator; // Optional visual indicator when targeted
    
    // Component references
    private Grappleable grappleable;
    private Renderer[] renderers;
    private Color[] originalColors;
    
    // State
    private bool isCurrentlyTargeted = false;
    
    // ITargetable implementation
    public Transform Transform => transform;
    public Transform TargetPoint => grappleable.grapplePoint != null ? grappleable.grapplePoint : transform;
    public int TargetPriority => priority;
    public TargetType TargetType => grappleable.isEnemy ? TargetType.Enemy : TargetType.Grappleable;
    
    public bool CanBeTargeted 
    { 
        get 
        {
            if (grappleable == null) return false;
            
            // Use Grappleable's logic for determining if it can be targeted
            if (!grappleable.canBeGrappled) return false;
            if (grappleable.IsBeingGrappled) return false;
            
            // Check if GameObject is active
            if (!gameObject.activeInHierarchy) return false;
            
            return true;
        }
    }
    
    void Awake()
    {
        // Get the required Grappleable component
        grappleable = GetComponent<Grappleable>();
        if (grappleable == null)
        {
            Debug.LogError($"TargetableGrappleable on {gameObject.name} requires a Grappleable component!");
            enabled = false;
            return;
        }
        
        // Get renderers for visual effects
        renderers = GetComponentsInChildren<Renderer>();
        
        // Adjust priority based on grappleable type
        if (grappleable.isEnemy)
        {
            priority = 4; // Enemy grapplebales have higher priority than regular objects
        }
        else if (grappleable.pullable)
        {
            priority = 2; // Pullable objects
        }
        else
        {
            priority = 1; // Interactive objects
        }
        
        // Store original colors for highlight effects
        StoreOriginalColors();
    }
    
    public void OnTargetSelected()
    {
        isCurrentlyTargeted = true;
        
        // Show visual indicator
        if (targetIndicator != null)
        {
            targetIndicator.SetActive(true);
        }
        
        // Apply target highlight effect
        if (showTargetEffects)
        {
            ApplyTargetHighlight();
        }
        
        // Could notify the Grappleable component if needed
        // grappleable.OnTargetSelected(); // If this method existed
    }
    
    public void OnTargetDeselected()
    {
        isCurrentlyTargeted = false;
        
        // Hide visual indicator
        if (targetIndicator != null)
        {
            targetIndicator.SetActive(false);
        }
        
        // Remove target highlight effect
        if (showTargetEffects)
        {
            RemoveTargetHighlight();
        }
        
        // Could notify the Grappleable component if needed
        // grappleable.OnTargetDeselected(); // If this method existed
    }
    
    public Bounds GetTargetBounds()
    {
        Collider targetCollider = GetComponent<Collider>();
        if (targetCollider != null)
        {
            return targetCollider.bounds;
        }
        
        // Fallback: calculate bounds from renderers
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        
        // Last resort: use transform position with default size
        return new Bounds(transform.position, Vector3.one * 2f);
    }
    
    /// <summary>
    /// Store original colors for highlight effects
    /// </summary>
    private void StoreOriginalColors()
    {
        if (renderers == null || renderers.Length == 0) return;
        
        originalColors = new Color[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                // Try to get the main color
                if (renderers[i].material.HasProperty("_Color"))
                {
                    originalColors[i] = renderers[i].material.color;
                }
                else if (renderers[i].material.HasProperty("_BaseColor"))
                {
                    originalColors[i] = renderers[i].material.GetColor("_BaseColor");
                }
                else
                {
                    originalColors[i] = Color.white;
                }
            }
        }
    }
    
    /// <summary>
    /// Apply visual highlight when targeted
    /// </summary>
    private void ApplyTargetHighlight()
    {
        if (renderers == null || renderers.Length == 0) return;
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                // Add a slight blue/green tint for grappleable objects
                Color highlightColor = originalColors[i] * 1.2f; // Brighten slightly
                
                if (grappleable.pullable)
                {
                    highlightColor.g = Mathf.Min(highlightColor.g + 0.3f, 1f); // Add green tint for pullable
                }
                else
                {
                    highlightColor.b = Mathf.Min(highlightColor.b + 0.3f, 1f); // Add blue tint for interactive
                }
                
                if (renderers[i].material.HasProperty("_Color"))
                {
                    renderers[i].material.color = highlightColor;
                }
                else if (renderers[i].material.HasProperty("_BaseColor"))
                {
                    renderers[i].material.SetColor("_BaseColor", highlightColor);
                }
            }
        }
    }
    
    /// <summary>
    /// Remove visual highlight
    /// </summary>
    private void RemoveTargetHighlight()
    {
        if (renderers == null || renderers.Length == 0) return;
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null && i < originalColors.Length)
            {
                if (renderers[i].material.HasProperty("_Color"))
                {
                    renderers[i].material.color = originalColors[i];
                }
                else if (renderers[i].material.HasProperty("_BaseColor"))
                {
                    renderers[i].material.SetColor("_BaseColor", originalColors[i]);
                }
            }
        }
    }
    
    /// <summary>
    /// Check if this target is currently being targeted by any system
    /// </summary>
    public bool IsCurrentlyTargeted => isCurrentlyTargeted;
    
    /// <summary>
    /// Get the underlying Grappleable component
    /// </summary>
    public Grappleable Grappleable => grappleable;
    
    void OnDrawGizmosSelected()
    {
        // Draw grapple point
        if (grappleable != null && grappleable.grapplePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(grappleable.grapplePoint.position, 0.3f);
            
            // Draw line from transform to grapple point
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, grappleable.grapplePoint.position);
        }
        
        // Draw bounds
        Bounds bounds = GetTargetBounds();
        Gizmos.color = grappleable != null && grappleable.pullable ? Color.green : Color.blue;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        #if UNITY_EDITOR
        string typeText = grappleable != null ? (grappleable.pullable ? "PULLABLE" : "INTERACTIVE") : "GRAPPLEABLE";
        if (isCurrentlyTargeted)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"TARGETED\n{typeText}\nPriority: {priority}");
        }
        else
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"{typeText}\nPriority: {priority}");
        }
        #endif
    }
} 