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
    // Update is called once per frame
    void Update()
    {
        
    }
}
