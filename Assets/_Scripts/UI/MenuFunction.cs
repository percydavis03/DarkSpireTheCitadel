using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;


public class MenuFunction : MonoBehaviour
{
    public GameObject theMenu;
    public GameObject instructions;

    
    public StudioEventEmitter buttonpress;

    public PlayerSaveState thisGameSave;
    
    
    public void ButtonSound()
    {
        buttonpress.Play();
    }
    public void Resume()
    {
        theMenu.SetActive(false);
    }
    public void Controls()
    {
        instructions.SetActive(true);
    }
    public void CloseMenu()
    {
        theMenu.SetActive(false);
        instructions.SetActive(false);
    }

    public void QuitGame()
    {
        Cursor.visible = true;
        Application.Quit();
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex +0); 
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene("Title");
    }

   
}
