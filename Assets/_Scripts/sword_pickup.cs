using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sword_pickup : MonoBehaviour
{
    public GameObject sword;
    public GameObject thisOne;
    public PlayerSaveState thisGameSave;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Player"))
        {
            print("collide");
            thisGameSave.canAttack = true;
            sword.SetActive(true);
            thisOne.SetActive(false);
        }
       
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
