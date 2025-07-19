using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
 

public class Enemy_Basic : MonoBehaviour, IKnockbackable
{
    
    public PlayerSaveState thisGameSave;

    public bool isLock;
    public bool isSpeartwo;
    public GameObject exit;
    //stats
    public float enemyHP = 50; // Increased from 30 to 50
    public float maxEnemyHP = 50; // Increased from 30 to 50
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
    public bool isHit;
    public bool dead;
    public GameObject thisGuy;
    public Transform player;
    public Image healthFill;
    public float waitThreshold = 0.05f;
    
    //combat
    public GameObject spear_hitbox;
    
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
    public int currentAttackPhase = 0;          // Track multi-phase attacks
    public int maxAttackPhases = 1;             // Single-phase by default
    public bool isInAttackStartup = false;      // Before parry window opens
    public bool isInAttackActive = false;       // Main attack phase (can be parried)
    public bool isInAttackRecovery = false;     // After attack (cannot be parried)

    [Header("Parry Difficulty Settings")]
    [Range(1, 5)]
    public int parryDifficulty = 2;             // 1=Easy, 5=Expert
    public float[] difficultyParryWindows = {0.4f, 0.3f, 0.2f, 0.1f, 0.05f}; // Easy to Expert
    public float[] difficultyPerfectWindows = {0.1f, 0.08f, 0.06f, 0.04f, 0.02f}; // Perfect timing windows
    
    //AI
    public NavMeshAgent agent;
    public LayerMask whatIsGround, whatIsPlayer;
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
    

   

