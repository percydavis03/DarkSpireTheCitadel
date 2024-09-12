using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpearV2 : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;

    public Transform target;

    public float lookRadius;

    public float distance;

    public float stoppingDistance;

    public Knock knock;

    public Rigidbody rB;

    public Animator anim;

    public string[] animNmaes;

    public bool attack;

    public bool canAttack;

    public float recoveryTime;

    public bool recover;

    public HurtManager hurtManager;

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

        if(distance <= lookRadius)
        {
          agent.SetDestination(target.position);
          FaceTarget(); 
        }

        if(distance <= stoppingDistance + 1 && !canAttack)
        {
            StartCoroutine(Attack());
            anim.SetBool(animNmaes[1], false);
        }

        Damage();

        anim.SetBool(animNmaes[0], canAttack);

        if(agent.remainingDistance > agent.stoppingDistance)
        {
            anim.SetBool(animNmaes[1], true);
        } else 
        {
            anim.SetBool(animNmaes[1], false);
        }
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
    }

    public void Damage()
    {
        if (knock.knockBack)
        {
            agent.enabled = false;
            rB.isKinematic = false;
            recover = true;
        }
        else if (knock.knockBack == false && recover == true)
        {
            StartCoroutine(Recover());
        }
    }

    private IEnumerator Recover()
    {
        recover = false;
        agent.enabled = false;
        rB.isKinematic = false;
        yield return new WaitForSeconds(recoveryTime);       
        yield return new WaitForSeconds(.15f);
        agent.enabled = true;
        rB.isKinematic = true;
        hurtManager.hurtNum = 0;
        anim.SetInteger(animNmaes[1], hurtManager.hurtNum);       
    }

    void baba()
    {
        canAttack = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }

    private IEnumerator Attack()
    {

        canAttack = true;
        agent.enabled = false;
        yield return new WaitForSeconds(.25f);
        canAttack = false;
        agent.enabled = true;
    }
}
