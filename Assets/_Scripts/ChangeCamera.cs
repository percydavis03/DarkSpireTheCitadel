using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{
    public GameObject newVCamera;
    public GameObject oldVCamera;

    //public GameObject newMainCam;
    //public GameObject oldMainCam;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        print("camera change");
        if (other.gameObject.CompareTag("Player") && oldVCamera.activeInHierarchy)
        {
            newVCamera.SetActive(true);
            oldVCamera.SetActive(false);
            /*if (!newMainCam.activeInHierarchy)
            {
                newMainCam.SetActive(true);
                oldMainCam.SetActive(false);
            }*/
            
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
