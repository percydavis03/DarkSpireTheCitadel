using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Worker : MonoBehaviour
{
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
    public GameObject shovelHitbox;
    //AI
    public NavMeshAgent agent;
    public LayerMask whatIsGround, whatIsPlayer;
    public bool isHit;
    public bool dead;
    public Transform player;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject thisGuy;

    private void Start()
    {
        anim = GetComponent<Animator>();
        isHit = false;
        dead = false;
        alreadyAttacked = false;
    }
    private void Awake()
    {
        
        damageTaken = thisGameSave.mainAttackDamage;
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

    }
    void Update()
    {
        
        if (enemyHP <= 0)
        {
            if (!dead)
            {
                agent.SetDestination(thisGuy.transform.position);
                shovelHitbox.SetActive(false);
                dead = true;
                /// fix this
                anim.SetBool("IsDead", true);
                Death();

            }
        }

        //Check for sight and attack
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

       
        if (playerInSightRange && !thisGameSave.inMenu && !playerInAttackRange && !dead && !anim.GetBool("IsHurting")) ChasePlayer();
        if (playerInAttackRange && !thisGameSave.inMenu && playerInSightRange && !isHit) AttackPlayer();
        if(!playerInAttackRange)
        {
            anim.SetBool("isAttacking", false);
        }

        if (agent.velocity.magnitude > 0.1f && !anim.GetBool("IsHurting") && !anim.GetBool("IsAttacking"))
        {
            anim.SetBool("IsRunning", true);
        }
        if (agent.velocity.magnitude < 0.1f)
        {
            anim.SetBool("IsRunning", false);
        }

    }
    private void ChasePlayer()
    {
        if (!anim.GetBool("isRunning"))
        {
            anim.SetBool("isRunning", true);
        }
        
        agent.SetDestination(player.position);
        transform.LookAt(player);
    }

    private void AttackPlayer()
    {
        anim.SetBool("isRunning", false);
        anim.SetBool("isAttacking", true);
        agent.SetDestination(thisGuy.transform.position);

      

        GetComponent<NavMeshAgent>().speed = 0;
        //transform.LookAt(player);

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;
            StartCoroutine(NavWait(1f));
        }
    }
    IEnumerator NavWait(float s)
    {
        yield return new WaitForSeconds(s);
        ResetAttack();
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
        //weaponHitbox.SetActive(false);
        GetComponent<NavMeshAgent>().speed = setSpeed;
        anim.SetBool("IsAttacking", false);
    }
    IEnumerator Wait(float s)
    {
        yield return new WaitForSeconds(s);
        StopHurt();
    }
    public void StopHurt()
    {
        anim.SetBool("IsHurting", false);
        isHit = false;
        print("stophurt");
    }
    private void OnTriggerEnter(Collider other)
    {
        print("collide");
        if (other.gameObject.CompareTag("Weapon"))
        {
            if (isHit == false)
            {
                TakeDamage();
               
            }
        }
    }
    public void TakeDamage()
    {
        isHit = true;
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsAttacking", false);
       
        if (enemyHP != 0)
        {
            anim.SetBool("IsHurting", true);
            enemyHP = enemyHP - damageTaken;
            print("literally take damage ");
            StartCoroutine(Wait(0.5f));
        }
    }

    public void WeaponOn()
    {
        shovelHitbox.SetActive(true);
        print("weapon on");
    }

    public void WeaponOff()
    {
        shovelHitbox.SetActive(false);
        print("weapon off");
    }
    public void StopAttacking()
    {
       
        anim.SetBool("IsAttacking", false);
        transform.LookAt(player);
        GetComponent<NavMeshAgent>().speed = setSpeed;
        print("stop attacking");
    }

    public void Death()
    {
        dead = true;
        anim.SetBool("isDead", true);
        setSpeed = 0;
        shovelHitbox.SetActive(false);
        Bleed();
        this.GetComponent<BoxCollider>().enabled = false;
        this.GetComponent<CapsuleCollider>().enabled = false;
        GameObject s = Instantiate(enemyDrop);
        s.transform.position = transform.position;
       
    }
    void Bleed()
    {
        if (!dead)
        {
            GameObject b = Instantiate(bloodSplats[randomListObject]);
            b.transform.position = transform.position;
            b.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
            b.transform.localScale = new Vector3(1, 1, 1);
        }
    }

}
