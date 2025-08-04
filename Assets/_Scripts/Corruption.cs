using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Corruption : MonoBehaviour
{
    //public static Corruption instance;

    public Image corruptionFill;
    public GameObject corruptionGlow;
    public float corruptionLevel;
    public AudioSource heartBeat;

    public bool hasEvilWeapon;
    public bool beginDamage;
    private bool wait;
    public GameObject corruptionUI;


    private void Awake()
    {
        //if (instance == null)
        {
            //instance = this;
        }
    }
    void Start()
    {
        beginDamage = false;
        hasEvilWeapon = false;
        wait = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        //corruptionFill.fillAmount = corruptionLevel/10f;

        

        if (Input.GetKeyDown(KeyCode.E) && Player_Manager.instance.specialWeaponValue > 0 && hasEvilWeapon == false)
        {
            hasEvilWeapon = true;
            corruptionUI.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.E) && corruptionLevel > 0)
        {
            Player_Manager.instance.specialWeaponValue = -1;
            hasEvilWeapon = false;
            beginDamage = false;
            corruptionLevel = 0f;
            corruptionGlow.SetActive(false);
            corruptionUI.SetActive(false);
            heartBeat.Stop();
            StopAllCoroutines();
        }

        if (hasEvilWeapon == true && wait == false && beginDamage == false)
        {
            StartCoroutine(EquipCorruptor());

        }
        if (beginDamage == true)
        {
            StartCoroutine(WeaponCorruption());
            
        }
        
    }

    private IEnumerator EquipCorruptor()
    {
        if (corruptionLevel >= 10f)
        {
            beginDamage = true;
        }
        
         wait = true;
         corruptionLevel++;
         heartBeat.Play();
         corruptionGlow.SetActive(true);
         yield return new WaitForSeconds(0.5f);
         corruptionGlow.SetActive(false);
        
        yield return new WaitForSeconds(3.5f);
        wait = false;
        
    }
    private IEnumerator WeaponCorruption()
    {
        heartBeat.Play();
        corruptionGlow.SetActive(true);
       //damage
        yield return new WaitForSeconds(2f);

    }
}
