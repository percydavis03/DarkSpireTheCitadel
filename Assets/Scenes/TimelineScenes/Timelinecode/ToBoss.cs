using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class ToBoss : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.LoadScene("Lvl", LoadSceneMode.Single);
    }
}