    private void Start()
    {
        anim = animationSource.GetComponent<Animator>();
        isHit = false;
        dead = false;
        alreadyAttacked = false;
        
        // Find the player reference properly
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Nyx");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning($"Enemy {name}: Could not find Player GameObject with tag 'nyx'");
            }
        }
    }
    private void Awake()
    {
        damageTaken = thisGameSave.mainAttackDamage; 
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        // Ensure we have a valid agent
        if (agent == null)
        {
            Debug.LogError($"Enemy {name}: NavMeshAgent component not found!");
            return;
        }

        //agent.angularSpeed = 0f;
        //agent.updateRotation = false;
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
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsAttacking", false);
        KnockbackEntity(player);
       
        GetKnockedBack(new Vector3(100, 100, 100));
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
        
        // Face the player when taking damage for better combat feedback
        if (player != null)
        {
            transform.LookAt(player);
        }
        
        KnockbackEntity(player);
       
        // Apply damage
        if (enemyHP > 0)
        {
            anim.SetBool("IsHurting", true);
            enemyHP = enemyHP - damage;
            print($"take {damage} damage, HP now: {enemyHP}");
            StartCoroutine(Wait(0.5f));
        }
    }

    // Enhanced Parry System Methods
    public bool IsAttacking()
    {
        return anim.GetBool("IsAttacking") || anim.GetBool("IsAttacking2");
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

        print("Enemy parried! Stunned.");
        
        // Stop current actions
        StopAllAttacks();
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsHurting", false);
        
        // Face the player when parried for better visual feedback
        if (player != null)
        {
            transform.LookAt(player);
        }
        
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
        
        print("Enemy recovering from stun");
        isStunned = false;
        anim.SetBool("IsStunned", false);
        
        // Re-enable movement
        if (!dead && agent != null)
        {
            agent.enabled = true;
            // Only set speed if agent is properly enabled and on NavMesh
            if (agent.enabled && agent.isOnNavMesh)
            {
                GetComponent<NavMeshAgent>().speed = setSpeed;
            }
        }
        
        stunCoroutine = null;
    }

    private void StopAllAttacks()
    {
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsAttacking2", false);
        spear_hitbox.SetActive(false);
        alreadyAttacked = false;
    }

    public void GetKnockedBack(Vector3 force)
    {
        print("knockback");
        StopMoving();
        // Simple knockback without complex physics
        StartCoroutine(SimpleKnockback(force));
    }

    private IEnumerator SimpleKnockback(Vector3 force)
    {
        // Don't use rigidbody physics - just move the transform directly
        Vector3 knockbackDirection = force.normalized;
        float knockbackDistance = force.magnitude * 0.1f; // Scale down the distance
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + knockbackDirection;
        
        float duration = 0.2f; // Very short knockback
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            // Simple lerp movement
            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }
        
        // Re-enable NavMesh agent
        if (agent != null)
        {
            agent.Warp(transform.position);
            agent.enabled = true;
            
            if (agent.enabled && agent.isOnNavMesh)
            {
                GetComponent<NavMeshAgent>().speed = setSpeed;
            }
        }
        
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
    }
    public void StopMoving()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            GetComponent<NavMeshAgent>().speed = 0;
            agent.isStopped = true;
            agent.enabled = false;
        }
    }
    public void Reposition()
    {
        transform.LookAt(player);
    }
    public void StopHurt()
    {
        anim.SetBool("IsHurting", false);
        isHit = false;
        print("stophurt");
       // pain.SetActive(false);
    }
    public void GetUp()
    {
        hitCount = 0;
        isHit = false;
        anim.SetBool("IsHurting", false);
        anim.SetInteger("HurtAnim", 0);
        print("getup");
    }
    public void WeaponOn()
    {
        spear_hitbox.SetActive(true);
        print("weapon on");
    }

    public void WeaponOff()
    {
        spear_hitbox.SetActive(false);
        print("weapon off");
    }
    public void StopAttacking()
    {
        spear_hitbox.SetActive(false);
        anim.SetBool("IsAttacking", false);
        
        // Face player when stopping attack
        if (player != null)
        {
            transform.LookAt(player);
        }
        
        if (agent != null && agent.enabled)
        {
            GetComponent<NavMeshAgent>().speed = setSpeed;
        }
        print("stop attacking");
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
                hitCount++;
            }
           
            //randomListObject = Random.Range(0, bloodSplats.Count);
        }
        */
    }

    public void Death()
    {
        dead = true;
        setSpeed = 0;
        spear_hitbox.SetActive (false);
        Bleed();
        this.GetComponent<BoxCollider>().enabled = false; 
        this.GetComponent<CapsuleCollider>().enabled = false;
        GameObject s = Instantiate(enemyDrop);
        s.transform.position = transform.position;
        if (isLock)
        {
            exit.SetActive(true);
        }
        //Destroy(thisGuy);
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

    // Update is called once per frame
    void Update()
    {
        // Update health bar if it exists
        if (healthFill != null)
        {
            healthFill.fillAmount = enemyHP / maxEnemyHP;
        }
        if (enemyHP <= 0)
        {
           if (!dead)
            {
                // Add safety check before using agent
                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    agent.SetDestination(thisGuy.transform.position);
                }
                spear_hitbox.SetActive(false);
                 /// fix this
                anim.SetBool("IsDead", true);
                
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

        if (!playerInSightRange && !playerInAttackRange && !anim.GetBool("IsHurting")) Patroling();
        if (playerInSightRange && !thisGameSave.inMenu && !playerInAttackRange && !dead && !anim.GetBool("IsHurting")) ChasePlayer();
        if (playerInAttackRange && !thisGameSave.inMenu && playerInSightRange && !isHit) AttackPlayer();

        
        if (agent != null && agent.enabled && agent.velocity.magnitude > 0.1f && !anim.GetBool("IsHurting") && !anim.GetBool("IsAttacking"))
        {
            anim.SetBool("IsRunning", true);
        }
        if (agent == null || !agent.enabled || agent.velocity.magnitude < 0.1f)
        {
            anim.SetBool("IsRunning", false);
        }

    }
    //----------AI-----------
    IEnumerator NavWait(float s)
    {
        yield return new WaitForSeconds(s);
        ResetAttack();
    }
    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }

    }

    private void SearchWalkPoint()
    {
        //calculate random point in range
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh && player != null)
        {
            agent.SetDestination(player.position);
            transform.LookAt(player); // Face player while chasing
        }
    }

    private void AttackPlayer()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(thisGuy.transform.position);
        }
        
        if (isSpeartwo)
        {
            anim.SetBool("IsAttacking2", true);
        }
        if (!isSpeartwo)
        {
            anim.SetBool("IsAttacking", true);
        }
        
        if (agent != null && agent.enabled)
        {
            GetComponent<NavMeshAgent>().speed = 0;
        }
        
        // Face player during attack
        if (player != null)
        {
            transform.LookAt(player);
        }

        if (!alreadyAttacked)
        {
            alreadyAttacked = true;
            StartCoroutine(NavWait(1f));
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        //weaponHitbox.SetActive(false);
        if (agent != null && agent.enabled)
        {
            GetComponent<NavMeshAgent>().speed = setSpeed;
        }
        anim.SetBool("IsAttacking", false);
    }



    //---------ANIMATIONS---------
    //---------ENHANCED ANIMATION EVENTS FOR PARRY SYSTEM---------
    
    // Called at the start of attack animation - enemy begins attack startup
    public void AnimAttackStartup()
    {
        Debug.Log("Attack Startup - No parry window yet");
        isInAttackStartup = true;
        isInAttackActive = false;
        isInAttackRecovery = false;
        canBeParriedNow = false;
        isInParryWindow = false;
        isInPerfectParryWindow = false;
        currentAttackPhase = 1;
    }
    
    // Called when parry window opens - enemy becomes vulnerable
    public void AnimParryWindowOpen()
    {
        Debug.Log($"Parry window OPENED - Difficulty {parryDifficulty}");
        isInAttackStartup = false;
        isInAttackActive = true;
        canBeParriedNow = true;
        isInParryWindow = true;
        
        // Start perfect parry window timer
        StartCoroutine(PerfectParryWindowCoroutine());
        
        // Close parry window after difficulty-based duration
        float windowDuration = difficultyParryWindows[Mathf.Clamp(parryDifficulty - 1, 0, 4)];
        StartCoroutine(CloseParryWindowAfterDelay(windowDuration));
    }
    
    // Called for frame-perfect parry timing (very brief window)
    public void AnimPerfectParryWindow()
    {
        Debug.Log("PERFECT PARRY WINDOW ACTIVE!");
        isInPerfectParryWindow = true;
        
        // Close perfect window after very short duration
        float perfectDuration = difficultyPerfectWindows[Mathf.Clamp(parryDifficulty - 1, 0, 4)];
        StartCoroutine(ClosePerfectParryWindowAfterDelay(perfectDuration));
    }
    
    // Called when attack becomes active and deals damage
    public void AnimAttackActive()
    {
        Debug.Log("Attack is now ACTIVE - dealing damage");
        isInAttackActive = true;
        WeaponOn(); // Enable hitbox
    }
    
    // Called when parry window closes - no longer vulnerable
    public void AnimParryWindowClose()
    {
        Debug.Log("Parry window CLOSED");
        isInParryWindow = false;
        isInPerfectParryWindow = false;
        canBeParriedNow = false;
    }
    
    // Called when attack enters recovery phase
    public void AnimAttackRecovery()
    {
        Debug.Log("Attack Recovery - no longer parrysable");
        isInAttackActive = false;
        isInAttackRecovery = true;
        isInParryWindow = false;
        isInPerfectParryWindow = false;
        canBeParriedNow = false;
    }
    
    // For multi-phase attacks - advance to next phase
    public void AnimNextAttackPhase()
    {
        currentAttackPhase++;
        Debug.Log($"Attack Phase {currentAttackPhase}/{maxAttackPhases}");
        
        if (currentAttackPhase <= maxAttackPhases)
        {
            // Reset for next phase
            AnimParryWindowOpen();
        }
        else
        {
            // Attack complete
            AnimAttackRecovery();
        }
    }
    
    // Coroutine to handle perfect parry window timing
    private IEnumerator PerfectParryWindowCoroutine()
    {
        // Wait a brief moment before perfect window opens
        yield return new WaitForSeconds(0.1f);
        AnimPerfectParryWindow();
    }
    
    private IEnumerator CloseParryWindowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isInParryWindow) // Only close if still open
        {
            AnimParryWindowClose();
        }
    }
    
    private IEnumerator ClosePerfectParryWindowAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isInPerfectParryWindow = false;
    }
    
    // Reset all parry states (called when attack ends or is interrupted)
    public void ResetParryStates()
    {
        isInAttackStartup = false;
        isInAttackActive = false;
        isInAttackRecovery = false;
        canBeParriedNow = false;
        isInParryWindow = false;
        isInPerfectParryWindow = false;
        currentAttackPhase = 0;
    }

    //---------ORIGINAL ANIMATION EVENTS---------
    public void AnimStab()
    {
       WeaponOn();
    }

    public void AnimWeaponOff()
    {
        WeaponOff();
    }
    public void AnimAttackEnd()
    {
        StopAttacking();
        ResetParryStates(); // Reset enhanced parry system states
    }
    public void AnimFallEnd()
    {
        GetUp();
    }
    public void AnimHurtEnd()
    {
        StopHurt();
    }

    public void AnimDied()
    {
        Death();
    }
}
