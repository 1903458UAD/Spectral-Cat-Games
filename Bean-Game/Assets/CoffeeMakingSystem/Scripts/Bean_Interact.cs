using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BeanInteraction : MonoBehaviour
{
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

}
