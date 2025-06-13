using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NyxGrapple : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float grappleRange = 10f;
    public float grappleAngle = 30f;
    public LayerMask grappleLayerMask = -1;
    public Transform nyxTransform;
    public Transform grappleOrigin;
    public Animator nyxAnimator;
    public KeyCode grappleKey = KeyCode.E;
    
    [Header("Hand/Joint Settings")]
    public Transform grappleJoint; // The hand/joint that reaches to targets
    public float jointMoveSpeed = 100f;
    public Collider jointTrigger; // Collider on the joint to detect when it reaches targets
    
    [Header("Pull Settings")]
    public float minDistanceFromOrigin = 2f;
    public float pullForce = 1000f;
    public float maxPullSpeed = 10f;
    public bool useSpringJoint = false;
    public float springStrength = 50f;
    public float springDamper = 5f;
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public bool showDebugRay = true;
    public bool useGizmos = true;
    public Color debugRayColor = Color.red;
    public Color debugHitColor = Color.green;
    
    // State Variables
    private bool isGrappling = false;           // Animation is playing
    private bool isGrappleableInRange = false;  // Valid target detected
    private bool isHandReaching = false;        // Hand moving to target
    private bool isPulling = false;             // Actively pulling target
    
    // Target Information
    private Grappleable currentTarget;
    private Rigidbody currentTargetRigidbody;
    private RaycastHit lastHit;
    private Vector3 originalJointPosition;
    private Quaternion originalJointRotation;
    private Transform originalJointParent;
    private SpringJoint currentSpringJoint;

    void Awake()
    {
        SetupReferences();
        StoreOriginalJointTransform();
    }
    
    void OnEnable()
    {
        ResetAllStates();
    }
    
    void Update()
    {
        DetectGrappleableInRange();
        HandleGrappleInput();
        UpdateAnimator();
        CheckPullingConditions();
    }
    
    void SetupReferences()
    {
        if (nyxTransform != null)
            grappleOrigin = nyxTransform;
        else if (grappleOrigin == null)
            grappleOrigin = transform;
            
        // Setup joint trigger if it exists
        if (jointTrigger != null)
        {
            jointTrigger.isTrigger = true;
            // Add GrappleJointTrigger component if it doesn't exist
            if (jointTrigger.GetComponent<GrappleJointTrigger>() == null)
            {
                GrappleJointTrigger triggerScript = jointTrigger.gameObject.AddComponent<GrappleJointTrigger>();
                triggerScript.Initialize(this);
            }
        }
    }
    
    void StoreOriginalJointTransform()
    {
        if (grappleJoint != null)
        {
            originalJointPosition = grappleJoint.localPosition;
            originalJointRotation = grappleJoint.localRotation;
            originalJointParent = grappleJoint.parent;
        }
    }
    
    void ResetAllStates()
    {
        isGrappling = false;
        isGrappleableInRange = false;
        isHandReaching = false;
        isPulling = false;
        currentTarget = null;
        currentTargetRigidbody = null;
        
        CleanupSpringJoint();
        ResetJointPosition();
        
        if (enableDebugLogs) Debug.Log("NyxGrapple: All states reset");
    }
    
    void DetectGrappleableInRange()
    {
        Vector3 rayOrigin = grappleOrigin.position;
        Vector3 rayDirection = grappleOrigin.forward;
        
        Collider[] colliders = Physics.OverlapSphere(rayOrigin, grappleRange, grappleLayerMask);
        
        float closestDistance = float.MaxValue;
        RaycastHit closestHit = new RaycastHit();
        bool validTargetFound = false;
        
        foreach (Collider collider in colliders)
        {
            Vector3 directionToTarget = (collider.transform.position - rayOrigin).normalized;
            float angleToTarget = Vector3.Angle(rayDirection, directionToTarget);
            
            if (angleToTarget <= grappleAngle && collider.CompareTag("CanBeGrappled"))
            {
                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, directionToTarget, grappleRange, grappleLayerMask);
                
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider == collider)
                    {
                        Grappleable grappleable = hit.collider.GetComponent<Grappleable>();
                        if (grappleable != null && grappleable.canBeGrappled && !grappleable.IsBeingGrappled)
                        {
                            if (hit.distance < closestDistance)
                            {
                                closestDistance = hit.distance;
                                closestHit = hit;
                                validTargetFound = true;
                                currentTarget = grappleable;
                            }
                        }
                        break;
                    }
                }
            }
        }
        
        bool wasInRange = isGrappleableInRange;
        isGrappleableInRange = validTargetFound;
        lastHit = closestHit;
        
        // Log state changes
        if (enableDebugLogs && wasInRange != isGrappleableInRange)
        {
            Debug.Log($"Grappleable in range: {isGrappleableInRange} - Target: {(currentTarget ? currentTarget.name : "None")}");
        }
        
        if (!validTargetFound)
        {
            currentTarget = null;
        }
    }
    
    void HandleGrappleInput()
    {
        // Grapple key pressed - start grappling animation
        if (Input.GetKeyDown(grappleKey) && !isGrappling)
        {
            StartGrappling();
        }
        
        // Grapple key released - stop everything
        if (Input.GetKeyUp(grappleKey) && isGrappling)
        {
            StopGrappling();
        }
        
        // If grappling and target in range, start reaching
        if (isGrappling && isGrappleableInRange && !isHandReaching && !isPulling)
        {
            StartReachingToTarget();
        }
    }
    
    void StartGrappling()
    {
        isGrappling = true;
        
        if (enableDebugLogs) Debug.Log("Grappling started - animation triggered");
        
        // Notify target if in range
        if (isGrappleableInRange && currentTarget != null)
        {
            currentTarget.StartGrapple();
            currentTargetRigidbody = currentTarget.GetComponent<Rigidbody>();
        }
    }
    
    void StopGrappling()
    {
        if (enableDebugLogs) Debug.Log("Grappling stopped - resetting all states");
        
        // Notify current target
        if (currentTarget != null)
        {
            currentTarget.ReleaseGrapple();
        }
        
        StopAllCoroutines();
        ResetAllStates();
    }
    
    void StartReachingToTarget()
    {
        if (currentTarget == null || grappleJoint == null) return;
        
        isHandReaching = true;
        
        if (enableDebugLogs) Debug.Log($"Hand reaching to target: {currentTarget.name}");
        
        Transform targetPoint = currentTarget.grapplePoint != null ? currentTarget.grapplePoint : currentTarget.transform;
        StartCoroutine(MoveJointToTarget(targetPoint));
    }
    
    IEnumerator MoveJointToTarget(Transform targetPoint)
    {
        Vector3 startPosition = grappleJoint.position;
        Vector3 targetPosition = targetPoint.position;
        
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float journeyTime = journeyLength / jointMoveSpeed;
        float elapsedTime = 0;
        
        while (elapsedTime < journeyTime && isHandReaching)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / journeyTime;
            
            grappleJoint.position = Vector3.Lerp(startPosition, targetPosition, progress);
            
            // Look at target
            Vector3 direction = (targetPosition - grappleJoint.position).normalized;
            if (direction != Vector3.zero)
            {
                grappleJoint.rotation = Quaternion.LookRotation(direction);
            }
            
            yield return null;
        }
        
        if (isHandReaching)
        {
            grappleJoint.position = targetPosition;
            if (enableDebugLogs) Debug.Log("Hand reached target position");
        }
    }
    
    // Called by GrappleJointTrigger when hand reaches target
    public void OnJointReachedTarget(Grappleable target)
    {
        if (!isHandReaching || target != currentTarget) return;
        
        if (enableDebugLogs) Debug.Log($"Joint trigger reached target: {target.name}");
        
        isHandReaching = false;
        
        // Only start pulling if target has rigidbody
        if (currentTargetRigidbody != null)
        {
            StartPulling();
        }
        else
        {
            if (enableDebugLogs) Debug.Log("Target has no rigidbody - no pulling required");
            // For objects like levers, just wait for them to handle their own logic
        }
    }
    
    void StartPulling()
    {
        if (currentTargetRigidbody == null) return;
        
        isPulling = true;
        
        if (enableDebugLogs) Debug.Log("Started pulling target");
        
        if (useSpringJoint)
        {
            CreateSpringJoint();
        }
        
        StartCoroutine(PullTargetCoroutine());
    }
    
    IEnumerator PullTargetCoroutine()
    {
        while (isPulling && currentTargetRigidbody != null && isGrappling)
        {
            float currentDistance = Vector3.Distance(currentTarget.transform.position, grappleOrigin.position);
            
            // Check if reached minimum distance
            if (currentDistance <= minDistanceFromOrigin)
            {
                if (enableDebugLogs) Debug.Log("Target reached minimum distance - stopping pull");
                StopGrappling();
                yield break;
            }
            
            // Apply pulling force (only if not using spring joint)
            if (!useSpringJoint)
            {
                Vector3 directionToOrigin = (grappleOrigin.position - currentTarget.transform.position).normalized;
                float distanceFactor = Mathf.Clamp01((currentDistance - minDistanceFromOrigin) / grappleRange);
                Vector3 pullForceVector = directionToOrigin * pullForce * distanceFactor;
                
                currentTargetRigidbody.AddForce(pullForceVector * Time.fixedDeltaTime);
                
                // Limit velocity
                if (currentTargetRigidbody.velocity.magnitude > maxPullSpeed)
                {
                    currentTargetRigidbody.velocity = currentTargetRigidbody.velocity.normalized * maxPullSpeed;
                }
                
                currentTargetRigidbody.velocity *= 0.98f; // Add drag
            }
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    void CheckPullingConditions()
    {
        // Auto-stop if target gets too close while pulling
        if (isPulling && currentTarget != null)
        {
            float distance = Vector3.Distance(currentTarget.transform.position, grappleOrigin.position);
            if (distance <= minDistanceFromOrigin)
            {
                StopGrappling();
            }
        }
    }
    
    void UpdateAnimator()
    {
        if (nyxAnimator != null)
        {
            nyxAnimator.SetBool("isGrappling", isGrappling);
        }
    }
    
    void CreateSpringJoint()
    {
        if (currentTargetRigidbody == null) return;
        
        Rigidbody grappleOriginRigidbody = grappleOrigin.GetComponent<Rigidbody>();
        if (grappleOriginRigidbody == null)
        {
            grappleOriginRigidbody = grappleOrigin.gameObject.AddComponent<Rigidbody>();
            grappleOriginRigidbody.isKinematic = true;
        }
        
        currentSpringJoint = currentTargetRigidbody.gameObject.AddComponent<SpringJoint>();
        currentSpringJoint.connectedBody = grappleOriginRigidbody;
        currentSpringJoint.autoConfigureConnectedAnchor = false;
        currentSpringJoint.anchor = Vector3.zero;
        currentSpringJoint.connectedAnchor = Vector3.zero;
        currentSpringJoint.spring = springStrength;
        currentSpringJoint.damper = springDamper;
        currentSpringJoint.minDistance = 0f;
        currentSpringJoint.maxDistance = 0f;
        
        if (enableDebugLogs) Debug.Log("Spring joint created");
    }
    
    void CleanupSpringJoint()
    {
        if (currentSpringJoint != null)
        {
            Destroy(currentSpringJoint);
            currentSpringJoint = null;
        }
    }
    
    void ResetJointPosition()
    {
        if (grappleJoint != null && originalJointParent != null)
        {
            grappleJoint.SetParent(originalJointParent);
            grappleJoint.localPosition = originalJointPosition;
            grappleJoint.localRotation = originalJointRotation;
        }
    }
    
    // Public method for external force release (like from LeverGrapple)
    public void ForceReleaseGrapple()
    {
        if (enableDebugLogs) Debug.Log("Force release grapple called");
        StopGrappling();
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (!useGizmos || !showDebugRay || grappleOrigin == null) return;
        
        Vector3 rayOrigin = grappleOrigin.position;
        Vector3 rayDirection = grappleOrigin.forward;
        
        Color gizmoColor = isGrappleableInRange ? debugHitColor : debugRayColor;
        Gizmos.color = gizmoColor;
        
        if (isGrappleableInRange && lastHit.collider != null)
        {
            Gizmos.DrawLine(rayOrigin, lastHit.point);
            Gizmos.DrawWireSphere(lastHit.point, 0.1f);
        }
        else
        {
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * grappleRange);
        }
        
        // Draw cone visualization
        DrawGizmoCone(rayOrigin, rayDirection, grappleRange, grappleAngle);
        
        // Draw state indicators
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(rayOrigin, 0.05f);
        
        // Draw minimum distance indicator
        if (isPulling && currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 directionToTarget = (currentTarget.transform.position - grappleOrigin.position).normalized;
            Vector3 minDistancePos = grappleOrigin.position + directionToTarget * minDistanceFromOrigin;
            Gizmos.DrawWireSphere(minDistancePos, 0.2f);
        }
    }
    
    void DrawGizmoCone(Vector3 origin, Vector3 direction, float range, float angle)
    {
        int segments = 12;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float currentAngle = i * angleStep;
            Vector3 coneDirection = GetConeDirection(direction, angle, currentAngle);
            Gizmos.DrawLine(origin, origin + coneDirection * range);
        }
        
        float[] distances = { range * 0.5f, range };
        foreach (float distance in distances)
        {
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;
                
                Vector3 point1 = origin + GetConeDirection(direction, angle, angle1) * distance;
                Vector3 point2 = origin + GetConeDirection(direction, angle, angle2) * distance;
                
                Gizmos.DrawLine(point1, point2);
            }
        }
    }
    
    Vector3 GetConeDirection(Vector3 forward, float coneAngle, float rotationAngle)
    {
        Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, forward).normalized;
        
        float rad = coneAngle * Mathf.Deg2Rad;
        float rotRad = rotationAngle * Mathf.Deg2Rad;
        
        Vector3 coneDirection = forward * Mathf.Cos(rad) + 
                               (right * Mathf.Cos(rotRad) + up * Mathf.Sin(rotRad)) * Mathf.Sin(rad);
        
        return coneDirection.normalized;
    }
}

// Helper component for joint trigger detection
public class GrappleJointTrigger : MonoBehaviour
{
    private NyxGrapple nyxGrapple;
    
    public void Initialize(NyxGrapple grapple)
    {
        nyxGrapple = grapple;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (nyxGrapple == null) return;
        
        Grappleable grappleable = other.GetComponent<Grappleable>();
        if (grappleable != null)
        {
            // Notify NyxGrapple that joint reached target
            nyxGrapple.OnJointReachedTarget(grappleable);
            
            // If it's a LeverGrapple, also notify it directly
            LeverGrapple leverGrapple = other.GetComponent<LeverGrapple>();
            if (leverGrapple != null)
            {
                leverGrapple.OnJointReached();
            }
        }
    }
}
