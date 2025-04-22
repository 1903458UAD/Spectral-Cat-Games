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
    [SerializeField] private UpgradeData inflationUpgrade;
    [SerializeField] private UpgradeData trapUpgrade;
    [SerializeField] private UpgradeData cageUpgrade;

    public GameObject trapHidingSpotPrefab;
    public GameObject cageSpotPrefab;

    public static GameManager Instance { get; private set; }

    [Header("Scene Elements")]

  


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
    
    [SerializeField] private int orderQuota;
    public int servedCustomers;

    [Header("Income Management")]
    private float income = 0f;
    private GameObject player;

    private List<Hiding_Spots> hidingSpots = new List<Hiding_Spots>();

    [Header("Spawn Points")]
    public Transform trapSpawnPoint;


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

        if (trapSpawnPoint == null)
        {
            var sPointObj = GameObject.FindGameObjectWithTag("TrapSpawn");
            
            if (sPointObj != null)
            {
                trapSpawnPoint = sPointObj.transform;
            }
            else
            {
                Debug.LogWarning("[GameManager] No TrapSpawn object found or assigned!");
            }
        }


        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {
        inflationUpgrade.internalUpgradeEnabled = false;
        trapUpgrade.internalUpgradeEnabled = false;
        trapUpgrade.internalBaseValue = 0;

        InitializeGame();

        Debug.Log("[GameManager] Spawning initial beans...");



        SpawnInitialBeans();  // Ensure this is called
        GameManager.Instance.SetIncome(StaticData.incomePassed);
        orderQuota = Random.Range(StaticData.lowerQuotaLimit, StaticData.higherQuotaLimit);

    }

    private void Update()
    {
        if(trapUpgrade.internalUpgradeEnabled == true && trapUpgrade.internalBaseValue <= 3)
        {
            SpawnTrapHidingSpot();


            trapUpgrade.internalUpgradeEnabled = false;
        }

        if (cageUpgrade.internalUpgradeEnabled == true)
        {
            SpawnCage();
            cageUpgrade.internalUpgradeEnabled = false;
        }
    }


    private void InitializeGame()
    {
        activeCustomers = new List<GameObject>();
        SetIncome(StaticData.incomePassed);
        orderQuota = Random.Range(StaticData.lowerQuotaLimit, StaticData.higherQuotaLimit);

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

        Vector3 spawnPos = AIManager.Instance.GetRandomSpawnPositionUsingNodes();
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

        Collider[] hitColliders = Physics.OverlapSphere(beanObj.transform.position, 0.5f);
        bool tooClose = false;
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Shelf"))
            {
                tooClose = true;
                break;
            }
        }

        if (tooClose)
        {
            Vector3 newPos = GetRandomNavMeshPosition();
            // Optionally, you might loop a few times until a valid position is found.
            beanObj.transform.position = newPos;
            beanAI.navMeshAgent.Warp(newPos);
            Debug.Log("[GameManager] Bean repositioned away from shelf.");
        }


        Debug.Log($"[GameManager] Spawned bean at {spawnPos}");
        AIManager.Instance.RegisterNPC(beanAI);

    }

    private Vector3 GetRandomNavMeshPosition()
    {
        return AIManager.Instance.GetRandomSpawnPositionUsingNodes();  //AIManager now handles nav positions
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

    private void SpawnTrapHidingSpot()
    {
        if (trapSpawnPoint == null)
        {
            Debug.LogWarning("[GameManager] No trapSpawnPoint assigned!");
            return;
        }

        Vector3 spawnPos = trapSpawnPoint.position;

        //Vector3 playerPos = GetPlayerPosition();

        //Vector3 spawnPos = GetNavMeshPositionNear(playerPos);

        GameObject trapGO = Instantiate(trapHidingSpotPrefab, spawnPos, Quaternion.identity);

        Hiding_Spots trapSpot = trapGO.GetComponent<Hiding_Spots>();

        if (trapSpot != null)
        {
            trapSpot.hidingType = Hiding_Spots.HidingType.Trap;

            hidingSpots.Add(trapSpot);
            Debug.Log($"[GameManager] Trap hiding spot spawned at {spawnPos}");
        }
       
    }

    private void SpawnCage()
    {
        Vector3 playerPos = GetPlayerPosition();

        Vector3 spawnPos = new Vector3(1.81799996f, 1.64100003f, -3.1329999f);

        GameObject cage = Instantiate(cageSpotPrefab, spawnPos, Quaternion.identity);

        Hiding_Spots cageSpot = cage.GetComponent<Hiding_Spots>();

        if (cageSpot != null)
        {
            cageSpot.hidingType = Hiding_Spots.HidingType.Cage;

            hidingSpots.Add(cageSpot);
            Debug.Log($"[GameManager] cage hiding spot spawned at {spawnPos}");

        }
    }

        private Vector3 GetNavMeshPositionNear(Vector3 origin)
    {
        UnityEngine.AI.NavMeshHit hit;
        

        Vector3 randomDirection = Random.insideUnitSphere * 3f;
        randomDirection += origin;
        
        
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }
        

        return origin;
    }



  
}


