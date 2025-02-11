using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("NPC Spawning")]
    public GameObject beanPrefab; // Prefab for NPC beans
    public int initialBeanCount = 5; // Number of beans to spawn at game start
    public Transform spawnArea; // Area where beans will spawn
    [SerializeField] private float spawnSpreadRadius = 10f; // Radius to spread out the spawning positions

    [Header("Game Stats")]
    [SerializeField] private int customersServed; // Total customers served
    [SerializeField] private float totalIncome; // Total income generated
    [SerializeField] private int playerLives; // Player's remaining lives

    [Header("Customer Management")]
    public GameObject customerPrefab; // Prefab for customer objects
    public Transform customerSpawnPoint; // Spawn point for customers
    private List<GameObject> activeCustomers; // Currently active customers

    [Header("Hiding Spot System")]
    private List<Hiding_Spots> hidingSpots; // List of all hiding spots in the scene

    private List<GameObject> beanInstances; // List of all spawned beans
    private List<NPC_AI> npcInstances; // List of all NPC AI components

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            UnityEngine.Debug.LogWarning("Duplicate GameManager detected. Destroying extra instance.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UnityEngine.Debug.Log("GameManager initialized.");
        InitializeGame();
    }

    private void InitializeGame()
    {
        hidingSpots = new List<Hiding_Spots>();
        beanInstances = new List<GameObject>();
        npcInstances = new List<NPC_AI>();
        activeCustomers = new List<GameObject>();

        FindAllHidingSpots(); // Locate all hiding spots in the scene
        SpawnInitialBeans(); // Spawn initial set of NPC beans
    }

    private void FindAllHidingSpots()
    {
        hidingSpots.AddRange(FindObjectsOfType<Hiding_Spots>());
        UnityEngine.Debug.Log($"Total hiding spots found: {hidingSpots.Count}");
    }

    private void SpawnInitialBeans()
    {
        for (int i = 0; i < initialBeanCount; i++)
        {
            SpawnBean();
        }
    }

    public void SpawnBean()
    {
        Vector3 spawnPosition = GetRandomNavMeshPosition();
        GameObject newBean = Instantiate(beanPrefab, spawnPosition, Quaternion.identity);

        if (newBean == null)
        {
            UnityEngine.Debug.LogError("Failed to spawn NPC.");
            return;
        }

        AddNPC(newBean); // Add NPC AI to the spawned bean
        beanInstances.Add(newBean); // Track spawned bean
    }

    private void AddNPC(GameObject newBean)
    {
        NPC_AI npcAI = newBean.GetComponent<NPC_AI>();
        if (npcAI != null)
        {
            npcAI.enabled = true; // Enable AI behavior for the NPC
            npcInstances.Add(npcAI); // Track NPC AI instance
        }
    }

    public void UpdatePlayerLives(int lives)
    {
        playerLives = lives; // Update player lives
        UnityEngine.Debug.Log($"Player Lives Updated: {playerLives}");

        if (playerLives <= 0)
        {
            TriggerGameOver(); // Trigger game over if no lives left
        }
    }

    private void TriggerGameOver()
    {
        UnityEngine.Debug.Log("Player has lost all lives. Triggering game over.");
        UIManager.Instance.ShowGameOverScreen(); // Display game over screen
    }

    public int GetPlayerLives() => playerLives; // Return current player lives

    public void UpdateIncome(float amount)
    {
        totalIncome = Mathf.Round((totalIncome + amount) * 100f) / 100f; // Update and round total income
        UIManager.Instance.UpdateIncomeDisplay(totalIncome); // Update UI display for income
        UnityEngine.Debug.Log($"Income Updated: ${totalIncome}");
    }

    public float GetIncome() => totalIncome; // Return total income

    public void SpawnCustomer()
    {
        if (activeCustomers.Count == 0)
        {
            GameObject newCustomer = Instantiate(customerPrefab, customerSpawnPoint.position, Quaternion.identity);
            activeCustomers.Add(newCustomer); // Track active customer
            UnityEngine.Debug.Log("New customer spawned.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Customer spawn attempt failed - a customer is already active.");
        }
    }

    public void RemoveCustomer(GameObject customer)
    {
        if (!activeCustomers.Remove(customer))
        {
            UnityEngine.Debug.LogError("Attempted to remove a non-existent customer.");
            return;
        }

        Destroy(customer); // Remove customer from the scene
        UnityEngine.Debug.Log("Customer removed.");
    }

    public Hiding_Spots GetRandomHidingSpot()
    {
        if (hidingSpots.Count == 0)
        {
            UnityEngine.Debug.LogWarning("No hiding spots available.");
            return null;
        }
        return hidingSpots[UnityEngine.Random.Range(0, hidingSpots.Count)]; // Return random hiding spot
    }

    public List<Hiding_Spots> GetAvailableHidingSpots()
    {
        List<Hiding_Spots> availableSpots = new List<Hiding_Spots>();
        foreach (var spot in hidingSpots)
        {
            if (spot.IsAvailable())
            {
                availableSpots.Add(spot); // Add available spots to the list
            }
        }
        return availableSpots; // Return list of available hiding spots
    }

    public bool IsSpotVisibleToPlayer(Hiding_Spots spot)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            UnityEngine.Debug.LogError("[GameManager] Player not found in scene.");
            return false;
        }

        Vector3 directionToSpot = spot.transform.position - player.transform.position;
        if (Physics.Raycast(player.transform.position, directionToSpot, out RaycastHit hit))
        {
            return hit.transform == spot.transform; // Check if spot is visible to the player
        }
        return false; // Spot is not visible
    }

    private Vector3 GetRandomNavMeshPosition()
    {
        if (spawnArea == null)
        {
            UnityEngine.Debug.LogWarning("Spawn area not set. Using default position.");
            return transform.position;
        }

        Vector3 randomOffset = new Vector3(
            UnityEngine.Random.Range(-spawnSpreadRadius, spawnSpreadRadius),
            0,
            UnityEngine.Random.Range(-spawnSpreadRadius, spawnSpreadRadius)
        );

        Vector3 randomPosition = spawnArea.position + randomOffset;

        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        UnityEngine.Debug.LogWarning("Failed to find valid NavMesh position. Using spawn area position.");
        return spawnArea.position;
    }

    public void CustomerServed()
    {
        customersServed++; // Increment customers served count
        UnityEngine.Debug.Log($"Customers Served: {customersServed}");
    }

    public void ChangeScene(int scenenum)
    {
        if (scenenum == 1)
        {
            StaticData.incomePassed = GetIncome();
        }
        SceneManager.LoadScene(scenenum);
    }

    public void SetIncome(float amount)
    {
        totalIncome = amount;
    }
}
