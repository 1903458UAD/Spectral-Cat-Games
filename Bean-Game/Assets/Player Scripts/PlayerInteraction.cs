using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 2f; //Default interaction distance for interactable objects (Might need some fine tuning for balancing)
    public LayerMask InteractableObjectLayer;
    public Transform cameraTransform;

    private InteractableObject heldObjectRight; // Right-hand object
    private InteractableObject heldObjectLeft;  // Left-hand object
    private bool isPickupBothHands; // Enable dual wielding

    private KeyCode rightPickup = KeyCode.Joystick1Button5; // Pickup keycode - used for responding to controller input - set to right bumper
    private KeyCode leftPickup = KeyCode.Joystick1Button4; // Pickup keycode - set to left bumper
    private KeyCode interaction = KeyCode.Joystick1Button2; // Interaction keycode - set to 'Y' button

    private enum InputType { Controller, Keyboard }; // Enum - used to determine whether input is controller or keyboard - likely will move to GameManager in future!
    private InputType currentInput;

    private void Start()
    {
        isPickupBothHands = StaticData.dualWieldUpgrade;
    }

    private void Update()
    {
        if (cameraTransform == null)
        {
            UnityEngine.Debug.LogError("[PlayerInteraction] cameraTransform is not assigned! Assign it in the Inspector.");
            return;
        }

        if (InteractableObjectLayer == 0)
        {
            UnityEngine.Debug.LogError("[PlayerInteraction] InteractableObjectLayer is not assigned! Assign a valid layer mask.");
            return;
        }


        // Check if player is holding an object and presses 'E' or controller key to release it
        if (heldObjectRight != null && Input.GetMouseButtonDown(0) || heldObjectRight != null && Input.GetKeyDown(rightPickup))
        {
            heldObjectRight.ReleaseObject();//Call function to release object being held from right hand
            heldObjectRight = null; // Clear reference after release
            return;
        }

        if (heldObjectLeft != null && Input.GetMouseButtonDown(1) || heldObjectLeft != null && Input.GetKeyDown(leftPickup)) // Use 'Q' or controller key to drop left-hand object
        {
            heldObjectLeft.ReleaseObject(); //Call function to release object being held from left hand
            heldObjectLeft = null;// Clear reference after release
            return;
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(interaction))
        {
            if (heldObjectRight != null)//If the player is holding an object in the right hand
            {
                BeanInteraction bean = heldObjectRight.GetComponent<BeanInteraction>(); //Get the BeanInteraction Component (If it has one) from the object being held
                CoffeeInteraction coffee = heldObjectRight.GetComponent<CoffeeInteraction>(); //Get the CoffeeInteraction Component (If it has one) from the object being held
                bean?.TryAddToCoffeeMachine(); //If it has a beanInteraction Component then call the 'TryAddToCoffeeMachine' function from the bean interaction script
                coffee?.TryAddToCustomerWindow();//If it has a CoffeeInteraction Component then call the 'TryAddToCustomerWindow' function from the bean interaction script
            }

            if (heldObjectLeft != null)//If the player is holding an object in the left hand
            {
                BeanInteraction bean = heldObjectLeft.GetComponent<BeanInteraction>();//Get the BeanInteraction Component (If it has one) from the object being held
                CoffeeInteraction coffee = heldObjectLeft.GetComponent<CoffeeInteraction>();//Get the CoffeeInteraction Component (If it has one) from the object being held
                bean?.TryAddToCoffeeMachine();//If it has a beanInteraction Component then call the 'TryAddToCoffeeMachine' function from the bean interaction script
                coffee?.TryAddToCustomerWindow();//If it has a CoffeeInteraction Component then call the 'TryAddToCustomerWindow' function from the bean interaction script
            }
        }


        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward); //Creates a ray from the players camera traveling forward
        RaycastHit hit; //store information from the object hit by the raycast

        if (Physics.Raycast(ray, out hit, interactionDistance, InteractableObjectLayer)) //If ray hits an object within the interactable layer (Within interactable range)
        {
            GameObject hitObject = hit.collider.gameObject; //Store the object hit
            
            InteractableObject interactable = hitObject.GetComponent<InteractableObject>(); //Get the IneractableObject component from the object that was hit (If it has one)
            BeanInteraction bean = hitObject.GetComponent<BeanInteraction>();//Get the BeanInteraction component from the object that was hit (If it has one)
            CoffeeInteraction coffee = hitObject.GetComponent<CoffeeInteraction>(); //Get the CoffeeInteraction component from the object that was hit (If it has one)


            if (interactable != null)// If the hit object is an interactable object
            {

                UIManager.Instance.SetCrosshairInteractable(); //Changes color of the crosshair to indicate an interactable object
                //UnityEngine.Debug.Log("[PlayerInteraction] Raycast hit: " + hitObject.name); //Commented as only for bug testing, checks what the raycast hit

                 if (heldObjectRight == null && Input.GetMouseButtonDown(0) || heldObjectRight == null && Input.GetKeyDown(rightPickup)) //If the right hand is empty and E is pressed
                {
                    interactable.PickUpObject(true); // call pickup object function ('true' indicates its the right hand)
                    heldObjectRight = interactable;// store reference of the object in 'heldOjectRight'
                    return; //Exit the method to prevent any more code being ran
                }

                // Pick up second object (Left Hand) using 'Q' if both-hands mode is active
                if (isPickupBothHands && heldObjectLeft == null && Input.GetMouseButtonDown(1) || isPickupBothHands && heldObjectLeft == null && Input.GetKeyDown(leftPickup))
                {
                    Debug.Log("[PlayerInteraction] Attempting to pick up object in left hand...");
                    interactable.PickUpObject(false); // call pickup object function ('false' indicates its the left hand)
                    heldObjectLeft = interactable;// store reference of the object in 'heldOjectLeft'
                    return;//Exit the method to prevent any more code being ran 
                }

            }
            else//If raycst doesnt detect an interactable object
            {
                UIManager.Instance.SetCrosshairDefault(); //set the Crosshair to default to indicate no interactablity optional (This one in theory shouldnt trigger but is a safety net incase an object is in interaction layer but not have that component)
            }


        }
        else //If the raycast doesnt hit an object in the interaction layer
        {
            UIManager.Instance.SetCrosshairDefault();//set the Crosshair to default to indicate no interactablity optional
        }

    }
}
