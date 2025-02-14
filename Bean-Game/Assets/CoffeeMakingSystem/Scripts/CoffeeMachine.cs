using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoffeeMachine : MonoBehaviour
{

   // public int requiredBeans = 3; // Number of beans required to make 1 coffee-- to be adjusted later for balancing
    public float coffeeCreationTime = 5f; // Time to create coffee after enough beans
    //private int currentBeanCount = 0; // Number of beans currently in the machine
    [SerializeField] private GameObject buttonLid;
    private Quaternion lidOpen;
    public GameObject coffeeCup1Bean; // Prefab for coffee with 1 bean
    public GameObject coffeeCup2Beans; // " with 2 beans
    public GameObject coffeeCup3Beans; // " with 3 beans

    private int currentBeans = 0;

    public void AddBean(BeanInteraction bean)
    {

        if (currentBeans < 3)
        {
            currentBeans++;
            Debug.Log($"[CoffeeMachine] Beans added: {currentBeans}");
            Destroy(bean.gameObject); // Destroy the bean after adding it to the machine
        }
        else
        {
            Debug.Log("Cannot add more beans! Machine is full.");
        }

    }

public void Start()
{
    lidOpen = buttonLid.transform.rotation;
}

public bool CanActivateMachine()
{
    return currentBeans > 0;
}

public void ActivateMachine()
{
    if (CanActivateMachine())
    {
        Debug.Log("Enough beans! Starting coffee creation...");
        Invoke(nameof(CreateCoffee), coffeeCreationTime);
    
    }
    else
    {
        Debug.Log("Not enough beans! Add more beans to activate.");
    }
}
    public void CreateCoffee()
    {
        Debug.Log("[CoffeeMachine] CreateCoffee() function called!");
        GameObject coffeeToSpawn = null;

        if (currentBeans == 1)
        {
            coffeeToSpawn = coffeeCup1Bean;
            coffeeToSpawn.GetComponent<CoffeeInteraction>().SetBeanCount(1); // Set 1 bean
        }
        else if (currentBeans == 2)
        {
            coffeeToSpawn = coffeeCup2Beans;
            coffeeToSpawn.GetComponent<CoffeeInteraction>().SetBeanCount(2); // Set 2 beans
        }
        else if (currentBeans == 3)
        {
            coffeeToSpawn = coffeeCup3Beans;
            coffeeToSpawn.GetComponent<CoffeeInteraction>().SetBeanCount(3); // Set 3 beans
        }

        if (coffeeToSpawn != null)
        {
            Instantiate(coffeeToSpawn, transform.position, Quaternion.identity);
            Debug.Log($"Brewed coffee with {currentBeans} beans.");
        }
        else
        {
           // Debug.LogError("[CoffeeMachine] No coffee prefab assigned or incorrect bean count!");
        }
        buttonLid.transform.rotation = lidOpen;
        currentBeans = 0;
    }
}
