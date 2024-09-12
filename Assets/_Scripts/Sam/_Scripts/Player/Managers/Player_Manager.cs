using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Player_Manager : MonoBehaviour
{
    public static Player_Manager instance;
    [Space]
    [Header("Player Actions")]
    public bool canMove;
    public bool canAttack;
    public bool isDashing;
    public bool canBeHurt;
    public bool isAttacking;
    public bool isParry;
    public bool isCounter;
    public bool isHurt;
    public bool isStinger;
    public bool hasSpecialWeapon;
    //  public bool isAttacking;
    [Space]
    [Header("Player Input")]
    public Vector2 inputVector;
    [Space]
    [Header("RigidBody")]
    public Rigidbody rb;
    [Header("Transform")]
    public Transform playerModel;
    public Transform orientation;
    [Header("Floats")]
    public float dashFroce;
    public float stingerForce;
    [Space]
    [Header("Scripts")]
    public Player movement;
    public Slash2 slash;
    public ParryBoxCollider ParryBoxCollider;

    [Space]
    [Header("Special Moves")]
    public Weapon specialWeapon;
    public Transform specialWeaponSpawn;
    public GameObject weaponObject;
    public float specialWeaponValue;
    public float maxSpecialWeaponValue;
    public Slider specialWeaponSlider;
    [Space]
    [Header("Animation")]
    public string[] animationName;
    public Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        if (instance != null)

        {
            Debug.LogWarning("Found more than one Player Manager");
        }
        instance = this;
        canMove = true;
        specialWeaponValue = maxSpecialWeaponValue;
        specialWeaponSlider.maxValue = maxSpecialWeaponValue;
    }
    public static Player_Manager GetInstance()
    {
        return instance;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (MenuScript.instance.movePause)
        {
            dashFroce = 0f;
        }
        else if (MenuScript.instance.movePause == false)
        {
            dashFroce = 8;
        }
        isAttacking = slash.isAttacking;
        isParry = slash.parry;

        specialWeaponSlider.value = specialWeaponValue;

        if (!canMove)
            inputVector = Vector2.zero;
        else
        {
            
            
                inputVector.x = Input.GetAxisRaw("Horizontal");
                inputVector.y = Input.GetAxisRaw("Vertical");
            
        }

        if (GetComponent<Knock>().knockBack == true)
        {
            canMove = false;
          //  movement.enabled = false;
        }
        

        if (isAttacking)
        {
            canMove = false;
        }

        if (isParry)
        {
            canMove = false;
        }

        if (isCounter)
        {
            canMove = false;
        }

        if (isStinger)
        {
            canMove = false;
        }



        if(specialWeapon != null && hasSpecialWeapon == false)
        {
            hasSpecialWeapon = true;
            GameObject special = Instantiate(specialWeapon.weaponPrefab, specialWeaponSpawn.transform);
            special.transform.parent = specialWeaponSpawn.transform;
            hasSpecialWeapon = true;
            weaponObject = special;
        } 

        if(specialWeaponValue < 0)
        {
            hasSpecialWeapon = false;
            print("FUKCING DELETE");
            specialWeapon = null;
            Destroy(weaponObject);
            isStinger = false;
            StopAllCoroutines();
            anim.SetBool(animationName[0], false);
            specialWeaponValue = 0;
        }

        //if (Input.GetKeyDown(KeyCode.E) && Corruption.instance.hasEvilWeapon == true)
        {
           // specialWeaponValue = -1;
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            inputVector = Vector2.zero;
            canMove = false;
            rb.velocity = playerModel.forward * dashFroce;
        }

        if (isStinger)
        {
            inputVector = Vector2.zero;
            canMove = false;
            rb.velocity = playerModel.forward * stingerForce;
        }

        
    }
}
