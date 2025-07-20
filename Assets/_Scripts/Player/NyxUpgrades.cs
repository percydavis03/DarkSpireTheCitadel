using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Enum for all available save state boolean properties
public enum SaveStateProperty
{
    None,
    hasArm,
    hasSword,
    canJump,
    canAttack,
    inMenu
}

[System.Serializable]
public class UpgradeEntry
{
    [Header("Upgrade Configuration")]
    [Tooltip("Name for this upgrade (for debugging)")]
    public string upgradeName = "New Upgrade";
    
    [Tooltip("The save state property to monitor")]
    public SaveStateProperty saveStateProperty = SaveStateProperty.None;
    
    [Header("Events")]
    [Tooltip("Called when this upgrade becomes active (true)")]
    public UnityEvent onUpgradeEnabled;
    
    [Tooltip("Called when this upgrade becomes inactive (false)")]
    public UnityEvent onUpgradeDisabled;
    
    [Header("Status")]
    [Tooltip("Current state of this upgrade (read-only)")]
    public bool isCurrentlyActive = false;
    
    // Internal tracking
    [System.NonSerialized]
    public bool lastKnownState = false;
}

public class NyxUpgrades : MonoBehaviour
{
    [Header("Save State Reference")]
    [Tooltip("Reference to the player's save state")]
    public PlayerSaveState playerSaveState;
    
    [Header("Upgrade Configuration")]
    [Tooltip("List of all upgrades to monitor")]
    public UpgradeEntry[] upgrades;
    
    [Header("Settings")]
    [Tooltip("Check for changes every frame vs only on start - DISABLE for performance")]
    public bool continuousMonitoring = false; // DISABLED - was causing lag
    
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogs = false; // DISABLED - was causing spam
    
    [Header("Auto-Setup")]
    [Tooltip("Try to find save state automatically if not assigned")]
    public bool autoFindSaveState = true;
    
    [Header("GameManager Integration")]
    [Tooltip("Try to find and use existing GameManager")]
    public bool useGameManager = true;

    // C# Events for code-based subscriptions
    public static System.Action<SaveStateProperty> OnUpgradeEnabled;
    public static System.Action<SaveStateProperty> OnUpgradeDisabled;

    private void Start()
    {
        // Auto-find save state if not assigned
        if (playerSaveState == null && autoFindSaveState)
        {
            AttemptAutoFindSaveState();
        }
        
        if (playerSaveState == null)
        {
            Debug.LogError("NyxUpgrades: No PlayerSaveState assigned! Please assign one in the inspector or ensure a Player_Movement component exists in the scene.");
            enabled = false;
            return;
        }
        
        // Initialize all upgrades
        InitializeUpgrades();
        
        // Apply initial states
        CheckAndApplyUpgrades();
        
        // Debug logs completely disabled for performance - was causing lag
        // if (enableDebugLogs)
        // {
        //     Debug.Log($"NyxUpgrades: Initialized with {upgrades.Length} upgrades");
        // }
    }

    private void Update()
    {
        if (continuousMonitoring && playerSaveState != null)
        {
            CheckAndApplyUpgrades();
        }
    }
    
    private void AttemptAutoFindSaveState()
    {
        // Try GameManager first if enabled
        if (useGameManager)
        {
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null && gameManager.thisGameSave != null)
            {
                playerSaveState = gameManager.thisGameSave;
                if (enableDebugLogs)
                {
                    Debug.Log("NyxUpgrades: Auto-found PlayerSaveState from GameManager");
                }
                return;
            }
        }
        
        // Try to find from Player_Movement
        var playerMovement = FindObjectOfType<Player_Movement>();
        if (playerMovement != null && playerMovement.thisGameSave != null)
        {
            playerSaveState = playerMovement.thisGameSave;
            if (enableDebugLogs)
            {
                Debug.Log("NyxUpgrades: Auto-found PlayerSaveState from Player_Movement");
            }
            return;
        }
        
        // Try to find from Main_Player
        var mainPlayer = FindObjectOfType<Main_Player>();
        if (mainPlayer != null && mainPlayer.thisGameSave != null)
        {
            playerSaveState = mainPlayer.thisGameSave;
            if (enableDebugLogs)
            {
                Debug.Log("NyxUpgrades: Auto-found PlayerSaveState from Main_Player");
            }
            return;
        }
        
