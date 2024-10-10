using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PlayerSaveState")]

public class PlayerSaveState : ScriptableObject
{
    public int hitpoints;
    public int playerSpeed;
    public int sprintSpeed;
    public int maxHP;
    public bool canJump;
    public bool inMenu;
    public bool canAttack;
    public int mainAttackDamage;

    public void Init()
    {
        hitpoints = 5;
        maxHP = 60;
        canJump = false;    
    }
}
