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
            Debug.Log("Bean added! Current beans: " + currentCoffeeCount);

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

        GameObject customer = GameObject.FindWithTag("Customer"); // Find the customer (assumes it has the "Customer" tag)
        customer.GetComponent<CustomerScript>().setIsOrderedTrue(); // Call the method to set the order as delivered
        Debug.Log("Customer has Coffee!");
    }
}
