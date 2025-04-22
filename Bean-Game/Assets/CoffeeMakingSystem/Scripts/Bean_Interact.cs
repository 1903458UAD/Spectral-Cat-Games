using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class BeanInteraction : MonoBehaviour
{
    private Hiding_Spots cage;
    private List<NPC_AI> npc = new List<NPC_AI>();

    public GameObject shatteredBeanPrefab;

    private GameObject spawnedShatter;

    //Beans are only added if held in the player's hand, Removed the Colliding Code
    public void TryAddToCoffeeMachine(CoffeeMachine coffeeMachine)
    {
        Debug.Log("BeanInteract: TRYING TO ADD A BEAN TO COFFEEMACHINE");
        if (coffeeMachine != null)
        {
            coffeeMachine.AddBean(this);
            Debug.Log("BeanInteract: CALLED ADD BEAN");
            
        }
       
    }

    public void ShatterBean()
    {
        if (shatteredBeanPrefab != null)
        {
            
            spawnedShatter = Instantiate(shatteredBeanPrefab, transform.position, transform.rotation);

            
            Rigidbody[] pieces = spawnedShatter.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody pieceRb in pieces)
            {
                pieceRb.AddExplosionForce(50f, transform.position, 1f);
            }

           
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.enabled = false;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider col in colliders)
            {
                col.enabled = false;
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }


    public void DestroyShatter()
    {
        {
            if (spawnedShatter != null)
            {
                Destroy(spawnedShatter);
            }

            Destroy(this.gameObject); 
        }
    }


    public void TryAddToCage(Hiding_Spots cage)
    {
        if (cage != null)
        {
            InteractableObject interactable = GetComponent<InteractableObject>();
            NPC_AI beanNPC = GetComponent<NPC_AI>();


            if (interactable != null && interactable.GetIsHeld())
            {
                interactable.ReleaseObject();
            }

            if (beanNPC != null)
            {
                beanNPC.SetHidingSpot(cage);
            }

            cage.AddBean(this);
        }

    }

}
