using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpear : MonoBehaviour
{
    [Header("Bools")]
    [Space]
    public bool isAttacking;
    public bool isHurt;
    public bool isRunning;
    public bool canMove;
    public bool canBeHit;
    public bool canAttack;
    public bool recover;
    [Space]
    [Header("Anim Names")]
    [Space]
    public string[] animNames;
    [Space]
    [Header("Refrences")]
    [Space]
    public NavMeshAgent agent;
    public Rigidbody rB;
    public Knock knockBack;
    public Animator anim;
    [Space]
    [Header("NavMeshModifications")]
    [Space]
    public float stoppingDistance;
    public float moveSpeed;
    public float disance;
    public float lookRadius;
    public Transform target;
    public Transform playerTarget;
    public Transform nullTarget;
    [Space]
    [Header("Hurt Stats")]
    [Space]
    public int hurtNum;
    public int maxHurt;
    public float recovryTime;
    [Space]
    [Header("Attack Stats")]
    [Space]
    public float attackDelay;
    
    // Start is called before the first frame update
    void Start()
    {
        agent.stoppingDistance = stoppingDistance;
        agent.speed = moveSpeed;
        target = playerTarget;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove == true)
        {
            agent.isStopped = false;
            target = playerTarget;         
        }
        else
        {
            agent.isStopped = true;
            anim.SetBool(animNames[1], false);
        }

        disance = Vector3.Distance(target.position, transform.position);

        anim.SetInteger(animNames[4], hurtNum);

       if(isHurt == true)
        {
            canMove = false;
        }

        //Move State
        if (disance <= lookRadius)
        {
            Move();
        }

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            anim.SetBool(animNames[1], true);
        }
        else
        {
            anim.SetBool(animNames[1], false);
        }

        //Attack State
        anim.SetBool(animNames[2], isAttacking);

        if (disance <= stoppingDistance + 1 && canAttack)
        {
            StartCoroutine(Attack());
           canAttack = false;
        }

        //Hurt State

        anim.SetBool(animNames[3], isHurt);

        if (knockBack.knockBack)
        {
            anim.SetBool(animNames[1], false);
            isHurt = true;
            recover = true;
            canMove = false;
          
        } 
        
        if(recover && knockBack.knockBack == false)
        {
            StartCoroutine(Recover());
        }
    }

   public void Move()
    {
        agent.SetDestination(target.position);
        FaceTarget();
       
       
    }

    public void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
    }

    private IEnumerator Attack()
    {

     
        yield return new WaitForSeconds(attackDelay);
        isAttacking = true;
        canMove = false;
        yield return new WaitForSeconds(attackDelay * 0.5f);
        isAttacking = false;
        canMove = true;
        yield return new WaitForSeconds(1f);
        canAttack = true;
        
    }

    private IEnumerator Recover()
    {
        recover = false;
        rB.isKinematic = false;
        canMove = false;
        agent.enabled = false;
        yield return new WaitForSeconds(recovryTime);
        isHurt = false;
        hurtNum = 0;
        yield return new WaitForSeconds(.15f);
        rB.isKinematic = true;
        agent.enabled = true;
        yield return new WaitForSeconds(.15f);
        canMove = true;
       
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
