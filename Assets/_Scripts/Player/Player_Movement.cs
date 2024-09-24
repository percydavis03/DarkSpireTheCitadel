using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    public static Player_Movement instance;
    public PlayerSaveState thisGameSave;



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

    public GameObject animationSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        speed = thisGameSave.playerSpeed;
    }
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        anim = animationSource.GetComponent<Animator>();


    }

    void Update()
    {
        speed = thisGameSave.playerSpeed;

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
            print("im runnin");
            anim.SetBool("isRun", true);
        }

        if (moveDirection.z != 0)
        {
            print("im runnin");
            anim.SetBool("isRun", true);
        }

        if(moveDirection.x == 0 && moveDirection.y == 0 && moveDirection.z == 0)
        {
            print("chill out time");
            anim.SetBool("isRun", false);
        }


        if (Input.GetKey(KeyCode.Space) && canMove && characterController.isGrounded)
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


        if (rotationDirection != Vector3.zero)
       {
           Quaternion toRotation = Quaternion.LookRotation(rotationDirection, Vector3.up);

            playerTransform.rotation = Quaternion.RotateTowards(playerTransform.rotation, toRotation, rotationSpeed * Time.deltaTime);
       }
    }
}
