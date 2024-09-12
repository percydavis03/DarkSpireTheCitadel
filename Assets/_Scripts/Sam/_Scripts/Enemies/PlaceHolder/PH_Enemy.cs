using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class PH_Enemy : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform target;

    public float lookRaius;

    public float distance;

    public float stoppingDistance;

    public Knock knock;

    public Rigidbody rb;

    public Animator anim;

    public string[] animNames;

    public bool attack;

    public bool canAttack;

    public float recovryTime;

    public bool recover;
    // Start is called before the first frame update
    void Start()
    {
        agent.stoppingDistance = stoppingDistance;

        target = GameManager.GetInstance().Player.gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.position);


        distance = Vector3.Distance(target.position, transform.position);
        
        if(distance <= lookRaius)
        {
            agent.SetDestination(target.position);
            FaceTarget();
        }

        if (knock.knockBack)
        {
            agent.enabled = false;
            rb.isKinematic = false;
            anim.SetBool(animNames[1], true);
            recover = true;
        } else if (knock.knockBack == false && recover == true)
        {                  
            StartCoroutine(Recover());        
        }

        //animations

        if(agent.remainingDistance > agent.stoppingDistance && knock.knockBack == false)
        {
            anim.SetBool(animNames[0], true);
        } else
        {
            anim.SetBool(animNames[0], false);
        }

      if(distance <= stoppingDistance + 1 && !canAttack)
        {        
            canAttack = true;
            StartCoroutine(Attack()); 
        }

        anim.SetBool(animNames[2], attack);
    }

    private IEnumerator Attack()
    {
      
        yield return new WaitForSeconds(1);
       
        attack = true;
        yield return new WaitForSeconds(.5f);
        attack = false;
        yield return new WaitForSeconds(2f);
        canAttack = false;
    }

    private IEnumerator Recover()
    {
        recover = false;
        agent.enabled = false;
        rb.isKinematic = false;
        yield return new WaitForSeconds(recovryTime);
        
        anim.SetBool(animNames[1], false);
        yield return new WaitForSeconds(.15f);
        agent.enabled = true;
        rb.isKinematic = true;

    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRaius);
    }
}
