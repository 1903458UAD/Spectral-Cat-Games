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
    //public List<Hiding_Spots> hidingSpots = new List<Hiding_Spots>();
    public List<NavNode> navNodes = new List<NavNode>();
    //public List<NPC_AI> beans = new List<NPC_AI>();

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

        //DontDestroyOnLoad(gameObject);
        // FindAllHidingSpots();
        // FindAllNavNodes();
        //FindAllBeans(); // Might reenable later if needed

        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Start()
    {

        InitializeGame();
        Debug.Log("[GameManager] Spawning initial beans...");
        //FindAllNavNodes();
        SpawnInitialBeans();  // Ensure this is called
        GameManager.Instance.SetIncome(StaticData.incomePassed);
        orderQuota = Random.Range(StaticData.lowerQuotaLimit, StaticData.higherQuotaLimit);
        //player = GameObject.FindGameObjectWithTag("Player");
        //InitializeGame();
        //FindAllNavNodes();
        //LinkAllNavNodes();
        //DebugNavNodes();
    }

    private void InitializeGame()
    {
        activeCustomers = new List<GameObject>();
        SetIncome(StaticData.incomePassed);
        orderQuota = Random.Range(StaticData.lowerQuotaLimit, StaticData.higherQuotaLimit);
        //FindAllHidingSpots();
        //SpawnInitialBeans();
        //FindAllNavNodes();
        //activeCustomers = new List<GameObject>();
        //SetIncome(StaticData.incomePassed);
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

        //Debug.Log("[GameManager] Found navigation nodes: " + navNodes.Count);
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
        // Ensure this is working  // AIManager now manages NPCs

        //beans.Add(beanAI);

        //RegisterBean(beanAI);                                        
        //StartCoroutine(FreezeYAxisTemporarily(beanAI)); //NEW: Freeze Y for a second
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



//private IEnumerator FreezeYAxisTemporarily(NPC_AI bean)
//{
//    Rigidbody rb = bean.GetComponent<Rigidbody>();

//    if (rb != null)
//    {
//        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation; //Freeze Y movement
//    }

//    yield return new WaitForSeconds(1f); // Wait a second

//    if (rb != null)
//    {
//        rb.constraints = RigidbodyConstraints.FreezeRotation; //Unfreeze Y movement after a second
//    }
//}






//private IEnumerator SnapBeanToNavMesh(NPC_AI bean)
//{
//    yield return new WaitForSeconds(0.1f); // Let physics settle

//    if (NavMesh.SamplePosition(bean.transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
//    {
//        bean.transform.position = hit.position; // Correct Y placement
//    }
//}




//public Hiding_Spots FindBetterHidingSpot(Vector3 npcPosition, Hiding_Spots lastHidingSpot)
//{
//    if (hidingSpots == null || hidingSpots.Count == 0)
//    {
//        Debug.LogError("[GameManager] No hiding spots available!");
//        return null;
//    }

//    List<Hiding_Spots> availableSpots = hidingSpots.FindAll(spot => spot.IsAvailable());

//    if (availableSpots.Count == 0)
//    {
//        Debug.LogWarning("[GameManager] No available hiding spots!");
//        return null;
//    }

//    Debug.Log($"[GameManager] Finding a better hiding spot for NPC at {npcPosition}");

//    Hiding_Spots bestSpot = null;
//    float bestScore = float.MaxValue;

//    foreach (var spot in availableSpots)
//    {
//        float distanceToPlayer = Vector3.Distance(spot.transform.position, GetPlayerPosition());
//        float distanceToNPC = Vector3.Distance(spot.transform.position, npcPosition);

//        bool isLastUsed = (spot == lastHidingSpot);
//        float reusePenalty = isLastUsed ? 100f : 0f;

//        float randomFactor = Random.Range(0f, 20f);
//        float score = distanceToNPC - (distanceToPlayer * 0.5f) + reusePenalty + randomFactor;

//        Debug.Log($"[GameManager] Spot {spot.name} Score: {score}");

//        if (!isLastUsed && score < bestScore)
//        {
//            bestScore = score;
//            bestSpot = spot;
//        }
//    }

//    if (bestSpot != null)
//    {
//        Debug.Log($"[GameManager] Assigned hiding spot: {bestSpot.name}");
//    }
//    else
//    {
//        Debug.LogWarning("[GameManager] No valid hiding spot found!");
//    }

//    return bestSpot;
//}


//public void RegisterNPCInSpot(NPC_AI npc, Hiding_Spots spot)
//{
//    if (spot == null)
//    {
//        Debug.LogWarning($"[GameManager] Tried to register {npc.gameObject.name} to a null hiding spot.");
//        return; // Prevent assigning null spots
//    }

//    if (!npcHidingAssignments.ContainsKey(npc))
//    {
//        npcHidingAssignments[npc] = spot;
//        Debug.Log($"[GameManager] Registered {npc.gameObject.name} to {spot.name}");
//    }
//}




//public void UnregisterNPCFromSpot(NPC_AI npc)
//{
//    if (npcHidingAssignments.ContainsKey(npc))
//    {
//        Hiding_Spots spot = npcHidingAssignments[npc]; // Get assigned spot
//        if (spot != null)
//        {
//            spot.DecrementOccupancy(); //Ensure the spot is freed properly
//        }

//        npcHidingAssignments.Remove(npc);
//        Debug.Log($"[GameManager] Unregistered {npc.gameObject.name} from {spot.name}");
//    }
//}



//public List<Hiding_Spots> GetAvailableHidingSpots()
//{
//    return hidingSpots;
//}

//public void RegisterBean(NPC_AI bean)
//{

//    if (!beans.Contains(bean))
//    {
//        beans.Add(bean); // Adds the bean if not already registered 
//    }
//}

//public void UnregisterBean(NPC_AI bean)
//{
//    if (beans.Contains(bean))
//    {
//        beans.Remove(bean);
//    }
//}


//if (navNodes == null || navNodes.Count == 0)
//{
//    Debug.LogError("[GameManager] No NavNodes found! No Spawn.");
//    return Vector3.zero;
//}

////if (navNodes.Count == 0)
////{
////    return Vector3.zero;
////}

//NavNode randomNode = navNodes[Random.Range(0, navNodes.Count)];
//Vector3 spawnPosition = randomNode.transform.position; //

////Ensure the position is actually on the NavMesh
//NavMeshHit hit;
//if (NavMesh.SamplePosition(spawnPosition, out hit, 1.0f, NavMesh.AllAreas))
//{
//     Debug.Log($"[GameManager] Found valid NavMesh position: {hit.position}");
//      return hit.position; //Return a valid NavMesh position
//}

//Debug.LogWarning("[GameManager] Failed to find a valid NavMesh position! Using fallback.");
//return spawnPosition; //If not valid, return original position (less ideal)



//void LinkAllNavNodes()
//{
//    foreach (NavNode node in navNodes)
//    {
//        node.connectedNodes.Clear();
//        foreach (NavNode otherNode in navNodes)
//        {
//            if (node != otherNode)
//            {
//                float distance = Vector3.Distance(node.transform.position, otherNode.transform.position);
//                if (distance <= nodeConnectionRadius)
//                {
//                    node.connectedNodes.Add(otherNode);
//                    //Debug.Log("[GameManager] Linked " + node.name + " with " + otherNode.name);
//                }
//            }
//        }
//    }
//}


//void FindAllHidingSpots()
//{
//    hidingSpots.Clear();
//    GameObject[] spots = GameObject.FindGameObjectsWithTag("HidingSpot");

//    if (spots.Length == 0)
//    {
//        //Debug.LogError("[GameManager] No hiding spots found! Maybe they are missing from the scene?");
//    }

//    foreach (GameObject obj in spots)
//    {
//        Hiding_Spots hidingSpotComponent = obj.GetComponent<Hiding_Spots>();
//        if (hidingSpotComponent != null)
//        {
//            hidingSpots.Add(hidingSpotComponent);
//            //Debug.Log("[GameManager] Registered hiding spot: " + obj.name);
//        }
//        else
//        {
//            //Debug.LogError("[GameManager] " + obj.name + " is tagged as 'HidingSpot' but missing Hiding_Spots component!");
//        }
//    }
//    //Debug.Log("[GameManager] Total hiding spots found: " + hidingSpots.Count);
//}