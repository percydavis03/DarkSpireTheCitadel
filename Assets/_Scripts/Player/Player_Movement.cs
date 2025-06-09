using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

public class Player_Movement : MonoBehaviour
{
    public static Player_Movement instance;
    public PlayerSaveState thisGameSave;
    //INPUTS
    public PlayerInputActions playerControls;
    private InputAction attack;
    private InputAction jump;
    private InputAction sprint;
    private InputAction openInfoMenu;
    private InputAction openMainMenu;
    private InputAction slash;
    private InputAction interact;

    //attacks
    public GameObject swordHitbox;
    private bool isAttacking;
    private bool isSlashing;
    public float spinDistance;

    //slash attack
    public GameObject gauntletHitbox;
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

    public Camera mainCamera;
    private float targetRotation = 0;
    private float rotationVelocity;
    public float RotationSmoothTime = 0.12f;

    public bool canTurn = true;
    private float prevRotation;

    [Header("Movement Settings")]
    public float accelerationSpeed = 10f;
    public float decelerationSpeed = 15f;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;

    public StudioEventEmitter attackSound;
    //arm
    public bool inPickupZone;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        canTurn = true;
        speed = thisGameSave.playerSpeed;
        playerControls = new PlayerInputActions();
    }
    void Start()
    {
        inPickupZone = false;
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

        slash = playerControls.General.Slash;
        slash.Enable();

        interact = playerControls.General.Interact;
        interact.Enable();
    }

    private void OnDisable()//need for input system
    {
        attack.Disable();
        jump.Disable();
        sprint.Disable();
        openInfoMenu.Disable();
        openMainMenu.Disable();
        slash.Disable();
        interact.Disable();
    }
    IEnumerator WaitUntil(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        anim.SetBool("isHurt", false);
        anim.SetBool("isAttacking", false);
        EndAttack();
    }
    public void Die()
    {
        if (GameManager.instance.justDied == true)
        {
            GameManager.instance.justDied = false;
            //anim.SetBool("isDead", true);
        }

    }

    public void FailSafe()
    {
        StartCoroutine("WaitUntil", 0.5f);
       if(!canMove)
        {
            canMove = true;
        }

      /*if (canMove)
        {
            canMove = false;
        }*/
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
    public void EndSlash()
    {
        anim.SetBool("isArmAttack", false);
        gauntletHitbox.SetActive(false);
        canMove = true;
        isSlashing = false;
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
        //attackSound.Play();
    }
    public void SwordOff()
    {
        swordHitbox.SetActive(false);
        canRotate = true;
    }

    public void GauntletOn()
    {
        gauntletHitbox.SetActive(true);
    }

    public void GauntletOff()
    {
        gauntletHitbox.SetActive(false);    
    }
    public void StopMoving()
    {
        moveDirection = Vector3.zero;
        speed = thisGameSave.playerSpeed;
    }

    public void ApplyKnockback(Vector3 knockbackForce)
    {
        // Apply the knockback force to the current movement
        moveDirection = knockbackForce;
        // Temporarily disable movement control but allow rotation
        canMove = false;
        canRotate = true;
        // Re-enable movement after a short delay
        StartCoroutine(EnableMovementAfterKnockback());
    }

    private IEnumerator EnableMovementAfterKnockback()
    {
        yield return new WaitForSeconds(0.3f);
        canMove = true;
        moveDirection = Vector3.zero;
    }
    private void FixedUpdate()
    {
        if (thisGameSave.inMenu)
        {
            moveDirection = new Vector3(0, 0, 0);
            anim.SetBool("isRun", false);
        }
    }
    void Update()
    {
       
        if(inPickupZone)
        {
            if(interact.IsPressed())
            {
                print("got arm");
                GetArm.instance.PickupArm();
            }
        }
        if (thisGameSave.inMenu)
        {
            canMove = false;
            anim.SetBool("isRun", false);
        }
       
        if (openInfoMenu.WasPressedThisFrame()) //INFO MENU
        {
            print("openmenu");
            anim.SetBool("isRun", false);
            MenuScript.instance.InfoMenu();

        }

        if (openMainMenu.WasPressedThisFrame())
        {
            anim.SetBool("isRun", false);
            MenuScript.instance.MainMenu();  //MAIN MENU

        }

        HandleInputs();
        
        if (canMove && canTurn && !thisGameSave.inMenu)
        {
            // Get input values
            float verticalInput = Input.GetAxis("Vertical");
            float horizontalInput = Input.GetAxis("Horizontal");

            // Get camera forward and right vectors
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;
            
            // Project vectors onto the horizontal plane
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate target velocity based on camera orientation
            targetVelocity = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized * speed;

            // Smoothly interpolate current velocity towards target velocity
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                Time.deltaTime * (targetVelocity.magnitude > 0.1f ? accelerationSpeed : decelerationSpeed)
            );

            // Apply movement
            moveDirection = new Vector3(currentVelocity.x, moveDirection.y, currentVelocity.z);
        }

        // Handle rotation
        if (moveDirection.magnitude > 0.1f && canRotate)
        {
            Vector3 rotationDirection = new Vector3(moveDirection.x, 0, moveDirection.z).normalized;
            if (rotationDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(rotationDirection, Vector3.up);
                playerTransform.rotation = Quaternion.RotateTowards(
                    playerTransform.rotation,
                    toRotation,
                    rotationSpeed * Time.deltaTime * 2f // Increased rotation speed for more responsive turning
                );
            }
        }

        // Handle gravity and jumping
        HandleGravityAndJump();

        // Apply final movement
        characterController.Move(moveDirection * Time.deltaTime);

        // Update animation states
        UpdateAnimations();
    }

    private void HandleInputs()
    {
        /*if (openInfoMenu.WasPressedThisFrame())
        {
            MenuScript.instance.InfoMenu();
        }

        if (openMainMenu.WasPressedThisFrame())
        {
            MenuScript.instance.MainMenu();
        }*/

        // Handle sprint
        if (sprint.IsPressed() && !isAttacking)
        {
            speed = thisGameSave.sprintSpeed;
            isSprint = true;
        }
        else
        {
            speed = thisGameSave.playerSpeed;
            isSprint = false;
        }

        // Handle attack inputs
        HandleAttackInputs();
    }

    private void HandleGravityAndJump()
    {
        if (jump.IsPressed() && canMove && characterController.isGrounded && thisGameSave.canJump)
        {
            isJumping = true;
            moveDirection.y = jumpPower;
            anim.SetBool("isJump", true);
        }
        else if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
            isJumping = false;
        }

        if (characterController.isGrounded)
        {
            anim.SetBool("isJump", false);
        }
    }

    private void UpdateAnimations()
    {
        if (thisGameSave.inMenu)
        {
            anim.SetBool("isRun", false);
            return;
        }
        // Use currentVelocity magnitude with a small threshold for more responsive detection
        bool isMoving = currentVelocity.magnitude > 0.05f;
        anim.SetBool("isRun", isMoving && !isJumping);
        if (!isMoving)
        {
            anim.SetBool("isRun", false);
        }
    }

    public void ResetCanTurn()
    {
        canTurn = true;
    }

    private void HandleAttackInputs()
    {
        if (slash.WasPressedThisFrame()  && !isSlashing && thisGameSave.hasArm ) //slash attack
        {
            print("slash attack");
            anim.SetBool("isArmAttack", true);
            moveDirection = Vector3.zero;
            canRotate = false;
            isSlashing = true;
            canMove = false;
        }
        else if (slash.WasPressedThisFrame() && isSlashing) //cancel slash
        {
            anim.SetBool("isArmAttack", false);
            gauntletHitbox.SetActive(false);
            canMove = true;
            isSlashing = false;
            canRotate = true;
        }
        if (attack.WasPressedThisFrame() && !isAttacking && !isSprint && thisGameSave.canAttack && !thisGameSave.inMenu) //ATTACK
        {
            anim.SetBool("isAttacking", true);
            anim.SetBool("isArmAttack", false);
            StartCoroutine("WaitUntil", 2f);
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
            canRotate = true;
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
            GauntletOff();
            anim.SetBool("isArmAttack", false);
            Debug.Log("ishurting");
        }
    }
}
