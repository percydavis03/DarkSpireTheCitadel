using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public static MenuScript instance;
    public PlayerSaveState thisGameSave;

    public GameObject infoMenu;
    public GameObject mainMenu;
    public bool isTitle;
    public DOTweenVisualManager visualManager;
    public DOTweenAnimation veinAnim;

    
    private bool inMainMenu;

    private void Awake()
    {
        instance = this; 
    }
    private void Start()
    {
        if (isTitle == true)
        {
            Cursor.visible = true;
        }
        
        infoMenu.SetActive(false);
        mainMenu.SetActive(false);
    }

   

    public void MainMenu()
    {
        SetFirstSelect.instance.SetPauseSelectButton();
        if (!mainMenu.activeInHierarchy && !infoMenu.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (mainMenu.activeInHierarchy)
        {
            mainMenu.SetActive(false);
            Player_Movement.instance.canMove = true;
            
            inMainMenu = false;
            thisGameSave.inMenu = false;
        }
        else if (!mainMenu.activeInHierarchy && !infoMenu.activeInHierarchy)
        {
           mainMenu.SetActive(true);
           SetFirstSelect.instance.SetPauseSelectButton();
           thisGameSave.inMenu = true;
           inMainMenu = true;
        }
    }

    public void InfoMenu()
    {
        InventoryManager.Instance.ListItems();
        SetFirstSelect.instance.SetInfoSelectButton();

        if (!infoMenu.activeInHierarchy && !mainMenu.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (infoMenu.activeInHierarchy)
        {
            infoMenu.SetActive(false);
           
            thisGameSave.playerSpeed = Player_Movement.instance.speed;
            thisGameSave.inMenu = false;
        }
        else if (!infoMenu.activeInHierarchy && !mainMenu.activeInHierarchy)
        {
            infoMenu.SetActive(true);
            thisGameSave.inMenu = true;
            if (visualManager != null)
            {
                visualManager.enabled = true;
            }
        }
    }

    private void Update()
    {
        if (infoMenu == null && isTitle == false)
        {
            Cursor.visible=false;
        }

        if (mainMenu == null && isTitle == false)
        {
            Cursor.visible = false;
        }
        //exit menus
        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (mainMenu.activeInHierarchy)
            {
                mainMenu.SetActive(false);
                thisGameSave.inMenu = false;
            }
           if (infoMenu.activeInHierarchy)
            {
                infoMenu.SetActive(false);
                thisGameSave.inMenu = false;
            }
        }*/
        
    }


}
