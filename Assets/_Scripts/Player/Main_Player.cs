using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Main_Player : MonoBehaviour
{
    public static Main_Player instance;
    public PlayerSaveState thisGameSave;
    private bool isDead;
    
    [Header("Damage")] 
    public AudioSource ough;
    public GameObject bloodSplat;
    public List<GameObject> bloodSplats = new List<GameObject>();
    public int randomListObject;
    public GameObject hurt;
    bool damageCooldown;
    public CanvasGroup hurtScreen;
    private bool isFaded;

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
        thisGameSave.inMenu = false;
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.3f);
        
        damageCooldown = false;
    }
   

    public void TakeDamage()
    {
        damageCooldown = true;
        GameManager.instance.DamagePlayer();
        ough.Play();
        Player_Movement.instance.FailSafe();
        Fade();
        Debug.Log("ow");
        //cameraShake.ShakeCamera(shakeIntensity, shakeTime);
        hurt.SetActive(true);
        StartCoroutine(Wait());
        Player_Movement.instance.GotHit();

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
            print("dofadein");
            StartCoroutine(FadeWait(0.5f));
        }
    }
    IEnumerator FadeWait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        hurtScreen.DOFade(0, 0.3f);
        hurt.SetActive(false);
        print("dofadeout");
        isFaded = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
           if (!damageCooldown && !isDead)
            {
                TakeDamage();
                
            }
            StartCoroutine(Wait());
        }
        if (other.gameObject.CompareTag("JumpReward"))
        {
            thisGameSave.canJump = true;
            print("graduated from loser, can jump now");
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