        if (enableDebugLogs)
        {
            Debug.LogWarning("NyxUpgrades: Could not auto-find PlayerSaveState. Please assign manually.");
        }
    }
    
    private void InitializeUpgrades()
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            var upgrade = upgrades[i];
            
            // Validate upgrade configuration
            if (upgrade.saveStateProperty == SaveStateProperty.None)
            {
                Debug.LogWarning($"NyxUpgrades: Upgrade '{upgrade.upgradeName}' has no save state property selected!");
                continue;
            }
            
            // Get initial state
            bool currentState = GetSaveStateProperty(upgrade.saveStateProperty);
            upgrade.lastKnownState = currentState;
            upgrade.isCurrentlyActive = currentState;
            
            // Debug logs completely disabled for performance - was causing lag
            // if (enableDebugLogs)
            // {
            //     Debug.Log($"NyxUpgrades: Initialized '{upgrade.upgradeName}' - Property: '{upgrade.saveStateProperty}' - State: {currentState}");
            // }
        }
    }
    
    private void CheckAndApplyUpgrades()
    {
        for (int i = 0; i < upgrades.Length; i++)
        {
            var upgrade = upgrades[i];
            
            if (upgrade.saveStateProperty == SaveStateProperty.None)
                continue;
                
            // Check if state has changed
            bool currentState = GetSaveStateProperty(upgrade.saveStateProperty);
            
            if (currentState != upgrade.lastKnownState)
            {
                // State changed - fire events
                TriggerUpgradeEvents(upgrade, currentState);
                upgrade.lastKnownState = currentState;
                upgrade.isCurrentlyActive = currentState;
                
                // Debug logs completely disabled for performance - was causing lag
                // if (enableDebugLogs)
                // {
                //     Debug.Log($"NyxUpgrades: '{upgrade.upgradeName}' state changed to {currentState}");
                // }
            }
        }
    }
    
    private bool GetSaveStateProperty(SaveStateProperty property)
    {
        if (playerSaveState == null)
            return false;
            
        switch (property)
        {
            case SaveStateProperty.hasArm:
                return playerSaveState.hasArm;
            case SaveStateProperty.canJump:
                return playerSaveState.canJump;
            case SaveStateProperty.canAttack:
                return playerSaveState.canAttack;
            case SaveStateProperty.inMenu:
                return playerSaveState.inMenu;
            default:
                return false;
        }
    }
    
    private void TriggerUpgradeEvents(UpgradeEntry upgrade, bool isEnabled)
    {
        if (isEnabled)
        {
            // Trigger UnityEvent
            upgrade.onUpgradeEnabled?.Invoke();
            
            // Trigger C# event
            OnUpgradeEnabled?.Invoke(upgrade.saveStateProperty);
            
            // Debug logs completely disabled for performance - was causing lag
            // if (enableDebugLogs)
            // {
            //     Debug.Log($"NyxUpgrades: Triggered 'onUpgradeEnabled' for '{upgrade.upgradeName}'");
            // }
        }
        else
        {
            // Trigger UnityEvent
            upgrade.onUpgradeDisabled?.Invoke();
            
            // Trigger C# event
            OnUpgradeDisabled?.Invoke(upgrade.saveStateProperty);
            
            // Debug logs completely disabled for performance - was causing lag
            // if (enableDebugLogs)
            // {
            //     Debug.Log($"NyxUpgrades: Triggered 'onUpgradeDisabled' for '{upgrade.upgradeName}'");
            // }
        }
    }
    
    // Public methods for manual control
    public void ForceCheckUpgrades()
    {
        CheckAndApplyUpgrades();
    }
    
    public void TriggerUpgradeByName(string upgradeName, bool isEnabled)
    {
        var upgrade = GetUpgradeByName(upgradeName);
        if (upgrade != null)
        {
            TriggerUpgradeEvents(upgrade, isEnabled);
            upgrade.isCurrentlyActive = isEnabled;
        }
    }
    
    public void TriggerUpgradeByProperty(SaveStateProperty property, bool isEnabled)
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade.saveStateProperty == property)
            {
                TriggerUpgradeEvents(upgrade, isEnabled);
                upgrade.isCurrentlyActive = isEnabled;
                break;
            }
        }
    }
    
    public bool IsUpgradeActive(string upgradeName)
    {
        var upgrade = GetUpgradeByName(upgradeName);
        return upgrade != null ? upgrade.isCurrentlyActive : false;
    }
    
    public bool IsUpgradeActive(SaveStateProperty property)
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade.saveStateProperty == property)
            {
                return upgrade.isCurrentlyActive;
            }
        }
        return false;
    }
    
    private UpgradeEntry GetUpgradeByName(string upgradeName)
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade.upgradeName == upgradeName)
            {
                return upgrade;
            }
        }
        return null;
    }
    
    // Editor/Debug methods
    [ContextMenu("Force Check All Upgrades")]
    private void ContextMenuForceCheck()
    {
        if (Application.isPlaying)
        {
            ForceCheckUpgrades();
        }
    }
    
    [ContextMenu("Log Current States")]
    private void ContextMenuLogStates()
    {
        if (Application.isPlaying && playerSaveState != null)
        {
            Debug.Log("=== Current Upgrade States ===");
            foreach (var upgrade in upgrades)
            {
                bool currentState = GetSaveStateProperty(upgrade.saveStateProperty);
                Debug.Log($"{upgrade.upgradeName}: {currentState} (Active: {upgrade.isCurrentlyActive})");
            }
        }
    }
}
