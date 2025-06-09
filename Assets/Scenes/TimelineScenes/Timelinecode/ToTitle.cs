using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToTitle : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.LoadScene("Title", LoadSceneMode.Single);
    }

}
