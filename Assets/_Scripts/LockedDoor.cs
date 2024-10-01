using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LockedDoor : MonoBehaviour
{
    public int index;
    public GameObject doorKey;
    public GameObject sword;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && doorKey.activeInHierarchy && sword.activeInHierarchy) 
        {
            SceneManager.LoadScene(index);
        }
    }
}
