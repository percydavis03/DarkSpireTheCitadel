using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Player_Health_Manager : MonoBehaviour
{
    public static Player_Health_Manager instance;
    [Header("Values")]
    public float maxHealth;
    public float currentHealth;
    public float maxHP;
    [Space]
    [Header("Tags")]
    public string[] hurtTags;
    [Space]
    [Header("Bools")]
    public bool isHurt;
    [Space]
    [Header("UI")]
    //public Slider healthSlider;
    public Animator anim;
    public string[] animNames;
    // new
    public Image healthFill;
    public PlayerSaveState thisGameSave;
  

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("Found more than one Player Health Manager - destroying duplicate");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Don't override instance here - it's already set in Awake()
        // if (instance != this) { Debug.LogWarning("Found more than one Player Health Manager"); }
        // instance = this; // REMOVED - was causing singleton issues

        
       
        //healthSlider.maxValue = maxHealth;
       
    }

    public static Player_Health_Manager GetInstance()
    {
        return instance;
    }

    // Update is called once per frame
    void Update()
    {
        currentHealth = thisGameSave.hitpoints;
        maxHP = thisGameSave.maxHP;
        //healthSlider.value = currentHealth;
        healthFill.fillAmount = currentHealth / maxHP;

        if (currentHealth <= 0)
        {
            // print("Dead"); // DISABLED - was causing spam
            //thisGameSave.Init();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            //StartCoroutine(DeathScreen());
        }

      
    }

    public void DamangePlayer(int f)
    {
        thisGameSave.hitpoints -= f;
        // print("gay sex"); // DISABLED - inappropriate content removed
    }
    
    
   

    
   
    private IEnumerator DeathScreen()
    {
        anim.SetBool(animNames[0], true);
        Player_Movement.instance.canMove = false;
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
