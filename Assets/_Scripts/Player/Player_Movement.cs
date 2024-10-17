using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Movement : MonoBehaviour
{
    public static Player_Movement instance;
    public PlayerSaveState thisGameSave;
    //INPUT
    public PlayerInputActions playerControls;
    private InputAction attack;
    private InputAction jump;
    private InputAction sprint;

    //attacks
    public GameObject swordHitbox;
    private bool isAttacking;
    //FOR THE MOVEMENT:
    //new:
    public int speed;
    public Transform playerTransform;
    public float rotationSpeed;

    public Animator anim;
    //public Camera playerCamera;
    //public float walkSpeed = 6f;
    //public float runSpeed = 12f;
    public float jumpPower = 0f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight;
    public float crouchHeight;
    public float crouchSpeed = 3f;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private bool canMove = true;
    bool isSprint = false;
    public bool isSpinAttack = false;

    public GameObject animationSource;
    public GameObject thisGameObject;
    public bool canRotate;
   

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        speed = thisGameSave.playerSpeed;
        playerControls = new PlayerInputActions();
    }
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        anim = animationSource.GetComponent<Animator>();
        isAttacking = false;
        canRotate = true;
    }
    private void OnEnable() //need for input system
    {
        attack = playerControls.General.Attack;
        attack.Enable();

        jump = playerControls.General.Jump;
        jump.Enable();

        sprint = playerControls.General.Sprint;
        sprint.Enable();
    }

    private void OnDisable()//need for input system
    {
        attack.Disable();
        jump.Disable();
        sprint.Disable();
    }
    IEnumerator WaitUntil(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    public void EndAttack()
    {
        anim.SetBool("isAttacking", false);
        anim.SetBool("isSpinAttack", false);
        swordHitbox.SetActive(false);
        canMove = true;
        isAttacking = false;
        print("attack ended");
        canRotate = true;
    }

    public void Reposition()
    {
        //thisGameObject.transform.position = new Vector3 (animationSource.transform.position.x, transform.position.y, animationSource.transform.position.z);
        Vector3 currentPos = new Vector3(thisGameObject.transform.position.x, transform.position.y, thisGameObject.transform.position.z);
        Vector3 posOffset = new Vector3(animationSource.transform.position.x, transform.position.y, animationSource.transform.position.z) - thisGameObject.transform.position;
        transform.position = currentPos + posOffset;
    }

    public void SwordOn()
    {
        swordHitbox.SetActive(true);
    }
    public void SwordOff()
    {
        swordHitbox.SetActive(false);
    }

    void Update()
    {

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

       
        float curSpeedX = canMove ? (speed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (speed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        //for making the character face forwards
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 rotationDirection = new Vector3(horizontalInput, 0, verticalInput);
        rotationDirection.Normalize();

        if (moveDirection.x != 0)
        {
            anim.SetBool("isRun", true);
            canRotate = true;
        }

        if (moveDirection.z != 0)
        {
            anim.SetBool("isRun", true);
            canRotate = true;
        }

        if(moveDirection.x == 0 && moveDirection.y == 0 && moveDirection.z == 0)
        {
            anim.SetBool("isRun", false);
        }

        if (sprint.IsPressed())
        {
            speed = thisGameSave.sprintSpeed;
            isSprint = true;    
        }

        if (!sprint.IsPressed())
        {
            speed = thisGameSave.playerSpeed;
            isSprint = false;
        }

        if (attack.WasPressedThisFrame() && !isAttacking && !isSprint && thisGameSave.canAttack) //ATTACK
            {
            anim.SetBool("isAttacking", true);
            swordHitbox.SetActive(true);
            canMove = false;
            canRotate = false;
            isAttacking = true;
            }
        else if (attack.WasPressedThisFrame() && isAttacking && isSpinAttack) //cancel attack
        {
            anim.SetBool("isAttacking", false);
            swordHitbox.SetActive(false);
            canMove = true;
            isAttacking = false;
        }
       
        if (attack.WasPressedThisFrame() && isSprint && !isAttacking && thisGameSave.canAttack) //SPIN ATTACK
        {
            isSpinAttack = true;
            anim.SetBool("isSpinAttack", true);
            canMove = false;
            canRotate = false;
            isAttacking = true;
            Debug.Log("spinAttack");
        }

        if (jump.IsPressed() && canMove && characterController.isGrounded) //JUMP
        {
            if (thisGameSave.canJump == true)
            {
                moveDirection.y = jumpPower;
                anim.SetBool("isJump", true);
            }
            else if (thisGameSave.canJump == false)
            {
                moveDirection.y = 0;
                anim.SetBool("isJump", false);
                print("loser cant jump :(");
            }
            
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        if (characterController.isGrounded == true)
        {
            anim.SetBool("isJump", false);
        }

        characterController.Move(moveDirection * Time.deltaTime);


        if (rotationDirection != Vector3.zero && canRotate)
       {
           Quaternion toRotation = Quaternion.LookRotation(rotationDirection, Vector3.up);

            playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, toRotation, rotationSpeed * Time.deltaTime);
       }
    }
}
