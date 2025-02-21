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
    private InputAction openInfoMenu;
    private InputAction openMainMenu;

    //attacks
    public GameObject swordHitbox;
    private bool isAttacking;
    public float spinDistance;
    //FOR THE MOVEMENT:
    //new:
    public int speed;
    public Transform playerTransform;
    public float rotationSpeed;

    public Animator anim;
    //public Camera playerCamera;
    //public float walkSpeed = 6f;
    //public float runSpeed = 12f;
    public float jumpPower;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight;
    public float crouchHeight;
    public float crouchSpeed = 3f;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    public bool canMove = true;
    bool isSprint = false;
    bool isJumping = false;
    public bool isSpinAttack = false;

    public GameObject animationSource;
    public GameObject thisGameObject;
    public bool canRotate;

    /*public GameObject cameraRoot;

    public Camera mainCamera;
    private float targetRotation = 0;
    private float rotationVelocity;
    public float RotationSmoothTime = 0.12f;*/

    public bool canTurn = true;
    private float prevRotation;




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
        openInfoMenu = playerControls.General.Inventory;
        openInfoMenu.Enable();

        openMainMenu = playerControls.General.PauseMenu;
        openMainMenu.Enable();

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
        openInfoMenu.Disable();
        openMainMenu.Disable();
    }
    IEnumerator WaitUntil(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    public void Die()
    {
        if (GameManager.instance.justDied == true)
        {
            GameManager.instance.justDied = false;
            //anim.SetBool("isDead", true);
        }

    }

    public void AttackFailSafe()
    {
       if(!canMove)
        {
            canMove = true;
        }

      if (canMove)
        {
            canMove = false;
        }
    }
    public void EndAttack()
    {
        anim.SetBool("isAttacking", false);
        anim.SetBool("isSpinAttack", false);
        swordHitbox.SetActive(false);
        canMove = true;
        isAttacking = false;
        canRotate = true;
    }
    public void GotHit()
    {
        anim.SetBool("isHurt", true);
    }

    public void Recover()
    {
        anim.SetBool("isHurt", false);
    }


    public void SwordOn()
    {
        swordHitbox.SetActive(true);
    }
    public void SwordOff()
    {
        swordHitbox.SetActive(false);
    }

    public void StopMoving()
    {
        moveDirection = Vector3.zero;
        speed = thisGameSave.playerSpeed;
    }
    void Update()
    {
        if (thisGameSave.inMenu)
        {
            canMove = false;
        }
        if (openInfoMenu.WasPressedThisFrame()) //INFO MENU
        {
            print("openmenu");
            MenuScript.instance.InfoMenu();
        }

        if (openMainMenu.WasPressedThisFrame())
        {
            MenuScript.instance.MainMenu();  //MAIN MENU
        }

        if (canMove && canTurn)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            float curSpeedX = canMove ? (speed) * Input.GetAxis("Vertical") : 0;
            float curSpeedY = canMove ? (speed) * Input.GetAxis("Horizontal") : 0;


            moveDirection = (forward * curSpeedX) + (right * curSpeedY);


        }

        if (!isAttacking)
        {
            canMove = true;
            canRotate = true;
            anim.SetBool("isAttacking", false);
        }
        float movementDirectionY = moveDirection.y;
        //for making the character face forwards

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 rotationDirection = new Vector3(horizontalInput, 0, verticalInput);
        rotationDirection.Normalize();


        if (horizontalInput + verticalInput != 0)
        {
            /*if (canTurn)
                targetRotation = Mathf.Atan2(horizontalInput, verticalInput) * Mathf.Rad2Deg +
                                  mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
                RotationSmoothTime);*/

            // rotate to face input direction relative to camera position
            //transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

            /*if (rotationDirection.z == -1)
            {
                canTurn = false;
                Invoke("ResetCanTurn", 0.75f);
            }*/
        }



        if (moveDirection.x != 0 && !isJumping)
        {
            anim.SetBool("isRun", true);
            canRotate = true;
        }

        if (moveDirection.z != 0 && !isJumping)
        {
            anim.SetBool("isRun", true);
            canRotate = true;
        }

        if (moveDirection.x == 0 && moveDirection.y == 0 && moveDirection.z == 0)
        {
            anim.SetBool("isRun", false);
        }

        if (sprint.IsPressed())  //SPRINT
        {
            speed = thisGameSave.sprintSpeed;
            isSprint = true;
        }

        if (!sprint.IsPressed())
        {
            speed = thisGameSave.playerSpeed;
            isSprint = false;
        }

        if (attack.WasPressedThisFrame() && !isAttacking && !isSprint && thisGameSave.canAttack && !thisGameSave.inMenu) //ATTACK
        {
            anim.SetBool("isAttacking", true);
           
            moveDirection = Vector3.zero;
            canRotate = false;
            isAttacking = true;
            canMove = false;
        }
        else if (attack.WasPressedThisFrame() && isAttacking && isSpinAttack) //cancel attack
        {
            anim.SetBool("isAttacking", false);
            swordHitbox.SetActive(false);
            canMove = true;
            isAttacking = false;
        }

        if (attack.WasPressedThisFrame() && isSprint && !isAttacking && thisGameSave.canAttack && !thisGameSave.inMenu) //SPIN ATTACK
        {
            isSpinAttack = true;
            anim.SetBool("isSpinAttack", true);
            canMove = false;
            canRotate = false;
            isAttacking = true;
            Debug.Log("spinAttack");
        }
        if (anim.GetBool("isHurt"))
        {
            canMove = true;
            Debug.Log("ishurting");
        }
        if (jump.IsPressed() && canMove && characterController.isGrounded) //JUMP
        {
            if (thisGameSave.canJump == true)
            {
                isJumping = true;
                print("jump");
                anim.SetBool("isJump", true);
                moveDirection.y = jumpPower;
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
            isJumping = false;
        }


        if (characterController.isGrounded == true)
        {
            anim.SetBool("isJump", false);
        }

        //Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
        characterController.Move(moveDirection * Time.deltaTime);

        
        if (rotationDirection != Vector3.zero && canRotate)
        {
            Quaternion toRotation = Quaternion.LookRotation(rotationDirection, Vector3.up);

            playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        
        //Update cameraRoot rotation
        //cameraRoot.transform.localRotation = playerTransform.rotation;

    }

    public void ResetCanTurn()
    {
        canTurn = true;
    }
}
