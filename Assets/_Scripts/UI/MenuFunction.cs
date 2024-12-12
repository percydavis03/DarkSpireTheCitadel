using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuFunction : MonoBehaviour
{
    public GameObject theMenu;
    
    public AudioSource buttonpress;

    public PlayerSaveState thisGameSave;
    
    
    public void ButtonSound()
    {
        buttonpress.Play();
    }
    public void Resume()
    {
        theMenu.SetActive(false);
    }

    public void CloseMenu()
    {
        theMenu.SetActive(false);
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
