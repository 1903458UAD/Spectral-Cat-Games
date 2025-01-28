using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeMachine : MonoBehaviour
{

    public int requiredBeans = 3; // Number of beans required to make 1 coffee-- to be adjusted later for balancing
    public float coffeeCreationTime = 5f; // Time to create coffee after enough beans
    private int currentBeanCount = 0; // Number of beans currently in the machine


    public GameObject coffeePrefab; // Prefab of the coffee in a cup








    public void AddBean(BeanInteraction bean)
    {
        if (bean != null)
        {
            // Increment the bean count and destroy the bean
            currentBeanCount++;
            Destroy(bean.gameObject);
            Debug.Log("Bean added! Current beans: " + currentBeanCount);




            //// Check if enough beans are present to create coffee
            //if (currentBeanCount >= requiredBeans)
            //{
            //    Debug.Log("Enough beans! Starting coffee creation...");
            //    Invoke(nameof(CreateCoffee), coffeeCreationTime);
            //    currentBeanCount = 0; // Reset beans for the next coffee
            //}
        }
    }

    public bool CanActivateMachine()
    {
        return currentBeanCount >= requiredBeans;
    }

    public void ActivateMachine()
    {
        if (CanActivateMachine())
        {
            Debug.Log("Enough beans! Starting coffee creation...");
            Invoke(nameof(CreateCoffee), coffeeCreationTime);
            currentBeanCount = 0; // Reset beans for the next coffee
        }
        else
        {
            Debug.Log("Not enough beans! Add more beans to activate.");
        }
    }



//public void AttemptToActivateMachine()
//    {
//        if (currentBeanCount >= requiredBeans)
//        {
//            Debug.Log("Enough beans! Starting coffee creation...");
//            Invoke(nameof(CreateCoffee), coffeeCreationTime);
//            currentBeanCount = 0; // Reset beans for the next coffee
//        }
//        else
//        {
//            Debug.Log("Not enough beans! Add more beans to activate.");
//        }
//    }

    private void CreateCoffee()
    {
        // Spawn coffee at the coffee machine's position
        Vector3 spawnPosition = transform.position + Vector3.forward; // Slightly above the machine //* 1.5f
        Instantiate(coffeePrefab, spawnPosition, Quaternion.identity);
        Debug.Log("Coffee created!");
    }
}
