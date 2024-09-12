using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Animation_Events : MonoBehaviour
{
    public string lastCalledState; //For Debugging Purposes


    public IEnumerator push()
    {
        Player_Manager.GetInstance().canMove = false;
        lastCalledState = "Push";
        print(lastCalledState);
       Player_Manager.GetInstance().isDashing = true; 
        yield return new WaitForSeconds(0.15f);
        Player_Manager.GetInstance().isDashing = false;
        Player_Manager.GetInstance().rb.velocity = Vector3.zero;
    }

    public void setInputTo0()
    {
        lastCalledState = "setInputTo0";
        print(lastCalledState);
        Player_Manager.GetInstance().inputVector = Vector2.zero;
    }

    public IEnumerator canMove()
    {
        lastCalledState = "canMove";
        print(lastCalledState);
        yield return new WaitForSeconds(0.75f);
        Player_Manager.GetInstance().canMove = true; 
    }

    public void dontMove()
    {
        lastCalledState = "dontMove";
        print(lastCalledState);
       
        Player_Manager.GetInstance().canMove = false;
        Player_Manager.GetInstance().inputVector = Vector2.zero;
    }

    public void dontAttack()
    {
        lastCalledState = "dontAttack";
        print(lastCalledState);
        Player_Manager.GetInstance().canAttack = false;
    }

    public void canAttack()
    {
        lastCalledState = "canAttack";
        print(lastCalledState);
        Player_Manager.GetInstance().canAttack= true;
    }

    public void setParryToFalse()
    {
        Player_Manager.GetInstance().slash.parry = false;
    }

    public void setSuccessToFalse()
    {
        Player_Manager.GetInstance().ParryBoxCollider.success = false;
        Player_Manager.GetInstance().isCounter = false;
        Player_Manager.GetInstance().canBeHurt = true;
    }

    public void canBeHurt()
    {
        Player_Manager.GetInstance().canBeHurt = true;
    }
}
