using UnityEngine;

public class SimpleKnockbackTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestKnockback();
        }
    }
    
    void TestKnockback()
    {
        // Find first object with KnockbackReceiver
        KnockbackReceiver target = FindObjectOfType<KnockbackReceiver>();
        
        if (target == null)
        {
            Debug.LogWarning("No KnockbackReceiver found! Add KnockbackReceiver component to an enemy.");
            return;
        }
        
        // Apply knockback
        KnockbackManager.Instance.ApplyKnockback(
            target.gameObject,
            transform.position,
            null, // Uses default data
            1f    // Normal force
        );
        
        Debug.Log("Applied test knockback!");
    }
} 