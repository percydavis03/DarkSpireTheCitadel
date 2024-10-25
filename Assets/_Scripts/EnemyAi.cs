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

    //Patrolling 
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;


    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject weaponHitbox;

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
    }
    private void Update()
    {
        //Check for sight and attack
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange && !anim.GetBool("IsHurting")) Patroling();
        if(playerInSightRange && !playerInAttackRange && !anim.GetBool("IsHurting")) ChasePlayer();
        if(playerInAttackRange && playerInSightRange && !anim.GetBool("IsHurting")) AttackPlayer();

        if (playerInAttackRange)
        {
            agent.SetDestination(transform.position);
        }

        if(enemyHP == 0)
        {
            
            GameObject s = Instantiate(soulPrefab);
            s.transform.position = transform.position;
            Destroy(gameObject);
        }
        if (agent.velocity.magnitude > 0.1f)
        {
            anim.SetBool("IsWalk", true);
        }
        if (agent.velocity.magnitude < 0.1f)
        {
            anim.SetBool("IsWalk", false);
        }
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
        //put attack here:
        //print("Attacking");
        anim.SetBool("IsAttacking", true);

        //make sure enemy doesnt move
        agent.SetDestination(transform.position);
        //weaponHitbox.SetActive(true);
        //print("attack");

        transform.LookAt(player);

        if(!alreadyAttacked)
        {
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        weaponHitbox.SetActive(false);
        //anim.SetBool("IsAttacking", false);
    }

}
