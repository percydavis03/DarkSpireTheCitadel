using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Stinger : MonoBehaviour
{
    [SerializeField] private TargetCheck target;
    [SerializeField] float StingerTime;
    [SerializeField] bool isStinging;
    [SerializeField] bool hasStinged;
    [SerializeField] Transform orientation;
    [SerializeField] float stignerSpeed;
    [SerializeField] Rigidbody rB;
    public Animator anim;
    public GameObject player;
    public bool sucess;
    public float stingerRadius;
    public LayerMask enemyLayer;
    // Start is called before the first frame update
    void Start()
    {
        target = FindObjectOfType<TargetCheck>();
        orientation = GameObject.FindWithTag("Orientation").transform;
        player = FindAnyObjectByType<Slash2>().playerObj.gameObject;
        anim = player.GetComponentInChildren<Animator>();

        Player_Manager.GetInstance().specialWeaponValue = Player_Manager.GetInstance().maxSpecialWeaponValue;
    }

    // Update is called once per frame
    void Update()
    {

        sucess = Physics.CheckSphere(orientation.position, stingerRadius, enemyLayer) && isStinging;

        if (Input.GetButtonDown("Fire4"))
        {
            isStinging = true;
            Player_Manager.GetInstance().specialWeaponValue -= 25f;
        }

        if (isStinging)
        {

            if (target.isTargeting)
            {
                player.transform.DOLookAt(new Vector3(target.targetTransform.transform.position.x, player.transform.position.y, target.targetTransform.transform.position.z), 0.15f);
            }
            Player_Manager.GetInstance().isStinger = true;
            Player_Manager.GetInstance().canMove = false;
            StartCoroutine(Sting());
            
        } else
        {
            Player_Manager.GetInstance().isStinger = false;
        }

        anim.SetBool("Stinger", isStinging);

        if (sucess)
        {
            isStinging = false;
            hasStinged = true;
        }

        if (hasStinged)
        {
            Player_Manager.GetInstance().canMove = false;
            StartCoroutine(hasSting());
        }
    }

    private IEnumerator Sting()
    {

        isStinging = true;      
        if(target.isTargeting)     
        yield return new WaitForSeconds(StingerTime);
        isStinging = false;
        hasStinged = true;
    }

    private IEnumerator hasSting()
    {
        yield return new WaitForSeconds(1f);
        hasStinged = false;
        Player_Manager.GetInstance().canMove = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(orientation.transform.position, stingerRadius);
    }

}
