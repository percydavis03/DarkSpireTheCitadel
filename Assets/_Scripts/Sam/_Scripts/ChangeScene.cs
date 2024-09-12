using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    public int scene;
    public AudioSource buttonclick;

    public void GoTo() 
    {
        buttonclick.Play();
        SceneManager.LoadScene(scene);
    }

    
}
