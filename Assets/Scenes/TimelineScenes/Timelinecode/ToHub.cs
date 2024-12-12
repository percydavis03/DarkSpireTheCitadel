using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class ToHub : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.LoadScene("Hub", LoadSceneMode.Single);
    }
}
