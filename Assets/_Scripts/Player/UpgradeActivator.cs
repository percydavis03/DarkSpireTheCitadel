using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple helper script to enable/disable GameObjects and scripts when called.
/// Use this to easily respond to upgrade events.
/// </summary>
public class UpgradeActivator : MonoBehaviour
{
    [Header("GameObjects to Control")]
    [Tooltip("GameObjects that will be enabled/disabled")]
    public GameObject[] targetGameObjects;
    
    [Header("Scripts to Control")]
    [Tooltip("MonoBehaviour scripts that will be enabled/disabled")]
    public MonoBehaviour[] targetScripts;
    
    [Header("Debug")]
    public bool enableDebugLogs = true;
    
    /// <summary>
    /// Enable all target GameObjects and scripts
    /// </summary>
    public void EnableTargets()
    {
        SetTargetsActive(true);
    }
    
    /// <summary>
    /// Disable all target GameObjects and scripts
    /// </summary>
    public void DisableTargets()
    {
        SetTargetsActive(false);
    }
    
    /// <summary>
    /// Toggle all target GameObjects and scripts
    /// </summary>
    public void ToggleTargets()
    {
        // Check the state of the first GameObject to determine toggle direction
        bool shouldEnable = false;
        if (targetGameObjects.Length > 0 && targetGameObjects[0] != null)
        {
            shouldEnable = !targetGameObjects[0].activeInHierarchy;
        }
        else if (targetScripts.Length > 0 && targetScripts[0] != null)
        {
            shouldEnable = !targetScripts[0].enabled;
        }
        
        SetTargetsActive(shouldEnable);
    }
    
    /// <summary>
    /// Set all targets to the specified active state
    /// </summary>
    public void SetTargetsActive(bool isActive)
    {
        // Enable/Disable GameObjects
        foreach (var gameObj in targetGameObjects)
        {
            if (gameObj != null)
            {
                gameObj.SetActive(isActive);
                
                if (enableDebugLogs)
                {
                    Debug.Log($"UpgradeActivator: {(isActive ? "Enabled" : "Disabled")} GameObject '{gameObj.name}'");
                }
            }
        }
        
        // Enable/Disable Scripts
        foreach (var script in targetScripts)
        {
            if (script != null)
            {
                script.enabled = isActive;
                
                if (enableDebugLogs)
                {
                    Debug.Log($"UpgradeActivator: {(isActive ? "Enabled" : "Disabled")} script '{script.GetType().Name}' on '{script.gameObject.name}'");
                }
            }
        }
    }
    
    /// <summary>
    /// Enable only GameObjects (not scripts)
    /// </summary>
    public void EnableGameObjectsOnly()
    {
        foreach (var gameObj in targetGameObjects)
        {
            if (gameObj != null)
            {
                gameObj.SetActive(true);
                if (enableDebugLogs)
                {
                    Debug.Log($"UpgradeActivator: Enabled GameObject '{gameObj.name}'");
                }
            }
        }
    }
    
    /// <summary>
    /// Enable only scripts (not GameObjects)
    /// </summary>
    public void EnableScriptsOnly()
    {
        foreach (var script in targetScripts)
        {
            if (script != null)
            {
                script.enabled = true;
                if (enableDebugLogs)
                {
                    Debug.Log($"UpgradeActivator: Enabled script '{script.GetType().Name}' on '{script.gameObject.name}'");
                }
            }
        }
    }
    
    /// <summary>
    /// Disable only GameObjects (not scripts)
    /// </summary>
    public void DisableGameObjectsOnly()
    {
        foreach (var gameObj in targetGameObjects)
        {
            if (gameObj != null)
            {
                gameObj.SetActive(false);
                if (enableDebugLogs)
                {
                    Debug.Log($"UpgradeActivator: Disabled GameObject '{gameObj.name}'");
                }
            }
        }
    }
    
    /// <summary>
    /// Disable only scripts (not GameObjects)
    /// </summary>
    public void DisableScriptsOnly()
    {
        foreach (var script in targetScripts)
        {
            if (script != null)
            {
                script.enabled = false;
                if (enableDebugLogs)
                {
                    Debug.Log($"UpgradeActivator: Disabled script '{script.GetType().Name}' on '{script.gameObject.name}'");
                }
            }
        }
    }
} 