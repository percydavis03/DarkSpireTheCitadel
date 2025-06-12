using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class Grappleable : MonoBehaviour
{
    [Header("Grappleable Settings")]
    public bool canBeGrappled = true;
    public bool isEnemy = false;
    public Animator animator;
    
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
    
    private bool isBeingGrappled = false;
    
    public bool IsBeingGrappled => isBeingGrappled;
    
    void Start()
    {
        if (isEnemy)
        {
            navAgent = GetComponent<NavMeshAgent>();
            workerScript = GetComponent<Worker>();
            if (navAgent != null)
            {
                originalSpeed = navAgent.speed;
            }
        }
    }
    
    // Called when this object starts being grappled
    public void StartGrapple()
    {
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
        Debug.Log($"{gameObject.name} is being grappled");
    }
    
    // Called when this object is released from grapple
    public void ReleaseGrapple()
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
