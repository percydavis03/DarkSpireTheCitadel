using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FMODUnity;
using Unity.VisualScripting;

public class Main_Player : MonoBehaviour
{
    public GameObject Player;
    public static Main_Player instance;
    public PlayerSaveState thisGameSave;
    private bool isDead;
    public bool canTakeDamage;
    
    [Header("Damage")] 
    public StudioEventEmitter ough;
    public GameObject bloodSplat;
    public List<GameObject> bloodSplats = new List<GameObject>();
    public int randomListObject;
    public GameObject hurt;
    public GameObject glow;
    bool damageCooldown;
    public CanvasGroup hurtScreen;
    public CanvasGroup hurtGlow;
    private bool isFaded;
    [SerializeField] private float knockbackForce = 15f; // Force of the knockback - increased for more noticeable effect
   

    [Header("Camera Shake")]
    [SerializeField] private CameraShakeController cameraShake;
    [SerializeField] public float shakeIntensity = 2;
    [SerializeField] public float shakeTime = 0.5f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    void Start()
    {
        isDead = false;
        isFaded = true;
        canTakeDamage = true; // Initialize to true so player can take damage
        thisGameSave.inMenu = false;
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.3f);
        
        damageCooldown = false;
    }
   

    public void TakeDamage()
    {

     //   Debug.Log($"🚨 LEGACY TakeDamage() called! This shouldn't happen if enemies are hitting player properly.");
        // Legacy method - use random direction for backward compatibility
        Vector3 randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
      //  Debug.Log($"🚨 Generated random direction: {randomDirection}, magnitude: {randomDirection.magnitude}");
        TakeDamageFromEnemy(null, randomDirection);
    }
    
    public void TakeDamageFromEnemy(Transform enemyTransform, Vector3? overrideDirection = null)
    {
        damageCooldown = true;
        GameManager.instance.DamagePlayer();
        //ough.Play();
        
        // Clear attack states but don't interfere with knockback
        Player_Movement.instance.EndAttack();
        Player_Movement.instance.ResetCombo();
        
        Fade();
        // Debug.Log("ow"); // DISABLED - was causing spam
        //cameraShake.ShakeCamera(shakeIntensity, shakeTime);
        hurt.SetActive(true);
        glow.SetActive(true);
        StartCoroutine(Wait());
        Player_Movement.instance.GotHit();
        
        //Debug.LogError("=== MAIN_PLAYER DAMAGE DEBUG START ===");

        // Apply knockback - use safe direction calculation
        Vector3 knockbackDirection = Vector3.forward; // Default direction
        
        if (overrideDirection.HasValue)
        {
            knockbackDirection = overrideDirection.Value;
           // Debug.Log($"🚀 Using override direction: {knockbackDirection}");
        }
        else if (enemyTransform != null)
        {
            // Calculate direction from enemy to player (push player away from enemy)
            knockbackDirection = (transform.position - enemyTransform.position).normalized;
          //  Debug.Log($"🚀 Knockback direction calculated: Enemy at {enemyTransform.position}, Player at {transform.position}, Direction: {knockbackDirection}");
        }
        else
        {
            // Fallback: use a random direction
            knockbackDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f)).normalized;
            //Debug.Log($"🚀 Using fallback random knockback direction: {knockbackDirection}");
        }
        
        // Safety check: ensure we have a valid direction
        if (knockbackDirection.magnitude < 0.1f)
        {
            // Emergency fallback: use forward direction
            knockbackDirection = Vector3.forward;
            ///Debug.LogWarning($"🚨 Zero knockback direction detected! Using emergency fallback: {knockbackDirection}");
        }
        
        knockbackDirection.y = 0; // Keep knockback horizontal
        knockbackDirection = knockbackDirection.normalized; // Ensure it's normalized
        
        Vector3 finalKnockbackForce = knockbackDirection * knockbackForce;
       /// Debug.Log($"🚀 Final knockback calculation: Direction={knockbackDirection}, Force={knockbackForce}, Result={finalKnockbackForce}");
        
        // Final safety check
        if (finalKnockbackForce.magnitude < 0.1f)
        {
           // Debug.LogError($"🚨 CRITICAL: Final knockback force is too small! Force={finalKnockbackForce}");
            finalKnockbackForce = Vector3.forward * knockbackForce; // Emergency override
        }
        
       // Debug.LogError("=== ABOUT TO APPLY KNOCKBACK ===");
        Player_Movement.instance.ApplyKnockback(finalKnockbackForce);
       // Debug.LogError("=== MAIN_PLAYER DAMAGE DEBUG END ===");

        randomListObject = Random.Range(0, bloodSplats.Count);
        GameObject b = Instantiate(bloodSplats[randomListObject]);
        b.transform.position = new Vector3(transform.position.x, transform.position.y - 0.9f, transform.position.z);
        b.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
    }

    public void Fade()
    {

        if (isFaded)
        {
            hurtScreen.DOFade(1, 0.3f);
            hurtGlow.DOFade(1, 0.1f);
            StartCoroutine(FadeWait(0.5f));
        }
    }
    IEnumerator FadeWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        hurtScreen.DOFade(0, 0.3f);
        hurtScreen.DOFade(0, 0.1f);
        glow.SetActive(false);
        hurt.SetActive(false);
        isFaded = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.LogError($"=== ONTRIGGERENTER CALLED with {other.gameObject.name}, tag: {other.gameObject.tag} ===");
        if (other.gameObject.CompareTag("Enemy"))
        {
           if (!damageCooldown && !isDead && canTakeDamage)
            {
              //  Debug.LogError($"=== CALLING TakeDamageFromEnemy with enemy: {other.transform.name} ===");
                TakeDamageFromEnemy(other.transform);
                
            }
            StartCoroutine(Wait());
        }
        if (other.gameObject.CompareTag("JumpReward"))
        {
            thisGameSave.canJump = true;
            // print("graduated from loser, can jump now"); // DISABLED - was causing spam
        }
    }

    void Update()
    {
        if (thisGameSave.hitpoints <= 0)
        {
            isDead = true;
        }
    }
}
