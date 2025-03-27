using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using FMOD.Studio;
using System.Linq;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [Header("AI Elements")]
    private List<NPC_AI> npcList = new List<NPC_AI>();
    private List<Hiding_Spots> hidingSpots = new List<Hiding_Spots>();
    private Dictionary<NPC_AI, Hiding_Spots> npcHidingAssignments = new Dictionary<NPC_AI, Hiding_Spots>();

    private Dictionary<NPC_AI, float> hidingTimers = new Dictionary<NPC_AI, float>();
    private float hidingDuration = 10f; // Time before NPC moves to another spot


    public NavGraph navGraph;
    private List<NavGraph.NavNodeData> nodeDataList = new List<NavGraph.NavNodeData>();

    //private List<NavNode> navNodes = new List<NavNode>();

    private float decisionInterval = 0.5f;  // Every NPC updates decisions every 0.5s
    private Dictionary<NPC_AI, float> nextDecisionTimes = new Dictionary<NPC_AI, float>();

    private bool isResetting = false;

    private bool start = false;

    public int beansLow = 4;
    public int beanRestock = 6;

    public float minDistanceFromShelf = .01f;



    public float maxRunTime = 8f; // Maximum timea bean is allowed to run continuously
    private Dictionary<NPC_AI, float> runStartTimes = new Dictionary<NPC_AI, float>();



    private List<Collider> shelfColliders = new List<Collider>();

    private EventInstance beanFootsteps;

    //[Header("AI Behavior Settings")]
    //public float reactionTime = 1.0f; // Adjustable delay before NPCs move back into cover

    [Header("AI Behavior Settings")]
    public float updateInterval = 0.1f; // Adjust this for difficulty


    public float hidingDurationMin = 5f; // Value to track the min hiding time 



    public float hidingDurationMax = 10f; // track the max hiding time


    private Dictionary<NPC_AI, float> updateTimers = new Dictionary<NPC_AI, float>();

    public int maxBeansToSwitch = 3; // Num beans that can switch spots at the same time
    private List<NPC_AI> beansToSwitch = new List<NPC_AI>(); // beans chosen to switch

    private Dictionary<NPC_AI, float> activeTimers = new Dictionary<NPC_AI, float>(); // Only track selected beans time
    private HashSet<NPC_AI> recentlySwitched = new HashSet<NPC_AI>(); // Track recently moved beans

    private List<Transform> shelfPositions = new List<Transform>();



    private GameObject player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //Debug.Log("[AIManager] Instance assigned.");
        }
        else
        {
            //Debug.LogError("[AIManager] Duplicate instance detected! Destroying.");
            Destroy(gameObject);
            return;
        }

        LoadNavGraphData();
        FindAllHidingSpots();


    }

    private void Start()
    {
        GatherShelfPositions();
        player = GameObject.FindGameObjectWithTag("Player");
        beanFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.beanFootsteps);
    }

    private void Update()
    {
        


        // Reset game if bean count is low.
        if (npcList.Count == beansLow && !isResetting)
        {
            //Debug.LogWarning("[AIManager] Bean count low! Triggering reset...");
            StartCoroutine(ResetGameCoroutine());
        }



        // Process each NPC in the list.
        foreach (NPC_AI npc in npcList)
        {
            if(npc==null)
            {
                continue;
            }


            if (npc.IsPickedUp())
            {
                
                continue;
            }

            //must have an active NavMeshAgent.
            if (npc.navMeshAgent == null || !npc.navMeshAgent.enabled || !npc.navMeshAgent.isOnNavMesh)
            {
               // Debug.LogWarning($"[AIManager] {npc.gameObject.name} not on valid NavMesh. Skipping.");
                continue;
            }

            // Ensure the agent is not stopped.
            if (npc.navMeshAgent.isStopped)
            {
                npc.navMeshAgent.isStopped = false;
            }

            // Initialize timer if missing.
            if (!hidingTimers.ContainsKey(npc))
            {
                hidingTimers[npc] = Time.time;
            }
                

            // Centralize all state transitions.
            EvaluateNPCState(npc);



            if (!nextDecisionTimes.ContainsKey(npc))
            {
                nextDecisionTimes[npc] = Time.time + Random.Range(0.2f, 1.0f);
            }
                
            if (Time.time >= nextDecisionTimes[npc])
            {

                nextDecisionTimes[npc] = Time.time + Random.Range(0.5f, 2.0f);
            }

            if (Mathf.Abs(npc.navMeshAgent.velocity.x) >= 0.5f || Mathf.Abs(npc.navMeshAgent.velocity.z) >= 0.5f)
            {

                npc.PlayBeanMoveSound(true);
            }
            else
            {
                npc.PlayBeanMoveSound(false);
            }
                
        }
    }


    private void LoadNavGraphData()
    {
        if (navGraph == null)
        {
            //Debug.LogError("[AIManager] NavGraph is missing! NPCs cannot navigate.");
            return;
        }

        nodeDataList = navGraph.nodes;
       // Debug.Log($"[AIManager] Loaded {nodeDataList.Count} nodes from precomputed NavGraph.");
    }


    private int GetNodeIndex(NavGraph.NavNodeData nodeData)
    {
        if(nodeData == null)
        {
            return -1;
        }


        for (int i = 0; i < nodeDataList.Count; i++)
        {
            if (Vector3.Distance(nodeDataList[i].position, nodeData.position) < 0.01f)
                return i;
        }
        return -1;
    }


    private List<NavGraph.NavNodeData> GetPrecomputedRoute(Vector3 startPos, Vector3 targetPos)
    {
        // Find the node closest to start and target.
        NavGraph.NavNodeData startNode = FindClosestNodeData(startPos);
        NavGraph.NavNodeData targetNode = FindClosestNodeData(targetPos);
        if (startNode == null || targetNode == null)
            return null;

        int sourceIndex = GetNodeIndex(startNode);
        int destIndex = GetNodeIndex(targetNode);
        if (sourceIndex < 0 || destIndex < 0)
            return null;

        // Look through precomputed routes for directly from source to destination.
        foreach (var route in navGraph.precomputedRoutes)
        {
            if (route.sourceIndex == sourceIndex && route.destinationIndex == destIndex)
            {
                if (route.pathIndices != null && route.pathIndices.Count > 0)
                {
                    List<NavGraph.NavNodeData> routeData = new List<NavGraph.NavNodeData>();
                    // Here we take the first candidate route.
                    List<int> chosenRoute = route.pathIndices[0];
                    foreach (int index in chosenRoute)
                    {
                        routeData.Add(nodeDataList[index]);
                    }
                    return routeData;
                }
            }
        }
        return null;
    }


    //chain multiple precomputed route segments together
    private List<NavGraph.NavNodeData> GetCombinedPrecomputedRoute(Vector3 startPos, Vector3 targetPos)
    {

        if (startPos == null || targetPos == null)
            return null;

        float tolerance = 1.0f; 
        List<NavGraph.NavNodeData> combinedRoute = new List<NavGraph.NavNodeData>();
        Vector3 currentStart = startPos;
        int maxIterations = 10; 

        for (int i = 0; i < maxIterations; i++)
        {
            // Get a segment from currentStart to targetPos.
            List<NavGraph.NavNodeData> segment = GetPrecomputedRoute(currentStart, targetPos);
            if (segment == null || segment.Count == 0)
            {
                // No route found for this segment.
                break;
            }
      
            if (combinedRoute.Count > 0)
            {
                if (Vector3.Distance(combinedRoute[combinedRoute.Count - 1].position, segment[0].position) < 0.01f)
                {
                    segment.RemoveAt(0);
                }
            }
            combinedRoute.AddRange(segment);

            currentStart = combinedRoute[combinedRoute.Count - 1].position;
            
            if (Vector3.Distance(currentStart, targetPos) <= tolerance)
            {
                return combinedRoute;
            }
        }

        if (combinedRoute.Count > 0 && Vector3.Distance(combinedRoute[combinedRoute.Count - 1].position, targetPos) <= tolerance)
            return combinedRoute;
        return null;
    }


    public Vector3 GetRandomNavMeshPosition()
    {
        if (nodeDataList == null || nodeDataList.Count == 0)
        {
            Debug.LogError("No nav data available for spawning.");
            return Vector3.zero;
        }

        int floorLayer = LayerMask.NameToLayer("Floor");
        List<NavGraph.NavNodeData> floorNodes = new List<NavGraph.NavNodeData>();

        foreach (var nodeData in nodeDataList)
        {
            NavNode navNode = GetNavNodeFromNodeData(nodeData);
            if (navNode != null && navNode.gameObject.layer == floorLayer)
            {
                floorNodes.Add(nodeData);
            }
        }

        Debug.Log($"Found {floorNodes.Count} Floor nodes out of {nodeDataList.Count} total nodes.");

        // If no floor node is found, fallback to all nodes.
        if (floorNodes.Count == 0)
        {
            Debug.LogWarning("No Floor nav nodes found, falling back to all nodes.");
            floorNodes = new List<NavGraph.NavNodeData>(nodeDataList);
        }

        int randomIndex = Random.Range(0, floorNodes.Count);
        Vector3 spawnPosition = floorNodes[randomIndex].position;
        Debug.Log($"Chosen spawn position: {spawnPosition}");
        return spawnPosition;
    }



    public void GatherShelfColliders()
    {
        shelfColliders.Clear();
        GameObject[] shelves = GameObject.FindGameObjectsWithTag("Shelf");
        foreach (GameObject shelf in shelves)
        {
            Collider col = shelf.GetComponent<Collider>();
            if (col != null)
            {
                shelfColliders.Add(col);
            }
        }
        Debug.Log("Found " + shelfColliders.Count + " shelf colliders.");
    }



    public void GatherShelfPositions()
    {
        //shelfPositions.Clear();
        //foreach (Hiding_Spots spot in hidingSpots)
        //{
        //    // heck both the hiding type and the layer
        //    if (spot.hidingType == Hiding_Spots.HidingType.Shelf || spot.gameObject.layer == LayerMask.NameToLayer("Shelf"))
        //    {
        //        shelfPositions.Add(spot.transform);
        //    }
        //}
        //Debug.Log("Shelf positions gathered: " + shelfPositions.Count);
    }



    private NavNode GetNavNodeFromNodeData(NavGraph.NavNodeData nodeData)
    {
        foreach (NavNode nav in NavNode.GetAllNodes())
        {
            if (nav == null)
            {
                continue;
            }

            if (Vector3.Distance(nav.transform.position, nodeData.position) < 0.01f)
            {
                return nav;
            }
        }
        return null;
    }

    //public Vector3 GetRandomNavMeshPosition()
    //{
    //    if (nodeDataList == null || nodeDataList.Count == 0)
    //    {
    //       // Debug.LogError("[AIManager] No nav data available for spawning.");
    //        return Vector3.zero;
    //    }
    //    int randomIndex = Random.Range(0, nodeDataList.Count);
    //    return nodeDataList[randomIndex].position;
    //}


    public void RegisterNPC(NPC_AI npc)
    {
        if (npc == null)
        {
           // Debug.LogError("[AIManager] Attempted to register a NULL NPC!");
            return;
        }

        if (!npcList.Contains(npc))
        {
            npcList.Add(npc);
           // Debug.Log($"[AIManager] Registered NPC: {npc.gameObject.name}");
        }
    }


    public void UnregisterNPC(NPC_AI npc)
    {
        if (npcList.Contains(npc))
        {
            npcList.Remove(npc);
        }
    }

    private void FindAllHidingSpots()
    {
        hidingSpots.Clear();
        hidingSpots.AddRange(FindObjectsOfType<Hiding_Spots>());

        if (hidingSpots.Count == 0)
        {
            //Debug.LogError("[AIManager] No hiding spots found in the scene! NPCs cannot hide.");
        }
        else
        {
           // Debug.Log($"[AIManager] Found {hidingSpots.Count} Hiding Spots.");
        }
    }


    public Vector3 GetRandomHidingSpot()
    {
        if (hidingSpots.Count == 0)
        {
            //Debug.LogWarning("[AIManager] No hiding spots available.");
            return Vector3.zero;
        }

        Hiding_Spots randomSpot = hidingSpots[Random.Range(0, hidingSpots.Count)];
        return randomSpot.transform.position;
    }


    public List<Hiding_Spots> GetAvailableHidingSpots()
    {
        return hidingSpots;
    }










    public void EvaluateNPCState(NPC_AI npc)
    {

        if (npc == null || !npc.navMeshAgent.enabled || !npc.navMeshAgent.isOnNavMesh)
        {
            return;
        }
        


        float distanceToPlayer = Vector3.Distance(npc.transform.position, GetPlayerPosition());

        // If the bean is already in Hiding, do nothing.
        if (npc.state == NPC_AI.NPCState.Hiding)
        {
            Hiding_Spots currentSpot = npc.GetHidingSpot();

            if (currentSpot != null && (currentSpot.hidingType == Hiding_Spots.HidingType.Shelf 
                || currentSpot.gameObject.CompareTag("Shelf")))
            {
                // If the bean has moved away from the shelf hiding spot 
                float distToSpot = Vector3.Distance(npc.transform.position, currentSpot.transform.position);

                if (distToSpot > npc.navMeshAgent.stoppingDistance + 0.3f)
                {
                    // Immediately release the shelf hiding spot with teleportation
                    npc.state = NPC_AI.NPCState.Idle;
                    ReleaseCurrentHidingSpot(npc, teleportIfShelf: true);
       
                    return;
                }
            }

            MaintainCover(npc);
            npc.PlayBeanMoveSound(false);
            return;
        }

        if (distanceToPlayer < npc.runRange)
        {
            // If bean is idle or lready running but has reached its escape destination
            if (npc.state == NPC_AI.NPCState.Idle ||
               (npc.state == NPC_AI.NPCState.Running &&
                !npc.navMeshAgent.pathPending &&
                npc.navMeshAgent.remainingDistance <= npc.navMeshAgent.stoppingDistance))
            {
                npc.state = NPC_AI.NPCState.Running;
                // Rlease any current hiding spot assignment.
                if (npcHidingAssignments.ContainsKey(npc))
                {
                    npcHidingAssignments[npc].DecrementOccupancy();
                    npcHidingAssignments.Remove(npc);
                }
                npc.SetHidingSpot(null);
                npc.navMeshAgent.ResetPath();
                npc.navMeshAgent.isStopped = false;
                AssignEscapeRoute(npc);
            }
        }
        else if (distanceToPlayer > npc.runRange * 2)
        {
            if (npc.state == NPC_AI.NPCState.Running)
            {
               // Debug.Log($"[EvaluateNPCState] {npc.gameObject.name} has run far enough. Transitioning from Running to Idle.");
                npc.state = NPC_AI.NPCState.Idle;
                npc.navMeshAgent.ResetPath(); // Clear the previous escape route.
                AssignNewHidingSpot(npc, true);
            }
        }
        
        if (npc.state == NPC_AI.NPCState.Idle)
        {
            // If a hiding spot is assigne
            if (npc.GetHidingSpot() != null)
            {
                float distanceToSpot = Vector3.Distance(npc.transform.position, npc.GetHidingSpot().transform.position);

                if (distanceToSpot <= npc.navMeshAgent.stoppingDistance + 0.3f)
                {
                    //Debug.Log($"[EvaluateNPCState] {npc.gameObject.name} has reached its hiding spot. Transitioning from Idle to Hiding.");
                    npc.OnReachedHidingSpot();
                }

            }
            else
            {
               
                //Debug.Log($"[EvaluateNPCState] {npc.gameObject.name} has no hiding spot. Assigning new hiding spot.");
                AssignNewHidingSpot(npc, false);
            }
        }
    }







    public void ResetHidingTimerForNPC(NPC_AI npc)
    {
        if (npc == null)
        {
            // Debug.LogError("[AIManager] Attempted to register a NULL NPC!");
            return;
        }
        if (hidingTimers.ContainsKey(npc))
        {
            hidingTimers[npc] = Time.time;
        }
        else
        {
            hidingTimers.Add(npc, Time.time);
        }
    }



    private void AssignEscapeRoute(NPC_AI npc)
    {
        if (npc == null)
            return;

        npc.state = NPC_AI.NPCState.Running;
        npc.SetHidingSpot(null);
        npc.navMeshAgent.ResetPath();
        npc.navMeshAgent.velocity = Vector3.zero;
        npc.navMeshAgent.isStopped = false;

        float targetDistance = npc.runRange * 3f;
        List<NavGraph.NavNodeData> route = GetPrecomputedEscapeRoute(npc, targetDistance);
        if (route != null && route.Count > 0)
        {
            

            StartCoroutine(FollowEscapeRoute(npc, route));
        }
        else
        {
            // choose a random nav node at least 2*runRange away from the player.
            float minDistance = npc.runRange * 2f;
            NavGraph.NavNodeData randomNode = GetRandomNavNodeAwayFromPlayer(minDistance);
            if (randomNode != null)
            {
                npc.MoveTo(randomNode.position);
            }
            else
            {
                // If no valid node is found, direct escape.
                Vector3 escapeDir = npc.transform.position - GetPlayerPosition();
                if (escapeDir.sqrMagnitude < 0.01f)
                {
                    escapeDir = Random.onUnitSphere;
                    escapeDir.y = 0; 
                    if (escapeDir == Vector3.zero)
                        escapeDir = Vector3.forward;
                }
                npc.MoveTo(npc.transform.position + escapeDir.normalized * targetDistance);
            }
            npc.hasReachedRouteEnd = true;
        }
    }


    private NavGraph.NavNodeData GetRandomNavNodeAwayFromPlayer(float minDistance)
    {
        Vector3 playerPos = GetPlayerPosition();
        List<NavGraph.NavNodeData> candidates = new List<NavGraph.NavNodeData>();

        foreach (var node in nodeDataList)
        {
            if (Vector3.Distance(node.position, playerPos) >= minDistance)
            {
                candidates.Add(node);
            }
        }

        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];

        return null;
    }


    private List<NavGraph.NavNodeData> GetPrecomputedEscapeRoute(NPC_AI npc, float targetDistance)
    {
        if (npc == null)
        {
            // Debug.LogError("[AIManager] Attempted to register a NULL NPC!");
            return null;
        }
        // Get the node nearest to the NPC's current position.
        NavGraph.NavNodeData startNode = FindClosestNodeData(npc.transform.position);
        if (startNode == null)
            return null;
        int sourceIndex = GetNodeIndex(startNode);
        if (sourceIndex < 0)
            return null;

        // Collect candidate routes that start at this source.
        List<NavGraph.RouteData> candidateRoutes = new List<NavGraph.RouteData>();
        foreach (var route in navGraph.precomputedRoutes)
        {
            if (route.sourceIndex == sourceIndex)
            {
                if (route.pathIndices != null && route.pathIndices.Count > 0)
                {
                    
                    List<int> candidate = route.pathIndices[0];


                    int lastIndex = candidate[candidate.Count - 1];

                    Vector3 finalPos = navGraph.nodes[lastIndex].position;
                    // Only consider routes that end sufficiently far from the player.
                    if (Vector3.Distance(finalPos, GetPlayerPosition()) >= targetDistance)
                        candidateRoutes.Add(route);
                }
            }
        }
        if (candidateRoutes.Count == 0)
            return null;

        // Sort candidate routes by hop count 
        candidateRoutes.Sort((a, b) => a.pathIndices[0].Count.CompareTo(b.pathIndices[0].Count));
        NavGraph.RouteData bestRoute = candidateRoutes[0];
        List<int> chosenRoute = bestRoute.pathIndices[0];

        List<NavGraph.NavNodeData> routeData = new List<NavGraph.NavNodeData>();
        foreach (int index in chosenRoute)
        {
            routeData.Add(nodeDataList[index]);
        }
        return routeData;
    }




    private IEnumerator FollowEscapeRoute(NPC_AI npc, List<NavGraph.NavNodeData> route)
    {
        if (npc == null)
        {
            // Debug.LogError("[AIManager] Attempted to register a NULL NPC!");
            yield break;
        }

        if(route == null)
        {
            yield return null;
        }
            

        foreach (var nodeData in route)
        {


            if (!npc.navMeshAgent.isOnNavMesh)
                yield break;

            npc.MoveTo(nodeData.position);
 
            int iterations = 0;

            while (npc.navMeshAgent.isOnNavMesh && (npc.navMeshAgent.pathPending ||
                    npc.navMeshAgent.remainingDistance > npc.navMeshAgent.stoppingDistance + 0.1f))
            {
                yield return null;
            }
        }
        npc.state = NPC_AI.NPCState.Idle;
        AssignNewHidingSpot(npc, true);
    }








    public void ReleaseCurrentHidingSpot(NPC_AI npc, bool teleportIfShelf = true)
    {
        if (npc == null)
            return;

        if (npc.GetHidingSpot() != null && npcHidingAssignments.ContainsKey(npc))
        {
            Hiding_Spots currentSpot = npc.GetHidingSpot();

            if (teleportIfShelf &&
                (currentSpot.hidingType == Hiding_Spots.HidingType.Shelf || currentSpot.gameObject.CompareTag("Shelf")))
            {
                TeleportBeanToNearestWarpNode(npc);
            }

            currentSpot.DecrementOccupancy();
            npcHidingAssignments.Remove(npc);
            npc.SetHidingSpot(null);
        }
    }





    public void AssignNewHidingSpot(NPC_AI npc, bool run, bool teleportIfShelf = false)
    {


        if (npc == null)
        {
            return;
        }

        if (npc.state == NPC_AI.NPCState.Running)
        {
            return;
        }


        ReleaseCurrentHidingSpot(npc, teleportIfShelf);
        npc.state = NPC_AI.NPCState.Idle;

        if (hidingSpots == null || hidingSpots.Count == 0)
        {
            return;
        }

        // Build a list of valid hiding spots
        List<Hiding_Spots> validSpots = new List<Hiding_Spots>();
        foreach (var spot in hidingSpots)
        {
            //if (spot == null || !spot.IsAvailable())
            //    continue;
            //if (IsSpotOverCapacity(spot))
            //    continue;
            //if (Vector3.Distance(spot.transform.position, GetPlayerPosition()) < npc.runRange)
            //    continue;
            if (run == true)
            {

                if (Vector3.Distance(spot.transform.position, npc.transform.position) < npc.runRange * 2)
                {
                    validSpots.Add(spot);
                }
               
                    

            }
            else
            {
                validSpots.Add(spot);
            }

            if (spot.hidingType == Hiding_Spots.HidingType.Cage)
            {
                continue;
            }


            //validSpots.Add(spot);
        }

        Hiding_Spots lastSpot = npc.GetLastHidingSpot();

        if (lastSpot != null && (lastSpot.hidingType == Hiding_Spots.HidingType.Shelf || lastSpot.gameObject.CompareTag("Shelf")))
        {
            validSpots = validSpots.Where(spot => !(spot.hidingType == Hiding_Spots.HidingType.Shelf || spot.gameObject.CompareTag("Shelf"))).ToList();
        }

        if (lastSpot != null)
        {
            validSpots.Remove(lastSpot);
        }

        // Use precomputed routes to choose a candidate spot.
        List<Hiding_Spots> candidateSpots = new List<Hiding_Spots>();
        Dictionary<Hiding_Spots, List<NavGraph.NavNodeData>> spotRoutesData = new Dictionary<Hiding_Spots, List<NavGraph.NavNodeData>>();
        float safeThreshold = npc.runRange;

        foreach (var spot in validSpots)
        {
            // Get a precomputed route between the NPC and the hiding spot.
            List<NavGraph.NavNodeData> safeRoute = GetPrecomputedRoute(npc.transform.position, spot.transform.position);
            if (safeRoute != null && safeRoute.Count > 0)
            {
                
                candidateSpots.Add(spot);
                spotRoutesData[spot] = safeRoute;
            }
        }

        if (candidateSpots.Count > 0)
        {
            Hiding_Spots chosenSpot = candidateSpots[Random.Range(0, candidateSpots.Count)];
            List<NavGraph.NavNodeData> chosenRoute = spotRoutesData[chosenSpot];

            npc.SetLastHidingSpot(chosenSpot);
            npcHidingAssignments[npc] = chosenSpot;
            //chosenSpot.IncrementOccupancy();
            npc.SetHidingSpot(chosenSpot);

            StartCoroutine(FollowEscapeRoute(npc, chosenRoute));
            StartCoroutine(ResolveHidingSpotConflict(npc, chosenSpot));
        }
        else
        {
            if (validSpots.Count > 0)
            {
                Hiding_Spots chosenSpot = validSpots[Random.Range(0, validSpots.Count)];
                npc.SetLastHidingSpot(chosenSpot);
                npcHidingAssignments[npc] = chosenSpot;
                npc.SetHidingSpot(chosenSpot);
                npc.MoveTo(chosenSpot.transform.position);

                StartCoroutine(ResolveHidingSpotConflict(npc, chosenSpot));
            }
        }
    }

    private bool IsOnShelf()
    {
        
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Shelf"))
                return true;
        }
        return false;
    }

    private void TeleportBeanToNearestWarpNode(NPC_AI npc)
    {
        GameObject[] warpNodes = GameObject.FindGameObjectsWithTag("warpNode");
        if (warpNodes.Length == 0)
        {
            Debug.LogError("No warpNodes found!");
            return;
        }

        GameObject closestWarpNode = null;
        float closestDistance = Mathf.Infinity;

        foreach (var warpNode in warpNodes)
        {
            float distance = Vector3.Distance(npc.transform.position, warpNode.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestWarpNode = warpNode;
            }
        }

        if (closestWarpNode != null)
        {
            npc.navMeshAgent.Warp(closestWarpNode.transform.position);
        }
    }

    /// you know what you did...
    ////////////////////////////////////private IEnumerator DelayedReassign(NPC_AI npc, float delay)
    ////////////////////////////////////{
    ////////////////////////////////////    yield return new WaitForSeconds(delay);   Fuck this peice of shit, seriously fucking die!!!!!!
    ////////////////////////////////////    // Only reassign if the NPC still isn't safely hidden.    
    ////////////////////////////////////    if (npc.state != NPC_AI.NPCState.Hiding)
    ////////////////////////////////////        AssignNewHidingSpot(npc, false);
    ////////////////////////////////////}



    private List<NavGraph.NavNodeData> FindSafeRouteData(NPC_AI npc, NavGraph.NavNodeData start, NavGraph.NavNodeData target, float safeDistance)
    {
        if (npc == null)
        {
            // Debug.LogError("[AIManager] Attempted to register a NULL NPC!");
            return null;
        }


        Queue<List<NavGraph.NavNodeData>> routesQueue = new Queue<List<NavGraph.NavNodeData>>();
        routesQueue.Enqueue(new List<NavGraph.NavNodeData> { start });
        Vector3 playerPos = GetPlayerPosition();

        while (routesQueue.Count > 0)
        {
            List<NavGraph.NavNodeData> currentRoute = routesQueue.Dequeue();
            NavGraph.NavNodeData currentNode = currentRoute[currentRoute.Count - 1];

            if (currentNode == target)
            {
                if (!IsPathThroughPlayer(currentNode.position, target.position))
                    return currentRoute;
            }

            foreach (int neighborIndex in currentNode.connectedNodeIndices)
            {
                if (neighborIndex < 0 || neighborIndex >= nodeDataList.Count)
                    continue;
                NavGraph.NavNodeData neighbor = nodeDataList[neighborIndex];
                if (currentRoute.Contains(neighbor))
                    continue;
                if (neighbor != target && Vector3.Distance(neighbor.position, playerPos) < safeDistance)
                    continue;
                List<NavGraph.NavNodeData> newRoute = new List<NavGraph.NavNodeData>(currentRoute) { neighbor };
                routesQueue.Enqueue(newRoute);
            }
        }
        return null;
    }








    private float DistanceFromPointToLineSegment(Vector3 point, Vector3 start, Vector3 end)
    {
        Vector3 segment = end - start;
        if (segment.sqrMagnitude == 0)
            return Vector3.Distance(point, start);
        float t = Vector3.Dot(point - start, segment) / segment.sqrMagnitude;
        t = Mathf.Clamp01(t);
        Vector3 projection = start + t * segment;
        return Vector3.Distance(point, projection);
    }


    private bool IsPathThroughPlayer(Vector3 start, Vector3 end)
    {
        Vector3 playerPos = GetPlayerPosition();
        float threshold = 1.0f;
        float distance = DistanceFromPointToLineSegment(playerPos, start, end);
        return distance < threshold;
    }




    private NavGraph.NavNodeData FindClosestNodeData(Vector3 position)
    {
        NavGraph.NavNodeData closest = null;
        float minDistance = float.MaxValue;
        foreach (var node in nodeDataList)
        {
            float d = Vector3.Distance(position, node.position);
            if (d < minDistance)
            {
                minDistance = d;
                closest = node;
            }
        }
        return closest;
    }








    private IEnumerator ResolveHidingSpotConflict(NPC_AI npc, Hiding_Spots chosenSpot)
    {
        if (npc == null || chosenSpot == null)
        {
            // Debug.LogError("[AIManager] Attempted to register a NULL NPC!");
            yield break;
        }

        yield return new WaitForSeconds(0.02f);

        List<NPC_AI> competingNPCs = new List<NPC_AI>();



        foreach (var assignment in npcHidingAssignments)
        {
            if (assignment.Key == null)
            {
                continue;
            }


            if (assignment.Value == chosenSpot)
            {
                competingNPCs.Add(assignment.Key);
            }
        }

        competingNPCs = competingNPCs.Where(npc => npc != null).ToList();



            competingNPCs.Sort((a, b) =>
           Vector3.Distance(a.transform.position, chosenSpot.transform.position)
           .CompareTo(Vector3.Distance(b.transform.position, chosenSpot.transform.position)));
        

        foreach (var challenger in competingNPCs.ToList())
        {
            if (chosenSpot.currentOccupancy < chosenSpot.MaxOccupancy)
            {

                if (challenger == null)
                {
                    continue;
                }

                npcHidingAssignments[challenger] = chosenSpot;
                chosenSpot.IncrementOccupancy();
                competingNPCs.Remove(challenger);
                
            }
            else
            {
                continue;
            }
        }

        foreach (var challenger in competingNPCs.ToList())
        {


            if (challenger == null)
            {
                continue;
            }
            AssignNewHidingSpot(challenger, false);
        }
    }




    private bool IsSpotOverCapacity(Hiding_Spots spot)
    {
        if (spot == null) return true;

        int incomingNPCs = 0;

        // Count how many NPCs are currently heading to this spot
        foreach (var assignment in npcHidingAssignments)
        {
            if (assignment.Value == spot)
            {
                incomingNPCs++;
            }
        }

        
        //Debug.Log($"[AIManager] Checking capacity for {spot.name}: Occupancy = {spot.Occupancy}, Incoming = {incomingNPCs}, Max = {spot.MaxOccupancy}");

       
        return (spot.Occupancy + incomingNPCs) >= spot.MaxOccupancy;
    }





    public void MaintainCover(NPC_AI npc)
    {
        if (npc == null)
        {
            return;
        }

        if (npc.animator != null && npc.playedOnce == false)
        {
            float normalizedTime = npc.stateInfo.normalizedTime % 1f;
            npc.animator.Play(npc.nonLooping, 0, normalizedTime);
            npc.playedOnce = true;
        }

       
        Hiding_Spots hidingSpot = npc.GetHidingSpot();
        if (hidingSpot == null)
        {
            return;
        }

        Vector3 playerPosition = GetPlayerPosition();
        Vector3 hidingSpotPosition = hidingSpot.transform.position;
        float distanceToPlayer = Vector3.Distance(npc.transform.position, playerPosition);

        // update position only at set intervals.
        if (!updateTimers.ContainsKey(npc))
        {
            updateTimers[npc] = Time.time + Random.Range(0f, updateInterval);
        }
        if (Time.time - updateTimers[npc] < updateInterval)
        {
            return;
        }
        updateTimers[npc] = Time.time; // Reset timer

        // Calculate the direction vector from the hiding spot to the player.
        Vector3 toPlayer = (playerPosition - hidingSpotPosition).normalized;
        Vector3 idealHidingPos = hidingSpotPosition - (toPlayer * 0.35f);

        Vector3 targetPosition = Vector3.zero;
        




        switch (hidingSpot.hidingType)
        {
            case Hiding_Spots.HidingType.BehindCover:
                {

                    targetPosition = hidingSpotPosition - toPlayer * 0.35f;
                    break;
                }
            case Hiding_Spots.HidingType.InsideCover:
                {

                    targetPosition = hidingSpotPosition;
                    break;
                }
            case Hiding_Spots.HidingType.Underneath:
                {

                    targetPosition = hidingSpotPosition - toPlayer * 0.15f;
                    break;
                }
            case Hiding_Spots.HidingType.Shelf:
                {

                    targetPosition = hidingSpotPosition - toPlayer * 0.35f;
                    //targetPosition = CalculateSafeOffsetForShelf(hidingSpot, npc);
                    break;
                }
            default:
                {
                    // Fallback: use BehindCover behavior.
                    targetPosition = hidingSpotPosition - toPlayer * 0.35f;
                    break;
                }
        }

        // If the bean is not near the target and the spot is not a cage, move toward the target.
        if (Vector3.Distance(npc.transform.position, targetPosition) > 0.2f && !hidingSpot.IsCage())
        {
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                npc.MoveTo(hit.position);
            }
        }
        else if (hidingSpot.IsCage())
        {
            npc.MoveTo(hidingSpot.transform.position);
        }

        // If the player is very close, keep the bean hidden.
        if (distanceToPlayer < npc.runRange)
        {
            return;
        }

        // Additional timing logic for switching hiding spots.
        if (!activeTimers.ContainsKey(npc) && hidingSpot.IsTrap())
        {
            beansToSwitch.Add(npc);
            activeTimers[npc] = Time.time + 10;
        }
        if (!activeTimers.ContainsKey(npc) && hidingSpot.IsCage())
        {
            // Additional cage-specific logic (if needed).
        }
        if (!activeTimers.ContainsKey(npc) && beansToSwitch.Count < maxBeansToSwitch &&
            !recentlySwitched.Contains(npc) && !hidingSpot.IsTrap() && !hidingSpot.IsCage())
        {
            beansToSwitch.Add(npc);
            activeTimers[npc] = Time.time + Random.Range(hidingDurationMin, hidingDurationMax);
        }

        // Check timers and reassign hiding spots if necessary.
        if (activeTimers.ContainsKey(npc))
        {
            if (Time.time >= activeTimers[npc])
            {
                ReleaseCurrentHidingSpot(npc, teleportIfShelf: true); // Explicitly release spot here with teleport check
                AssignNewHidingSpot(npc, false); // Explicitly assign new spot afterward
                npc.playedOnce = false;
                npc.animator.Play(npc.looping);
                recentlySwitched.Add(npc);
                activeTimers.Remove(npc);
                beansToSwitch.Remove(npc);
                StartCoroutine(RemoveFromRecentlySwitched(npc, Random.Range(5f, 15f)));
            }
        }
    }

    private Vector3 CalculateSafeOffsetForShelf(Hiding_Spots shelfSpot, NPC_AI npc)
    {
        Vector3 shelfPos = shelfSpot.transform.position;
        Vector3 playerPos = AIManager.Instance.GetPlayerPosition();
        
        playerPos.y = shelfPos.y;
        Vector3 toPlayer = (playerPos - shelfPos).normalized;
        float offsetDistance = 0.35f; 
        return shelfPos - toPlayer * offsetDistance;
    }





    // remove beanss from the recently switched list after a cooldown
    private IEnumerator RemoveFromRecentlySwitched(NPC_AI npc, float delay)
    {
        yield return new WaitForSeconds(delay);
        recentlySwitched.Remove(npc);
    }












    private void ResetGame()
    {
        //Debug.Log("[AIManager] Resetting game...");

        // Clear NPC List and Assignments
        npcList.Clear();
        npcHidingAssignments.Clear();
        hidingTimers.Clear();
        nextDecisionTimes.Clear();
        recentlySwitched.Clear();
        activeTimers.Clear();
        beansToSwitch.Clear();

        // Reset all hiding spots
        foreach (var spot in hidingSpots)
        {
            if (spot != null)
            {
                spot.ResetHidingSpot();
                //spot.DecrementOccupancy(); // Ensure all spots start empty
            }
        }

        // Spawn 6 new beans
        StartCoroutine(RespawnBeans(beanRestock));
    }

    private IEnumerator ResetGameCoroutine()
    {
        isResetting = true;

        ResetGame();

        yield return new WaitForSeconds(5f); // Wait to prevent instant looping

        isResetting = false;
    }


    private IEnumerator RespawnBeans(int count)
    {
        yield return new WaitForSeconds(1f); // Small delay before respawning

        //Debug.Log($"[AIManager] Spawning {count} new beans...");

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = AIManager.Instance.GetRandomNavMeshPosition();
            if (spawnPosition == Vector3.zero)
            {
               // Debug.LogError("[AIManager] No valid spawn location found!");
                continue;
            }

            // Use GameManager's beanPrefab instead of Resources.Load()
            GameObject beanPrefab = GameManager.Instance.beanPrefab;
            //if (beanPrefab == null)
            //{
            //    Debug.LogError("[AIManager] ERROR: GameManager's beanPrefab is NULL! Cannot spawn beans.");
            //    return;
            //}

            GameObject newBean = Instantiate(beanPrefab, spawnPosition, Quaternion.identity);

            NPC_AI newNPC = newBean.GetComponent<NPC_AI>();
            if (newNPC != null)
            {
                RegisterNPC(newNPC);
            }
        }
    }





    public Vector3 GetPlayerPosition()
    {
        return player != null ? player.transform.position : Vector3.zero;
    }
}


