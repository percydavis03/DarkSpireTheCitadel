using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lock-on indicator that appears above targeted enemies
/// Can work as both world-space and screen-space indicator
/// </summary>
public class LockOnIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    [SerializeField] private bool isWorldSpace = true; // World space vs screen space
    [SerializeField] private float heightOffset = 0.1f; // How high above enemy
    [SerializeField] private bool followTarget = true; // Should it follow the target
    [SerializeField] private bool faceCamera = true; // Should it always face camera
    
    [Header("Animation")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseScale = 0.1f;
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 90f; // degrees per second
    
    [Header("Visual Elements")]
    [SerializeField] private Image iconImage; // UI Image component
    [SerializeField] private SpriteRenderer spriteRenderer; // For world space
    [SerializeField] private Sprite lockOnSprite; // The lock-on icon
    [SerializeField] private Color indicatorColor = Color.red;
    [SerializeField] private Gradient colorOverTime; // Optional color animation
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoFindComponents = true;
    
    // Target tracking
    private Transform targetTransform;
    private Camera mainCamera;
    private Canvas worldCanvas;
    private Vector3 originalScale;
    private float animationTime;
    
    // Components
    private RectTransform rectTransform;
    
    void Awake()
    {
        if (autoFindComponents)
        {
            SetupComponents();
        }
        
        // Store original scale for pulsing
        originalScale = transform.localScale;
        
        // Find main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
    }
    
    void Start()
    {
        // Initially hide the indicator
        gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (targetTransform != null && followTarget)
        {
            UpdatePosition();
        }
        
        if (faceCamera && mainCamera != null)
        {
            FaceCamera();
        }
        
        UpdateAnimation();
    }
    
    /// <summary>
    /// Show the indicator above the specified target
    /// </summary>
    public void ShowOnTarget(Transform target)
    {
        targetTransform = target;
        gameObject.SetActive(true);
        
        if (target != null)
        {
            UpdatePosition();
            Debug.Log($"🔴 INDICATOR POSITION: {transform.position} (Target: {target.position})");
            Debug.Log($"🔴 INDICATOR SCALE: {transform.localScale}");
            Debug.Log($"🔴 INDICATOR ACTIVE: {gameObject.activeInHierarchy}");
        }
        
        // Reset animation
        animationTime = 0f;
        
        Debug.Log($"Lock-on indicator shown on: {target?.name}");
    }
    
    /// <summary>
    /// Hide the indicator
    /// </summary>
    public void Hide()
    {
        targetTransform = null;
        gameObject.SetActive(false);
        
        Debug.Log("Lock-on indicator hidden");
    }
    
    /// <summary>
    /// Update the indicator position to follow target
    /// </summary>
    private void UpdatePosition()
    {
        if (targetTransform == null) return;
        
        if (isWorldSpace)
        {
            // World space positioning
            Vector3 targetPosition = targetTransform.position;
            
            // Add height offset (try to get bounds height if possible)
            Collider targetCollider = targetTransform.GetComponent<Collider>();
            if (targetCollider != null)
            {
                targetPosition.y = targetCollider.bounds.max.y + heightOffset;
            }
            else
            {
                targetPosition.y += heightOffset;
            }
            
            transform.position = targetPosition;
        }
        else if (mainCamera != null && rectTransform != null)
        {
            // Screen space positioning
            Vector3 targetWorldPos = targetTransform.position;
            
            // Add height offset
            Collider targetCollider = targetTransform.GetComponent<Collider>();
            if (targetCollider != null)
            {
                targetWorldPos.y = targetCollider.bounds.max.y + heightOffset;
            }
            else
            {
                targetWorldPos.y += heightOffset;
            }
            
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetWorldPos);
            
            // Only show if in front of camera
            if (screenPos.z > 0)
            {
                rectTransform.position = screenPos;
                if (!gameObject.activeSelf) gameObject.SetActive(true);
            }
            else
            {
                if (gameObject.activeSelf) gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Make the indicator always face the camera
    /// </summary>
    private void FaceCamera()
    {
        if (isWorldSpace && mainCamera != null)
        {
            Vector3 directionToCamera = mainCamera.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
    }
    
    /// <summary>
    /// Update animations (pulsing, rotation, color)
    /// </summary>
    private void UpdateAnimation()
    {
        animationTime += Time.deltaTime;
        
        // Pulsing animation
        if (enablePulse)
        {
            float pulse = Mathf.Sin(animationTime * pulseSpeed) * pulseScale;
            transform.localScale = originalScale + Vector3.one * pulse;
        }
        
        // Rotation animation
        if (enableRotation && isWorldSpace)
        {
            // Only rotate around Y axis to keep facing camera
            Vector3 currentEuler = transform.eulerAngles;
            currentEuler.y += rotationSpeed * Time.deltaTime;
            
            // Maintain camera-facing while rotating
            if (faceCamera && mainCamera != null)
            {
                Vector3 directionToCamera = mainCamera.transform.position - transform.position;
                Quaternion cameraRotation = Quaternion.LookRotation(-directionToCamera);
                transform.rotation = cameraRotation * Quaternion.Euler(0, currentEuler.y, 0);
            }
            else
            {
                transform.eulerAngles = currentEuler;
            }
        }
        
        // Color animation
        if (colorOverTime != null && colorOverTime.colorKeys.Length > 1)
        {
            float colorTime = (animationTime * 0.5f) % 1f; // Slower color cycle
            Color animatedColor = colorOverTime.Evaluate(colorTime);
            
            if (iconImage != null)
            {
                iconImage.color = animatedColor;
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.color = animatedColor;
            }
        }
    }
    
    /// <summary>
    /// Automatically find and setup components
    /// </summary>
    private void SetupComponents()
    {
        // Get RectTransform for UI positioning
        rectTransform = GetComponent<RectTransform>();
        
        // Try to find Image component
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
            if (iconImage == null)
            {
                iconImage = GetComponentInChildren<Image>();
            }
        }
        
        // Try to find SpriteRenderer component
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
        
        // Set up based on what we found
        if (iconImage != null)
        {
            isWorldSpace = false; // UI Image means screen space
            if (lockOnSprite != null)
            {
                iconImage.sprite = lockOnSprite;
            }
            iconImage.color = indicatorColor;
        }
        else if (spriteRenderer != null)
        {
            isWorldSpace = true; // SpriteRenderer means world space
            if (lockOnSprite != null)
            {
                spriteRenderer.sprite = lockOnSprite;
            }
            spriteRenderer.color = indicatorColor;
        }
        
        Debug.Log($"Lock-on indicator setup: WorldSpace={isWorldSpace}, Image={iconImage != null}, SpriteRenderer={spriteRenderer != null}");
    }
    
    /// <summary>
    /// Set the indicator color
    /// </summary>
    public void SetColor(Color color)
    {
        indicatorColor = color;
        
        if (iconImage != null)
        {
            iconImage.color = color;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
    
    /// <summary>
    /// Set the indicator sprite
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        lockOnSprite = sprite;
        
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
        }
    }
} 