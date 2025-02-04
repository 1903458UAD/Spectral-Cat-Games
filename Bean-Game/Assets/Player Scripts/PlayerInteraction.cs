using System.Collections;
using System.Collections.Generic;
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


        // Check if player is holding an object and presses 'E' to release it
        if (heldObjectRight != null && Input.GetKeyDown(KeyCode.E))
        {
            heldObjectRight.ReleaseObject();
            heldObjectRight = null; // Clear reference after release
            return;
        }

        if (heldObjectLeft != null && Input.GetKeyDown(KeyCode.Q)) // Use 'Q' to drop left-hand object
        {
            heldObjectLeft.ReleaseObject();
            heldObjectLeft = null;
            return;
        }

        if (Input.GetMouseButtonDown(0))
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

                if (heldObjectRight == null && Input.GetKeyDown(KeyCode.E))
                {
                    interactable.PickUpObject(true); // Right side
                    heldObjectRight = interactable;
                    return;
                }

                // Pick up second object (Left Hand) using 'Q' if both-hands mode is active
                if (isPickupBothHands && heldObjectLeft == null && Input.GetKeyDown(KeyCode.Q))
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
