using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npc_anims : MonoBehaviour
{
    public Animator anim;
    public bool isPraying;
    public bool isWrithing;
    public bool isSleeping;
    public bool isInjured;
    public bool isLaying;
    public bool isDemure;
    public bool isOld;

    

    // Start is called before the first frame update
    void Start()
    {
        if (anim)
        {
            float randomOffset = Random.Range(0f, 1f);
            anim.Play(0, -3, randomOffset);
           
        }
        if (isWrithing)
        {
            anim.SetBool("writhing", true);
        }
        if (isPraying)
        {
            anim.SetBool("praying", true);
        }
        if (isSleeping)
        {
            if (anim)
            {
                anim.SetBool("sleeping", true);
                float randomOffset = Random.Range(0f, 1f);
                anim.Play(0, -3, randomOffset);

            }
            
        }
        if (isLaying)
        {
            anim.SetBool("laying", true);
        }
        if (isDemure)
        {
            anim.SetBool("demure", true);
        }
        if (isOld)
        {
            anim.SetBool("oldman", true);
        }
        if (isInjured)
        {
            anim.SetBool("injuredstanding", true);
        }

    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
