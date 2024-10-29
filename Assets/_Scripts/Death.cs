using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Death : MonoBehaviour
{
    public PlayerSaveState thisGameSave;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (thisGameSave.hitpoints <= 0)
        {
            thisGameSave.Init();

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
