using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMenu : MonoBehaviour
{
    public GameObject thisMenu;

    public GameObject otherMenu1;
    public GameObject otherMenu2;
    public GameObject otherMenu3;
    
    // Start is called before the first frame update
    void Start()
    {
      
    }

    public void MenuSelected()
    {
        thisMenu.SetActive(true);
        otherMenu1.SetActive(false);
        otherMenu2.SetActive(false);
        otherMenu3.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
