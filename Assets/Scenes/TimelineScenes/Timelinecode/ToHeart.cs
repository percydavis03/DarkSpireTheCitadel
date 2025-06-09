using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class ToHeart : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.LoadScene("Heart_Grey", LoadSceneMode.Single);
    }
}
