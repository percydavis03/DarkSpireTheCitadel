using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIckUpItem : MonoBehaviour
{
    public Weapon itemWeapon;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player_Manager.GetInstance().specialWeapon = itemWeapon;
            //Corruption.instance.hasEvilWeapon = true;
            Destroy(gameObject);
        }
    }
}
