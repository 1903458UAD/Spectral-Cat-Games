using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RunAway : MonoBehaviour
{
   
    public float interactionDistance = 1.5f; // Distance to interact with the bean (Pickup Range)
    public float holdDistance = 1f; //The Distance infront of the player for the bean to be suspended --To be adjusted after arm is implemented
    public float holdHeightOffset = 0.5f; // Offset to position the bean at the player's arm height -- To be Adjusted when crouching
    private bool isHeld = false; // Is the bean being held?
    private Rigidbody beanRigidbody; // Rigidbody of the bean
    private RunAway runAway; // Reference to the bean's AI behavior script "NPC_AI"
    private Transform player; // Reference to the player's transform
    private Collider playerCollider; // Reference the player's Collider to determine height of the bean suspension


    // Start is called before the first frame update
    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player").transform; //Finding the player using the "Player" Tag
        playerCollider = player.GetComponent<Collider>(); //Get player height


        
        beanRigidbody = GetComponent<Rigidbody>();//Get the Rigidbody
        runAway = GetComponent<RunAway>(); //Reference to the "NPC_AI" Script

    }

    // Update is called once per frame
    void Update()
    {
        if (isHeld)
        {


            
            if (Input.GetKeyDown(KeyCode.E)) //Check if the player presses "E" again to drop the bean
            {
                ReleaseBean();
                return;
            }


           
            Vector3 holdPosition = player.position + player.forward * holdDistance;  //Update the bean's position to infront player (Later adjusted to be players hand)


            float playerMidHeight = playerCollider.bounds.center.y;
            holdPosition.y = playerMidHeight + holdHeightOffset; //Add offset 
            

            transform.position = holdPosition;
            transform.rotation = player.rotation;
        }
        else
        {
            
            if (player != null && Vector3.Distance(transform.position, player.position) <= interactionDistance && Input.GetKeyDown(KeyCode.E)) //Check if the player is withing interact distasnce and presses "E"
            {
                PickUpBean();
            }
        }
    }
    private void PickUpBean()
    {
       
        if (runAway != null)  //Disable the bean's AI script and physics to stop it from running away
        {
            runAway.enabled = false;
        }
            

        if (beanRigidbody != null)
        {
            beanRigidbody.isKinematic = true; // Disable physics
            beanRigidbody.useGravity = false;
        }

        isHeld = true; // Mark the bean as held
    }

    public void ReleaseBean()
    {
       
        if (runAway != null) //Enable the bean's AI and physics
        { 
            runAway.enabled = true; 
        }

        if (beanRigidbody != null)
        {
            beanRigidbody.isKinematic = false;
            beanRigidbody.useGravity = true;
        }

        isHeld = false; //Mark the bean as not held
       
        //Drop the bean slightly in front of the player
        transform.position = player.position + player.forward * 0.5f + new Vector3(0f, 0.27f,0f); //Offest the height to 0.27 so bean doesnt get dropped halfway through the floor


    }

}
