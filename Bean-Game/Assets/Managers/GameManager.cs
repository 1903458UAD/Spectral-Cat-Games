using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("NPC Spawning")]
    public GameObject beanPrefab;
    public int initialBeanCount = 5;
    public Transform spawnArea;

    [Header("Game Tracking")]
    [SerializeField] private int customersServed = 0;
    [SerializeField] private float totalIncome = 0;
    [SerializeField] private int playerLives = 3;


    //[Header("Income Management")]
    //private float totalIncome = 0;

    //[Header("Player Stats")]
    //private int playerLives = 3; // ✅ Added player lives tracking

    [Header("NPC & Customer Management")]
    public GameObject customerPrefab;
    public Transform customerSpawnPoint;
    private List<GameObject> activeCustomers = new List<GameObject>();

    [Header("Hiding Spots")]
    private List<Hiding_Spots> hidingSpots = new List<Hiding_Spots>();


    private List<GameObject> beanInstances = new List<GameObject>();
    private List<NPC_AI> npcInstances = new List<NPC_AI>();
   // private List<Hiding_Spots> hidingSpots = new List<Hiding_Spots>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        FindAllHidingSpots();
        SpawnInitialBeans();
        hidingSpots = new List<Hiding_Spots>(FindObjectsOfType<Hiding_Spots>());
    }

    private void FindAllHidingSpots()
    {
        hidingSpots = new List<Hiding_Spots>(FindObjectsOfType<Hiding_Spots>());
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

        NPC_AI npcAI = newBean.GetComponent<NPC_AI>();
        if (npcAI != null)
        {
            npcAI.enabled = true;
            npcInstances.Add(npcAI);
        }

        beanInstances.Add(newBean);
    }

    // ✅ PLAYER LIVES MANAGEMENT
    public void UpdatePlayerLives(int lives)
    {
        playerLives = lives;
        UnityEngine.Debug.Log("[GameManager] Player Lives: " + playerLives);

        if (playerLives <= 0)
        {
            UIManager.Instance.ShowDeathScreen();
        }
    }

    public int GetPlayerLives()
    {
        return playerLives;
    }

    // ✅ INCOME MANAGEMENT
    public void UpdateIncome(float amount)
    {
        totalIncome += amount;
        totalIncome = Mathf.Round(totalIncome * 100.0f) * 0.01f;
        UIManager.Instance.UpdateIncomeDisplay(totalIncome);
    }

    public float GetIncome()
    {
        return totalIncome;
    }

    // ✅ CUSTOMER MANAGEMENT
    public void SpawnCustomer()
    {
        if (activeCustomers.Count == 0)
        {
            GameObject newCustomer = Instantiate(customerPrefab, customerSpawnPoint.position, Quaternion.identity);
            activeCustomers.Add(newCustomer);
        }
    }

    public void RemoveCustomer(GameObject customer)
    {
        activeCustomers.Remove(customer);
        Destroy(customer);
    }

    // ✅ HIDING SPOT MANAGEMENT
    public Hiding_Spots GetRandomHidingSpot()
    {
        if (hidingSpots.Count == 0) return null;
        return hidingSpots[UnityEngine.Random.Range(0, hidingSpots.Count)];
    }

    private Vector3 GetRandomNavMeshPosition()
    {
        Vector3 randomPosition = spawnArea != null
            ? new Vector3(
                UnityEngine.Random.Range(spawnArea.position.x - 5f, spawnArea.position.x + 5f),
                spawnArea.position.y,
                UnityEngine.Random.Range(spawnArea.position.z - 5f, spawnArea.position.z + 5f))
            : new Vector3(UnityEngine.Random.Range(-10f, 10f), 0f, UnityEngine.Random.Range(-10f, 10f));

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPosition, out hit, 10f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    public void NotifyNPCReachedHidingSpot(NPC_AI npc) // ✅ Restored: Allows NPCs to notify GameManager
    {
        UnityEngine.Debug.Log("[GameManager] NPC " + npc.gameObject.name + " reached a hiding spot.");
    }

    //public Hiding_Spots GetRandomHidingSpot() // ✅ Restored: NPCs use this to find a hiding spot
    //{
    //    if (hidingSpots == null || hidingSpots.Count == 0)
    //    {
    //        return null;
    //    }
    //    return hidingSpots[UnityEngine.Random.Range(0, hidingSpots.Count)];
    //}

    //public void UpdatePlayerLives(int lives)
    //{
    //    playerLives = lives;
    //    UnityEngine.Debug.Log("[GameManager] Player Lives: " + playerLives);

    //    if (playerLives <= 0)
    //    {
    //        UIManager.Instance.ShowDeathScreen();
    //    }
    //}

    //public void UpdateIncome(float amount)
    //{
    //    totalIncome += amount;
    //    UIManager.Instance.UpdateIncomeDisplay(totalIncome);
    //}

    public void CustomerServed()
    {
        customersServed++;
        UnityEngine.Debug.Log("[GameManager] Customer Served: " + customersServed);
    }


}
