using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main_Player : MonoBehaviour
{
    public static Main_Player instance;
    public PlayerSaveState thisGameSave;
    
    //damage effects 
    public AudioSource ough;
    public GameObject bloodSplat;
    public List<GameObject> bloodSplats = new List<GameObject>();
    public int randomListObject;
    public GameObject hurt;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

    }
    
    void Start()
    {
        
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        hurt.SetActive(false);
    }

    public void TakeDamage()
    {
        GameManager.instance.DamagePlayer();
        print("ow");
        hurt.SetActive(true);
        StartCoroutine(Wait());

        randomListObject = Random.Range(0, bloodSplats.Count);
        GameObject b = Instantiate(bloodSplats[randomListObject]);
        b.transform.position = new Vector3(transform.position.x, transform.position.y - 0.9f, transform.position.z);
        b.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        //ough.Play();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
           TakeDamage();
        }
        if (other.gameObject.CompareTag("JumpReward"))
        {
            thisGameSave.canJump = true;
            print("graduated from loser, can jump now");
        }
    }
    
    void Update()
    {
        
    }
}
