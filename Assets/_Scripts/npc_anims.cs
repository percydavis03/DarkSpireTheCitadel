using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class npc_anims : MonoBehaviour
{
    public Animator anim;
    public bool isPraying;
    public bool isWrithing;

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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
