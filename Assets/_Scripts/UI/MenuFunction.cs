using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuFunction : MonoBehaviour
{
    public GameObject theMenu;
    
    public AudioSource buttonpress;

  

    // Start is called before the first frame update
    void Start()
    {
       
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
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
        //Cursor.visible = false;
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
