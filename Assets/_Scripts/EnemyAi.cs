using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public NavMeshAgent agent;
   
    public Transform player;
    public GameObject thisGuy;

    public LayerMask whatIsGround, whatIsPlayer;

    //Enemy
    public int enemyHP;
    public int setSpeed;

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

    //Animation
    public Animator anim;
    public GameObject animationSource;

    //SoulPoints
    public GameObject soulPrefab;

    private void Awake()
    {
        enemyHP = 2;

        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();

        agent.angularSpeed = 0f;
        agent.updateRotation = false;

        //anim = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Animator>();
    }
    private void Start()
    {
        anim = animationSource.GetComponent<Animator>();
        alreadyAttacked = false;
    }
    private void Update()
    {
        //Check for sight and attack
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange && !anim.GetBool("IsHurting")) Patroling();
        if(playerInSightRange && !playerInAttackRange && !Enemy_Basic.instance.dead && !anim.GetBool("IsHurting")) ChasePlayer();
        if(playerInAttackRange && playerInSightRange && !Enemy_Basic.instance.isHit) AttackPlayer();

        /*if (playerInAttackRange)
        {
            agent.SetDestination(transform.position);
            if (alreadyAttacked)
            {
                GetComponent<NavMeshAgent>().speed = setSpeed;
                ResetAttack();
            }
        }*/

        if(enemyHP == 0)
        {
            GameObject s = Instantiate(soulPrefab);
            s.transform.position = transform.position;
            Destroy(gameObject);
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
    IEnumerator Wait(float s)
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
        if(distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
       
    }

    private void SearchWalkPoint()
    {
        //calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3 (transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

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
        agent.SetDestination(transform.position);
        anim.SetBool("IsAttacking", true);

        //transform.LookAt(player);

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;
            StartCoroutine(Wait(1.5f));
        }
        /*anim.SetBool("IsAttacking", true);
        yield return new WaitForSeconds(.25f);
        if (!alreadyAttacked)
        {
            alreadyAttacked = true;
            print("wjat???");
            anim.SetBool("IsAttacking", true);
            StartCoroutine(Wait(2));
            alreadyAttacked = true;
            weaponHitbox.SetActive(true);
            
            GetComponent<NavMeshAgent>().speed = 0;
            //make sure enemy doesnt move
            agent.SetDestination(transform.position);

            transform.LookAt(player);

            
        }*/






    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        //weaponHitbox.SetActive(false);
        GetComponent<NavMeshAgent>().speed = setSpeed;
        anim.SetBool("IsAttacking", false);
    }

}
