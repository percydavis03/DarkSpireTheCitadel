using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipIntro : MonoBehaviour
{
    public int sceneIndex;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            print("skip");
            SceneManager.LoadScene(sceneIndex);
        }
    }
}
