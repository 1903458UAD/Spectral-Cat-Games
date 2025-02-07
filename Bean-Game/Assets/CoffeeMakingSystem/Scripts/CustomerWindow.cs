using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerWindow : MonoBehaviour
{
    public int requiredCoffee = 1; // SHould be changed to a varible later potenitally
    private int currentCoffeeCount = 0; // Number of beans currently in the machine
    public CustomerScript customerScript; // Reference to the customer script



    public void GiveCoffeeToWindow(CoffeeInteraction coffee)
    {
        if (coffee != null)
        {
            // Increment the bean count and destroy the bean
            currentCoffeeCount++;
            Destroy(coffee.gameObject);
           

            // Check if enough beans are present to create coffee
            if (currentCoffeeCount >= requiredCoffee)
            {
                Debug.Log("Coffee Given to window");
                CustomerTakesCoffee();
                currentCoffeeCount = 0; // Reset beans for the next coffee
                 

            }
        }
    }

    private void CustomerTakesCoffee()
    {

        GameObject customer = GameObject.FindWithTag("Customer"); // Find the customer by tag
        if (customer != null)
        {
            CustomerScript customerScript = customer.GetComponent<CustomerScript>();
            if (customerScript != null)
            {
                customerScript.SetIsOrderedTrue(); // Ensure this is actually setting orderDelivered = true
                Debug.Log("Customer acknowledged order and should move!");
            }
        }
    }


}
