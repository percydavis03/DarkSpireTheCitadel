using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlittleMenu : MonoBehaviour
{
    private bool already;
    public GameObject credits;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void PressButton()
    {
        if (!already)
        {
            credits.SetActive(true);
            already = true;
        }
        else if (already)
        {
            credits.SetActive(false);
            already = false;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
