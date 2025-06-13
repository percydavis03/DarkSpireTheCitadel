using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverGrapple : Grappleable
{
    [Header("Lever Settings")]
    public string leverAnimationTrigger = "UseLever";
    public float animationDuration = 2f; // Duration of the lever animation
    
    [Header("Lever Events")]
    public UnityEngine.Events.UnityEvent OnLeverActivated;
    public UnityEngine.Events.UnityEvent OnLeverCompleted;
    
    private bool isAnimating = false;
    private NyxGrapple nyxGrappleRef;
    private bool leverUsed = false;
    
    void Start()
    {
        // Don't call base.Start() since we don't want enemy-specific behavior
        // Find the NyxGrapple reference for communication
        nyxGrappleRef = FindObjectOfType<NyxGrapple>();
        
        if (nyxGrappleRef == null)
        {
            Debug.LogWarning("LeverGrapple: Could not find NyxGrapple in scene!");
        }
    }
    
    // Override the StartGrapple method to handle lever-specific behavior
    public override void StartGrapple()
    {
        if (!canBeGrappled || leverUsed) return;
        
        isBeingGrappled = true;
        
        Debug.Log($"{gameObject.name} lever grapple started");
        OnGrappleStarted?.Invoke();
        
        // Start monitoring for when the joint reaches the grapple point
        StartCoroutine(MonitorJointPosition());
    }
    
    // Override the ReleaseGrapple method
    public override void ReleaseGrapple()
    {
        if (!isAnimating)
        {
            isBeingGrappled = false;
            OnGrappleReleased?.Invoke();
            Debug.Log($"{gameObject.name} lever grapple released");
        }
        // If animating, don't release immediately - let animation complete
    }
    
    private IEnumerator MonitorJointPosition()
    {
        if (nyxGrappleRef == null || nyxGrappleRef.grappleJoint == null)
        {
            Debug.LogWarning("LeverGrapple: Missing NyxGrapple or grapple joint reference!");
            yield break;
        }
        
        Transform jointTransform = nyxGrappleRef.grappleJoint;
        Transform targetPoint = grapplePoint != null ? grapplePoint : transform;
        
        // Wait for the joint to get close to the grapple point
        while (isBeingGrappled && !isAnimating)
        {
            float distance = Vector3.Distance(jointTransform.position, targetPoint.position);
            
            // Check if joint is close enough to trigger the lever
            if (distance <= 0.5f) // Adjust this threshold as needed
            {
                TriggerLeverAnimation();
                break;
            }
            
            yield return null;
        }
    }
    
    // This method can also be called directly by the joint trigger system
    public void OnJointReached()
    {
        if (isBeingGrappled && !isAnimating)
        {
            TriggerLeverAnimation();
        }
    }
    
    private void TriggerLeverAnimation()
    {
        if (isAnimating || leverUsed) return;
        
        isAnimating = true;
        leverUsed = true; // Prevent multiple uses
        
        Debug.Log($"{gameObject.name} lever animation triggered");
        
        // Trigger the lever animation
        if (animator != null)
        {
            animator.SetTrigger(leverAnimationTrigger);
        }
        
        OnLeverActivated?.Invoke();
        
        // Start the animation duration coroutine
        StartCoroutine(HandleAnimationDuration());
    }
    
    private IEnumerator HandleAnimationDuration()
    {
        // Wait for the animation to complete
        yield return new WaitForSeconds(animationDuration);
        
        // Animation is done, now we can release the grapple
        isAnimating = false;
        isBeingGrappled = false;
        
        OnLeverCompleted?.Invoke();
        OnGrappleReleased?.Invoke();
        
        Debug.Log($"{gameObject.name} lever animation completed - releasing grapple");
        
        // Tell NyxGrapple to release the grapple
        if (nyxGrappleRef != null)
        {
            // Force release the grapple from NyxGrapple's side
            nyxGrappleRef.SendMessage("ForceReleaseGrapple", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    // Allow external scripts to check if lever is currently animating
    public bool IsAnimating => isAnimating;
    
    // Allow resetting the lever for testing purposes
    [ContextMenu("Reset Lever")]
    public void ResetLever()
    {
        leverUsed = false;
        isAnimating = false;
        isBeingGrappled = false;
        
        if (animator != null)
        {
            animator.ResetTrigger(leverAnimationTrigger);
        }
    }
}
