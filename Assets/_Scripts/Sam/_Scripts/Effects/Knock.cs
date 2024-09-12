using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knock : MonoBehaviour
{
    public bool knockBack;  
    public Transform center;
  public Rigidbody rB;
    public float knockBackVel;
    public float knockBackTime;
    public bool isPlayer;
    public Vector3 knockBackDir;
    public CameraShake CameraShake;




    private void Update()
    {
        Player_Manager.GetInstance().isHurt = knockBack;
    }

    public void KnockBack(Transform t)
    {


        var dir = (center.forward - t.forward) * -1;

        if (isPlayer && Player_Manager.GetInstance().canBeHurt == true)
        {

            rB.velocity = new Vector3(dir.x, 0, dir.z).normalized * knockBackVel;


            knockBack = true;
            knockBackDir = dir;
        } 

        if(isPlayer == false)
        {
            rB.velocity = new Vector3(dir.x, 0, dir.z).normalized * knockBackVel;


            knockBack = true;
            knockBackDir = dir;
        }
       
   //     print(dir);
        StartCoroutine(Unkockback());
        //CameraShake.Shake();
    }
    public void KnockBackBig(Transform t)
    {


        var dir = (center.forward - t.forward) * -10;

        if (isPlayer && Player_Manager.GetInstance().canBeHurt == true)
        {

            rB.velocity = new Vector3(dir.x, 0, dir.z).normalized * -30;


            knockBack = true;
            knockBackDir = dir;
        }

        if (isPlayer == false)
        {
            rB.velocity = new Vector3(dir.x, 0, dir.z).normalized * -30;


            knockBack = true;
            knockBackDir = dir;
        }

        //     print(dir);
        StartCoroutine(Unkockback());
        CameraShake.Shake();
    }
    public void KnockBackY(Transform t)
    {
        var dir = center.forward - t.forward;
       // CameraShake.Instance.ShakeCamera(5f, .1f);
           
            rB.velocity = new Vector3(0, dir.y * knockBackVel, 0);
       
           
        
      
        knockBack = true;
        knockBackDir = dir;


     //   print(dir);
        StartCoroutine(Unkockback());
    }

    

    private IEnumerator Unkockback()
    {
        yield return new WaitForSeconds(knockBackTime);
        knockBack = false;
    }
}
