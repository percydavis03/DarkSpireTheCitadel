using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class MainLevel : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.LoadScene("Alchemist_Lab", LoadSceneMode.Single);
    }
}
