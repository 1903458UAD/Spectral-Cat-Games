using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.AI;
using FMOD.Studio;

[RequireComponent(typeof(Rigidbody))]
public class InteractableObject : MonoBehaviour
{
    public float holdDistance = 1f; // Distance in front of the player for the object to hover
    public float holdHeightOffset = 0.0f; // Offset to position the object at player hand height
    public float holdWidthOffset = 0.5f;//Offset the width (For making duel wielding pick up)
    public float pickupCooldown = 0.1f; // Minimum time before the object can be released
    public bool canRelease = true; // Determines if we can release the object


    public bool isHeld = false; // Is the object being held?
    private Rigidbody objectRigidbody; // Rigidbody of the object
    private Transform playerCamera; // Reference to the player's camera

    private NavMeshAgent navMeshAgent; // Reference to the NavMeshAgent
    private NPC_AI aiScript; // Reference to the NPC_AI script
    private Transform originalParent; // Original parent to restore on release

    private EventInstance beanFootsteps;

    private void Start()
    {

        // Find the camera dynamically
        playerCamera = GameObject.Find("First Person Camera").transform;

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

    public void PickUpObject(bool isRightHand)
    {
        if (isHeld)
        {
            return; // Prevent picking up the object again
        }

         if (aiScript != null)
            aiScript.OnPickedUp();

        //aiScript.beanFootsteps.stop(STOP_MODE.IMMEDIATE);
        // Store the original parent for restoration on release
        originalParent = transform.parent;

        // Disable AI and NavMeshAgent
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

       
            //aiScript.enabled = false;


        // Parent the object to the player camera
        transform.SetParent(playerCamera);

        float sideOffset = isRightHand ? 0.5f : -0.5f; // Right hand = 0.5f, Left hand = -0.5f

        Vector3 offsetPosition = playerCamera.position + (playerCamera.forward * holdDistance) + (playerCamera.right * sideOffset);


        // Set the object's position and rotation
        transform.position = offsetPosition;
        transform.rotation = Quaternion.identity;

        if (CompareTag("Bean"))
        {
            FacePlayer();
        }

        // Mark the object as held
        isHeld = true;

        // Disable Rigidbody physics while holding
        if (objectRigidbody != null)
        {
            objectRigidbody.isKinematic = true; //This the troublemaker right here!
            objectRigidbody.useGravity = false;

            // Ignore collisions with the player only
            Collider objectCollider = GetComponent<Collider>();
            GameObject player = GameObject.FindGameObjectWithTag("Player");


            if (player != null)
            {
                Collider playerCollider = player.GetComponent<Collider>();

                if (objectCollider != null && playerCollider != null)
                {
                    Physics.IgnoreCollision(objectCollider, playerCollider, true);
                }

            }
        }
        canRelease = true;
        Invoke(nameof(EnableRelease), pickupCooldown); // Set the cooldown before allowing release
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
            if(!gameObject.CompareTag("Bean"))
            {
                objectRigidbody.isKinematic = false;
            }

            objectRigidbody.useGravity = true;

            Collider objectCollider = GetComponent<Collider>();
            Collider playerCollider = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Collider>();

            if (objectCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(objectCollider, playerCollider, false);
            }
        }

        // Unparent from the camera
        transform.SetParent(null, true); // Ensure the object is fully detached
     
        // Re-enable AI and NavMeshAgent
        if (navMeshAgent != null)
            navMeshAgent.enabled = true;

        if (aiScript != null)
            aiScript.OnDropped();
            //aiScript.enabled = true;

        canRelease = false;
        Invoke(nameof(EnableRelease), pickupCooldown);

        //Debug.Log(gameObject.name + " released!"); //Commented out log incase needed again

    }

    private void FacePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 directionToPlayer = player.transform.position - transform.position;
            directionToPlayer.y = 0; 
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
    }
}
