using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddEntryNoDestroy : MonoBehaviour
{
    public Item Item;
    public bool canPickup = false;
    public GameObject visual;


    //TESTING
    private void Start()
    {

    }

    void Pickup()
    {

        InventoryManager.Instance.Add(Item);
        InventoryManager.Instance.Popup(Item);

        Destroy(visual);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (canPickup)
        {
            Pickup();
            canPickup = false;
        }
       
    }

    private void OnTriggerExit(Collider other)
    {
      
    }

    private void OnMouseDown()
    {
        //Pickup();
    }
}
