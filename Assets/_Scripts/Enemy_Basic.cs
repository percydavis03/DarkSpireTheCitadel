using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : MonoBehaviour
{
    public PlayerSaveState thisGameSave;
    public float enemyHP = 30;
    public int damageTaken;
    public GameObject enemyDrop;
    void Start()
    {
        
    }
    private void Awake()
    {
        damageTaken = thisGameSave.mainAttackDamage; 
        print(damageTaken);
    }
    private void OnTriggerEnter(Collider other)
    {
        print("collide");
        if (other.gameObject.CompareTag("Weapon"))
        {
            enemyHP = enemyHP - damageTaken;
            
            print("ow");
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (enemyHP <= 0)
        {
            GameObject s = Instantiate(enemyDrop);
            s.transform.position = transform.position;
            Destroy(gameObject);
        } 
            
    }
}
