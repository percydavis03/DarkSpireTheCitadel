using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation; //The object for the camera to orbit around
    public Transform player; //The Player controller
    public Transform playerOBJ; //The Player object
    public Rigidbody rB; //The Ridgidbody for the Player

    public float rotationSpeed; //The speed that the player will rotate
     //A Vector2 to track the input values
    // Start is called before the first frame update
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //Lock the cursor to the center of the screen
        Cursor.visible = false; //Make the cursor invisivble 
    }

    // Update is called once per frame
    void Update()
    {
        if (Player_Manager.GetInstance().canMove == true)
        {

            Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z); //A new Vector 3 that is created by taking the Player's position and subtracting that with another Vector3 that tracks the position of the Camera on the X and Z axis with the Y having to track the Player's position on the Y axis 
            orientation.forward = viewDir.normalized; //The Forward direction of the orientation object is determined by the normalized sum or the viewDir vector


            //Asign the value of the X vector with the Horizontal Input


            //Asing the value of the Y vector with the Vertical Input 
            Vector3 inputDir = (orientation.forward * Player_Manager.GetInstance().inputVector.y + orientation.right * Player_Manager.GetInstance().inputVector.x);
            

            if (inputDir != Vector3.zero && Player_Manager.GetInstance().canMove == true) //if the input direction is not zero for all the vecotrs 
                playerOBJ.forward = Vector3.Slerp(playerOBJ.forward, inputDir.normalized , Time.deltaTime * rotationSpeed); //The forward direction and rotation for the Player object is determined by player's roation interpling to the player objects new forward direction to the normalzied input direction with the speed determined by the rotation speed 
            
        }
    }
}
