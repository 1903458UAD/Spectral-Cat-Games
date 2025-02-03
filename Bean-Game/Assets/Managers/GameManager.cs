using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages core game mechanics including NPC spawning, player lives, and customer interactions.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("NPC Spawning")]
    public GameObject beanPrefab;
    public int initialBeanCount = 5;
    public Transform spawnArea;

    [Header("Game Stats")]
    [SerializeField] private int customersServed = 0;
    [SerializeField] private float totalIncome = 0f;
    [SerializeField] private int playerLives = 3;

    [Header("Customer Management")]
    public GameObject customerPrefab;
    public Transform customerSpawnPoint;
    private List<GameObject> activeCustomers = new List<GameObject>();

    [Header("Hiding Spot System")]
    private List<Hiding_Spots> hidingSpots = new List<Hiding_Spots>();

    private List<GameObject> beanInstances = new List<GameObject>();
    private List<NPC_AI> npcInstances = new List<NPC_AI>();

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
        FindAllHidingSpots();
        SpawnInitialBeans();
    }

    /// <summary>
    /// Finds all available hiding spots in the scene.
    /// </summary>
    private void FindAllHidingSpots()
    {
        hidingSpots = new List<Hiding_Spots>(FindObjectsOfType<Hiding_Spots>());
        UnityEngine.Debug.Log($"Total hiding spots found: {hidingSpots.Count}");
    }

    /// <summary>
    /// Spawns the initial set of NPCs at the start of the game.
    /// </summary>
    private void SpawnInitialBeans()
    {
        for (int i = 0; i < initialBeanCount; i++)
        {
            SpawnBean();
        }
    }

    /// <summary>
    /// Spawns a new NPC character.
    /// </summary>
    public void SpawnBean()
    {
        Vector3 spawnPosition = GetRandomNavMeshPosition();
        GameObject newBean = Instantiate(beanPrefab, spawnPosition, Quaternion.identity);

        if (newBean == null)
        {
            UnityEngine.Debug.LogError("Failed to spawn NPC.");
            return;
        }

        NPC_AI npcAI = newBean.GetComponent<NPC_AI>();
        if (npcAI != null)
        {
            npcAI.enabled = true;
            npcInstances.Add(npcAI);
        }

        beanInstances.Add(newBean);
    }

    /// <summary>
    /// Updates the player's remaining lives and checks for game-over conditions.
    /// </summary>
    public void UpdatePlayerLives(int lives)
    {
        playerLives = lives;
        UnityEngine.Debug.Log($"Player Lives Updated: {playerLives}");

        if (playerLives <= 0)
        {
            UnityEngine.Debug.Log("Player has lost all lives. Triggering game over.");
            UIManager.Instance.ShowDeathScreen();
        }
    }

    public int GetPlayerLives() => playerLives;

    /// <summary>
    /// Updates the player's total income and refreshes the UI display.
    /// </summary>
    public void UpdateIncome(float amount)
    {
        totalIncome = Mathf.Round((totalIncome + amount) * 100f) / 100f;
        UIManager.Instance.UpdateIncomeDisplay(totalIncome);
        UnityEngine.Debug.Log($"Income Updated: ${totalIncome}");
    }

    public float GetIncome() => totalIncome;

    /// <summary>
    /// Spawns a new customer at the assigned spawn point.
    /// </summary>
    public void SpawnCustomer()
    {
        if (activeCustomers.Count == 0)
        {
            GameObject newCustomer = Instantiate(customerPrefab, customerSpawnPoint.position, Quaternion.identity);
            activeCustomers.Add(newCustomer);
            UnityEngine.Debug.Log("New customer spawned.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Customer spawn attempt failed - a customer is already active.");
        }
    }

    /// <summary>
    /// Removes a customer from the scene.
    /// </summary>
    public void RemoveCustomer(GameObject customer)
    {
        if (activeCustomers.Contains(customer))
        {
            activeCustomers.Remove(customer);
            Destroy(customer);
            UnityEngine.Debug.Log("Customer removed.");
        }
        else
        {
            UnityEngine.Debug.LogError("Attempted to remove a non-existent customer.");
        }
    }

    /// <summary>
    /// Returns a random hiding spot from the available list.
    /// </summary>
    public Hiding_Spots GetRandomHidingSpot()
    {
        if (hidingSpots.Count == 0)
        {
            UnityEngine.Debug.LogWarning("No hiding spots available.");
            return null;
        }
        return hidingSpots[UnityEngine.Random.Range(0, hidingSpots.Count)];
    }

    /// <summary>
    /// Generates a random valid position on the NavMesh.
    /// </summary>
    private Vector3 GetRandomNavMeshPosition()
    {
        if (spawnArea == null)
        {
            UnityEngine.Debug.LogWarning("Spawn area not set. Using default position.");
            return transform.position;
        }

        Vector3 randomPosition = new Vector3(
            UnityEngine.Random.Range(spawnArea.position.x - 5f, spawnArea.position.x + 5f),
            spawnArea.position.y,
            UnityEngine.Random.Range(spawnArea.position.z - 5f, spawnArea.position.z + 5f));

        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        UnityEngine.Debug.LogWarning("Failed to find valid NavMesh position.");
        return transform.position;
    }

    /// <summary>
    /// Increments the customer served counter.
    /// </summary>
    public void CustomerServed()
    {
        customersServed++;
        UnityEngine.Debug.Log($"Customers Served: {customersServed}");
    }
}
