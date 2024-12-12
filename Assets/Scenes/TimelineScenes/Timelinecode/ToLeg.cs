using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class ToLeg : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.LoadScene("Leg", LoadSceneMode.Single);
    }
}
