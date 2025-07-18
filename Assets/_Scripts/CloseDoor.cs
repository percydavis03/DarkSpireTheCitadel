using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseDoor : MonoBehaviour
{
    
    public Animator anim;
    public GameObject door;
    private void Start()
    {
        anim = door.GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            anim.SetBool("CloseDoor", true);
        }
    }
    public void OpenDoor()
    {
        anim.SetBool("OpenDoor", true);
    }
    public void ClosedDoor()
    {
        anim.SetBool("OpenDoor", false);
    }
}
