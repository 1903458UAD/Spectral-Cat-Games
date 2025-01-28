using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeInteraction : MonoBehaviour
{

    private bool inContactWithCustomerWindow = false;
    private CustomerWindow customerWindow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CustomerWindow"))
        {
            inContactWithCustomerWindow = true;
            customerWindow = other.GetComponent<CustomerWindow>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CustomerWindow"))
        {
            inContactWithCustomerWindow = false;
            customerWindow = null;
        }
    }

    //public float interactionDistance = 1.5f; // Distance to interact with the Coffee cup (Pickup Range)
    //public float holdDistance = 1f; //The Distance infront of the player for the Coffee cup to be suspended --To be adjusted after arm is implemented
    //public float holdHeightOffset = 0.5f; // Offset to position the Coffee cup at the player's arm height -- To be Adjusted when crouching

    //private bool isHeld = false; // Is the Coffee being held?
    //private Rigidbody coffeeRigidbody; // Rigidbody of the Coffee cup
    //private Transform player; // Reference to the player's transform
    //private Collider playerCollider; // Reference the player's Collider to determine height of the Coffee cup suspension
    //private bool isInteractable = true; // Is the Coffee cup interactable?

    //private bool inContactWithCustomerWindow = false; // Is the Coffee cup in contact with the customer Window?
    //private CustomerWindow customerWindow; // Reference to the customer Window it is touching

    //// Expose the isHeld value with a public getter
    //public bool IsHeld
    //{
    //    get { return isHeld; }
    //}




    //// Start is called before the first frame update
    //void Start()
    //{

    //    player = GameObject.FindGameObjectWithTag("Player").transform; //Finding the player using the "Player" Tag
    //    playerCollider = player.GetComponent<Collider>(); //Get player height
    //    coffeeRigidbody = GetComponent<Rigidbody>();//Get the Rigidbody


    //    if (coffeeRigidbody == null)
    //    {
    //        Debug.LogError("No Rigidbody found on the Coffee cup!");
    //    }

    //}



    //// Update is called once per frame
    //void Update()
    //{
    //    if (isHeld)
    //    {

    //        // Drop the Coffee if Press 'E'
    //        if (Input.GetKeyDown(KeyCode.E))
    //        {
    //            ReleaseCoffee();
    //            return;
    //        }


    //        Vector3 holdPosition = player.position + player.forward * holdDistance;  //Update the Coffee's position to infront player (Later adjusted to be players hand)


    //        float playerMidHeight = playerCollider.bounds.center.y;
    //        holdPosition.y = playerMidHeight + holdHeightOffset; //Add offset 


    //        transform.position = holdPosition;
    //        transform.rotation = player.rotation;

    //        // Add Coffee to customer if left-clicked while in contact
    //        if (inContactWithCustomerWindow && customerWindow != null && Input.GetMouseButtonDown(0))
    //        {
    //            customerWindow.GiveCoffeeToWindow(this);
    //        }
    //    }
    //    else
    //    {

    //        if (isInteractable && player != null && Vector3.Distance(transform.position, player.position) <= interactionDistance && Input.GetKeyDown(KeyCode.E)) //Check if the player is withing interact distasnce and presses "E"
    //        {
    //            Debug.Log("Picked up Coffee function triggered!");
    //            PickUpCoffee();
    //        }
    //    }
    //}
    //private void PickUpCoffee()
    //{


    //    if (coffeeRigidbody != null)
    //    {
    //        coffeeRigidbody.isKinematic = true; // Disable physics
    //        coffeeRigidbody.useGravity = false;
    //    }

    //    isHeld = true; // Mark the Coffee as held
    //    Debug.Log("Coffee picked up!");
    //}

    //public void ReleaseCoffee(bool destroyCoffee = false)
    //{
    //    //Debug.Log("Release Coffee Function Triggered");

    //    if (coffeeRigidbody != null)
    //    {
    //        coffeeRigidbody.isKinematic = false;
    //        coffeeRigidbody.useGravity = true;
    //    }

    //    isHeld = false; //Mark the Coffee as not held


    //    // If the Coffee is destroyed (added to the coffee machine), remove it completely
    //    if (destroyCoffee)
    //    {
    //        Destroy(gameObject);
    //    }
    //    else
    //    {
    //        Debug.Log("Coffee Drop Triggered");

    //        transform.parent = null;
    //        //Coffee uninteractable to prevent immediate re-pickup
    //        isInteractable = false;
    //        Invoke(nameof(MakeInteractable), 0.5f); // Re-enable interaction after a short delay


    //        //Debug.Log("Coffee Drop Triggered");


    //    }


    //}

    //private void MakeInteractable()
    //{
    //    isInteractable = true;
    //    Debug.Log("Coffee is now interactable again!");
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    // Check if the Coffee cup is in contact with the customer Window
    //    if (other.CompareTag("CustomerWindow"))
    //    {
    //        inContactWithCustomerWindow = true;
    //        customerWindow = other.GetComponent<CustomerWindow>();
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    // Reset when the Coffee leaves the coffee machine
    //    if (other.CompareTag("CustomerWindow"))
    //    {
    //        inContactWithCustomerWindow = false;
    //        customerWindow = null;
    //    }
    //}


}

