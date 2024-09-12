using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Collect_Health : MonoBehaviour
{
    public int hpValue;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            print("got soup");
            GameManager.instance.HealPlayer(hpValue);
            Destroy(this.gameObject);
        }
    }
    
}
