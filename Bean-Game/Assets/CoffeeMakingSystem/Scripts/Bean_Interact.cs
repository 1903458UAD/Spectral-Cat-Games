using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BeanInteraction : MonoBehaviour
{
    private bool inContactWithCoffeeMachine = false; //To check if touching CoffeeMachine
    private CoffeeMachine coffeeMachine; //Reference to the CoffeeMachine that the bean is in contact with

    private void OnTriggerEnter(Collider other) //On colliding with another object
    {
        if (other.CompareTag("CoffeeMachine")) //Check if it has the tag CoffeeMachine
        {
            inContactWithCoffeeMachine = true; //Change contact with CoffeeMachine Boolean to true
            coffeeMachine = other.GetComponent<CoffeeMachine>(); //Store a reference to the CoffeeMAchine Script
        }
    }

    private void OnTriggerExit(Collider other) //On no longer colliding with another object
    {
        if (other.CompareTag("CoffeeMachine")) //if other object has coffeeMachine tag
        {
            inContactWithCoffeeMachine = false; //Assign Boolean for incontact to false
            coffeeMachine = null; // Remove the reference to the CoffeeMachine
        }
    }


    public void TryAddToCoffeeMachine() //Attempt to add coffee to the coffeeMachine
    {
        if (inContactWithCoffeeMachine && coffeeMachine != null) //If the bean is in contact with a/the CoffeeMachine and the reference is valid 
        {
           // Debug.Log("Adding bean to coffee machine!"); //Log to show bean has been added to the coffeeMachine, Commented out but not deleted incase needed again
            coffeeMachine.AddBean(this); //Call the addBean Function in the CoffeeMachine Script
        }
 
    }
    
}
