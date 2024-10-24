using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Anims : MonoBehaviour
{
    public void AttackEnd()
    {
        Enemy_Basic.instance.StopAttacking();
    }
    public void FallEnd()
    {
        Enemy_Basic.instance.GetUp();
    }
    public void HurtEnd()
    {
        Enemy_Basic.instance.StopHurt();
    }
    
}
