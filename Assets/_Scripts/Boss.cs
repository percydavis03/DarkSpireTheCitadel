using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent boss;
    public Transform player;
    public HurtManager hurtManager;
    public AudioSource yippie;

    [SerializeField] private float projectileSpeed;
    [SerializeField] private projectile projectilePrefab;
    [SerializeField] private float timer = 5;
    private float projectileTime;
    public Transform spawnPoint;
    public Transform spawnPoint2;
    public float enemySpeed;

    //EXPLODE
    public float radius = 5f;
    public float power = 10f;
    private Vector3 explosionPos;
    private Rigidbody rb;

    public GameObject ringOfFire;
    private bool inRing;
    public float currentHealth;
    // Start is called before the first frame update
    void Start()
    {
         explosionPos = transform.position;
        rb = GetComponent<Rigidbody>();
        inRing = false;
        currentHealth = hurtManager.Health;
    }
   
    // Update is called once per frame
    void Update()
    {
        transform.LookAt(player);
        boss.SetDestination(player.position);
        ShootAtPlayer();
        
        currentHealth = hurtManager.Health;
        if (currentHealth <= 0)
        {
            Dead();
        }
        if (currentHealth <= 150)
        {
            ShootAtPlayer2();
        }
    }

    void Dead()
    {
        yippie.Play();
        Destroy(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            inRing = true;
            ringOfFire.SetActive(true);
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
            player.GetComponent<Knock>().KnockBackBig(player);
            print("gotkncoked");
        }
        else
        {
            ringOfFire.SetActive(false);
        }
    }
    void ShootAtPlayer()
    {
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

}
