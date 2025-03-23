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
