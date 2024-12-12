using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class ToSkin : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.LoadScene("Skin", LoadSceneMode.Single);
    }
}
