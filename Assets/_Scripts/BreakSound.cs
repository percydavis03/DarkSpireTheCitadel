using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakSound : MonoBehaviour
{
    public static BreakSound instance;
    public bool isBrokenOnce;
    public AudioSource breakingSound;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        isBrokenOnce = false;
    }


    public void BreakSoundGO()
    {
        isBrokenOnce = true;
        breakingSound.Play();
    }    
}
