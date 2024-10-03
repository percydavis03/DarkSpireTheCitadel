using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations_Nyx : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
  
    public void AttackAnimEnd()
    {
        Player_Movement.instance.EndAttack();
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
