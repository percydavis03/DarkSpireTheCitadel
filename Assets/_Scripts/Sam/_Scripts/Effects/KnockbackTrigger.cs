using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
