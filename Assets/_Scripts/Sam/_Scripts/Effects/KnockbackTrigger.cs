using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ⚠️ LEGACY/DEPRECATED ⚠️
/// This trigger-based knockback system is deprecated. Use the new KnockbackManager system instead.
/// See Assets/_Scripts/Knockback/ for the modern, industry-standard system.
/// 
/// Migration: Use KnockbackManager.Instance.ApplyKnockback() in your OnTriggerEnter method instead.
/// </summary>
[System.Obsolete("Use KnockbackManager.Instance.ApplyKnockback() instead. See Assets/_Scripts/Knockback/", false)]
public class KnockbackTrigger : MonoBehaviour
{
    //Amount of force for knockback
    public float knockBackValue;

    //Get the specific tag 
    public string knocTag;

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag(knocTag))
        {
            var kockObj = other.gameObject.GetComponent<Knock>();
            if (kockObj != null)
            {
                kockObj.knockBackVel = knockBackValue; //use knock back
                print(kockObj.name);
                kockObj.KnockBack(transform);

            }
        }
    }
}
