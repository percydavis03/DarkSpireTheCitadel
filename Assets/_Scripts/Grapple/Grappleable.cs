using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Grappleable : MonoBehaviour
{
    [Header("Grappleable Settings")]
    public bool canBeGrappled = true;
    
    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnGrappleStarted;
    public UnityEngine.Events.UnityEvent OnGrappleReleased;
    
    private bool isBeingGrappled = false;
    
    public bool IsBeingGrappled => isBeingGrappled;
    
    // Called when this object starts being grappled
    public void StartGrapple()
    {
        if (!canBeGrappled) return;
        
        isBeingGrappled = true;
        OnGrappleStarted?.Invoke();
        Debug.Log($"{gameObject.name} is being grappled");
    }
    
    // Called when this object is released from grapple
    public void ReleaseGrapple()
    {
        isBeingGrappled = false;
        OnGrappleReleased?.Invoke();
        Debug.Log($"{gameObject.name} grapple released");
    }
}
