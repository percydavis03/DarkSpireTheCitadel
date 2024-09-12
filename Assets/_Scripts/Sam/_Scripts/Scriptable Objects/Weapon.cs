using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data",menuName = "Data/Weapon", order = 1)]
public class Weapon : ScriptableObject
{
    public int damage;
    public string weaponName;
    public GameObject weaponPrefab;
    
}
