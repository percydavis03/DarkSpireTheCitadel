using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

/// <summary>
/// Simple and lightweight lock-on system for Nyx
/// Allows cycling through enemies in camera view with middle mouse button
/// Features:
/// - Highlights all targetable enemies in camera view
/// - Special highlight for locked target vs in-view enemies
/// - Automatic highlight management when enemies enter/leave view
/// - Integration with TargetableEnemy component highlighting system
/// </summary>
public class SimpleNyxLockOn : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Transform nyxTransform;
    [SerializeField] private Transform playerTransform; // The actual player object that rotates
    [SerializeField] private Camera playerCamera;
    
    [Header("Lock-On Settings")]
    [SerializeField] private float lockOnRange = 25f;
    [SerializeField] private float cameraFieldOfView = 60f; // How wide the detection area is
    [SerializeField] private LayerMask enemyLayers = -1; // What layers enemies are on
    [SerializeField] private string[] enemyTags = {"Enemy"}; // Enemy tags to look for
    
    [Header("Player Rotation")]
    [SerializeField] private bool enableAutoRotation = false; // Disabled to prevent camera movement when switching targets
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool smoothRotation = true;
    
    [Header("Lock-On Indicator")]
    [SerializeField] private GameObject lockOnIndicatorPrefab;
    [SerializeField] private float indicatorHeightOffset = 2f;
    
    [Header("Enemy Highlighting")]
    [SerializeField] private bool highlightEnemiesInView = true; // Highlight all targetable enemies
    [SerializeField] private bool useTargetableEnemyHighlighting = true; // Use TargetableEnemy component highlighting
    
    [Header("Input")]
    [SerializeField] private PlayerInputActions playerControls;
    [SerializeField] private bool useScrollWheel = true; // Use scroll wheel instead of middle mouse if preferred
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    // Private variables
    private List<Transform> enemiesInView = new List<Transform>();
    private List<Transform> previousEnemiesInView = new List<Transform>(); // Track previous frame for highlight changes
    private int currentTargetIndex = -1; // -1 means no target locked
    private Transform currentLockedTarget;
    private GameObject currentIndicator;
    private InputAction cycleTargetAction;
    
    // Properties
    public Transform CurrentTarget => currentLockedTarget;
    public bool IsLockedOn => currentLockedTarget != null;
    public Transform NyxTransform => nyxTransform;
    
    void Awake()
    {
        // Auto-find components if not assigned
        if (nyxTransform == null)
        {
            GameObject nyxObj = GameObject.FindWithTag("Nyx");
            if (nyxObj != null) nyxTransform = nyxObj.transform;
        }
        
        if (playerTransform == null)
        {
            playerTransform = transform; // Default to this object
        }
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) playerCamera = FindObjectOfType<Camera>();
        }
        
        // Get player controls
        if (playerControls == null)
        {
            var playerMovement = GetComponent<Player_Movement>();
            if (playerMovement?.playerControls != null)
            {
                playerControls = playerMovement.playerControls;
            }
            else
            {
                playerControls = new PlayerInputActions();
            }
        }
    }
    
    void OnEnable()
    {
        if (playerControls != null)
        {
            cycleTargetAction = playerControls.General.CycleTargets;
            cycleTargetAction.Enable();
        }
    }
    
    void OnDisable()
    {
        cycleTargetAction?.Disable();
        ClearLockOn();
        
        // Clear all enemy highlights when disabling
        if (highlightEnemiesInView && useTargetableEnemyHighlighting)
        {
            ClearAllEnemyHighlights();
        }
    }
    
    void Update()
    {
        // Update enemies in view
        UpdateEnemiesInView();
        
        // Handle input for cycling targets
        HandleCycleInput();
        
        // Rotate player towards locked target
        if (IsLockedOn && enableAutoRotation)
        {
            RotateTowardsTarget();
        }
        
        // Update indicator position
        UpdateIndicator();
        
        // Debug info
        if (showDebugInfo && Time.frameCount % 60 == 0) // Every 1 second at 60fps
        {
            string enemyNames = enemiesInView.Count > 0 ? string.Join(", ", enemiesInView.ConvertAll(e => e.name)) : "None";
            
            // Also count total enemies vs alive enemies
            int totalEnemies = 0;
            foreach (string tag in enemyTags)
            {
                try
                {
                    totalEnemies += GameObject.FindGameObjectsWithTag(tag).Length;
                }
                catch (UnityException) { }
            }
            
            DebugLog($"Total enemies: {totalEnemies}, Alive in view: {enemiesInView.Count} [{enemyNames}], Locked: {(currentLockedTarget?.name ?? "None")}, Index: {currentTargetIndex}, Indicator: {(currentIndicator?.activeInHierarchy ?? false)}");
        }
    }
    
    private void UpdateEnemiesInView()
    {
        // Store previous enemies for highlight management
        previousEnemiesInView.Clear();
        previousEnemiesInView.AddRange(enemiesInView);
        
        enemiesInView.Clear();
        
        // Find all potential enemies
        GameObject[] allEnemies = new GameObject[0];
        foreach (string tag in enemyTags)
        {
            try
            {
                GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag(tag);
                allEnemies = allEnemies.Concat(taggedEnemies).ToArray();
            }
            catch (UnityException)
            {
                // Tag doesn't exist, skip it
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[SimpleNyxLockOn] Tag '{tag}' is not defined. Skipping this tag.");
                }
            }
        }
        
        foreach (GameObject enemy in allEnemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;
            
            // Check if enemy is alive/targetable
            if (!IsEnemyAlive(enemy)) continue;
            
            Transform enemyTransform = enemy.transform;
            
            // Check if enemy is within range
            float distance = Vector3.Distance(nyxTransform.position, enemyTransform.position);
            if (distance > lockOnRange) continue;
            
            // Check if enemy is in camera view
            if (IsInCameraView(enemyTransform))
            {
                enemiesInView.Add(enemyTransform);
            }
        }
        
        // Sort by distance (closest first)
        enemiesInView = enemiesInView.OrderBy(enemy => 
            Vector3.Distance(nyxTransform.position, enemy.position)).ToList();
        
        // Update enemy highlights
        if (highlightEnemiesInView && useTargetableEnemyHighlighting)
        {
            UpdateEnemyHighlights();
        }
        
        // Check if current locked target is still valid
        if (currentLockedTarget != null)
        {
            // Check if target is still alive
            if (!IsEnemyAlive(currentLockedTarget.gameObject))
            {
                DebugLog($"Current target {currentLockedTarget.name} died - clearing lock-on");
                ClearLockOn();
                return;
            }
            
            // Check if target is still in view
            if (!enemiesInView.Contains(currentLockedTarget))
            {
                DebugLog("Current target lost from view - clearing lock-on");
                ClearLockOn();
            }
        }
    }
    
    private bool IsInCameraView(Transform target)
    {
        if (playerCamera == null) return false;
        
        // Get direction from camera to target
        Vector3 directionToTarget = (target.position - playerCamera.transform.position).normalized;
        Vector3 cameraForward = playerCamera.transform.forward;
        
        // Check if target is in front of camera
        if (Vector3.Dot(directionToTarget, cameraForward) <= 0)
            return false;
        
        // Check if target is within field of view angle
        float angle = Vector3.Angle(cameraForward, directionToTarget);
        return angle <= cameraFieldOfView * 0.5f;
    }
    
    private bool IsEnemyAlive(GameObject enemy)
    {
        if (enemy == null || !enemy.activeInHierarchy) return false;
        
        // Check for common "dead" states
        
        // Method 1: Check for Enemy_Basic component with HP
        var enemyBasic = enemy.GetComponent<Enemy_Basic>();
        if (enemyBasic != null)
        {
            // Use reflection to check enemyHP field
            var hpField = typeof(Enemy_Basic).GetField("enemyHP", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (hpField != null)
            {
                float hp = (float)hpField.GetValue(enemyBasic);
                if (hp <= 0)
                {
                    DebugLog($"Enemy {enemy.name} has enemyHP <= 0 ({hp}), filtering out");
                    return false;
                }
            }
            
            // Also check the 'dead' boolean field
            var deadField = typeof(Enemy_Basic).GetField("dead", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (deadField != null)
            {
                bool isDead = (bool)deadField.GetValue(enemyBasic);
                if (isDead)
                {
                    DebugLog($"Enemy {enemy.name} is marked as dead, filtering out");
                    return false;
                }
            }
        }
        
        // Method 2: Check for Worker component with HP
        var worker = enemy.GetComponent<Worker>();
        if (worker != null)
        {
            // Use reflection to check enemyHP field
            var hpField = typeof(Worker).GetField("enemyHP", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (hpField != null)
            {
                float hp = (float)hpField.GetValue(worker);
                if (hp <= 0)
                {
                    DebugLog($"Worker {enemy.name} has enemyHP <= 0 ({hp}), filtering out");
                    return false;
                }
            }
            
            // Also check the 'dead' boolean field
            var deadField = typeof(Worker).GetField("dead", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (deadField != null)
            {
                bool isDead = (bool)deadField.GetValue(worker);
                if (isDead)
                {
                    DebugLog($"Worker {enemy.name} is marked as dead, filtering out");
                    return false;
                }
            }
        }
        
        // Method 3: Check for Boss component with HP
        var boss = enemy.GetComponent<Boss>();
        if (boss != null)
        {
            // Use reflection to check currentHealth field
            var hpField = typeof(Boss).GetField("currentHealth", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (hpField != null)
            {
                float hp = (float)hpField.GetValue(boss);
                if (hp <= 0)
                {
                    DebugLog($"Boss {enemy.name} has currentHealth <= 0 ({hp}), filtering out");
                    return false;
                }
            }
        }
        
        // Method 4: Check animator for "isDead" parameter
        var animator = enemy.GetComponent<Animator>();
        if (animator != null)
        {
            if (HasAnimatorParameter(animator, "isDead", AnimatorControllerParameterType.Bool))
            {
                if (animator.GetBool("isDead"))
                {
                    DebugLog($"Enemy {enemy.name} has isDead=true, filtering out");
                    return false;
                }
            }
        }
        
        // Method 5: Check if main collider is disabled (common when dead)
        var mainCollider = enemy.GetComponent<Collider>();
        if (mainCollider != null && !mainCollider.enabled)
        {
            // Only filter out if it's definitely disabled due to death
            // (not just temporarily disabled for other reasons)
            var characterController = enemy.GetComponent<CharacterController>();
            if (characterController == null || !characterController.enabled)
            {
                DebugLog($"Enemy {enemy.name} has disabled colliders, filtering out");
                return false;
            }
        }
        
        // Method 6: Check if enemy is in "death" layer (if your project uses this)
        if (enemy.layer == LayerMask.NameToLayer("Dead") && enemy.layer != -1)
        {
            DebugLog($"Enemy {enemy.name} is on Dead layer, filtering out");
            return false;
        }
        
        return true; // Enemy appears to be alive
    }
    
    private bool HasAnimatorParameter(Animator animator, string paramName, AnimatorControllerParameterType paramType)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == paramType)
            {
                return true;
            }
        }
        return false;
    }
    
    private void HandleCycleInput()
    {
        if (cycleTargetAction == null) return;
        
        float scrollInput = cycleTargetAction.ReadValue<float>();
        
        // Detect scroll wheel movement
        if (Mathf.Abs(scrollInput) > 0.1f)
        {
            if (scrollInput > 0)
            {
                DebugLog("Scroll up detected - cycling to next target");
                CycleToNextTarget();
            }
            else if (scrollInput < 0)
            {
                DebugLog("Scroll down detected - cycling to previous target");
                CycleToPreviousTarget();
            }
        }
    }
    
    private void CycleToNextTarget()
    {
        if (enemiesInView.Count == 0)
        {
            DebugLog("No enemies in view to cycle to");
            ClearLockOn();
            return;
        }
        
        currentTargetIndex++;
        
        // If we go past the last enemy, go to "none" state (clear lock-on)
        if (currentTargetIndex >= enemiesInView.Count)
        {
            DebugLog("Cycling past last enemy - clearing lock-on");
            ClearLockOn();
            return;
        }
        
        // Lock onto the new target
        LockOnToTarget(enemiesInView[currentTargetIndex]);
    }
    
    private void CycleToPreviousTarget()
    {
        // If no target is locked, start from the last enemy
        if (currentTargetIndex == -1)
        {
            if (enemiesInView.Count > 0)
            {
                currentTargetIndex = enemiesInView.Count - 1;
                LockOnToTarget(enemiesInView[currentTargetIndex]);
            }
            return;
        }
        
        currentTargetIndex--;
        
        // If we go before the first enemy, clear lock-on
        if (currentTargetIndex < 0)
        {
            DebugLog("Cycling before first enemy - clearing lock-on");
            ClearLockOn();
            return;
        }
        
        // Lock onto the new target
        LockOnToTarget(enemiesInView[currentTargetIndex]);
    }
    
    private void LockOnToTarget(Transform target)
    {
        if (target == null) return;
        
        // Clear previous target's locked highlight (but it may still have in-view highlight)
        if (currentLockedTarget != null)
        {
            TargetableEnemy previousTargetable = currentLockedTarget.GetComponent<TargetableEnemy>();
            if (previousTargetable != null)
            {
                previousTargetable.OnTargetDeselected();
                
                // If previous target is still in view, restore its in-view highlight
                if (enemiesInView.Contains(currentLockedTarget) && highlightEnemiesInView && useTargetableEnemyHighlighting)
                {
                    var highlightRenderer = GetEnemyHighlightRenderer(previousTargetable);
                    if (highlightRenderer != null)
                    {
                        highlightRenderer.enabled = true;
                        DebugLog($"Restored in-view highlight for previous target: {currentLockedTarget.name}");
                    }
                }
            }
        }
        
        currentLockedTarget = target;
        currentTargetIndex = enemiesInView.IndexOf(target);
        
        DebugLog($"Locked onto: {target.name} (Index: {currentTargetIndex})");
        
        // Activate new target's locked highlight (this overrides any in-view highlight)
        TargetableEnemy targetableEnemy = target.GetComponent<TargetableEnemy>();
        if (targetableEnemy != null)
        {
            targetableEnemy.OnTargetSelected();
        }
        
        // Show indicator
        ShowIndicator();
    }
    
    private void ClearLockOn()
    {
        // Clear target's locked highlight but potentially restore in-view highlight
        if (currentLockedTarget != null)
        {
            TargetableEnemy targetableEnemy = currentLockedTarget.GetComponent<TargetableEnemy>();
            if (targetableEnemy != null)
            {
                targetableEnemy.OnTargetDeselected();
                
                // If the target is still in view, restore its in-view highlight
                if (enemiesInView.Contains(currentLockedTarget) && highlightEnemiesInView && useTargetableEnemyHighlighting)
                {
                    var highlightRenderer = GetEnemyHighlightRenderer(targetableEnemy);
                    if (highlightRenderer != null)
                    {
                        highlightRenderer.enabled = true;
                        DebugLog($"Restored in-view highlight after unlocking: {currentLockedTarget.name}");
                    }
                }
            }
        }
        
        currentLockedTarget = null;
        currentTargetIndex = -1;
        
        // Hide indicator
        HideIndicator();
        
        DebugLog("Lock-on cleared");
    }
    
    private void RotateTowardsTarget()
    {
        // Auto-rotation disabled by default to prevent camera movement when switching targets
        if (currentLockedTarget == null || playerTransform == null) return;
        
        // Calculate direction to target
        Vector3 directionToTarget = (currentLockedTarget.position - playerTransform.position).normalized;
        directionToTarget.y = 0; // Keep rotation only on Y axis (horizontal)
        
        if (directionToTarget.magnitude < 0.1f) return; // Too close or same position
        
        // Calculate target rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        
        if (smoothRotation)
        {
            // Smooth rotation
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        else
        {
            // Instant rotation
            playerTransform.rotation = targetRotation;
        }
    }
    
    private void ShowIndicator()
    {
        if (currentLockedTarget == null) 
        {
            DebugLog("ShowIndicator: No locked target");
            return;
        }
        
        // Create indicator if it doesn't exist
        if (currentIndicator == null)
        {
            if (lockOnIndicatorPrefab != null)
            {
                currentIndicator = Instantiate(lockOnIndicatorPrefab);
                DebugLog("Created indicator from prefab");
            }
            else
            {
                // Create a simple red sphere as fallback
                currentIndicator = CreateSimpleIndicator();
                DebugLog("Created simple red sphere indicator");
            }
        }
        
        currentIndicator.SetActive(true);
        UpdateIndicatorPosition();
        DebugLog($"Indicator shown at position: {currentIndicator.transform.position} for target: {currentLockedTarget.name}");
    }
    
    private void HideIndicator()
    {
        if (currentIndicator != null)
        {
            currentIndicator.SetActive(false);
        }
    }
    
    private void UpdateIndicator()
    {
        if (currentIndicator != null && currentIndicator.activeInHierarchy && currentLockedTarget != null)
        {
            UpdateIndicatorPosition();
        }
    }
    
    private void UpdateIndicatorPosition()
    {
        if (currentIndicator == null || currentLockedTarget == null) return;
        
        Vector3 targetPosition = currentLockedTarget.position;
        
        // Try to position above the enemy's head
        Collider targetCollider = currentLockedTarget.GetComponent<Collider>();
        if (targetCollider != null)
        {
            targetPosition.y = targetCollider.bounds.max.y + indicatorHeightOffset;
        }
        else
        {
            targetPosition.y += indicatorHeightOffset;
        }
        
        currentIndicator.transform.position = targetPosition;
        
        // Make indicator face camera
        if (playerCamera != null)
        {
            Vector3 directionToCamera = playerCamera.transform.position - currentIndicator.transform.position;
            currentIndicator.transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }
    }
    
    private GameObject CreateSimpleIndicator()
    {
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.name = "LockOnIndicator";
        
        // Remove collider
        if (indicator.GetComponent<Collider>())
        {
            DestroyImmediate(indicator.GetComponent<Collider>());
        }
        
        // Make it red, emissive, and visible
        Renderer renderer = indicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material redMaterial = new Material(Shader.Find("Standard"));
            redMaterial.color = Color.red;
            redMaterial.SetColor("_EmissionColor", Color.red * 2f); // Make it glow
            redMaterial.EnableKeyword("_EMISSION");
            redMaterial.SetFloat("_Metallic", 0f);
            redMaterial.SetFloat("_Glossiness", 0.8f);
            renderer.material = redMaterial;
        }
        
        // Make it bigger and more visible
        indicator.transform.localScale = Vector3.one * 0.8f;
        indicator.SetActive(false);
        
        DebugLog("Simple red indicator created");
        
        return indicator;
    }
    
    private void UpdateEnemyHighlights()
    {
        // Remove highlights from enemies no longer in view
        foreach (Transform enemy in previousEnemiesInView)
        {
            if (!enemiesInView.Contains(enemy) && enemy != currentLockedTarget)
            {
                RemoveEnemyHighlight(enemy);
            }
        }
        
        // Add highlights to new enemies in view
        foreach (Transform enemy in enemiesInView)
        {
            if (!previousEnemiesInView.Contains(enemy) && enemy != currentLockedTarget)
            {
                AddEnemyHighlight(enemy);
            }
        }
    }
    
    private void AddEnemyHighlight(Transform enemy)
    {
        if (enemy == null) return;
        
        TargetableEnemy targetableEnemy = enemy.GetComponent<TargetableEnemy>();
        if (targetableEnemy != null)
        {
            // Directly enable the highlight renderer for enemies in view (not locked)
            // This avoids setting isCurrentlyTargeted = true for multiple enemies
            var highlightRenderer = GetEnemyHighlightRenderer(targetableEnemy);
            if (highlightRenderer != null)
            {
                highlightRenderer.enabled = true;
                DebugLog($"Added in-view highlight to enemy: {enemy.name}");
            }
            else
            {
                // Fallback: use the regular target selection if no highlight renderer found
                targetableEnemy.OnTargetSelected();
                DebugLog($"Added fallback highlight to enemy in view: {enemy.name}");
            }
        }
    }
    
    private void RemoveEnemyHighlight(Transform enemy)
    {
        if (enemy == null) return;
        
        TargetableEnemy targetableEnemy = enemy.GetComponent<TargetableEnemy>();
        if (targetableEnemy != null)
        {
            // Only disable highlight if this enemy is not the currently locked target
            if (enemy != currentLockedTarget)
            {
                var highlightRenderer = GetEnemyHighlightRenderer(targetableEnemy);
                if (highlightRenderer != null)
                {
                    highlightRenderer.enabled = false;
                    DebugLog($"Removed in-view highlight from enemy: {enemy.name}");
                }
                else
                {
                    // Fallback: use regular deselection if no highlight renderer found
                    targetableEnemy.OnTargetDeselected();
                    DebugLog($"Removed fallback highlight from enemy: {enemy.name}");
                }
            }
        }
    }
    
    private SkinnedMeshRenderer GetEnemyHighlightRenderer(TargetableEnemy targetableEnemy)
    {
        // Use reflection to access the private enemyHighlightRenderer field
        var highlightField = typeof(TargetableEnemy).GetField("enemyHighlightRenderer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (highlightField != null)
        {
            return (SkinnedMeshRenderer)highlightField.GetValue(targetableEnemy);
        }
        
        return null;
    }
    
    private void ClearAllEnemyHighlights()
    {
        // Clear highlights from all previously highlighted enemies
        foreach (Transform enemy in previousEnemiesInView)
        {
            RemoveEnemyHighlight(enemy);
        }
        
        foreach (Transform enemy in enemiesInView)
        {
            RemoveEnemyHighlight(enemy);
        }
    }
    
    private void DebugLog(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[SimpleNyxLockOn] {message}");
        }
    }
    
    // Public methods for external access
    public void ForceUnlock()
    {
        ClearLockOn();
    }
    
    public void SetRotationEnabled(bool enabled)
    {
        enableAutoRotation = enabled;
    }
    
    public void SetEnemyHighlightingEnabled(bool enabled)
    {
        bool wasEnabled = highlightEnemiesInView;
        highlightEnemiesInView = enabled;
        
        // If we're disabling highlighting, clear all current highlights
        if (wasEnabled && !enabled)
        {
            ClearAllEnemyHighlights();
        }
        // If we're enabling highlighting, apply highlights to current enemies in view
        else if (!wasEnabled && enabled && useTargetableEnemyHighlighting)
        {
            foreach (Transform enemy in enemiesInView)
            {
                if (enemy != currentLockedTarget)
                {
                    AddEnemyHighlight(enemy);
                }
            }
        }
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (nyxTransform == null) return;
        
        // Draw lock-on range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(nyxTransform.position, lockOnRange);
        
        // Draw camera view cone
        if (playerCamera != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 cameraPos = playerCamera.transform.position;
            Vector3 cameraForward = playerCamera.transform.forward;
            
            // Draw center line
            Gizmos.DrawLine(cameraPos, cameraPos + cameraForward * lockOnRange);
            
            // Draw FOV edges
            float halfFOV = cameraFieldOfView * 0.5f;
            Vector3 leftEdge = Quaternion.Euler(0, -halfFOV, 0) * cameraForward;
            Vector3 rightEdge = Quaternion.Euler(0, halfFOV, 0) * cameraForward;
            
            Gizmos.DrawLine(cameraPos, cameraPos + leftEdge * lockOnRange);
            Gizmos.DrawLine(cameraPos, cameraPos + rightEdge * lockOnRange);
        }
        
        // Draw line to locked target
        if (currentLockedTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(nyxTransform.position, currentLockedTarget.position);
        }
        
        // Draw enemies in view
        Gizmos.color = Color.green;
        foreach (Transform enemy in enemiesInView)
        {
            if (enemy != null && enemy != currentLockedTarget)
            {
                Gizmos.DrawWireSphere(enemy.position, 0.5f);
            }
        }
    }
} 