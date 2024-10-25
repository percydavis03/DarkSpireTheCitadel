using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class HurtManager : MonoBehaviour
{
  //  public EnemySpear enemy;
  [Header("Hit Counts")]
  [Space]
    public bool canbeHit;
    public bool heavy;
    public int hurtNum;
    public int maxHurt;
    public Animator anim;
    public string[] annimNames;
    [Header("Health")]
    [Space]
    public float Health;
    public float maxHealth;
    public GameObject prefabDrop;
    [Header("Enemy Spawner")]
    [Space]
    public EnemySpear enemySpawn;

    // Start is called before the first frame update
    void Start()
    {
        Health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(hurtNum > maxHurt && heavy == false)
        {
            hurtNum = 0;
        }

        if (canbeHit == false)
        {
            Invoke("beHit",.25f);
        }

        //anim.SetInteger(annimNames[0], hurtNum);

        if(Health <= 0)
        {
            
           
                StartCoroutine(isDead());
            
            
           
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Light") && canbeHit)
        {
            hurtNum++;
            canbeHit = false;
            Health -= 15f;
        }

        if(other.gameObject.CompareTag("Heavy") && canbeHit)
        {
            heavy = true;
            hurtNum = 3;
            Health -= 30f;
        }
    }

    public void beHit()
    {
        canbeHit = true;
    }

   
    private IEnumerator isDead()
    {
            hurtNum = 3;
            yield return new WaitForSeconds(1f);
            GameObject.FindAnyObjectByType<TargetCheck>().targetIndex++;
            Instantiate(prefabDrop, transform.position, transform.rotation);
            Destroy(gameObject);
        
    }
}
