using UnityEngine;

/// <summary>
/// Base implementation of ITargetable for enemies
/// Can be added to any enemy GameObject to make it targetable by Nyx's systems
/// </summary>
public class TargetableEnemy : MonoBehaviour, ITargetable
{
    [Header("Target Settings")]
    [SerializeField] private TargetType targetType = TargetType.Enemy;
    [SerializeField] private int priority = 1;
    [SerializeField] private Transform targetPoint; // Specific point to target (e.g., head, center mass)
    [SerializeField] private bool canBeTargetedWhenDead = false;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject targetIndicator; // Optional visual indicator when targeted
    [SerializeField] private bool showTargetEffects = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip targetSelectedSound;
    [SerializeField] private AudioClip targetDeselectedSound;
    
    // Component references (auto-detected)
    private Enemy_Basic enemyBasic;
    private Worker worker;
    private Boss boss;
    private Collider targetCollider;
    private Renderer[] renderers;
    private AudioSource audioSource;
    
    // State
    private bool isCurrentlyTargeted = false;
    private Material[] originalMaterials;
    private Color[] originalColors;
    
    // ITargetable implementation
    public Transform Transform => transform;
    public Transform TargetPoint => targetPoint != null ? targetPoint : transform;
    public int TargetPriority => priority;
    public TargetType TargetType => targetType;
    
    public bool CanBeTargeted 
    { 
        get 
        {
            // Check if enemy is alive (unless we allow targeting when dead)
            if (!canBeTargetedWhenDead)
            {
                if (enemyBasic != null && enemyBasic.dead) return false;
                if (worker != null && worker.dead) return false;
                if (boss != null && boss.currentHealth <= 0) return false;
            }
            
            // Check if GameObject is active
            if (!gameObject.activeInHierarchy) return false;
            
            return true;
        }
    }
    
    void Awake()
    {
        // Auto-detect enemy components
        enemyBasic = GetComponent<Enemy_Basic>();
        worker = GetComponent<Worker>();
        boss = GetComponent<Boss>();
        
        // Get collider for bounds calculation
        targetCollider = GetComponent<Collider>();
        if (targetCollider == null)
        {
            targetCollider = GetComponentInChildren<Collider>();
        }
        
        // Get renderers for visual effects
        renderers = GetComponentsInChildren<Renderer>();
        
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (targetSelectedSound != null || targetDeselectedSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = 0.5f;
        }
        
        // Auto-assign target point if not set
        if (targetPoint == null)
        {
            // Try to find a reasonable target point (head, center, etc.)
            Transform head = FindChildByName("Head") ?? FindChildByName("head");
            if (head != null)
            {
                targetPoint = head;
            }
            else
            {
                // Create a target point at center mass
                GameObject targetObj = new GameObject("TargetPoint");
                targetObj.transform.SetParent(transform);
                targetObj.transform.localPosition = Vector3.up * (GetTargetBounds().size.y * 0.6f); // Roughly chest/head height
                targetPoint = targetObj.transform;
            }
        }
        
        // Set target type based on enemy type
        if (boss != null)
        {
            targetType = TargetType.Boss;
            priority = 10; // Bosses have high priority
        }
        else if (enemyBasic != null)
        {
            targetType = TargetType.Enemy;
            priority = 5; // Regular enemies
        }
        else if (worker != null)
        {
            targetType = TargetType.Enemy;
            priority = 3; // Workers have lower priority
        }
        
        // Store original materials for visual effects
        StoreOriginalMaterials();
    }
    
    public void OnTargetSelected()
    {
        isCurrentlyTargeted = true;
        
        // Play sound effect
        if (audioSource != null && targetSelectedSound != null)
        {
            audioSource.PlayOneShot(targetSelectedSound);
        }
        
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
        
        // Notify enemy components if they have target selection logic
        if (enemyBasic != null)
        {
            // Could add custom logic here for when enemy is targeted
        }
        
        if (worker != null)
        {
            // Could add custom logic here for when worker is targeted
        }
        
        if (boss != null)
        {
            // Could add custom logic here for when boss is targeted
        }
    }
    
    public void OnTargetDeselected()
    {
        isCurrentlyTargeted = false;
        
        // Play sound effect
        if (audioSource != null && targetDeselectedSound != null)
        {
            audioSource.PlayOneShot(targetDeselectedSound);
        }
        
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
    }
    
    public Bounds GetTargetBounds()
    {
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
    /// Find a child transform by name (case insensitive)
    /// </summary>
    private Transform FindChildByName(string name)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.name.ToLower().Contains(name.ToLower()))
            {
                return child;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Store original materials for highlight effects
    /// </summary>
    private void StoreOriginalMaterials()
    {
        if (renderers == null || renderers.Length == 0) return;
        
        originalMaterials = new Material[renderers.Length];
        originalColors = new Color[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                originalMaterials[i] = renderers[i].material;
                
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
                // Add a slight red tint or outline effect
                Color highlightColor = originalColors[i] * 1.2f; // Brighten slightly
                highlightColor.r = Mathf.Min(highlightColor.r + 0.3f, 1f); // Add red tint
                
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
    /// Manually set the target priority (useful for dynamic priority changes)
    /// </summary>
    public void SetPriority(int newPriority)
    {
        priority = newPriority;
    }
    
    /// <summary>
    /// Manually override the target type
    /// </summary>
    public void SetTargetType(TargetType newType)
    {
        targetType = newType;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw target point
        if (targetPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPoint.position, 0.3f);
            
            // Draw line from transform to target point
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPoint.position);
        }
        
        // Draw bounds
        Bounds bounds = GetTargetBounds();
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        // Show priority as text
        #if UNITY_EDITOR
        if (isCurrentlyTargeted)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"TARGETED\nPriority: {priority}");
        }
        else
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Priority: {priority}");
        }
        #endif
    }
} 