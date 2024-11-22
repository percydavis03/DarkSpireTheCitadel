using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class simple_lock : MonoBehaviour
{
    public int index;
    public GameObject doorKey;
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && doorKey.activeInHierarchy)
        {
            SceneManager.LoadScene(index);
        }
    }
}
