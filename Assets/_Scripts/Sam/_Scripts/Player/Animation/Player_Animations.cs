using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Animations : MonoBehaviour
{
    public Animator anim;
    [SerializeField] string[] animName;
    [SerializeField] Player playerMovement;
    [SerializeField] Slash2 playerSlash;
    [SerializeField] ParryBoxCollider parry;
    [SerializeField] Roll roll;
    [SerializeField] float walkFlaot;
    [SerializeField] float walkFloatMultiplier;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        anim.SetFloat(animName[0], walkFlaot);
        if (Player_Manager.GetInstance().canMove == true && playerMovement.inputVector != Vector2.zero && playerSlash.attackInput == 0)
        {
            walkFlaot += Time.deltaTime * walkFloatMultiplier;
               
            if(walkFlaot >= 1)
            {
                walkFlaot = 1;
            }
        }
        else
        {
            
            walkFlaot -= Time.deltaTime * walkFloatMultiplier;

            if(walkFlaot <= 0)
            {
                walkFlaot = 0;
            }
        }

        anim.SetInteger(animName[1], playerSlash.attackInput);
        
        if(playerSlash.attackInput != 0)
        {
            anim.SetBool(animName[2], true);
        } else
        {
            anim.SetBool(animName[2], false);
        }

        anim.SetBool(animName[3], playerSlash.parry);

        anim.SetBool(animName[4], Player_Health_Manager.GetInstance().isHurt);

        anim.SetBool(animName[5], parry.success);

        anim.SetBool(animName[6], roll.isDashing);
    }

}
