using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Basic : MonoBehaviour
{
    public static Enemy_Basic instance;
    public PlayerSaveState thisGameSave;
    public float enemyHP = 30;
    public int damageTaken;
    public List<GameObject> bloodSplats = new List<GameObject>();
    public int randomListObject;
    public GameObject enemyDrop;
    private Rigidbody rb;

    //Animation
    public Animator anim;
    public GameObject animationSource;
    //damage
    private int hitCount;
    bool isHit;
    bool dead;
    public GameObject thisGuy;
    //combat
    public GameObject spear_hitbox;
    private void Start()
    {
        anim = animationSource.GetComponent<Animator>();
        hitCount = 0;
        dead = false;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        damageTaken = thisGameSave.mainAttackDamage; 
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage()
    {
        isHit = true;
       if (enemyHP != 0)
        {
            enemyHP = enemyHP - damageTaken;
        }
      
      
        if (hitCount == 1)
        {
            anim.SetInteger("HurtAnim", 1);
        }
        if (hitCount == 2)
        {
            anim.SetInteger("HurtAnim", 2);
        }
        if (hitCount == 3) //fall down
        {
            anim.SetInteger("HurtAnim", 3);
            
        }
    }
    public void StopHurt()
    {
        isHit = false;
        anim.SetInteger("HurtAnim", 0);
        print("hit");
    }
    public void GetUp()
    {
        hitCount = 0;
        isHit = false;
        anim.SetInteger("HurtAnim", 0);
        print("bighit");
    }
    public void WeaponOn()
    {
        spear_hitbox.SetActive(true);
    }
    public void StopAttacking()
    {
        anim.SetBool("IsAttacking", false);
        spear_hitbox.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        print("collide");
        if (other.gameObject.CompareTag("Weapon"))
        {
            hitCount++;
            TakeDamage();
            if (isHit == false)
            {
                
            }
           
            //randomListObject = Random.Range(0, bloodSplats.Count);
            
            print("ow");
        }
    }

    void Death()
    {
        dead = true;
        GameObject s = Instantiate(enemyDrop);
        s.transform.position = transform.position;
        GameObject b = Instantiate(bloodSplats[randomListObject]);
        b.transform.position = new Vector3(transform.position.x, transform.position.y - 1.2f, transform.position.z);
        b.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        Destroy(thisGuy);
    }
    // Update is called once per frame
    void Update()
    {
        if (enemyHP <= 0)
        {
           if (!dead)
            {
                Death();
            }
        } 
            
    }
}
