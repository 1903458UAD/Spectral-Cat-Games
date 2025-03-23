using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Diagnostics;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scene Elements")]

    public List<NavNode> navNodes = new List<NavNode>();


    [Header("Bean Management")]
    public GameObject beanPrefab;
    public int initialBeanCount = 10;

    [Header("Game Stats")]
    [SerializeField] private int customersServed;
    [SerializeField] private float totalIncome;
    [SerializeField] private int playerLives;

    [Header("Customer Management")]
    public GameObject customerPrefab;
    public GameObject customerSpawnPoint;
    private List<GameObject> activeCustomers = new List<GameObject>();
    public float nodeConnectionRadius = 3.0f;
    [SerializeField] private int orderQuota;
    public int servedCustomers;

    [Header("Income Management")]
    private float income = 0f;
    private GameObject player;


    //private Dictionary<NPC_AI, Hiding_Spots> npcHidingAssignments = new Dictionary<NPC_AI, Hiding_Spots>();


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



        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {

        InitializeGame();
        Debug.Log("[GameManager] Spawning initial beans...");

        SpawnInitialBeans();  // Ensure this is called
        GameManager.Instance.SetIncome(StaticData.incomePassed);
        orderQuota = Random.Range(StaticData.lowerQuotaLimit, StaticData.higherQuotaLimit);

    }

    private void InitializeGame()
    {
        activeCustomers = new List<GameObject>();
        SetIncome(StaticData.incomePassed);
        orderQuota = Random.Range(StaticData.lowerQuotaLimit, StaticData.higherQuotaLimit);

    }

    void FindAllNavNodes()
    {
        navNodes.Clear();

        navNodes = new List<NavNode>(FindObjectsOfType<NavNode>());

        if (navNodes.Count == 0)
        {
            Debug.LogError("[GameManager] No NavNodes found in the scene! NPCs cannot move.");
        }
        else
        {
            Debug.Log($"[GameManager] Found {navNodes.Count} NavNodes.");
        }

    }

    private void SpawnInitialBeans()
    {
        for (int i = 0; i < initialBeanCount; i++)
        {
            Debug.Log($"[GameManager] Spawning bean {i + 1}...");
            SpawnBean();
        }
    }

    public void SpawnBean()
    {
        if (beanPrefab == null)
        {
            Debug.LogError("[GameManager] Bean Prefab is NULL!");
            return;
        }

        Vector3 spawnPos = GetRandomNavMeshPosition();
        if (spawnPos == Vector3.zero)
        {
            Debug.LogError("[GameManager] Failed to find a valid spawn position.");
            return;
        }

        GameObject beanObj = Instantiate(beanPrefab, spawnPos, Quaternion.identity);
        if (beanObj == null)
        {
            Debug.LogError("[GameManager] Bean object failed to instantiate!");
            return;
        }

        NPC_AI beanAI = beanObj.GetComponent<NPC_AI>();
        if (beanAI == null)
        {
            Debug.LogError("[GameManager] Spawned bean is missing NPC_AI component!");
            return;
        }

        Debug.Log($"[GameManager] Spawned bean at {spawnPos}");
        AIManager.Instance.RegisterNPC(beanAI);

    }

    private Vector3 GetRandomNavMeshPosition()
    {
        return AIManager.Instance.GetRandomNavMeshPosition();  //AIManager now handles nav positions
    }

    public void CheckOrderQuota()
    {
        if(orderQuota == servedCustomers)
        {
            StaticData.incomePassed = totalIncome;
            StaticData.lowerQuotaLimit += 2;
            StaticData.higherQuotaLimit += 2;
            UIManager.Instance.ShowDayEndScreen();
        }
    }

    public void IncreaseServedAmount()
    {
        servedCustomers++;
    }

    public void SpawnCustomer()
    {
        if (activeCustomers.Count == 0)
        {
            GameObject newCustomer = Instantiate(customerPrefab, customerSpawnPoint.transform.position, Quaternion.identity);
            activeCustomers.Add(newCustomer);
        }
    }

    public void RemoveCustomer(GameObject customer)
    {
        if (!activeCustomers.Remove(customer))
        {
            return;
        }

        Destroy(customer);
    }

    public void UpdatePlayerLives(int lives)
    {
        playerLives = lives;
        if (playerLives <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        UIManager.Instance.ShowGameOverScreen();
    }

    public int GetPlayerLives() => playerLives;

    public void UpdateIncome(float amount)
    {
        totalIncome += amount;
        UIManager.Instance.UpdateIncomeDisplay(totalIncome);
    }

    public float GetIncome() => totalIncome;

    public void SetIncome(float amount)
    {
        totalIncome = amount;
    }

    public void CustomerServed()
    {
        customersServed++;
    }

    public List<NavNode> GetNavNodes()
    {
        return navNodes;
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

    void DebugNavNodes()
    {
        foreach (NavNode node in navNodes)
        {
            //Debug.Log("[Debug] " + node.name + " has " + node.connectedNodes.Count + " connections.");
        }
    }
}


