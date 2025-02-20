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
    public List<Hiding_Spots> hidingSpots = new List<Hiding_Spots>();
    public List<NavNode> navNodes = new List<NavNode>();
    public List<NPC_AI> beans = new List<NPC_AI>();

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
        //FindAllBeans(); // Might reenable later if needed
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        InitializeGame();
        FindAllNavNodes();
        LinkAllNavNodes();
        DebugNavNodes();
    }

    private void InitializeGame()
    {
        FindAllHidingSpots();
        SpawnInitialBeans();
        FindAllNavNodes();
        activeCustomers = new List<GameObject>();
        SetIncome(StaticData.incomePassed);
    }

    void FindAllHidingSpots()
    {
        hidingSpots.Clear();
        GameObject[] spots = GameObject.FindGameObjectsWithTag("HidingSpot");

        if (spots.Length == 0)
        {
            //Debug.LogError("[GameManager] No hiding spots found! Maybe they are missing from the scene?");
        }

        foreach (GameObject obj in spots)
        {
            Hiding_Spots hidingSpotComponent = obj.GetComponent<Hiding_Spots>();
            if (hidingSpotComponent != null)
            {
                hidingSpots.Add(hidingSpotComponent);
                //Debug.Log("[GameManager] Registered hiding spot: " + obj.name);
            }
            else
            {
                //Debug.LogError("[GameManager] " + obj.name + " is tagged as 'HidingSpot' but missing Hiding_Spots component!");
            }
        }
        //Debug.Log("[GameManager] Total hiding spots found: " + hidingSpots.Count);
    }

    void FindAllNavNodes()
    {
        navNodes = new List<NavNode>(FindObjectsOfType<NavNode>());
        //Debug.Log("[GameManager] Found navigation nodes: " + navNodes.Count);
    }

    void LinkAllNavNodes()
    {
        foreach (NavNode node in navNodes)
        {
            node.connectedNodes.Clear();
            foreach (NavNode otherNode in navNodes)
            {
                if (node != otherNode)
                {
                    float distance = Vector3.Distance(node.transform.position, otherNode.transform.position);
                    if (distance <= nodeConnectionRadius)
                    {
                        node.connectedNodes.Add(otherNode);
                        //Debug.Log("[GameManager] Linked " + node.name + " with " + otherNode.name);
                    }
                }
            }
        }
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
        if (beanPrefab == null)
        {
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
        }
    }

    private Vector3 GetRandomNavMeshPosition()
    {
        if (navNodes.Count == 0)
        {
            return Vector3.zero;
        }

        NavNode randomNode = navNodes[Random.Range(0, navNodes.Count)];
        return randomNode.transform.position;
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

    public Hiding_Spots FindBetterHidingSpot(Vector3 npcPosition, Hiding_Spots lastHidingSpot)
    {
        if (hidingSpots == null || hidingSpots.Count == 0)
        {
            Debug.LogError("[GameManager] No hiding spots availible!"); 
            return null;
        }

        List<Hiding_Spots> availableSpots = hidingSpots.FindAll(spot => spot.IsAvailable());

        if (availableSpots.Count == 0)
        {
            availableSpots = new List<Hiding_Spots>(hidingSpots);
        }

        Hiding_Spots bestSpot = null;
        float bestScore = float.MaxValue;

        foreach (var spot in availableSpots)
        {
            float distanceToPlayer = Vector3.Distance(spot.transform.position, GetPlayerPosition());
            float distanceToNPC = Vector3.Distance(spot.transform.position, npcPosition);

            bool isLastUsed = (spot == lastHidingSpot);
            float reusePenalty = isLastUsed ? 100f : 0f;
            float score = distanceToNPC - (distanceToPlayer * 0.5f) + reusePenalty;

            if (!isLastUsed && score < bestScore)
            {
                bestScore = score;
                bestSpot = spot;
            }
        }

        return bestSpot ?? hidingSpots.OrderBy(x => Random.value).FirstOrDefault();
    }

    void DebugNavNodes()
    {
        foreach (NavNode node in navNodes)
        {
            //Debug.Log("[Debug] " + node.name + " has " + node.connectedNodes.Count + " connections.");
        }
    }
}
