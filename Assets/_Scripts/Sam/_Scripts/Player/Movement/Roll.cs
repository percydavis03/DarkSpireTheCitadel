using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : MonoBehaviour
{
    public Rigidbody rB;
    public Transform orientation;
    public Vector3 rollVector;
    public bool isDashing;
    public bool canDash;

    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;

    public float dashCD;
    private float dashCDTimer;

    // Start is called before the first frame update
    void Start()
    {
       
        canDash = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire3") && canDash && Player_Manager.GetInstance().isAttacking == false)
        {
            isDashing = true;
            canDash = false;
            rollVector = new Vector3(Player_Manager.GetInstance().inputVector.x, 0, Player_Manager.GetInstance().inputVector.y);
            Player_Manager.GetInstance().canMove = false;
        }

        if (isDashing)
        {
            rB.velocity = orientation.forward * dashForce;
            StartCoroutine(StopDash());
            Player_Manager.GetInstance().canMove = false;
            Player_Manager.GetInstance().canBeHurt = false;
           
        } else
        {
            Player_Manager.GetInstance().canMove = true;
           // Player_Manager.GetInstance().canBeHurt = true;
        }

        if(Player_Manager.GetInstance().isHurt == true)
        {
            isDashing = false;
            StartCoroutine(StopDash());
        }
    }

    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;        
        yield return new WaitForSeconds(dashCD);
        canDash = true;
        Player_Manager.GetInstance().canBeHurt = true;
    }
}
