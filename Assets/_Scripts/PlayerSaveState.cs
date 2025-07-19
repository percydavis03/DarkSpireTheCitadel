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
    public int mainAttackDamage = 3; // Reduced from 5 to match combo1Damage
    
    [Header("Combo Attack Damage")]
    public int combo1Damage = 3;  // First attack damage (reduced from 5)
    public int combo2Damage = 4;  // Second attack damage (reduced from 7)
    public int combo3Damage = 5;  // Third attack damage (reduced from 8)
    public int finisherDamage = 6; // Finisher move damage (reduced from 10)
    
    public bool hasArm;
    
    public int GetComboDamage(int comboCount)
    {
        switch (comboCount)
        {
            case 1: return combo1Damage;
            case 2: return combo2Damage;
            case 3: return combo3Damage;
            default: return mainAttackDamage;
        }
    }
    public void Init()
    {
        hitpoints = 60;
        maxHP = 60;
        //canJump = false;    
    }
}
