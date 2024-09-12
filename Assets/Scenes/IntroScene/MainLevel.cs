using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class MainLevel : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.LoadScene("Main_Test", LoadSceneMode.Single);
    }
}
