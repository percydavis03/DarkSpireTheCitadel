using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
 

public class Enemy_Basic : MonoBehaviour
{
    public static Enemy_Basic instance;
    public PlayerSaveState thisGameSave;
    //stats
    public float enemyHP = 30;
    public float maxEnemyHP = 30;
    public int damageTaken;
    public int setSpeed;

    public List<GameObject> bloodSplats = new List<GameObject>();
    public int randomListObject;
    public GameObject enemyDrop;
    private Rigidbody rb;

    //Animation
    public Animator anim;
    public GameObject animationSource;
    //damage
    private int hitCount;
    public bool isHit;
    public bool dead;
    public GameObject thisGuy;
    public Transform player;
    public GameObject pain;
    public Image healthFill;
    //combat
    public GameObject spear_hitbox;
   
    private void Start()
    {
        anim = animationSource.GetComponent<Animator>();
        isHit = false;
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

    private void KnockbackEntity(Transform player)
    {
        Vector3 dir = (transform.position - player.transform.position).normalized;
        transform.Translate(dir * -10 * Time.deltaTime);
        //rb.AddForce(dir, ForceMode.Impulse);
    }

    IEnumerator Wait(float s)
    {
        yield return new WaitForSeconds(s);
        StopHurt();
        
    }

    public void TakeDamage()
    {
        isHit = true;
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsAttacking", false);
        KnockbackEntity(player);
        pain.SetActive(true);
        if (enemyHP != 0)
        {
            anim.SetBool("IsHurting", true);
            enemyHP = enemyHP - damageTaken;
            print("literally take damage ");
            StartCoroutine(Wait(0.5f));
        }
    }
    public void StopHurt()
    {
        anim.SetBool("IsHurting", false);
        isHit = false;
        print("stophurt");
        pain.SetActive(false);
    }
    public void GetUp()
    {
        hitCount = 0;
        isHit = false;
        anim.SetBool("IsHurting", false);
        anim.SetInteger("HurtAnim", 0);
        print("getup");
        pain.SetActive(false);
    }
    public void WeaponOn()
    {
        spear_hitbox.SetActive(true);
        print("weapon on");
    }

    public void WeaponOff()
    {
        spear_hitbox.SetActive(false);
        print("weapon off");
    }
    public void StopAttacking()
    {
        spear_hitbox.SetActive(false);
        anim.SetBool("IsAttacking", false);
        GetComponent<NavMeshAgent>().speed = setSpeed;
        print("stop attacking");
    }
    private void OnTriggerEnter(Collider other)
    {
        print("collide");
        if (other.gameObject.CompareTag("Weapon"))
        {
            if (isHit == false)
            {
                TakeDamage();
                hitCount++;
            }
           
            //randomListObject = Random.Range(0, bloodSplats.Count);
        }
    }

    public void Death()
    {
        dead = true;
        GameObject s = Instantiate(enemyDrop);
        s.transform.position = transform.position;
        GameObject b = Instantiate(bloodSplats[randomListObject]);
        b.transform.position = new Vector3(transform.position.x, transform.position.y - 1.2f, transform.position.z);
        b.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        //Destroy(thisGuy);
    }
    // Update is called once per frame
    void Update()
    {
        //healthFill.fillAmount = enemyHP / maxEnemyHP;
        if (enemyHP <= 0)
        {
           if (!dead)
            {
                spear_hitbox.SetActive(false);
                anim.SetBool("IsDead", true);
            }
        } 
            
    }
}
