using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main_Player : MonoBehaviour
{
    public static Main_Player instance;
    public PlayerSaveState thisGameSave;
    //--------------------------------------------
    public AudioSource ough;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

    }
    
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            GameManager.instance.DamagePlayer();
            print("ow");
            //ough.Play();
        }
        if (other.gameObject.CompareTag("JumpReward"))
        {
            thisGameSave.canJump = true;
            print("graduated from loser, can jump now");
        }
    }
    
    void Update()
    {
        
    }
}
