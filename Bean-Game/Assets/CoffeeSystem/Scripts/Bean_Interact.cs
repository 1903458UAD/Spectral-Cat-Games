using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BeanInteraction : MonoBehaviour
{
   
    public float interactionDistance = 1.5f; // Distance to interact with the bean (Pickup Range)
    public float holdDistance = 1f; //The Distance infront of the player for the bean to be suspended --To be adjusted after arm is implemented
    public float holdHeightOffset = 0.5f; // Offset to position the bean at the player's arm height -- To be Adjusted when crouching
    
    private bool isHeld = false; // Is the bean being held?
    private Rigidbody beanRigidbody; // Rigidbody of the bean
    private Transform player; // Reference to the player's transform
    private Collider playerCollider; // Reference the player's Collider to determine height of the bean suspension
    private bool isInteractable = true; // Is the bean interactable?

    private bool inContactWithCoffeeMachine = false; // Is the bean in contact with the coffee machine?
    private CoffeeMachine coffeeMachine; // Reference to the coffee machine it is touching


    // Expose the isHeld value with a public getter
    public bool IsHeld
    {
        get { return isHeld; }
    }

    // Start is called before the first frame update
    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player").transform; //Finding the player using the "Player" Tag
        playerCollider = player.GetComponent<Collider>(); //Get player height


        
        beanRigidbody = GetComponent<Rigidbody>();//Get the Rigidbody


        if (beanRigidbody == null)
        {
            Debug.LogError("No Rigidbody found on the bean!");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (isHeld)
        {

            // Drop the bean if Press 'E'
            if (Input.GetKeyDown(KeyCode.E))
            {
                ReleaseBean();
                return;
            }

           
            Vector3 holdPosition = player.position + player.forward * holdDistance;  //Update the bean's position to infront player (Later adjusted to be players hand)


            float playerMidHeight = playerCollider.bounds.center.y;
            holdPosition.y = playerMidHeight + holdHeightOffset; //Add offset 
            

            transform.position = holdPosition;
            transform.rotation = player.rotation;

            // Add bean to coffee machine if left-clicked while in contact
            if (inContactWithCoffeeMachine && coffeeMachine != null && Input.GetMouseButtonDown(0))
            {
                coffeeMachine.AddBean(this);
            }
        }
        else
        {
            
            if (isInteractable && player != null && Vector3.Distance(transform.position, player.position) <= interactionDistance && Input.GetKeyDown(KeyCode.E)) //Check if the player is withing interact distasnce and presses "E"
            {
                Debug.Log("Picked up Bean function triggered!");
                PickUpBean();
            }
        }
    }
    private void PickUpBean()
    {


        if (beanRigidbody != null)
        {
            beanRigidbody.isKinematic = true; // Disable physics
            beanRigidbody.useGravity = false;
        }

        isHeld = true; // Mark the bean as held
        Debug.Log("Bean picked up!");
    }

    public void ReleaseBean(bool destroyBean = false)
    {
        //Debug.Log("Release Bean Function Triggered");

            if (beanRigidbody != null)
            {
                beanRigidbody.isKinematic = false;
                beanRigidbody.useGravity = true;
            }

            isHeld = false; //Mark the bean as not held


            // If the bean is destroyed (added to the coffee machine), remove it completely
            if (destroyBean)
            {
                Destroy(gameObject);
            }
            else
            {
            Debug.Log("Bean Drop Triggered");

            transform.parent = null;
            // Temporarily make the bean uninteractable to prevent immediate re-pickup
            isInteractable = false;
            Invoke(nameof(MakeInteractable), 0.5f); // Re-enable interaction after a short delay


            //Debug.Log("Bean Drop Triggered");


        }

 
    }

    private void MakeInteractable()
    {
        isInteractable = true;
        Debug.Log("Bean is now interactable again!");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the bean is in contact with the coffee machine
        if (other.CompareTag("CoffeeMachine"))
        {
            inContactWithCoffeeMachine = true;
            coffeeMachine = other.GetComponent<CoffeeMachine>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Reset when the bean leaves the coffee machine
        if (other.CompareTag("CoffeeMachine"))
        {
            inContactWithCoffeeMachine = false;
            coffeeMachine = null;
        }
    }


}
