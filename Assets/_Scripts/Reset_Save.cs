using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reset_Save : MonoBehaviour
{
    public PlayerSaveState thisGameSave;

    private void Awake()
    {
        

    }
    
    void Start()
    {
        thisGameSave.Init();
        thisGameSave.canAttack = false;
        thisGameSave.canJump = false;
        thisGameSave.hasArm  = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
