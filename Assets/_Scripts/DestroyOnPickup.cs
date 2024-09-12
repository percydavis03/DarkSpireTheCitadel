using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DestroyOnPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            print("rip");
            StartCoroutine(Wait());
            Destroy(this.gameObject);
        }
    }
   
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        print("waited");
    }

}
