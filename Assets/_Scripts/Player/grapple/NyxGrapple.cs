using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NyxGrapple : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float grappleRange = 10f;
    public float grappleRadius = 1f;
    public LayerMask grappleLayerMask = -1;
    public Transform nyxTransform;
    public Transform grappleOrigin;
    
    [Header("Grapple Pull Settings")]
    public KeyCode grappleKey = KeyCode.E;
    public float minDistanceFromOrigin = 2f;
    public float pullTime = 1f;
    
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
        HandleGrappleInput();
    }
    
    void HandleGrappleInput()
    {
        // Check if grapple key is pressed and we can start grappling
        if (Input.GetKeyDown(grappleKey) && lastHitDetected && !isGrappling)
        {
            // Check if hit object has the "CanBeGrappled" tag
            if (lastHit.collider.CompareTag("CanBeGrappled"))
            {
                // Check if the object has a Grappleable script
                Grappleable grappleable = lastHit.collider.GetComponent<Grappleable>();
                if (grappleable != null && grappleable.canBeGrappled && !grappleable.IsBeingGrappled)
                {
                    StartGrapple(grappleable);
                }
            }
        }
        
        // Check if grapple key is released and we're currently grappling
        if (Input.GetKeyUp(grappleKey) && isGrappling)
        {
            ReleaseGrapple();
        }
    }
    
    void StartGrapple(Grappleable target)
    {
        if (!target.canBeGrappled) return;
        
        currentGrappleTarget = target;
        isGrappling = true;
        grappleStartPosition = target.transform.position;
        
        // Notify the grappleable object
        target.StartGrapple();
        
        Debug.Log($"Started grappling: {target.name}");
        
        // Start the grapple coroutine
        StartCoroutine(GrappleCoroutine());
    }
    
    void ReleaseGrapple()
    {
        if (isGrappling)
        {
            Debug.Log("Grapple released");
            
            // Notify the grappleable object
            if (currentGrappleTarget != null)
            {
                currentGrappleTarget.ReleaseGrapple();
            }
            
            isGrappling = false;
            currentGrappleTarget = null;
            StopAllCoroutines();
        }
    }
    
    IEnumerator GrappleCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startPos = currentGrappleTarget.transform.position;
        
        while (elapsedTime < pullTime && isGrappling)
        {
            // Check if grapple key is still held
            if (!Input.GetKey(grappleKey))
            {
                ReleaseGrapple();
                yield break;
            }
            
            // Calculate current distance from grapple origin
            float currentDistance = Vector3.Distance(currentGrappleTarget.transform.position, grappleOrigin.position);
            
            // If we've reached the minimum distance, auto-release
            if (currentDistance <= minDistanceFromOrigin)
            {
                Debug.Log("Minimum distance reached - auto releasing grapple");
                ReleaseGrapple();
                yield break;
            }
            
            // Calculate direction from object to grapple origin
            Vector3 directionToOrigin = (grappleOrigin.position - currentGrappleTarget.transform.position).normalized;
            
            // Calculate target position at minimum distance from origin
            Vector3 targetPos = grappleOrigin.position - directionToOrigin * minDistanceFromOrigin;
            
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / pullTime;
            
            // Smooth interpolation toward target position
            Vector3 newPosition = Vector3.Lerp(startPos, targetPos, progress);
            currentGrappleTarget.transform.position = newPosition;
            
            yield return null;
        }
        
        // Auto-release when pull time is reached
        if (isGrappling)
        {
            Debug.Log("Pull time completed - auto releasing grapple");
            ReleaseGrapple();
        }
    }
    
    void PerformGrappleRaycast()
    {
        // Get the forward direction from the grapple origin
        Vector3 rayDirection = grappleOrigin.forward;
        Vector3 rayOrigin = grappleOrigin.position;
        
        // Perform the raycast
        RaycastHit hit;
        bool hitDetected = Physics.Raycast(rayOrigin, rayDirection, out hit, grappleRange, grappleLayerMask);
        
        // Store for Gizmos
        lastHit = hit;
        lastHitDetected = hitDetected;
        
        // Debug visualization
        if (showDebugRay)
        {
            Color rayColor = debugRayColor;
            
            // Change color if we can grapple this object
            if (hitDetected && hit.collider.CompareTag("CanBeGrappled"))
            {
                Grappleable grappleable = hit.collider.GetComponent<Grappleable>();
                if (grappleable != null && grappleable.canBeGrappled && !grappleable.IsBeingGrappled)
                {
                    rayColor = debugHitColor;
                }
            }
            
            if (hitDetected)
            {
                // Draw ray to hit point
                Debug.DrawLine(rayOrigin, hit.point, rayColor);
                // Draw a small cross at hit point
                Vector3 hitPoint = hit.point;
                Debug.DrawLine(hitPoint + Vector3.up * 0.1f, hitPoint + Vector3.down * 0.1f, rayColor);
                Debug.DrawLine(hitPoint + Vector3.left * 0.1f, hitPoint + Vector3.right * 0.1f, rayColor);
            }
            else
            {
                // Draw full length ray when no hit
                Debug.DrawRay(rayOrigin, rayDirection * grappleRange, rayColor);
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
            // Draw full length ray when no hit
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * grappleRange);
        }
        
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
