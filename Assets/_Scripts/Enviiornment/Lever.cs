using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    [Header("Lever Settings")]
    public Animator leverAnimator;
    
    [Header("Save State")]
    public PlayerSaveState thisGameSave; // Direct reference to save state
    
    [Header("Interaction Settings")]
    public bool requiresArm = true; // Set to false if lever doesn't require the arm
    
    [Header("Events")]
    [Tooltip("Called when the lever is successfully activated")]
    public UnityEvent OnLeverActivated;
    [Tooltip("Called when activation fails due to missing arm")]
    public UnityEvent OnActivationFailed;
    [Tooltip("Called when player enters the trigger zone")]
    public UnityEvent OnPlayerEnterTrigger;
    [Tooltip("Called when player exits the trigger zone")]
    public UnityEvent OnPlayerExitTrigger;
    
    private bool isActivated = false;
    private bool playerInTrigger = false;
    
    // Public property to check if lever is activated
    public bool IsActivated => isActivated;
    
    // Start is called before the first frame update
    void Start()
    {
        // Ensure we have an animator component
        if (leverAnimator == null)
        {
            leverAnimator = GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check for player interaction when in trigger
        if (playerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            TryActivateLever();
        }
    }
    
    /// <summary>
    /// Public function to activate the lever - sets isActivated to true in animator
    /// </summary>
    public void ActivateLever()
    {
        if (leverAnimator != null && !isActivated)
        {
            leverAnimator.SetBool("isActivated", true);
            isActivated = true;
            Debug.Log($"Lever {gameObject.name} activated!");
            
            // Invoke the activation event
            OnLeverActivated?.Invoke();
        }
    }
    
    /// <summary>
    /// Tries to activate the lever, checking if player has arm if required
    /// </summary>
    private void TryActivateLever()
    {
        // Check if lever requires arm and if player doesn't have it
        if (requiresArm && !HasArm())
        {
            // TODO: Show dialog for needing arm - dialog system will be added later
            ShowNeedArmDialog();
            return;
        }
        
        // Player has arm or lever doesn't require it - activate the lever
        ActivateLever();
    }
    
    /// <summary>
    /// Checks if Nyx has his new arm by checking the save state
    /// </summary>
    private bool HasArm()
    {
        // Direct check using save state reference (same pattern as Player_Movement)
        if (thisGameSave != null)
        {
            return thisGameSave.hasArm;
        }
        
        // Fallback: Check if ArmActivate instance exists and check the save state
        if (ArmActivate.instance != null && ArmActivate.instance.thisGameSave != null)
        {
            return ArmActivate.instance.thisGameSave.hasArm;
        }
        
        // Final fallback: assume player doesn't have arm if save state doesn't exist
        return false;
    }
    
    /// <summary>
    /// Placeholder for showing dialog when arm is needed
    /// Dialog system will be implemented later
    /// </summary>
    private void ShowNeedArmDialog()
    {
        Debug.Log("Player needs arm to activate this lever - Dialog will be shown here later");
        // TODO: Implement dialog showing "You need your arm to operate this lever"
        
        // Invoke the activation failed event
        OnActivationFailed?.Invoke();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            Debug.Log("Player entered lever trigger zone");
            // TODO: Show interaction prompt UI here if needed
            
            // Invoke the player enter trigger event
            OnPlayerEnterTrigger?.Invoke();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
            Debug.Log("Player exited lever trigger zone");
            // TODO: Hide interaction prompt UI here if needed
            
            // Invoke the player exit trigger event
            OnPlayerExitTrigger?.Invoke();
        }
    }
}
