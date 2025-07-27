using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Grappleable : MonoBehaviour, ITargetable
{
    [Header("Grappleable Settings")]
    public bool canBeGrappled = true;
    public bool pullable = true; // Default is pullable
    public bool isEnemy = false;
    public Animator animator;
    public Transform grapplePoint; // The specific point where the grapple wrist will attach
    
    [Header("Player Save State Reference")]
    public PlayerSaveState playerSaveState; // Reference to check if player has arm upgrade
    
    [Header("Enemy Settings")]
    private NavMeshAgent navAgent;
    private Worker workerScript;
    private float originalSpeed;
    private bool navAgentWasEnabled;
    [Tooltip("Seconds the enemy remains stunned after being released from grapple before regaining movement.")]
    public float postReleaseStunDuration = 1f;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnGrappleStarted;
    public UnityEngine.Events.UnityEvent OnGrappleReleased;
    
    [Header("Targeting System")]
    [SerializeField] private int targetPriority = 2; // Lower than enemies by default
    [SerializeField] private bool showTargetEffects = true;
    [SerializeField] private GameObject targetIndicator; // Optional visual indicator when targeted
    
    protected bool isBeingGrappled = false;
    private bool isCurrentlyTargeted = false;
    private Renderer[] renderers;
    private Color[] originalColors;
    
    public bool IsBeingGrappled => isBeingGrappled;
    public bool IsPullable => pullable;
    
    // ITargetable implementation
    public Transform Transform => transform;
    public Transform TargetPoint => grapplePoint != null ? grapplePoint : transform;
    public int TargetPriority => isEnemy ? 4 : (pullable ? 2 : 1); // Enemy grapplebales have higher priority
    public TargetType TargetType => isEnemy ? TargetType.Enemy : TargetType.Grappleable;
    
    public bool CanBeTargeted 
    { 
        get 
        {
            // FIRST CHECK: Player must have arm upgrade to grapple anything
            if (playerSaveState != null && !playerSaveState.hasArm)
            {
                return false;
            }
            
            // Use existing Grappleable logic for determining if it can be targeted
            if (!canBeGrappled) return false;
            if (isBeingGrappled) return false;
            
            // Check if GameObject is active
            if (!gameObject.activeInHierarchy) return false;
            
            return true;
        }
    }
    
    void Start()
    {
        // Auto-find PlayerSaveState if not assigned
        if (playerSaveState == null)
        {
            // Try to find it from a Worker component first (if this is an enemy)
            Worker worker = GetComponent<Worker>();
            if (worker != null && worker.thisGameSave != null)
            {
                playerSaveState = worker.thisGameSave;
            }
            else
            {
                // Fallback: search for any object with PlayerSaveState in the scene
                Worker[] allWorkers = FindObjectsOfType<Worker>();
                foreach (Worker w in allWorkers)
                {
                    if (w.thisGameSave != null)
                    {
                        playerSaveState = w.thisGameSave;
                        break;
                    }
                }
                
                // If still not found, log a warning
                if (playerSaveState == null)
                {
                    Debug.LogWarning($"Grappleable {name}: Could not find PlayerSaveState reference. Grappling will be disabled until hasArm=true is verified.");
                }
            }
        }
        
        // Handle rigid body based on pullable setting
        Rigidbody rb = GetComponent<Rigidbody>();
        if (!pullable && rb != null)
        {
            // Remove or disable rigid body for non-pullable objects
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        if (isEnemy)
        {
            navAgent = GetComponent<NavMeshAgent>();
            workerScript = GetComponent<Worker>();
            if (navAgent != null)
            {
                originalSpeed = navAgent.speed;
            }
        }
        
        // Initialize targeting system components
        SetupTargetingSystem();
    }
    
    void SetupTargetingSystem()
    {
        // Get renderers for visual effects
        renderers = GetComponentsInChildren<Renderer>();
        
        // Store original colors for highlight effects
        StoreOriginalColors();
    }
    
    void StoreOriginalColors()
    {
        if (renderers != null && renderers.Length > 0)
        {
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null && renderers[i].material != null)
                {
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
    }
    
    // ITargetable interface methods
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
    }
    
    public Bounds GetTargetBounds()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds;
        }
        
        // Fallback: create bounds around transform
        return new Bounds(transform.position, Vector3.one);
    }
    
    void ApplyTargetHighlight()
    {
        if (renderers == null || renderers.Length == 0) return;
        
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].material != null)
            {
                // Add a slight blue/green tint for grappleable objects
                Color highlightColor = originalColors[i] * 1.2f; // Brighten slightly
                
                if (pullable)
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
    
    void RemoveTargetHighlight()
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
    
    // Called when this object starts being grappled
    public virtual void StartGrapple()
    {
        // FIRST CHECK: Player must have arm upgrade to grapple anything
        if (playerSaveState != null && !playerSaveState.hasArm)
        {
            Debug.LogWarning($"{gameObject.name}: Cannot be grappled - Player does not have arm upgrade (hasArm=false)");
            return;
        }
        
        if (!canBeGrappled) return;
        
        isBeingGrappled = true;
        
        // Handle enemy-specific behavior
        if (isEnemy)
        {
            // Set animator parameter
            if (animator != null)
            {
                animator.SetBool("isGrappled", true);
            }
            
            // Stop NavMeshAgent movement
            if (navAgent != null)
            {
                navAgentWasEnabled = navAgent.enabled;
                navAgent.enabled = false; // fully disables navigation but keeps animator running
            }
            
            // Stop Worker script behaviors
            if (workerScript != null)
            {
                workerScript.isHit = true;  // Use the existing hit state to prevent attacks
                workerScript.anim.SetBool("IsRunning", false);
                workerScript.anim.SetBool("IsAttacking", false);
            }
        }
        
        OnGrappleStarted?.Invoke();
        
        if (pullable)
        {
            Debug.Log($"{gameObject.name} is being grappled and can be pulled");
        }
        else
        {
            Debug.Log($"{gameObject.name} is being grappled but cannot be pulled");
        }
    }
    
    // Called when this object is released from grapple
    public virtual void ReleaseGrapple()
    {
        isBeingGrappled = false;
        
        // Handle enemy-specific behavior
        if (isEnemy)
        {
            // Reset animator parameter
            if (animator != null)
            {
                animator.SetBool("isGrappled", false);
            }
            
            // Resume NavMeshAgent movement
            if (navAgent != null)
            {
                navAgent.enabled = false; // keep disabled during stun
                StartCoroutine(ResumeMovementAfterDelay());
            }
            
            // Resume Worker script behaviors
            if (workerScript != null)
            {
                workerScript.isHit = false;  // Allow attacks again
                // Note: Don't need to set running/attacking - Worker script will handle these states
            }
        }
        
        OnGrappleReleased?.Invoke();
        Debug.Log($"{gameObject.name} grapple released");
    }

    private IEnumerator ResumeMovementAfterDelay()
    {
        yield return new WaitForSeconds(postReleaseStunDuration);
        
        if (isEnemy && navAgent != null)
        {
            navAgent.enabled = navAgentWasEnabled;
            if (navAgent.enabled)
            {
                navAgent.speed = originalSpeed;
                navAgent.isStopped = false;
            }
        }
    }
}
