using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class InteractableObject : MonoBehaviour
{
    public float holdDistance = 1f; // Distance in front of the player for the object to hover
    public float holdHeightOffset = 0.0f; // Offset to position the object at player hand height
    public float pickupCooldown = 0.1f; // Minimum time before the object can be released
    private bool canRelease = true; // Determines if we can release the object


    private bool isHeld = false; // Is the object being held?
    private Rigidbody objectRigidbody; // Rigidbody of the object
    private Transform playerCamera; // Reference to the player's camera

    private NavMeshAgent navMeshAgent; // Reference to the NavMeshAgent
    private NPC_AI aiScript; // Reference to the NPC_AI script
    private Transform originalParent; // Original parent to restore on release

    private void Start()
    {

        // Find the camera dynamically
        playerCamera = Camera.main.transform;
  


        // Initialize references
        objectRigidbody = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        aiScript = GetComponent<NPC_AI>();

        if (objectRigidbody == null)
        {
            Debug.LogError("No Rigidbody found on the interactable object!");
        }


    }

    public bool GetIsHeld()
    {
        return isHeld;
    }

    public void PickUpObject()
    {
        if (isHeld)
        {
            return; // Prevent picking up the object again
        }
           

        // Store the original parent for restoration on release
        originalParent = transform.parent;

        // Disable AI and NavMeshAgent
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        if (aiScript != null)
            aiScript.enabled = false;


        // Parent the object to the player camera
        transform.SetParent(playerCamera);

        // Set a local position relative to the camera
        transform.localPosition = new Vector3(0, holdHeightOffset, holdDistance);
        transform.localRotation = Quaternion.identity;

        // Mark the object as held
        isHeld = true;

        // Disable Rigidbody physics while holding
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true;
            objectRigidbody.useGravity = false;
        }
        canRelease = false;
        Invoke(nameof(EnableRelease), pickupCooldown); // Set the cooldown before allowing release

        Debug.Log(gameObject.name + " picked up!");
    }


    private void EnableRelease()
    {
        canRelease = true;
    }
    public void ReleaseObject()
    {
        if (!isHeld || !canRelease) // Ensure release is allowed
        {
            return;
        }

        // Mark the object as no longer held
        isHeld = false;

        // Re-enable Rigidbody physics
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = false;
            objectRigidbody.useGravity = true;
        }

        // Unparent from the camera
        transform.SetParent(null, true); // Ensure the object is fully detached
     

        // Re-enable AI and NavMeshAgent
        if (navMeshAgent != null)
            navMeshAgent.enabled = true;

        if (aiScript != null)
            aiScript.enabled = true;

        canRelease = false;
        Invoke(nameof(EnableRelease), pickupCooldown);

        Debug.Log(gameObject.name + " released!");
    }
}
