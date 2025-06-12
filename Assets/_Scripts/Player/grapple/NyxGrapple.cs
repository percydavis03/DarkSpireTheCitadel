using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NyxGrapple : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float grappleRange = 10f;
    public float grappleAngle = 30f; // Half angle of the cone in degrees
    public LayerMask grappleLayerMask = -1;
    public Transform nyxTransform;
    public Transform grappleOrigin;
    public Animator nyxAnimator;
    public string grappleLayerName = "Grapple"; // The name of the animator layer for grappling
    
    [Header("Grapple Pull Settings")]
    public KeyCode grappleKey = KeyCode.E;
    public float minDistanceFromOrigin = 2f;
    public float pullForce = 1000f;
    public float maxPullSpeed = 10f;
    public bool useSpringJoint = false;
    public float springStrength = 50f;
    public float springDamper = 5f;
    
    [Header("Grapple Cooldown")]
    public float grappleCooldown = 0.5f;  // Adjustable in Inspector
    private float cooldownTimer = 0f;
    private bool canGrapple = true;
    private bool wasGrappleKeyPressed = false;
    
    [Header("Debug")]
    public bool showDebugRay = true;
    public bool useGizmos = true;
    public Color debugRayColor = Color.red;
    public Color debugHitColor = Color.green;
    
    private RaycastHit lastHit;
    private bool lastHitDetected;
    private bool isGrappling = false;
    private Grappleable currentGrappleTarget;
    private Vector3 grappleStartPosition;
    private Rigidbody currentTargetRigidbody;
    private SpringJoint currentSpringJoint;

    // Start is called before the first frame update
    void Start()
    {
        // If Nyx transform is set, use it as the grapple origin
        if (nyxTransform != null)
            grappleOrigin = nyxTransform;
        // If no grapple origin is set, use this transform
        else if (grappleOrigin == null)
            grappleOrigin = transform;
    }

    // Update is called once per frame
    void Update()
    {
        PerformGrappleRaycast();
        
        // Handle cooldown
        if (!canGrapple)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canGrapple = true;
                cooldownTimer = 0;
            }
        }
        
        // Track key state for requiring new press
        bool isGrappleKeyDown = Input.GetKey(grappleKey);
        
        // If key was released, allow new grapple on next press
        if (!isGrappleKeyDown && wasGrappleKeyPressed)
        {
            wasGrappleKeyPressed = false;
        }
        
        HandleGrappleInput();
        
        // Check if grappled object has reached minimum distance
        if (isGrappling && currentGrappleTarget != null)
        {
            float currentDistance = Vector3.Distance(currentGrappleTarget.transform.position, grappleOrigin.position);
            if (currentDistance <= minDistanceFromOrigin)
            {
                Debug.Log("Minimum distance reached - auto releasing grapple");
                // Stop the grappling animation
                if (nyxAnimator != null)
                {
                    nyxAnimator.SetBool("isGrappling", false);
                }
                ReleaseGrapple();
                StartCooldown();
            }
        }
        
        // Update animator based on grappling state
        if (nyxAnimator != null)
        {
            nyxAnimator.SetBool("isGrappling", isGrappling);
        }
        
        // Store key state for next frame
        wasGrappleKeyPressed = isGrappleKeyDown;
    }
    
    void StartCooldown()
    {
        Debug.Log($"Starting cooldown - duration: {grappleCooldown}");
        canGrapple = false;
        cooldownTimer = grappleCooldown;
    }
    
    void HandleGrappleInput()
    {
        // Only allow new grapple if:
        // 1. Not in cooldown
        // 2. Key wasn't already pressed
        // 3. Have a valid target
        // 4. Not currently grappling
        if (canGrapple && !wasGrappleKeyPressed && Input.GetKeyDown(grappleKey) && lastHitDetected && !isGrappling)
        {
            Debug.Log($"Grapple conditions met - canGrapple:{canGrapple}, wasGrappleKeyPressed:{wasGrappleKeyPressed}, lastHitDetected:{lastHitDetected}, isGrappling:{isGrappling}");
            // Check if hit object has the "CanBeGrappled" tag
            if (lastHit.collider.CompareTag("CanBeGrappled"))
            {
                // Check if the object has a Grappleable script
                Grappleable grappleable = lastHit.collider.GetComponent<Grappleable>();
                if (grappleable != null && grappleable.canBeGrappled && !grappleable.IsBeingGrappled)
                {
                    StartGrapple(grappleable);
                    wasGrappleKeyPressed = true;
                }
                else
                {
                    Debug.Log($"Grappleable check failed - hasComponent:{grappleable != null}, canBeGrappled:{grappleable?.canBeGrappled}, isBeingGrappled:{grappleable?.IsBeingGrappled}");
                }
            }
            else
            {
                Debug.Log("Hit object does not have CanBeGrappled tag");
            }
        }
        else if (Input.GetKeyDown(grappleKey))
        {
            Debug.Log($"Grapple conditions NOT met - canGrapple:{canGrapple}, wasGrappleKeyPressed:{wasGrappleKeyPressed}, lastHitDetected:{lastHitDetected}, isGrappling:{isGrappling}");
        }
        
        // Check if grapple key is released and we're currently grappling
        if (Input.GetKeyUp(grappleKey) && isGrappling)
        {
            ReleaseGrapple();
            StartCooldown();
        }
    }
    
    void StartGrapple(Grappleable target)
    {
        if (!target.canBeGrappled) return;
        
        // Check if target has a Rigidbody
        Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();
        if (targetRigidbody == null)
        {
            Debug.LogWarning($"Cannot grapple {target.name} - no Rigidbody found!");
            return;
        }
        
        currentGrappleTarget = target;
        currentTargetRigidbody = targetRigidbody;
        isGrappling = true;
        grappleStartPosition = target.transform.position;
        
        // Set Nyx's animator state
        if (nyxAnimator != null)
        {
            nyxAnimator.SetBool("isGrappling", true);
        }
        
        // Notify the grappleable object
        target.StartGrapple();
        
        Debug.Log($"Started grappling: {target.name}");
        
        // Check if target is already at minimum distance
        float currentDistance = Vector3.Distance(target.transform.position, grappleOrigin.position);
        if (currentDistance <= minDistanceFromOrigin)
        {
            Debug.Log("Target already at minimum distance - brief grapple");
            StartCoroutine(BriefGrappleCoroutine());
            return;
        }
        
        // Create spring joint if enabled
        if (useSpringJoint)
        {
            CreateSpringJoint();
        }
        
        // Start the physics-based grapple coroutine
        StartCoroutine(PhysicsGrappleCoroutine());
    }
    
    void ReleaseGrapple()
    {
        if (isGrappling)
        {
            Debug.Log("Grapple released - resetting state");
            
            // Destroy spring joint if it exists
            if (currentSpringJoint != null)
            {
                Destroy(currentSpringJoint);
                currentSpringJoint = null;
            }
            
            // Notify the grappleable object
            if (currentGrappleTarget != null)
            {
                currentGrappleTarget.ReleaseGrapple();
            }
            
            isGrappling = false;
            currentGrappleTarget = null;
            currentTargetRigidbody = null;
            StopAllCoroutines();
        }
    }
    
    IEnumerator PhysicsGrappleCoroutine()
    {
        while (isGrappling && currentTargetRigidbody != null)
        {
            // Check if grapple key is still held
            if (!Input.GetKey(grappleKey))
            {
                ReleaseGrapple();
                yield break;
            }
            
            // Calculate current distance from grapple origin
            float currentDistance = Vector3.Distance(currentGrappleTarget.transform.position, grappleOrigin.position);
            
            // Only apply forces if not using spring joint
            if (!useSpringJoint)
            {
                // Calculate direction from object to grapple origin
                Vector3 directionToOrigin = (grappleOrigin.position - currentGrappleTarget.transform.position).normalized;
                
                // Calculate distance factor (stronger pull when farther away)
                float distanceFactor = Mathf.Clamp01((currentDistance - minDistanceFromOrigin) / grappleRange);
                
                // Apply force toward grapple origin
                Vector3 pullForceVector = directionToOrigin * pullForce * distanceFactor;
                currentTargetRigidbody.AddForce(pullForceVector * Time.fixedDeltaTime);
                
                // Limit maximum velocity to prevent objects flying too fast
                if (currentTargetRigidbody.velocity.magnitude > maxPullSpeed)
                {
                    currentTargetRigidbody.velocity = currentTargetRigidbody.velocity.normalized * maxPullSpeed;
                }
                
                // Add some drag to make the motion feel more controlled
                currentTargetRigidbody.velocity *= 0.98f;
            }
            
            yield return new WaitForFixedUpdate();
        }
    }
    
    IEnumerator BriefGrappleCoroutine()
    {
        // Wait for a brief moment to allow animation and effects to play
        yield return new WaitForSeconds(0.3f);
        
        // Release the grapple and start cooldown
        ReleaseGrapple();
        StartCooldown();
    }
    
    void CreateSpringJoint()
    {
        // Add a Rigidbody to the grapple origin if it doesn't have one
        Rigidbody grappleOriginRigidbody = grappleOrigin.GetComponent<Rigidbody>();
        if (grappleOriginRigidbody == null)
        {
            grappleOriginRigidbody = grappleOrigin.gameObject.AddComponent<Rigidbody>();
            grappleOriginRigidbody.isKinematic = true; // Keep it stationary
        }
        
        // Create spring joint on the target object
        currentSpringJoint = currentTargetRigidbody.gameObject.AddComponent<SpringJoint>();
        currentSpringJoint.connectedBody = grappleOriginRigidbody;
        currentSpringJoint.autoConfigureConnectedAnchor = false;
        currentSpringJoint.anchor = Vector3.zero;
        currentSpringJoint.connectedAnchor = Vector3.zero;
        
        // Set spring properties
        currentSpringJoint.spring = springStrength;
        currentSpringJoint.damper = springDamper;
        currentSpringJoint.minDistance = 0f; // Allow object to get close
        currentSpringJoint.maxDistance = 0f; // Force tight connection
        
        Debug.Log("Spring joint created for dynamic grappling");
    }
    
    void PerformGrappleRaycast()
    {
        // Get the forward direction from the grapple origin
        Vector3 rayDirection = grappleOrigin.forward;
        Vector3 rayOrigin = grappleOrigin.position;
        
        // Find all colliders within range
        Collider[] colliders = Physics.OverlapSphere(rayOrigin, grappleRange, grappleLayerMask);
        
        float closestDistance = float.MaxValue;
        RaycastHit closestHit = new RaycastHit();
        bool hitDetected = false;
        
        foreach (Collider collider in colliders)
        {
            Vector3 directionToTarget = (collider.transform.position - rayOrigin).normalized;
            float angleToTarget = Vector3.Angle(rayDirection, directionToTarget);
            
            // Check if target is within our cone angle
            if (angleToTarget <= grappleAngle)
            {
                // Check if this collider is grappleable before doing expensive raycast
                if (!collider.CompareTag("CanBeGrappled"))
                    continue;
                
                // Perform raycast, but ignore non-grappleable objects
                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, directionToTarget, grappleRange, grappleLayerMask);
                
                foreach (RaycastHit hit in hits)
                {
                    // Only consider hits on grappleable objects
                    if (hit.collider.CompareTag("CanBeGrappled") && hit.collider == collider)
                    {
                        if (hit.distance < closestDistance)
                        {
                            closestDistance = hit.distance;
                            closestHit = hit;
                            hitDetected = true;
                        }
                        break; // Found our target, no need to check other hits
                    }
                }
            }
        }
        
        // Store for Gizmos
        lastHit = closestHit;
        lastHitDetected = hitDetected;
        
        // Debug visualization
        if (showDebugRay)
        {
            Color rayColor = debugRayColor;
            
            // Change color if we can grapple this object
            if (hitDetected && lastHit.collider.CompareTag("CanBeGrappled"))
            {
                Grappleable grappleable = lastHit.collider.GetComponent<Grappleable>();
                if (grappleable != null && grappleable.canBeGrappled && !grappleable.IsBeingGrappled)
                {
                    rayColor = debugHitColor;
                }
            }
            
            if (hitDetected)
            {
                // Draw ray to hit point
                Debug.DrawLine(rayOrigin, lastHit.point, rayColor);
                // Draw a small cross at hit point
                Vector3 hitPoint = lastHit.point;
                Debug.DrawLine(hitPoint + Vector3.up * 0.1f, hitPoint + Vector3.down * 0.1f, rayColor);
                Debug.DrawLine(hitPoint + Vector3.left * 0.1f, hitPoint + Vector3.right * 0.1f, rayColor);
            }
            
            // Draw cone visualization
            DrawDebugCone(rayOrigin, rayDirection, grappleRange, grappleAngle, rayColor);
        }
    }
    
    void DrawDebugCone(Vector3 origin, Vector3 direction, float range, float angle, Color color)
    {
        // Draw center line
        Debug.DrawRay(origin, direction * range, color);
        
        // Draw cone edges
        int segments = 8;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float currentAngle = i * angleStep;
            Vector3 coneDirection = GetConeDirection(direction, angle, currentAngle);
            Debug.DrawRay(origin, coneDirection * range, color);
        }
        
        // Draw cone circles at different distances
        float[] distances = { range * 0.33f, range * 0.66f, range };
        foreach (float distance in distances)
        {
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep;
                float angle2 = (i + 1) * angleStep;
                
                Vector3 point1 = origin + GetConeDirection(direction, angle, angle1) * distance;
                Vector3 point2 = origin + GetConeDirection(direction, angle, angle2) * distance;
                
                Debug.DrawLine(point1, point2, color);
            }
        }
    }
    
    Vector3 GetConeDirection(Vector3 forward, float coneAngle, float rotationAngle)
    {
        // Create a vector at the cone angle
        Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
        Vector3 up = Vector3.Cross(right, forward).normalized;
        
        // Calculate the direction at the edge of the cone
        float rad = coneAngle * Mathf.Deg2Rad;
        float rotRad = rotationAngle * Mathf.Deg2Rad;
        
        Vector3 coneDirection = forward * Mathf.Cos(rad) + 
                               (right * Mathf.Cos(rotRad) + up * Mathf.Sin(rotRad)) * Mathf.Sin(rad);
        
        return coneDirection.normalized;
    }
    
    void DrawGizmoCone(Vector3 origin, Vector3 direction, float range, float angle)
    {
        // Draw cone edges
        int segments = 12;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float currentAngle = i * angleStep;
            Vector3 coneDirection = GetConeDirection(direction, angle, currentAngle);
            Gizmos.DrawLine(origin, origin + coneDirection * range);
        }
        
        // Draw cone circles at different distances
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
    
    void OnDrawGizmos()
    {
        if (!useGizmos || !showDebugRay || grappleOrigin == null)
            return;
            
        Vector3 rayOrigin = grappleOrigin.position;
        Vector3 rayDirection = grappleOrigin.forward;
        
        // Set gizmo color based on grappleable status
        Color gizmoColor = debugRayColor;
        if (lastHitDetected && lastHit.collider.CompareTag("CanBeGrappled"))
        {
            Grappleable grappleable = lastHit.collider.GetComponent<Grappleable>();
            if (grappleable != null && grappleable.canBeGrappled && !grappleable.IsBeingGrappled)
            {
                gizmoColor = debugHitColor;
            }
        }
        
        Gizmos.color = gizmoColor;
        
        if (lastHitDetected)
        {
            // Draw line to hit point
            Gizmos.DrawLine(rayOrigin, lastHit.point);
            
            // Draw a small sphere at hit point
            Gizmos.DrawWireSphere(lastHit.point, 0.1f);
        }
        else
        {
            // Draw center line when no hit
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * grappleRange);
        }
        
        // Draw cone visualization with Gizmos
        DrawGizmoCone(rayOrigin, rayDirection, grappleRange, grappleAngle);
        
        // Draw small sphere at origin
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(rayOrigin, 0.05f);
        
        // Draw minimum distance indicator if we're grappling
        if (isGrappling && currentGrappleTarget != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 directionToTarget = (currentGrappleTarget.transform.position - grappleOrigin.position).normalized;
            Vector3 minDistancePos = grappleOrigin.position + directionToTarget * minDistanceFromOrigin;
            Gizmos.DrawWireSphere(minDistancePos, 0.2f);
        }
    }
}
