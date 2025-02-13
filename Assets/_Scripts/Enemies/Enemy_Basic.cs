using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
 

public class Enemy_Basic : MonoBehaviour
{
    
    public PlayerSaveState thisGameSave;

    public bool isLock;
    public GameObject exit;
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
    public Image healthFill;
    //combat
    public GameObject spear_hitbox;
    //AI
    public NavMeshAgent agent;
    public LayerMask whatIsGround, whatIsPlayer;
    //Patrolling 
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;
    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    

   

    private void Start()
    {
        anim = animationSource.GetComponent<Animator>();
        isHit = false;
        dead = false;
        alreadyAttacked = false;
    }
    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        damageTaken = thisGameSave.mainAttackDamage; 
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        agent.angularSpeed = 0f;
        agent.updateRotation = false;
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
       // pain.SetActive(false);
    }
    public void GetUp()
    {
        hitCount = 0;
        isHit = false;
        anim.SetBool("IsHurting", false);
        anim.SetInteger("HurtAnim", 0);
        print("getup");
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
        
        spear_hitbox.SetActive (false);
        Bleed();
        this.GetComponent<BoxCollider>().enabled = false; 
        this.GetComponent<CapsuleCollider>().enabled = false;
        GameObject s = Instantiate(enemyDrop);
        s.transform.position = transform.position;
        if (isLock)
        {
            exit.SetActive(true);
        }
        //Destroy(thisGuy);
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

    // Update is called once per frame
    void Update()
    {
        //healthFill.fillAmount = enemyHP / maxEnemyHP;
        if (enemyHP <= 0)
        {
           if (!dead)
            {
                agent.SetDestination(thisGuy.transform.position);
                spear_hitbox.SetActive(false);
                 /// fix this
                anim.SetBool("IsDead", true);
                
            }
        }

        //Check for sight and attack
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange && !anim.GetBool("IsHurting")) Patroling();
        if (playerInSightRange && !thisGameSave.inMenu && !playerInAttackRange && !dead && !anim.GetBool("IsHurting")) ChasePlayer();
        if (playerInAttackRange && !thisGameSave.inMenu && playerInSightRange && !isHit) AttackPlayer();

        
        if (agent.velocity.magnitude > 0.1f && !anim.GetBool("IsHurting") && !anim.GetBool("IsAttacking"))
        {
            anim.SetBool("IsRunning", true);
        }
        if (agent.velocity.magnitude < 0.1f)
        {
            anim.SetBool("IsRunning", false);
        }

    }
    //----------AI-----------
    IEnumerator NavWait(float s)
    {
        yield return new WaitForSeconds(s);
        ResetAttack();
    }
    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }

    }

    private void SearchWalkPoint()
    {
        //calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        transform.LookAt(player);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(thisGuy.transform.position);
        anim.SetBool("IsAttacking", true);
        GetComponent<NavMeshAgent>().speed = 0;
        //transform.LookAt(player);

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;
            StartCoroutine(NavWait(1.5f));
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        //weaponHitbox.SetActive(false);
        GetComponent<NavMeshAgent>().speed = setSpeed;
        anim.SetBool("IsAttacking", false);
    }



    //---------ANIMATIONS---------
    public void AnimStab()
    {
       WeaponOn();
    }

    public void AnimWeaponOff()
    {
        WeaponOff();
    }
    public void AnimAttackEnd()
    {
        StopAttacking();
    }
    public void AnimFallEnd()
    {
        GetUp();
    }
    public void AnimHurtEnd()
    {
        StopHurt();
    }

    public void AnimDied()
    {
        Death();
    }
}
