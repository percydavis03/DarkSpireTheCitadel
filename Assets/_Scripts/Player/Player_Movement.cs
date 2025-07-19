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
    private InputAction roll;

    //attacks
    public GameObject swordHitbox;
    private bool isAttacking;
    private bool isSlashing;
    public float spinDistance;

    //combo system
    [Header("Combo System")]
    public bool isComboing = false;
    public int comboCount = 0;
    public float comboWindow = 1.5f; // Time window to continue combo
    private Coroutine comboResetCoroutine;
    public bool canComboNext = false; // Simple flag to allow next combo attack

    [Header("Animation Management")]
    public string[] attackAnimations = {"isAttacking", "isComboing", "isSpinAttack", "AttackInt"};
    // 0: isAttacking, 1: isComboing, 2: isSpinAttack, 3: AttackInt (parameter name for int)

    //slash attack
    public GameObject gauntletHitbox;
    //FOR THE MOVEMENT:
    //new:
    public int speed;
    public Transform playerTransform;
    public float rotationSpeed;
    private bool rolling;

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

        roll = playerControls.General.Roll;
        roll.Enable();
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
        roll.Disable();
        
        // Reset combo when script is disabled
        ResetCombo();
    }
    IEnumerator WaitUntil(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        anim.SetBool("isHurt", false);
        anim.SetBool("isAttacking", false);
        EndAttack();
    }

    IEnumerator ComboTimeout()
    {
        yield return new WaitForSeconds(comboWindow);
        // Combo window expired, reset combo
        ResetCombo();
    }

    IEnumerator EnableComboAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isComboing && comboCount < 3)
        {
            canComboNext = true;
            Debug.Log($"Combo window opened for attack {comboCount + 1}");
        }
    }

    public void ResetCombo()
    {
        isComboing = false;
        comboCount = 0;
        canComboNext = false;
        
        // Update animator using array like Sam's approach
        anim.SetBool(attackAnimations[1], false); // isComboing
        anim.SetInteger(attackAnimations[3], 0);   // AttackInt
        
        // Stop any running combo timeout
        if (comboResetCoroutine != null)
        {
            StopCoroutine(comboResetCoroutine);
            comboResetCoroutine = null;
        }
    }

    // Call this from animation events or automatically to allow next attack
    public void EnableComboNext()
    {
        canComboNext = true;
    }


    public void Die()
    {
        if (GameManager.instance.justDied == true)
        {
            GameManager.instance.justDied = false;
            ResetCombo(); // Reset combo when dying
            //anim.SetBool("isDead", true);
        }

    }

    public void FailSafe()
    {
        StartCoroutine("WaitUntil", 0.5f);
        ResetCombo(); // Reset combo on failsafe
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
        anim.SetBool(attackAnimations[0], false); // isAttacking
        anim.SetBool(attackAnimations[2], false); // isSpinAttack
        swordHitbox.SetActive(false);
        canMove = true;
        isAttacking = false;
        canRotate = true;
        isSpinAttack = false;
        
        // Don't reset combo here - let it continue or timeout naturally
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
        if (gauntletHitbox != null)
        {
            gauntletHitbox.SetActive(true);
        }
    }

    public void GauntletOff()
    {
        if (gauntletHitbox != null)
        {
            gauntletHitbox.SetActive(false);
        }
    }
    public void StopMoving()
    {
        moveDirection = Vector3.zero;
        speed = thisGameSave.playerSpeed;
    }

    public void StopRolling()
    {
        rolling = false;
        anim.SetBool("isRolling", false);
        Main_Player.instance.canTakeDamage = true;
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
            anim.SetBool("isWalk", false);
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
            anim.SetBool("isWalk", false);
        }
       
        if (openInfoMenu.WasPressedThisFrame()) //INFO MENU
        {
            print("openmenu");
            anim.SetBool("isRun", false);
            anim.SetBool("isWalk", false);
            MenuScript.instance.InfoMenu();

        }

        if (openMainMenu.WasPressedThisFrame())
        {
            anim.SetBool("isRun", false);
            anim.SetBool("isWalk", false);
            MenuScript.instance.MainMenu();  //MAIN MENU

        }

        HandleInputs();
        
        // Update attack states like Sam's approach
        UpdateAttackStates();
        
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
        // Roll Dodge
        if (roll.IsPressed() && !rolling)
        {
            rolling = true;
            isAttacking = false;
            ResetCombo(); // Reset combo when rolling
            Main_Player.instance.canTakeDamage = false;
            anim.SetBool("isRolling", true);
        }
        
        // Safety check: ensure player can take damage when not rolling
        if (!rolling && Main_Player.instance != null && !Main_Player.instance.canTakeDamage)
        {
            Main_Player.instance.canTakeDamage = true;
        }
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
            anim.SetBool("isWalk", false);
            return;
        }
        
        // Use currentVelocity magnitude with a small threshold for more responsive detection
        bool isMoving = currentVelocity.magnitude > 0.05f;
        bool isGrounded = characterController.isGrounded;
        
        if (isMoving && isGrounded && !isJumping)
        {
            if (isSprint)
            {
                // Running animation when sprinting
                anim.SetBool("isWalk", false);
                anim.SetBool("isRun", true);
            }
            else
            {
                // Walking animation when not sprinting
                anim.SetBool("isRun", false);
                anim.SetBool("isWalk", true);
            }
        }
        else
        {
            // Not moving or not grounded - disable both
            anim.SetBool("isRun", false);
            anim.SetBool("isWalk", false);
        }
    }

    public void ResetCanTurn()
    {
        canTurn = true;
    }

    // Helper method to safely set animator bool parameters
    private void SafeSetAnimatorBool(string paramName, bool value)
    {
        if (anim != null)
        {
            // Check if parameter exists before trying to set it
            foreach (AnimatorControllerParameter param in anim.parameters)
            {
                if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
                {
                    anim.SetBool(paramName, value);
                    return;
                }
            }
            // If we reach here, parameter doesn't exist - log a warning only once
            if (paramName == "isArmAttack")
            {
                Debug.LogWarning($"Animator parameter '{paramName}' not found - arm attack functionality may be disabled");
            }
        }
    }

    private void HandleAttackInputs()
    {
        if (slash.WasPressedThisFrame()  && !isSlashing && thisGameSave.hasArm ) //slash attack
        {
            print("slash attack");
            SafeSetAnimatorBool("isArmAttack", true);
            moveDirection = Vector3.zero;
            canRotate = false;
            isSlashing = true;
            canMove = false;
        }
        else if (slash.WasPressedThisFrame() && isSlashing) //cancel slash
        {
            SafeSetAnimatorBool("isArmAttack", false);
            if (gauntletHitbox != null)
            {
                gauntletHitbox.SetActive(false);
            }
            canMove = true;
            isSlashing = false;
            canRotate = true;
        }

        // SIMPLE COMBO ATTACK SYSTEM
        if (attack.WasPressedThisFrame() && thisGameSave.canAttack && !thisGameSave.inMenu)
        {
            // Handle sprint attack (spin attack) - takes priority
            if (isSprint && !isAttacking)
            {
                // Reset any existing combo for spin attack
                ResetCombo();
                
                isSpinAttack = true;
                anim.SetBool(attackAnimations[2], true); // isSpinAttack
                SetAttackState();
                Debug.Log("spinAttack");
                return;
            }

            // Simple combo system - allow attack when not attacking OR when can combo next
            if ((!isAttacking && !isSprint) || (isComboing && canComboNext && comboCount < 3))
            {
                // Start or continue combo
                comboCount++;
                if (comboCount > 3) comboCount = 1; // Reset to 1 after 3
                
                isComboing = true;
                canComboNext = false; // Reset flag after using it
                
                // Update animator
                anim.SetBool(attackAnimations[1], true);     // isComboing  
                anim.SetInteger(attackAnimations[3], comboCount); // AttackInt
                anim.SetBool(attackAnimations[0], true);     // isAttacking
                SafeSetAnimatorBool("isArmAttack", false);
                
                // Set attack state
                SetAttackState();
                
                // Start/restart combo timer
                if (comboResetCoroutine != null)
                {
                    StopCoroutine(comboResetCoroutine);
                }
                comboResetCoroutine = StartCoroutine(ComboTimeout());
                
                Debug.Log($"Combo attack {comboCount}");
            }
            else if (isAttacking && isSpinAttack) // Cancel spin attack
            {
                anim.SetBool(attackAnimations[0], false); // isAttacking
                anim.SetBool(attackAnimations[2], false); // isSpinAttack
                ClearAttackState();
                isSpinAttack = false;
            }
        }

        // Handle hurt state
        if (anim.GetBool("isHurt"))
        {
            canMove = true;
            GauntletOff();
            SafeSetAnimatorBool("isArmAttack", false);
            ResetCombo(); // Reset combo when hurt
            Debug.Log("ishurting");
        }
    }



    private void SetAttackState()
    {
        // Centralized attack state setup like Sam's approach
        StartCoroutine("WaitUntil", 2f);
        moveDirection = Vector3.zero;
        canRotate = false;
        isAttacking = true;
        canMove = false;
        
        // Enable combo after a short delay (allows for combo timing)
        StartCoroutine(EnableComboAfterDelay(0.3f));
    }

    private void ClearAttackState()
    {
        // Centralized attack state cleanup
        canMove = true;
        isAttacking = false;
        canRotate = true;
        swordHitbox.SetActive(false);
    }

    private void UpdateAttackAnimations()
    {
        // Update attack-related animations based on current state
        if (isAttacking)
        {
            if (!canMove)
            {
                canMove = false; // Ensure movement is disabled during attacks
            }
        }
    }

    // State checking methods like Sam's approach
    public bool IsInAttackState()
    {
        return isAttacking || isSlashing || rolling;
    }

    public bool CanPerformAction()
    {
        return canMove && !IsInAttackState() && !thisGameSave.inMenu;
    }



    private void UpdateAttackStates()
    {
        // Simple state management
        if (isAttacking)
        {
            canMove = false;
        }
    }
}
