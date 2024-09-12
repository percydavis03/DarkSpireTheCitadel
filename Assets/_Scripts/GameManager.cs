using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //PlayerInfo
    public PlayerSaveState thisGameSave;


    //Scene
    public GameObject Player;
    public Transform spawnPos;
    public int thisScene;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Game Manager");
        }
        instance = this;
        //idk what this is here for
        Player = GameObject.FindWithTag("Player");
    }

    //get rid of this later?
    public static GameManager GetInstance()
    {
        return instance;
    }

    public void DamagePlayer()
    {
        --thisGameSave.hitpoints;
    }

    public void HealPlayer(int amt)
    {
        print("good soup");

        if (thisGameSave.hitpoints < thisGameSave.maxHP)
        {
            thisGameSave.hitpoints += amt;
        }
        
    }

    public void SpeedBoost()
    {
        thisGameSave.playerSpeed = 8;
    }
    public void RestartScene()
    {
        SceneManager.LoadScene(thisScene);
    }

    void Update()
    {
        if (thisGameSave.hitpoints <= 0)
        {
            RestartScene();
        }
        if (thisGameSave.hitpoints >= thisGameSave.maxHP)
        {
            
            thisGameSave.hitpoints = thisGameSave.maxHP;
        }
    }
}
