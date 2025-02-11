using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerWindow : MonoBehaviour
{
    public int requiredCoffee = 1; // SHould be changed to a varible later potenitally
    private int currentCoffeeCount = 0; // Number of Coffees currently in the Window
    public CustomerScript customerScript; // Reference to the customer script



    public void GiveCoffeeToWindow(CoffeeInteraction coffee)
    {
        if (coffee != null)
        {
        

            currentCoffeeCount++;
            Destroy(coffee.gameObject);

   
            GameObject customer = GameObject.FindWithTag("Customer"); // Find the customer by tag
            if (customer != null)
            {
                customerScript = customer.GetComponent<CustomerScript>();
            }
            if (customerScript != null)
            {
                // Check if enough beans are present to create coffee
                if (currentCoffeeCount >= requiredCoffee)
                {
                    if (coffee.beanCount == customerScript.requiredBeans)
                    {
                        CustomerTakesCoffee();
                        Debug.Log("Coffee Given to window: Correct order");
                    }
                    else
                    {
                        Debug.Log("Coffee Given to window: Incorrect order");
                        LoseLifeForWrongOrder();
                    }


                    currentCoffeeCount = 0; // Reset beans for the next coffee
                }
            }
           
            else
            {
                Debug.Log("Received null coffee!");

            }



        }
        else {
            Debug.LogError("No coffee provided.");
        }
    }

    private void CustomerTakesCoffee()
    {


        if (customerScript != null)
        {
            customerScript.SetIsOrderedTrue(); // Mark the order as delivered

                Debug.Log("Customer acknowledged order and should move!");

            
        }
        else
        {
            Debug.LogError("Customer object not found!");
        }


    }
    private void LoseLifeForWrongOrder()
    {
        // Ensure you call this method to penalize the player for a wrong order
        PlayerHealth playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.LoseLife();  // Decrease the player's health
            Debug.Log("Player lost a life for wrong order");
        }
        else
        {
            Debug.LogError("PlayerHealth component not found!");
        }
    }
}
