using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class HallwayTheme : MonoBehaviour
{
    public AudioSource hallwayTheme;
    public BoxCollider musicTrigger;

    private void Start()
    {
      
        
    }

    private void OnTriggerEnter(Collider other)
    {
        hallwayTheme.Play();
    }

   
}
