using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class CustomerWindow : MonoBehaviour
{
    public int requiredCoffee = 1; // SHould be changed to a varible later potenitally
    private int currentCoffeeCount = 0; // Number of Coffees currently in the Window
    public CustomerScript customerScript; // Reference to the customer script

    [SerializeField] private EventReference correctOrderFX;
    [SerializeField] private EventReference wrongOrderFX;

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
                if (currentCoffeeCount >= requiredCoffee && coffee.syrup == customerScript.requiredSyrup)
                {
                    if (coffee.beanCount == customerScript.requiredBeans)
                    {
                        CustomerTakesCoffee();
                        AudioManager.instance.PlayOneShot(correctOrderFX, this.transform.position);
                        Debug.Log("Coffee Given to window: Correct order");
                    }

                    else
                    {
                        Debug.Log("Coffee Given to window: Incorrect order");
                        AudioManager.instance.PlayOneShot(wrongOrderFX, this.transform.position);
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

    public void CustomerTakesCoffee()
    {


        if (customerScript != null)
        {
            customerScript.SetIsOrderedTrue(); // Mark the order as delivered

                Debug.Log("Customer acknowledged order and should move!");

            if (UIManager.Instance != null)
            {
                string requiredSyrup = customerScript.requiredSyrup;
                UIManager.Instance.UpdateCustomerOrder(0, requiredSyrup); // Resets the UI after the customer leaves
            }
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
            playerHealth.LoseLife("Wrong Order");  // Decrease the player's health
            Debug.Log("Player lost a life for wrong order");
        }
        else
        {
            Debug.LogError("PlayerHealth component not found!");
        }
    }
}
