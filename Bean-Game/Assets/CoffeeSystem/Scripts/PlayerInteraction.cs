using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 15f; // Distance within which the player can interact
    public LayerMask InteractableObjectLayer; // LayerMask to filter interactable objects
    public Transform cameraTransform; // Reference to the player's camera

    private void Update()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("cameraTransform is not assigned! Please assign the cameraTransform in the Inspector.");
            return;
        }

        if (InteractableObjectLayer == 0)
        {
            Debug.LogError("interactableObjects LayerMask is not assigned! Please assign a valid layer mask.");
            return;
        }

        Debug.Log("PlayerInteraction update function running");
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward); // Ray from the center of the screen

        RaycastHit hit;



        // Check if the ray hits something within interaction range
        if (Physics.Raycast(ray, out hit, interactionDistance, InteractableObjectLayer))
        {

            Debug.Log("Raycast hit object: " + hit.collider.gameObject.name);
            Debug.Log("raycast hit an object!");
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                Debug.Log("Object hit by raycast is interactable");


                
                if (Input.GetKeyDown(KeyCode.E)) // Pick up the object if 'e' is pressed
                {
                    Debug.Log(" Object Picked up!");
                    interactable.PickUpObject();

                    
                }
                else
                {
                    Debug.Log("Object is not interactable");
                }
            }
        }
        else
        {
            Debug.Log("Raycast did not hit anything");
        }
        //Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactionDistance, Color.green); //Doesnt really work only shows when game scene paused

    }


  


}
