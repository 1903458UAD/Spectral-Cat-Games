using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEditor;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private UpgradeData upgradeData;

    public float interactionDistance = 2f; //Default interaction distance for interactable objects (Might need some fine tuning for balancing)
    public LayerMask InteractableObjectLayer;
    public LayerMask FunctionalObjectLayer;
    public Transform cameraTransform;

    private InteractableObject heldObjectRight; // Right-hand object
    private InteractableObject heldObjectLeft;  // Left-hand object
    private bool isPickupBothHands; // Enable dual wielding

    private KeyCode Pickup_AND_Interact;// = KeyCode.Joystick1Button5; // Pickup keycode - used for responding to controller input - set to right bumper
    private KeyCode Drop;// = KeyCode.Joystick1Button4; // Pickup keycode - set to left bumper
    //private KeyCode interaction = KeyCode.Joystick1Button2; // Interaction keycode - set to 'Y' button

    private enum InputType { Controller, Keyboard }; // Enum - used to determine whether input is controller or keyboard - likely will move to GameManager in future!
    private InputType currentInput;

    [SerializeField] private EventReference pickupSound;

    private void Start()
    {
        isPickupBothHands = StaticData.dualWieldUpgrade;

        Pickup_AND_Interact = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("InteractKey", "Mouse0")); //Player pref saves over game sessions, It is also a new concept for me, Documentation: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerPrefs.html
        Drop = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DropKey", "Mouse1"));
    }

    public void UpdateKeybindings()
    {
        
        Pickup_AND_Interact = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("InteractKey", "Mouse0")); 
        Drop = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DropKey", "Mouse1"));
    }

    private void Update()
    {

        if (cameraTransform == null)
        {
            Debug.LogError("[PlayerInteraction] cameraTransform is not assigned!");
            return;
        }


        if (heldObjectLeft != null && Input.GetKeyDown(Drop)) 
        {
            heldObjectLeft.ReleaseObject(); //Call function to release object being held from left hand
            heldObjectLeft = null;// Clear reference after release
            return;
        }
        else if (heldObjectRight != null && Input.GetKeyDown(Drop))
        {
            heldObjectRight.ReleaseObject(); //Call function to release object being held from left hand
            heldObjectRight = null;// Clear reference after release
            return;

        }




        if (Input.GetKeyDown(Pickup_AND_Interact))
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 0.1f);


            if (Physics.Raycast(ray, out hit, interactionDistance, InteractableObjectLayer))
            {
                Debug.Log("RayCast Hit a Interactable Object");
                GameObject hitObject = hit.collider.gameObject;
                InteractableObject interactable = hitObject.GetComponent<InteractableObject>();


                if (interactable != null)
                {

                    if (heldObjectRight == null || heldObjectLeft == null)
                    {
                        AudioManager.instance.PlayOneShot(pickupSound, this.transform.position);
                    }

                    // Pick up object if hand is free
                    if (heldObjectRight == null)
                    {
                        interactable.PickUpObject(true);
                        heldObjectRight = interactable;
                        //return;
                    }
                    else if (heldObjectLeft == null && isPickupBothHands) // Allow left-hand pickup if dual-wielding is active
                    {
                        interactable.PickUpObject(false);
                        heldObjectLeft = interactable;
                        //return;
                    }
                }
            }
            

            else if (Physics.Raycast(ray, out hit, interactionDistance, FunctionalObjectLayer)) //-- Prioritise function over pick up
            {
                Debug.Log("RayCast Hit a functional Object");
                GameObject hitObject = hit.collider.gameObject;
                CoffeeMachine coffeeMachine = hitObject.GetComponent<CoffeeMachine>();
                CustomerWindow customerWindow = hitObject.GetComponent<CustomerWindow>();
                Till till = hitObject.GetComponent<Till>();
                ButtonForCoffeeMachine coffeeButton = hitObject.GetComponent<ButtonForCoffeeMachine>();

                if (coffeeButton != null)
                {
                    coffeeButton.PressButton();
                    Debug.Log("Pressed Coffee Machine Button");
                    return;
                }

                if (till != null)
                {
                    UIUpgradeManager.Instance.EnableUpgradeMenu();
                    Debug.Log("Pressed Till");
                }

                if (coffeeMachine != null)
                {
                    if (heldObjectRight)
                    {
                        heldObjectRight.GetComponent<BeanInteraction>().TryAddToCoffeeMachine(coffeeMachine);
                        Debug.Log("Called tryAddToCoffeeMachine (Right hand)");
                        return;
                    }
                    else if (heldObjectLeft)
                    {
                        heldObjectLeft.GetComponent<BeanInteraction>().TryAddToCoffeeMachine(coffeeMachine);
                        Debug.Log("Called tryAddToCoffeeMachine (Left hand)");
                        return;

                    }

                }

                if (customerWindow != null)
                {

                    if (heldObjectRight && heldObjectRight.GetComponent<CoffeeInteraction>())
                    {

                        heldObjectRight.GetComponent<CoffeeInteraction>().TryAddToCustomerWindow();
                        return;
                    }
                    else if (heldObjectLeft && heldObjectLeft.GetComponent<CoffeeInteraction>())
                    {

                        heldObjectLeft.GetComponent<CoffeeInteraction>().TryAddToCustomerWindow();

                        return;

                    }

                    return;
                }

            }

            else if (Physics.Raycast(ray, out hit, interactionDistance, InteractableObjectLayer))
            {
                Debug.Log("RayCast Hit a Interactable Object");
                GameObject hitObject = hit.collider.gameObject;
                InteractableObject interactable = hitObject.GetComponent<InteractableObject>();


                if (interactable != null)
                {

                    if (heldObjectRight == null || heldObjectLeft == null)
                    {
                        AudioManager.instance.PlayOneShot(pickupSound, this.transform.position);
                    }

                    // Pick up object if hand is free
                    if (heldObjectRight == null)
                    {
                        interactable.PickUpObject(true);
                        heldObjectRight = interactable;
                        //return;
                    }
                    else if (heldObjectLeft == null && upgradeData.internalUpgradeEnabled) // Allow left-hand pickup if dual-wielding is active
                    {
                        interactable.PickUpObject(false);
                        heldObjectLeft = interactable;
                        //return;
                    }
                }
            }
        }
    }
}

 

