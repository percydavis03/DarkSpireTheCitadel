using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public static MenuScript instance;
    public GameObject infoMenu;
    public GameObject pauseMenu;
    public bool isTitle;
    public DOTweenVisualManager visualManager;
    public DOTweenAnimation veinAnim;

    public bool movePause;
    //input system
    public PlayerInputActions playerControls;
    private InputAction openInfoMenu;
    private InputAction openPauseMenu;

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
        
        movePause = false;
        infoMenu.SetActive(false);
        pauseMenu.SetActive(false);
    }

    private void OnEnable()
    {
       // openInfoMenu = playerControls.General.Inventory;
        //openInfoMenu.Enable();
        //openPauseMenu = playerControls.General.PauseMenu;
        //openPauseMenu.Enable();
    }
    private void OnDisable()
    {
        //openInfoMenu.Disable();
        //openPauseMenu.Disable();
    }



    private void Update()
    {
        if (infoMenu == null && isTitle == false)
        {
            Cursor.visible=false;
        }

        if (pauseMenu == null && isTitle == false)
        {
            Cursor.visible = false;
        }
        //exit menus
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeInHierarchy)
            { 
                pauseMenu.SetActive(false);
            }
           if (infoMenu.activeInHierarchy)
            {
                infoMenu.SetActive(false);
            }
        }
        //change when the input system stops being a little bitch
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pauseMenu.activeInHierarchy && !infoMenu.activeInHierarchy)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (pauseMenu.activeInHierarchy)
            {
                pauseMenu.SetActive(false);
            }
            else
            {
                pauseMenu.SetActive(true);
            }
        }
        //change when the input system stops being a little bitch
        if (Input.GetKeyDown(KeyCode.Tab))
        //if (openInfoMenu.IsPressed())
        {
            InventoryManager.Instance.ListItems();
            

            if (!infoMenu.activeInHierarchy)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                movePause = true;
                
            }
            if (infoMenu.activeInHierarchy)
            {
                infoMenu.SetActive(false);
               movePause = false;
                
            }
            else
            {
                infoMenu.SetActive(true);
                if (visualManager != null)
                {
                    visualManager.enabled = true;
                }
            }

        }
    }


}
