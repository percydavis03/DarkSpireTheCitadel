using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Basic : MonoBehaviour
{
    public static Enemy_Basic instance;
    public PlayerSaveState thisGameSave;
    //stats
    public float enemyHP = 30;
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
    bool isHit;
    bool dead;
    public GameObject thisGuy;
    public Transform player;
    public GameObject pain;
    //combat
    public GameObject spear_hitbox;
    private void Start()
    {
        anim = animationSource.GetComponent<Animator>();
        hitCount = 0;
        dead = false;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        damageTaken = thisGameSave.mainAttackDamage; 
        rb = GetComponent<Rigidbody>();
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
        anim.SetBool("IsWalk", false);
        KnockbackEntity(player);
        pain.SetActive(true);
        if (enemyHP != 0)
        {
            enemyHP = enemyHP - damageTaken;
            print("literally take damage ");
        }
      
      
        if (hitCount == 1 && !anim.GetBool("IsHurting"))
        {
           
            anim.SetBool("IsHurting", true);
            anim.SetInteger("HurtAnim", 1);
            print("hurt1");
            StartCoroutine(Wait(1f));
        }
        else if (hitCount == 2 && !anim.GetBool("IsHurting"))
        {
            
            anim.SetBool("IsHurting", true);

            anim.SetInteger("HurtAnim", 2);
            print("hurt2");
            StartCoroutine(Wait(1f));
        }
        else if (hitCount == 3 && !anim.GetBool("IsHurting")) //fall down
        {
            
            anim.SetBool("IsHurting", true);
            anim.SetInteger("HurtAnim", 3);
            print("hurt3");
            StartCoroutine(Wait(2f));
        }
    }
    public void StopHurt()
    {
        anim.SetBool("IsHurting", false);
        isHit = false;
        print("stophurt");
        pain.SetActive(false);
    }
    public void GetUp()
    {
        hitCount = 0;
        isHit = false;
        anim.SetBool("IsHurting", false);
        anim.SetInteger("HurtAnim", 0);
        print("getup");
        pain.SetActive(false);
    }
    public void WeaponOn()
    {
        spear_hitbox.SetActive(true);
        print("weapon on");
    }
    public void StopAttacking()
    {
        anim.SetBool("IsAttacking", false);
        spear_hitbox.SetActive(false);
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
                hitCount++;
                TakeDamage();
            }
           
            //randomListObject = Random.Range(0, bloodSplats.Count);
        }
    }

    void Death()
    {
        dead = true;
        GameObject s = Instantiate(enemyDrop);
        s.transform.position = transform.position;
        GameObject b = Instantiate(bloodSplats[randomListObject]);
        b.transform.position = new Vector3(transform.position.x, transform.position.y - 1.2f, transform.position.z);
        b.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        Destroy(thisGuy);
    }
    // Update is called once per frame
    void Update()
    {
        if (enemyHP <= 0)
        {
           if (!dead)
            {
                Death();
            }
        } 
            
    }
}
