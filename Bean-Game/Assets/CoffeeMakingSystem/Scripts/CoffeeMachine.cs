using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class CoffeeMachine : MonoBehaviour
{

   // public int requiredBeans = 3; // Number of beans required to make 1 coffee-- to be adjusted later for balancing
    public float coffeeCreationTime = 5f; // Time to create coffee after enough beans
    [SerializeField] private GameObject buttonLid;
    private Quaternion lidOpen;
    public GameObject coffeeCup1Bean; // Prefab for coffee with 1 bean
    public GameObject coffeeCup2Beans; // " with 2 beans
    public GameObject coffeeCup3Beans; // " with 3 beans

    [SerializeField] private EventReference coffeeMachineSound;
    [SerializeField] private EventReference buttonSound;

    [SerializeField] private GameObject hopperSpawn;
    [SerializeField] private GameObject hopperBeanPrefab;
    [SerializeField] private GameObject shatteredBeanPrefab;
    private GameObject[] hopperBeansArray;
    private AudioSource audioSource;
    public AudioClip[] clips;
    [Range(0.01f, 1f)]
    public float volume;
    private int audioIndex;

    public Transform spawnPoint;

    private int currentBeans = 0;
    private bool isCoffeeMaking = false;


 
    public void AddBean(BeanInteraction bean)
    {
        //Debug.Log($"[CoffeeMachine] Instance ID: {this.GetInstanceID()}, Beans: {currentBeans}");

        if (isCoffeeMaking)
        {
            return; // Needed to fix bug with placing bean while machine active
        }

        if (currentBeans < 3)
        {
            currentBeans++;
            Debug.Log($"[CoffeeMachine] Beans added: {currentBeans}");
            Destroy(bean.gameObject); // Destroy the bean after adding it to the machine
            GameManager.Instance.ExclamationOff();

            if (shatteredBeanPrefab == null)
            {
                Debug.LogError("[CoffeeMachine] shatteredBeanPrefab is STILL null at runtime!");
                return;
            }

            if (!isCoffeeMaking)
            {
                Instantiate(hopperBeanPrefab, hopperSpawn.transform.position, Quaternion.identity);
            }
            //GameObject hopperBean = Instantiate(hopperBeanPrefab, hopperSpawn.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("Cannot add more beans! Machine is full.");
        }

    }

public void Start()
{
    lidOpen = buttonLid.transform.rotation;
    audioSource = GetComponent<AudioSource>();
    clips = Resources.LoadAll<AudioClip>("Grinding");
}

public bool CanActivateMachine()
{
       Debug.Log($"[CoffeeMachine] Instance ID: {this.GetInstanceID()}, Beans: {currentBeans}");

        if (currentBeans >= 1 && !isCoffeeMaking)
        {
            
            return true;
        }
        
        return false;
}

private AudioClip playRandom()
{
    audioIndex = Random.Range(0,clips.Length);

    return clips[audioIndex];
}

public void ActivateMachine()
{
        Debug.Log($"[DEBUG] CoffeeMachine instance: {this.name}, shatteredBeanPrefab: {shatteredBeanPrefab}");

        if (isCoffeeMaking)
        {
           
            return; //Needed to fix bug with placing bean while machine active
        }



        if (CanActivateMachine() == true)
    {
            hopperBeansArray = GameObject.FindGameObjectsWithTag("Respawn");



            foreach (GameObject bean in hopperBeansArray)
            {
                if (bean != null)
                {
                   
                    Vector3 beanPosition = bean.transform.position;
                    Quaternion beanRotation = bean.transform.rotation;

                    StaticData.dailyBeans += 1;
                    StaticData.totalBeans += 1;
                    Destroy(bean);

                    
                    if (shatteredBeanPrefab != null)
                    {
                        GameObject shatteredBean = Instantiate(shatteredBeanPrefab, beanPosition, beanRotation);

                        ShatteredBeanSwirl swirlScript = shatteredBean.GetComponent<ShatteredBeanSwirl>();
                        if (swirlScript != null)
                        {
                            swirlScript.swirlCenter = this.transform; 
                        }

                        foreach (Rigidbody rb in shatteredBean.GetComponentsInChildren<Rigidbody>())
                        {
                            rb.AddExplosionForce(1f, beanPosition, 1f);
                        }
                    }
                    else
                    {
                        Debug.LogError("Shattered bean prefab not assigned!");
                    }
                }
            }
                isCoffeeMaking = true;
        Debug.Log("Enough beans! Starting coffee creation...");
        AudioManager.instance.PlayOneShot(buttonSound, this.transform.position);
        AudioManager.instance.PlayOneShot(coffeeMachineSound, this.transform.position);
        hopperBeansArray = GameObject.FindGameObjectsWithTag("Respawn");
        audioSource.PlayOneShot(playRandom(), volume);


        Invoke(nameof(CreateCoffee), coffeeCreationTime);
    }
    else
    {
        Debug.Log("Not enough beans! Add more beans to activate.");
    }
}
    public void CreateCoffee()
    {

        GameObject[] shatteredBeans = GameObject.FindGameObjectsWithTag("ShatteredBean");
        foreach (GameObject bean in shatteredBeans)
        {
            if (bean != null)
            {
                Destroy(bean);
            }
        }

        // Destroy all shattered beans now that coffee is made
        foreach (GameObject bean in hopperBeansArray)
        {
            if (bean != null)
            {
                BeanInteraction beanScript = bean.GetComponent<BeanInteraction>();
                if (beanScript != null)
                {
                    beanScript.DestroyShatter();
                }
                else
                {
                    Destroy(bean); 
                }
            }
        }

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
            Vector3 spawnPosition = spawnPoint ? spawnPoint.position : transform.position;

            Instantiate(coffeeToSpawn, spawnPosition, Quaternion.identity);
            Debug.Log($"Brewed coffee with {currentBeans} beans.");

            currentBeans = 0;
        }
        else
        {
           // Debug.LogError("[CoffeeMachine] No coffee prefab assigned or incorrect bean count!");
        }
        buttonLid.transform.rotation = lidOpen;
        isCoffeeMaking = false;
    }
}
