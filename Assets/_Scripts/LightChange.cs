using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LightChange : MonoBehaviour
{
    // Start is called before the first frame update
    public Slider slider;
    public Light sceneLight;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        sceneLight.intensity = slider.value;
    }
}
