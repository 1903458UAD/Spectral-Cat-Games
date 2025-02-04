using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactionDistance = 5f;
    public LayerMask InteractableObjectLayer;
    public Transform cameraTransform;

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

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, InteractableObjectLayer))
        {
            GameObject hitObject = hit.collider.gameObject;
            UnityEngine.Debug.Log("[PlayerInteraction] Raycast hit: " + hitObject.name);

            InteractableObject interactable = hitObject.GetComponent<InteractableObject>();
            BeanInteraction bean = hitObject.GetComponent<BeanInteraction>();
            CoffeeInteraction coffee = hitObject.GetComponent<CoffeeInteraction>();

     

            if (interactable != null && Input.GetKeyDown(KeyCode.E))
            {
                if (interactable.GetIsHeld())
                {
                    interactable.ReleaseObject();
                }
                else
                {
                    interactable.PickUpObject();
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                bean?.TryAddToCoffeeMachine();
                coffee?.TryAddToCustomerWindow();
            }
        }
        else
        {
            UIManager.Instance.SetCrosshairDefault();
        }
    }
}
