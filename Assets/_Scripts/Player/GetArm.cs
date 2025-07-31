using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetArm : MonoBehaviour
{
    public static GetArm instance; 
   
    public GameObject pickupText;

    public Item Item;
    public bool canPickup = false;
   

    
    //TESTING
    private void Start()
    {

    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void PickupArm()
    {
        // Clean up the pickup state before destroying
        Player_Movement.instance.inPickupZone = false;
        pickupText.SetActive(false);
        canPickup = false;
        
        ArmActivate.instance.AddArm();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        
        Player_Movement.instance.inPickupZone = true;
        pickupText.SetActive(true);
        canPickup = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Player_Movement.instance.inPickupZone = false;
        pickupText.SetActive(false);
        canPickup = false;
    }

    private void OnMouseDown()
    {
        
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
