using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    public static Boss instance;    
    public UnityEngine.AI.NavMeshAgent boss;
    public Transform player;
    public HurtManager hurtManager;
    //public AudioSource yippie;
    public Animator anim;

    [SerializeField] private float projectileSpeed;
    [SerializeField] public projectile projectilePrefab;
    [SerializeField] private float timer = 5;
    private float projectileTime;
    public Transform spawnPoint;
    public Transform spawnPoint2;
    public float enemySpeed;
    public GameObject blockExit;

    //EXPLODE
    public float radius = 5f;
    public float power = 10f;
    private Vector3 explosionPos;
    private Rigidbody rb;

    public GameObject ringOfFire;
    private bool inRing;

    //hp
    public float currentHealth;
    public float maxHP;
    public Image healthFill;

    //so sorry about this i have no brain cells left
    public GameObject fireSpawn1;
    public GameObject fireSpawn2;
    public GameObject fireSpawn3;
    public GameObject fireSpawn4;
    public GameObject fireSpawn5;
    public GameObject fireSpawn6;
    public GameObject fireSpawn7;
    public GameObject fireSpawn8;
    public bool isFire;
    void Start()
    {
        explosionPos = transform.position;
        
        inRing = false;
        isFire = false;
        currentHealth = 300;
        //currentHealth = hurtManager.Health;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        rb = GetComponent<Rigidbody>();
    }


    // Update is called once per frame
    void Update()
    {
        transform.LookAt(player);
        boss.SetDestination(player.position);
        ShootAtPlayer();
        healthFill.fillAmount = currentHealth / maxHP;

        //currentHealth = hurtManager.Health;
        if (currentHealth <= 0)
        {
            Dead();
        }
        if (currentHealth <= 150)
        {
            ShootAtPlayer2();
        }
        if(!isFire)
        {
            StartCoroutine(WaitUntil(20));
        }
    }
    IEnumerator WaitUntil(float seconds)
    {
        isFire = true;
        yield return new WaitForSeconds(seconds);
        anim.SetBool("isAttacking", false);
        if(anim.GetBool("wasHit") == false)
        {
            anim.SetBool("isBigAttack", true);
            print("fire time");
            
        }
    }
    void Dead()
    {
        //yippie.Play();
        //blockExit.SetActive(false);
        anim.SetBool("isDead", true);
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon"))
        {
            inRing = true;
            ringOfFire.SetActive(true);
            currentHealth -= 10;
            //Rigidbody rb = hit.GetComponent<Rigidbody>();
            Invoke("DoKnock", 3);
            //print("uhh");
            if (rb != null)
                rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
            //print("explode?");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            inRing = false;
        }
    }
    void DoKnock()
    {
        if (inRing)
        {
            ringOfFire.SetActive(false);
            //player.GetComponent<Knock>().KnockBackBig(player);
            //print("gotkncoked");
        }
        else
        {
            ringOfFire.SetActive(false);
        }
    }
    void ShootAtPlayer()
    {
        anim.SetBool("isAttacking", true);
        projectileTime -= Time.deltaTime;

        if (projectileTime > 0) return;

        projectileTime = timer;

        var position = spawnPoint.transform.position + transform.forward;
        var rotation = spawnPoint.transform.rotation;
        var projectile = Instantiate(projectilePrefab, position, rotation);
        projectile.Fire(projectileSpeed, spawnPoint.transform.forward);
     
    }
    void ShootAtPlayer2()
    {
        projectileTime -= Time.deltaTime;

        if (projectileTime > 0) return;

        projectileTime = timer;

        var position = spawnPoint2.transform.position + transform.forward;
        var rotation = spawnPoint2.transform.rotation;
        var projectile = Instantiate(projectilePrefab, position, rotation);
        projectile.Fire(projectileSpeed, spawnPoint2.transform.forward);
    }
    public void FireRingAttack()
    {
        StartCoroutine(RingOfFIreAttack(fireSpawn1));
        StartCoroutine(RingOfFIreAttack(fireSpawn2));
        StartCoroutine(RingOfFIreAttack(fireSpawn3));
        StartCoroutine(RingOfFIreAttack(fireSpawn4));
        StartCoroutine(RingOfFIreAttack(fireSpawn5));
        StartCoroutine(RingOfFIreAttack(fireSpawn6));
        StartCoroutine(RingOfFIreAttack(fireSpawn7));
        StartCoroutine(RingOfFIreAttack(fireSpawn8));
    }
    IEnumerator RingOfFIreAttack(GameObject spawnPoint)
    {
        var position = spawnPoint.transform.position + transform.forward;
        var rotation = spawnPoint.transform.rotation;
        var projectile = Instantiate(projectilePrefab, position, rotation);
        projectile.Fire(projectileSpeed, spawnPoint.transform.forward);
        yield return new WaitForSeconds(1f);
    }
    
}
