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
   

    
    void Start()
    {
        if (instance != null)

        {
            Debug.LogWarning("Found more than one Player Health Manager");
        }
        instance = this;
      
        currentHealth = maxHealth;
       
        //healthSlider.maxValue = maxHealth;
       
    }

    public static Player_Health_Manager GetInstance()
    {
        return instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {

        }
            if (GetComponent<Knock>().knockBack == false && isHurt == true)
        {
            isHurt = false;
        }
        //healthSlider.value = currentHealth;
        healthFill.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            print("Dead");
            StartCoroutine(DeathScreen());
        }

      
    }

    public void DamangePlayer(float f)
    {
        currentHealth -= f;
    }
    
    
   

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(hurtTags[0]) && Player_Manager.GetInstance().canBeHurt == true)
        {
            isHurt=true;
            DamangePlayer(other.gameObject.GetComponent<HurtBox>().damageValye);
        }
    }
    
   
    private IEnumerator DeathScreen()
    {
        anim.SetBool(animNames[0], true);
        Player_Manager.GetInstance().canMove = false;
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
