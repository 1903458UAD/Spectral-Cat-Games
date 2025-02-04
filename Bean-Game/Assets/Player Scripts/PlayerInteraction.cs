using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 2f;
    public LayerMask InteractableObjectLayer;
    public Transform cameraTransform;

   // private InteractableObject heldObject; // Track the currently held object
    private InteractableObject heldObjectRight; // Right-hand object
    private InteractableObject heldObjectLeft;  // Left-hand object
    public bool isPickupBothHands = false; // Enable dual wielding

    private KeyCode rightPickup = KeyCode.Joystick1Button5; // Pickup keycode - used for responding to controller input - set to right bumper
    private KeyCode leftPickup = KeyCode.Joystick1Button4; // Pickup keycode - set to left bumper
    private KeyCode interaction = KeyCode.Joystick1Button2; // Interaction keycode - set to 'Y' button

    private enum InputType { Controller, Keyboard }; // Enum - used to determine whether input is controller or keyboard - likely will move to GameManager in future!
    private InputType currentInput;

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
        if (heldObjectRight != null && Input.GetKeyDown(KeyCode.E) || heldObjectRight != null && Input.GetKeyDown(rightPickup))
        {
            heldObjectRight.ReleaseObject();
            heldObjectRight = null; // Clear reference after release
            return;
        }

        if (heldObjectLeft != null && Input.GetKeyDown(KeyCode.Q) || heldObjectLeft != null && Input.GetKeyDown(leftPickup)) // Use 'Q' or controller key to drop left-hand object
        {
            heldObjectLeft.ReleaseObject();
            heldObjectLeft = null;
            return;
        }

        if (currentInput == InputType.Keyboard && Input.GetMouseButtonDown(0) || currentInput == InputType.Controller && Input.GetKeyDown(interaction))
        {
            if (heldObjectRight != null)
            {
                BeanInteraction bean = heldObjectRight.GetComponent<BeanInteraction>();
                CoffeeInteraction coffee = heldObjectRight.GetComponent<CoffeeInteraction>();
                bean?.TryAddToCoffeeMachine();
                coffee?.TryAddToCustomerWindow();
            }

            if (heldObjectLeft != null)
            {
                BeanInteraction bean = heldObjectLeft.GetComponent<BeanInteraction>();
                CoffeeInteraction coffee = heldObjectLeft.GetComponent<CoffeeInteraction>();
                bean?.TryAddToCoffeeMachine();
                coffee?.TryAddToCustomerWindow();
            }
        }

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, InteractableObjectLayer))
        {
            GameObject hitObject = hit.collider.gameObject;
            

            InteractableObject interactable = hitObject.GetComponent<InteractableObject>();
            BeanInteraction bean = hitObject.GetComponent<BeanInteraction>();
            CoffeeInteraction coffee = hitObject.GetComponent<CoffeeInteraction>();
          

            if (interactable != null)
            {


                //if (Input.GetMouseButtonDown(0))
                //{
                //    bean?.TryAddToCoffeeMachine();
                //    coffee?.TryAddToCustomerWindow();
                //}

                UIManager.Instance.SetCrosshairInteractable();
                UnityEngine.Debug.Log("[PlayerInteraction] Raycast hit: " + hitObject.name);

                if (heldObjectRight == null && Input.GetKeyDown(KeyCode.E) || heldObjectRight == null && Input.GetKeyDown(rightPickup))
                {
                    interactable.PickUpObject(true); // Right side
                    heldObjectRight = interactable;
                    return;
                }

                // Pick up second object (Left Hand) using 'Q' if both-hands mode is active
                if (isPickupBothHands && heldObjectLeft == null && Input.GetKeyDown(KeyCode.Q) || isPickupBothHands && heldObjectLeft == null && Input.GetKeyDown(leftPickup))
                {
                    Debug.Log("[PlayerInteraction] Attempting to pick up object in left hand...");
                    interactable.PickUpObject(false); // Left side
                    heldObjectLeft = interactable;
                    return;
                }



            }
            else
            {
                UIManager.Instance.SetCrosshairDefault();
            }


        }
        else
        {
            UIManager.Instance.SetCrosshairDefault();
        }

    }
}
