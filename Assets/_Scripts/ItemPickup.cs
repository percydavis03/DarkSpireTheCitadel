using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//using UnityEngine.InputSystem;


public class ItemPickup : MonoBehaviour
{
    public Item Item;
    public bool canPickup = false;
    public AudioSource pickupSound;


    //TESTING
    private void Start()
    {
        
    }

    void Pickup()
    {
        pickupSound.Play();
        InventoryManager.Instance.Add(Item);
        InventoryManager.Instance.Popup(Item);
        
        Destroy(gameObject);

    }

    private void OnTriggerEnter(Collider other)
    {
        Pickup();
        
        canPickup = true;
    }

    private void OnTriggerExit(Collider other)
    {
        canPickup = false;
    }

    private void OnMouseDown()
    {
        Pickup();
    }
}
