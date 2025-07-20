using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dedicated script for foot collider - handles knockdown attacks only
/// Attach this to your foot collider GameObject
/// </summary>
public class FootKickScript : MonoBehaviour
{
    [Header("Knockdown Settings")]
    [Tooltip("Force applied when kicking enemies")]
    public Vector3 kickForce = new Vector3(20, 5, 20);
    
    [Header("Debug Settings")]
    [Tooltip("Enable debug logs for foot kicks")]
    public bool enableDebugLogs = true;
    
    private void OnTriggerEnter(Collider other)
    {
        if (enableDebugLogs) Debug.Log($"🦵 FOOT COLLIDER hit: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        
        // Only affect enemies
        if (other.gameObject.CompareTag("Enemy"))
        {
            ApplyKnockdown(other);
        }
        else
        {
            if (enableDebugLogs) Debug.Log($"🦵 Not an enemy: {other.gameObject.name}");
        }
    }
    
    /// <summary>
    /// Apply knockdown to enemy - no damage, just knockdown animation
    /// </summary>
    private void ApplyKnockdown(Collider enemyCollider)
    {
        if (enableDebugLogs) Debug.Log($"🦵 APPLYING KNOCKDOWN to {enemyCollider.name}");
        
        bool knockdownApplied = false;
        
        // Handle Enemy_Basic
        if (enemyCollider.TryGetComponent(out Enemy_Basic enemyBasic))
        {
            if (enemyBasic.anim != null)
            {
                enemyBasic.anim.SetBool("isKnockedDown", true);
                enemyBasic.StopMoving();
                knockdownApplied = true;
                if (enableDebugLogs) Debug.Log($"🦵 Enemy_Basic {enemyBasic.name} KNOCKED DOWN!");
            }
            else
            {
                if (enableDebugLogs) Debug.LogWarning($"🦵 Enemy_Basic {enemyBasic.name} has no animator!");
            }
        }
        // Handle Worker
        else if (enemyCollider.TryGetComponent(out Worker worker))
        {
            if (worker.anim != null)
            {
                worker.anim.SetBool("isKnockedDown", true);
                worker.StopMoving();
                knockdownApplied = true;
                if (enableDebugLogs) Debug.Log($"🦵 Worker {worker.name} KNOCKED DOWN!");
            }
            else
            {
                if (enableDebugLogs) Debug.LogWarning($"🦵 Worker {worker.name} has no animator!");
            }
        }
        else
        {
            if (enableDebugLogs) Debug.LogWarning($"🦵 {enemyCollider.name} has no Enemy_Basic or Worker component!");
        }
        
        if (!knockdownApplied)
        {
            if (enableDebugLogs) Debug.LogError($"🦵 FAILED to apply knockdown to {enemyCollider.name}");
        }
    }
    
    /// <summary>
    /// Call this to enable/disable debug logging
    /// </summary>
    public void SetDebugLogging(bool enabled)
    {
        enableDebugLogs = enabled;
    }
} 