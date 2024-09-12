using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//using UnityEngine.InputSystem;


public class ItemPickup : MonoBehaviour
{
    public Item Item;
    public bool canPickup = false;
   


    //TESTING
    private void Start()
    {
        
    }

    void Pickup()
    {

        InventoryManager.Instance.Add(Item);
       // InventoryManager.Instance.Popup(Item);
        
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
