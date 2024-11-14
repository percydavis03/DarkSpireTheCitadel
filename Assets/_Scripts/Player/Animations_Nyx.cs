using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations_Nyx : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    

    public void Recover()
    {

    }
    public void AttackAnimEnd()
    {
        Player_Movement.instance.EndAttack();
    }

    public void SpinAttackEnd()
    {
        
        Player_Movement.instance.EndAttack();
        Player_Movement.instance.isSpinAttack = false;
        
    }
    public void StoppedMoving()
    {
        Player_Movement.instance.StopMoving();
    }
    public void StartSpinAttack()
    {
        //Player_Movement.instance.isSpinAttack = true;
        Player_Movement.instance.Reposition();
    }

    public void ActivateSword()
    {
        Player_Movement.instance.SwordOn();
    }

    public void DeactivateSword()
    {
        Player_Movement.instance.SwordOff();    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
