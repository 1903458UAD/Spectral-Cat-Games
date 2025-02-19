using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Elements")]
    public List<Hiding_Spots> hidingSpots = new List<Hiding_Spots>();
    public List<NavNode> navNodes = new List<NavNode>();
    public List<NPC_AI> beans = new List<NPC_AI>();

    [Header("Bean Management")]
    public GameObject beanPrefab;
    public int initialBeanCount = 10;

    [Header("Game Stats")]
    [SerializeField] private int customersServed; // Total customers served
    [SerializeField] private float totalIncome; // Total income generated
    [SerializeField] private int playerLives; // Player's remaining lives


    [Header("Customer Management")]
    public GameObject customerPrefab; // Prefab for customer objects
    public GameObject customerSpawnPoint; // Spawn point for customers
    private List<GameObject> activeCustomers; // Currently active customers

    [Header("Income Management")]
    private float income = 0f;

    private GameObject player;

    public int gameScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        FindAllHidingSpots();
        FindAllNavNodes();
    }

    private void Start()
    {
        UnityEngine.Debug.Log("GameManager initialized.");
        player = GameObject.FindGameObjectWithTag("Player");
        InitializeGame();
    }



    private void InitializeGame()
    {
        FindAllHidingSpots();
        SpawnInitialBeans();
        FindAllNavNodes();
        activeCustomers = new List<GameObject>();
        SetIncome(StaticData.incomePassed);
    }



    private void FindAllHidingSpots()
    {
        hidingSpots.Clear();
        hidingSpots.AddRange(FindObjectsOfType<Hiding_Spots>());
        UnityEngine.Debug.Log($"Total hiding spots found: {hidingSpots.Count}");
    }

    private void FindAllNavNodes()
    {
        navNodes.Clear();
        navNodes.AddRange(FindObjectsOfType<NavNode>());
        UnityEngine.Debug.Log($"Total navigation nodes found: {navNodes.Count}");
    }

    private void SpawnInitialBeans()
    {
        for (int i = 0; i < initialBeanCount; i++)
        {
            SpawnBean();
        }
    }

    private void FindAllBeans()
    {
        beans = new List<NPC_AI>(FindObjectsOfType<NPC_AI>());
        Debug.Log($"Total beans found: {beans.Count}");
    }


    public void SpawnBean()
    {
        if (beanPrefab == null)
        {
            UnityEngine.Debug.LogError("[GameManager] Bean prefab not assigned!");
            return;
        }

        Vector3 spawnPos = GetRandomNavMeshPosition();
        if (spawnPos != Vector3.zero)
        {
            GameObject beanObj = Instantiate(beanPrefab, spawnPos, Quaternion.identity);
            NPC_AI beanAI = beanObj.GetComponent<NPC_AI>();

            if (beanAI != null)
            {
                beans.Add(beanAI);
                RegisterBean(beanAI);
            }
            else
            {
                UnityEngine.Debug.LogError("[GameManager] Spawned bean does not have NPC_AI script!");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("[GameManager] Bean attempted to spawn outside NavMesh!");
        }
    }

    private Vector3 GetRandomNavMeshPosition()
    {
        if (navNodes.Count == 0)
        {
            UnityEngine.Debug.LogError("[GameManager] No navigation nodes found.");
            return Vector3.zero;
        }

        NavNode randomNode = navNodes[UnityEngine.Random.Range(0, navNodes.Count)];
        return randomNode.transform.position;
    }

    public void SpawnCustomer()
    {
        if (activeCustomers.Count == 0)
        {
            GameObject newCustomer = Instantiate(customerPrefab, customerSpawnPoint.transform.position, Quaternion.identity);
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

    public void SetIncome(float amount)
    {
        totalIncome = amount;
    }

    public void CustomerServed()
    {
        customersServed++; // Increment customers served count
        UnityEngine.Debug.Log($"Customers Served: {customersServed}");
    }



    public List<NavNode> GetNavNodes()
    {
        return navNodes;
    }

    public List<Hiding_Spots> GetAvailableHidingSpots()
    {
        return hidingSpots;
    }

    public void RegisterBean(NPC_AI bean)
    {
        if (!beans.Contains(bean))
        {
            beans.Add(bean);
        }
    }

    public void UnregisterBean(NPC_AI bean)
    {
        if (beans.Contains(bean))
        {
            beans.Remove(bean);
        }
    }

    public Vector3 GetPlayerPosition()
    {
        return player != null ? player.transform.position : Vector3.zero;
    }

    public void ChangeScene(int scenenum)
    {
        if (scenenum == 1)
        {
            StaticData.incomePassed = GetIncome();
        }
        SceneManager.LoadScene(scenenum);
    }


    public Hiding_Spots FindBestHidingSpot(Vector3 npcPosition)
    {
        if (hidingSpots.Count == 0)
            return null;

        Hiding_Spots bestSpot = null;
        float maxDistance = 0f;

        foreach (var spot in hidingSpots)
        {
            float distanceToPlayer = Vector3.Distance(spot.transform.position, GetPlayerPosition());
            if (distanceToPlayer > maxDistance)
            {
                maxDistance = distanceToPlayer;
                bestSpot = spot;
            }
        }

        return bestSpot;
    }




}
