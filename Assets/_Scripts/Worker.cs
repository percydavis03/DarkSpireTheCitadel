using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Worker : MonoBehaviour
{
    public PlayerSaveState thisGameSave;

    //stats
    public float enemyHP = 40; // Increased from 30 to 40 (slightly less than Enemy_Basic)
    public float maxEnemyHP = 40; // Increased from 30 to 40
    public int damageTaken;
    public int setSpeed;

    public List<GameObject> bloodSplats = new List<GameObject>();
    public int randomListObject;
    public GameObject enemyDrop;
    private Rigidbody rb;

    //Animation
    public Animator anim;
    public GameObject shovelHitbox;
    
    //Parry/Stun System
    [Header("Enhanced Parry System")]
    public bool isStunned = false;
    public float stunDuration = 2.0f;
    public float defaultStunDuration = 2.0f;
    private Coroutine stunCoroutine;
    public bool canBeParried = true; // Can this enemy be parried?

    [Header("Animation-Driven Parry Windows")]
    public bool isInParryWindow = false;        // Set by animation events
    public bool isInPerfectParryWindow = false; // Set by animation events for frame-perfect timing
    public bool canBeParriedNow = false;        // Set by animation events when vulnerable

    [Header("Attack Phase System")]
    public bool isInAttackStartup = false;      // Before parry window opens
    public bool isInAttackActive = false;       // Main attack phase (can be parried)
    public bool isInAttackRecovery = false;     // After attack (cannot be parried)

    [Header("Worker Parry Settings (Easier than Elite enemies)")]
    public float workerParryWindow = 0.3f;      // Generous window for workers
    public float workerPerfectWindow = 0.1f;    // Perfect timing window
    
    //AI
    public NavMeshAgent agent;
    public LayerMask whatIsGround, whatIsPlayer;
    public bool isHit;
    public bool dead;
    public Transform player;

    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;
    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject thisGuy;

    private void Start()
    {
        anim = GetComponent<Animator>();
        isHit = false;
        dead = false;
        alreadyAttacked = false;
    }
    private void Awake()
    {
        
        damageTaken = thisGameSave.mainAttackDamage;
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

    }
    void Update()
    {
        
        if (enemyHP <= 0)
        {
            if (!dead)
            {
                agent.SetDestination(thisGuy.transform.position);
                shovelHitbox.SetActive(false);
                dead = true;
                /// fix this
                anim.SetBool("IsDead", true);
                Death();

            }
        }

        // Don't do any AI behaviors while stunned
        if (isStunned)
        {
            return;
        }

        //Check for sight and attack
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

       
        if (playerInSightRange && !thisGameSave.inMenu && !playerInAttackRange && !dead && !anim.GetBool("IsHurting")) ChasePlayer();
        if (playerInAttackRange && !thisGameSave.inMenu && playerInSightRange && !isHit) AttackPlayer();
        if(!playerInAttackRange)
        {
            anim.SetBool("isAttacking", false);
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
    private void ChasePlayer()
    {
        if (!anim.GetBool("isRunning"))
        {
            anim.SetBool("isRunning", true);
        }
        
        agent.SetDestination(player.position);
        transform.LookAt(player);
    }

    private void AttackPlayer()
    {
        anim.SetBool("isRunning", false);
        anim.SetBool("isAttacking", true);
        agent.SetDestination(thisGuy.transform.position);

      

        GetComponent<NavMeshAgent>().speed = 0;
        //transform.LookAt(player);

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;
            StartCoroutine(NavWait(1f));
        }
    }
    IEnumerator NavWait(float s)
    {
        yield return new WaitForSeconds(s);
        ResetAttack();
    }
    private void ResetAttack()
    {
        alreadyAttacked = false;
        //weaponHitbox.SetActive(false);
        GetComponent<NavMeshAgent>().speed = setSpeed;
        anim.SetBool("IsAttacking", false);
    }
    IEnumerator Wait(float s)
    {
        yield return new WaitForSeconds(s);
        StopHurt();
    }
    public void StopHurt()
    {
        anim.SetBool("IsHurting", false);
        isHit = false;
        print("stophurt");
    }
    private void OnTriggerEnter(Collider other)
    {
        print("collide");
        // OLD SYSTEM - Now handled by WeaponScript for combo damage
        /*
        if (other.gameObject.CompareTag("Weapon"))
        {
            if (isHit == false)
            {
                TakeDamage();
               
            }
        }
        */
    }
    public void TakeDamage()
    {
        isHit = true;
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsAttacking", false);
       
        if (enemyHP != 0)
        {
            anim.SetBool("IsHurting", true);
            enemyHP = enemyHP - damageTaken;
            print("literally take damage ");
            StartCoroutine(Wait(0.5f));
        }
    }

    // New method for combo-specific damage
    public void TakeComboDamage(int damage)
    {
        // Don't take damage if stunned (parry already handled damage)
        if (isStunned) return;
        
        isHit = true;
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsAttacking", false);
       
        // No knockback for workers - handled by WeaponScript if needed
        if (enemyHP != 0)
        {
            anim.SetBool("IsHurting", true);
            enemyHP = enemyHP - damage;
            print($"literally take {damage} combo damage");
            StartCoroutine(Wait(0.5f));
        }
    }

    // Enhanced Parry System Methods
    public bool IsAttacking()
    {
        return anim.GetBool("isAttacking");
    }
    
    // NEW: Enhanced parry methods for animation-driven timing
    public bool CanBeParriedNow()
    {
        return canBeParriedNow && canBeParried && !dead;
    }
    
    public bool IsInParryWindow()
    {
        return isInParryWindow && isInAttackActive;
    }
    
    public bool IsInPerfectParryWindow()
    {
        return isInPerfectParryWindow && isInAttackActive;
    }

    public void GetParried(float customStunDuration = -1f)
    {
        if (!canBeParried || isStunned || dead) return;

        print("Worker parried! Stunned.");
        
        // Stop current actions
        StopAllAttacks();
        anim.SetBool("isRunning", false);
        anim.SetBool("IsHurting", false);
        
        // Apply stun
        isStunned = true;
        anim.SetBool("IsStunned", true);
        
        // Set stun duration
        float duration = customStunDuration > 0 ? customStunDuration : defaultStunDuration;
        
        // Stop existing stun coroutine if running
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        
        stunCoroutine = StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        // Disable movement and attacks
        StopMoving();
        
        yield return new WaitForSeconds(duration);
        
        // Recover from stun
        RecoverFromStun();
    }

    public void RecoverFromStun()
    {
        if (!isStunned) return;
        
        print("Worker recovering from stun");
        isStunned = false;
        anim.SetBool("IsStunned", false);
        
        // Re-enable movement
        if (!dead)
        {
            agent.enabled = true;
            GetComponent<NavMeshAgent>().speed = setSpeed;
        }
        
        stunCoroutine = null;
    }

    private void StopAllAttacks()
    {
        anim.SetBool("isAttacking", false);
        shovelHitbox.SetActive(false);
        alreadyAttacked = false;
    }

    private void StopMoving()
    {
        GetComponent<NavMeshAgent>().speed = 0;
        agent.isStopped = true;
        agent.enabled = false;
    }

    //---------ENHANCED ANIMATION EVENTS FOR WORKER PARRY SYSTEM---------
    
    // Called at the start of worker attack animation
    public void AnimAttackStartup()
    {
        Debug.Log("Worker Attack Startup - No parry window yet");
        isInAttackStartup = true;
        isInAttackActive = false;
        isInAttackRecovery = false;
        canBeParriedNow = false;
        isInParryWindow = false;
        isInPerfectParryWindow = false;
    }
    
    // Called when worker parry window opens (easier timing than elite enemies)
    public void AnimParryWindowOpen()
    {
        Debug.Log("Worker Parry window OPENED (Easy mode)");
        isInAttackStartup = false;
        isInAttackActive = true;
        canBeParriedNow = true;
        isInParryWindow = true;
        
        // Start perfect parry window timer
        StartCoroutine(WorkerPerfectParryWindowCoroutine());
        
        // Close parry window after generous duration for workers
        StartCoroutine(CloseParryWindowAfterDelay(workerParryWindow));
    }
    
    // Called for worker perfect parry timing
    public void AnimPerfectParryWindow()
    {
        Debug.Log("WORKER PERFECT PARRY WINDOW ACTIVE!");
        isInPerfectParryWindow = true;
        
        // Close perfect window after duration
        StartCoroutine(ClosePerfectParryWindowAfterDelay(workerPerfectWindow));
    }
    
    // Called when worker attack becomes active
    public void AnimAttackActive()
    {
        Debug.Log("Worker Attack is now ACTIVE");
        isInAttackActive = true;
        WeaponOn(); // Enable shovel hitbox
    }
    
    // Called when worker parry window closes
    public void AnimParryWindowClose()
    {
        Debug.Log("Worker Parry window CLOSED");
        isInParryWindow = false;
        isInPerfectParryWindow = false;
        canBeParriedNow = false;
    }
    
    // Called when worker attack enters recovery
    public void AnimAttackRecovery()
    {
        Debug.Log("Worker Attack Recovery");
        isInAttackActive = false;
        isInAttackRecovery = true;
        isInParryWindow = false;
        isInPerfectParryWindow = false;
        canBeParriedNow = false;
    }
    
    // Worker-specific coroutines
    private IEnumerator WorkerPerfectParryWindowCoroutine()
    {
        // Wait before perfect window opens (generous timing for workers)
        yield return new WaitForSeconds(0.15f);
        AnimPerfectParryWindow();
    }
    
    private IEnumerator CloseParryWindowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isInParryWindow)
        {
            AnimParryWindowClose();
        }
    }
    
    private IEnumerator ClosePerfectParryWindowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isInPerfectParryWindow = false;
    }
    
    // Reset all worker parry states
    public void ResetParryStates()
    {
        isInAttackStartup = false;
        isInAttackActive = false;
        isInAttackRecovery = false;
        canBeParriedNow = false;
        isInParryWindow = false;
        isInPerfectParryWindow = false;
    }
    
    //---------ORIGINAL WORKER METHODS---------
    public void WeaponOn()
    {
        shovelHitbox.SetActive(true);
        print("weapon on");
    }

    public void WeaponOff()
    {
        shovelHitbox.SetActive(false);
        print("weapon off");
    }
    public void StopAttacking()
    {
       
        anim.SetBool("IsAttacking", false);
        transform.LookAt(player);
        GetComponent<NavMeshAgent>().speed = setSpeed;
        ResetParryStates(); // Reset enhanced parry system states
        print("stop attacking");
    }

    public void Death()
    {
        dead = true;
        anim.SetBool("isDead", true);
        setSpeed = 0;
        shovelHitbox.SetActive(false);
        Bleed();
        this.GetComponent<BoxCollider>().enabled = false;
        this.GetComponent<CapsuleCollider>().enabled = false;
        GameObject s = Instantiate(enemyDrop);
        s.transform.position = transform.position;
       
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

}
