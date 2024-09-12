using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.DemiLib;
using DG.Tweening;

public class Slash2 : MonoBehaviour
{
    public int attackInput;
    public float attackTimer;
    public float attackTimerAmount;
    public bool canHit;
    public bool comboComplete;
    public bool isAttacking;
    public TargetCheck targetCheck;
    public Transform playerObj;
    public bool parry;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Three Hit Combo

        if(!comboComplete)
        Countdown();


        if (Input.GetButtonDown("Fire1"))
        {

            if (targetCheck.isTargeting)
            {
                playerObj.transform.DODynamicLookAt(new Vector3(targetCheck.targetTransform.transform.position.x, transform.position.y, targetCheck.targetTransform.transform.position.z), 0.15f);
              
            }

            if (canHit)
            {

                AddInt();
                if (attackInput != 0)
                {
                    isAttacking = true;
                    canHit = false;
                    playerObj.transform.Rotate(Vector3.zero);
                    attackTimer = attackTimerAmount;


                }
            }
        }

        if(canHit == false && comboComplete == false)
        {
            StartCoroutine(InputDelay());
        }

        if(attackInput >= 3 && comboComplete == false)
        {
            comboComplete = true;
            canHit = false;
            StartCoroutine(ComboCoolDown());
        }

        //Parry
       

        if (Input.GetButtonDown("Fire2"))
        {
            parry = true;
            if (targetCheck.isTargeting)
                playerObj.transform.DODynamicLookAt(new Vector3(targetCheck.targetTransform.transform.position.x, transform.position.y, targetCheck.targetTransform.transform.position.z), 0.15f);
        }

     if(parry == true)
        {
            Player_Manager.GetInstance().canMove = false;
            Player_Manager.GetInstance().canBeHurt = false;
        } else
        {
           // Player_Manager.GetInstance().canBeHurt = true;
        }
    }

    private void FixedUpdate()
    {
        if (isAttacking)
        {
            Player_Manager.GetInstance().canMove = false;
        } 
    }

    void Countdown()
    {
        if(attackTimer > 0 && attackInput != 0)
        {
            attackTimer -= Time.deltaTime;
            if(attackTimer <= 0 && !comboComplete)
            {
                attackTimer = 0;
                attackInput = 0;
                isAttacking = false;
                print("Ran Out Of Time");
            }
        }
    }

    void AddInt()
    {
        attackInput++;
        Player_Manager.GetInstance().rb.velocity = Vector3.zero;
        print("Hit " + attackInput);
    }

    private IEnumerator InputDelay()
    {
        yield return new WaitForSeconds(0.15f);
        canHit = true;
    }

    private IEnumerator ComboCoolDown()
    {
        print("Combo Completed");
        Player_Manager.GetInstance().canMove = false;
        attackTimer = 0;
        canHit = false;
        yield return new WaitForSeconds(1f);
        attackInput = 0;      
        comboComplete = false;
        isAttacking = false;   
        canHit = true;
        print("You can attack again");
    }

   
 
}
