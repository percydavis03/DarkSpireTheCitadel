using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LockedDoor : MonoBehaviour
{
    public int index;
    public GameObject doorKey;
    //public GameObject sword;
    public Animator anim;
    public GameObject door;
    private void Start()
    {
        anim = door.GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && doorKey.activeInHierarchy) 
        {
            anim.SetBool("OpenDoor", true);
        }
    }
}
