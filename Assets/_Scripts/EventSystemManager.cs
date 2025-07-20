using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ensures only one Event System exists in the scene
/// Attach this to any GameObject to automatically clean up duplicate Event Systems
/// </summary>
public class EventSystemManager : MonoBehaviour
{
    [Header("Event System Management")]
    [Tooltip("If true, this will be the primary Event System and others will be destroyed")]
    public bool isPrimary = true;
    
    void Awake()
    {
        // Find all Event Systems in the scene
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        
        if (eventSystems.Length > 1)
        {
            Debug.LogWarning($"Found {eventSystems.Length} Event Systems in scene. Cleaning up duplicates...");
            
            EventSystem primaryEventSystem = null;
            
            // Try to find one marked as primary
            foreach (var es in eventSystems)
            {
                var manager = es.GetComponent<EventSystemManager>();
                if (manager != null && manager.isPrimary)
                {
                    primaryEventSystem = es;
                    break;
                }
            }
            
            // If no primary found, use the first one
            if (primaryEventSystem == null)
            {
                primaryEventSystem = eventSystems[0];
            }
            
            // Destroy all others
            foreach (var es in eventSystems)
            {
                if (es != primaryEventSystem)
                {
                    Debug.Log($"Destroying duplicate Event System on {es.gameObject.name}");
                    if (Application.isPlaying)
                    {
                        Destroy(es.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(es.gameObject);
                    }
                }
            }
            
            Debug.Log($"Event System cleanup complete. Primary Event System: {primaryEventSystem.gameObject.name}");
        }
        else if (eventSystems.Length == 1)
        {
            Debug.Log("✓ Only one Event System found - no cleanup needed");
        }
        else
        {
            Debug.LogError("No Event System found in scene! UI input will not work.");
        }
    }
} 