using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] public Rigidbody rB;
    
    [Space]
    public float moveSpeed;
    [Space]
    public Transform oreintation;
    [Space]
    public Vector3 moveDir;
    [Header("Jumping")]
    [Space]
    public float jumpVelocity; //The amount of force of a jump
    [Space]
    public float fallMultiplier = 2.5f; //How fast the player will fall
    [Space]
    public float lowJumpMultiplier = 2f; //The smallest amount of jump force
    [Header("Ground Check")]
    [Space]
    public Transform groundCheck;
    [Space]
    public bool isGrounded;
    [Space]
    public LayerMask whatIsGround;
    [Space]
    public float checkRadius = 0.25f;
    [Space]
    public Vector2 inputVector;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Set up Ground Bool
        isGrounded = Physics.CheckSphere(groundCheck.position, checkRadius, whatIsGround);

       

        //Player Input 
        if (Player_Manager.GetInstance().canMove)
            inputVector = new Vector2(Player_Manager.GetInstance().inputVector.x, Player_Manager.GetInstance().inputVector.y).normalized;
        else
            inputVector = Vector2.zero;

      
        //Jump Logic
       

        //Velocity 
        //if (rB.velocity.y < 0)
        //{
        //    rB.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
         
        //}
        
        
    }

    private void FixedUpdate()
    {
        if (Player_Manager.GetInstance().canMove)
        {
            //Moving the Player
            moveDir = oreintation.forward * inputVector.y + oreintation.right * inputVector.x;

            if (Player_Manager.GetInstance().canMove == true)
                rB.velocity = new Vector3(moveDir.x * moveSpeed, rB.velocity.y, moveDir.z * moveSpeed);
        }
       
    }
}
