using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetFirstSelect : MonoBehaviour
{
    public static SetFirstSelect instance;

    public Button pauseMenuFirstSelect;
    public Button infoMenuFirstSelect;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void SetPauseSelectButton()
    {
        pauseMenuFirstSelect.Select();
    }

    public void SetInfoSelectButton()
    {
        infoMenuFirstSelect.Select();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
