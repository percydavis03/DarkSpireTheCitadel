using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : MonoBehaviour
{
    public PlayerSaveState thisGameSave;
    public float enemyHP = 30;
    public int damageTaken;
    public List<GameObject> bloodSplats = new List<GameObject>();
    public int randomListObject;
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
            randomListObject = Random.Range(0, bloodSplats.Count);
            
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
            GameObject b = Instantiate(bloodSplats[randomListObject]);
            b.transform.position = new Vector3(transform.position.x, transform.position.y - 1.2f, transform.position.z);
            b.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
            Destroy(gameObject);
        } 
            
    }
}
