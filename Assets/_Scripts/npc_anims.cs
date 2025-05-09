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

    public float delayTime;

    // Start is called before the first frame update
    void Start()
    {
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
            anim.SetBool("sleeping", true) ;
        }
        if (isLaying)
        {
            anim.SetBool("sleeping", true);
        }
    }
    public void restartAnim()
    {
        anim.SetBool("writhing", false);
        StartCoroutine(WaitUntil(delayTime));
    }
    IEnumerator WaitUntil(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        anim.SetBool("writhing", true);

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
