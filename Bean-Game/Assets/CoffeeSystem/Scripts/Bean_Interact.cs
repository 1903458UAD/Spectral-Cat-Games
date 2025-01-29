using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BeanInteraction : MonoBehaviour
{
    private bool inContactWithCoffeeMachine = false;
    private CoffeeMachine coffeeMachine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CoffeeMachine"))
        {
            inContactWithCoffeeMachine = true;
            coffeeMachine = other.GetComponent<CoffeeMachine>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CoffeeMachine"))
        {
            inContactWithCoffeeMachine = false;
            coffeeMachine = null;
        }
    }


    public void TryAddToCoffeeMachine()
    {
        if (inContactWithCoffeeMachine && coffeeMachine != null)
        {
            Debug.Log("Adding bean to coffee machine!");
            coffeeMachine.AddBean(this);
        }
        else
        {
            Debug.Log("Bean is not near a coffee machine!");
        }
    }
    
}
