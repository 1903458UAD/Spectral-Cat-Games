using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using FMOD.Studio;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [Header("AI Elements")]
    private List<NPC_AI> npcList = new List<NPC_AI>();
    private List<Hiding_Spots> hidingSpots = new List<Hiding_Spots>();
    private Dictionary<NPC_AI, Hiding_Spots> npcHidingAssignments = new Dictionary<NPC_AI, Hiding_Spots>();

    private Dictionary<NPC_AI, float> hidingTimers = new Dictionary<NPC_AI, float>();
    private float hidingDuration = 10f; // Time before NPC moves to another spot


    private List<NavNode> navNodes = new List<NavNode>();

    private float decisionInterval = 0.5f;  // Every NPC updates decisions every 0.5s
    private Dictionary<NPC_AI, float> nextDecisionTimes = new Dictionary<NPC_AI, float>();

    private bool isResetting = false;

    private bool start = false;

    public int beansLow = 4;
    public int beanRestock = 6;



    public float maxRunTime = 8f; // Maximum timea bean is allowed to run continuously
    private Dictionary<NPC_AI, float> runStartTimes = new Dictionary<NPC_AI, float>();



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




    private GameObject player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[AIManager] Instance assigned.");
        }
        else
        {
            Debug.LogError("[AIManager] Duplicate instance detected! Destroying.");
            Destroy(gameObject);
            return;
        }

        FindAllHidingSpots();
        FindAllNavNodes();
    }

    private void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player");
        beanFootsteps = AudioManager.instance.CreateInstance(FMODEvents.instance.beanFootsteps);
    }

    private void Update()
    {
        // Reset game if bean count is low.
        if (npcList.Count == beansLow && !isResetting)
        {
            Debug.LogWarning("[AIManager] Bean count low! Triggering reset...");
            StartCoroutine(ResetGameCoroutine());
        }

        // Process each NPC in the list.
        foreach (NPC_AI npc in npcList)
        {
            if (npc.IsPickedUp()) continue;

            // Validate NPC: must have an active NavMeshAgent.
            if (npc.navMeshAgent == null || !npc.navMeshAgent.enabled || !npc.navMeshAgent.isOnNavMesh)
            {
                Debug.LogWarning($"[AIManager] {npc.gameObject.name} not on valid NavMesh. Skipping.");
                continue;
            }

            // Ensure the agent is not stopped.
            if (npc.navMeshAgent.isStopped)
                npc.navMeshAgent.isStopped = false;

            // Initialize timer if missing.
            if (!hidingTimers.ContainsKey(npc))
                hidingTimers[npc] = Time.time;

            // Centralize all state transitions.
            EvaluateNPCState(npc);

            // If the bean is in Hiding, let it maintain cover.
            if (npc.state == NPC_AI.NPCState.Hiding)
            {
                MaintainCover(npc);
            }

            
            if (!nextDecisionTimes.ContainsKey(npc))
                nextDecisionTimes[npc] = Time.time + Random.Range(0.2f, 1.0f);
            if (Time.time >= nextDecisionTimes[npc])
                nextDecisionTimes[npc] = Time.time + Random.Range(0.5f, 2.0f);

            if (Mathf.Abs(npc.navMeshAgent.velocity.x) >= 0.5f || Mathf.Abs(npc.navMeshAgent.velocity.z) >= 0.5f)
                npc.PlayBeanMoveSound(true);
            else
                npc.PlayBeanMoveSound(false);
        }
    }





    private List<NavNode> FindAllNavNodes()
    {
        navNodes.Clear(); // Clear existing nodes if any
        navNodes.AddRange(FindObjectsOfType<NavNode>()); // Populate navNodes list with all NavNode objects found in the scene

        if (navNodes.Count == 0)
        {
            Debug.LogError("[AIManager] No NavNodes found in the scene! NPCs cannot move.");
        }
        else
        {
            Debug.Log($"[AIManager] Found {navNodes.Count} NavNodes.");
        }

        return navNodes; // Return the list of NavNode objects
    }


    public Vector3 GetRandomNavMeshPosition()
    {
        if (navNodes == null || navNodes.Count == 0)
        {
            Debug.LogError("[AIManager] No NavNodes available for spawning.");
            return Vector3.zero;
        }

        NavNode randomNode = navNodes[Random.Range(0, navNodes.Count)];
        return randomNode.transform.position;
    }

    public void RegisterNPC(NPC_AI npc)
    {
        if (npc == null)
        {
            Debug.LogError("[AIManager] Attempted to register a NULL NPC!");
            return;
        }

        if (!npcList.Contains(npc))
        {
            npcList.Add(npc);
            Debug.Log($"[AIManager] Registered NPC: {npc.gameObject.name}");
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
            Debug.LogError("[AIManager] No hiding spots found in the scene! NPCs cannot hide.");
        }
        else
        {
            Debug.Log($"[AIManager] Found {hidingSpots.Count} Hiding Spots.");
        }
    }


    public Vector3 GetRandomHidingSpot()
    {
        if (hidingSpots.Count == 0)
        {
            Debug.LogWarning("[AIManager] No hiding spots available.");
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
        // Validate that the NPC and its NavMeshAgent are active.
        if (npc == null || !npc.navMeshAgent.enabled || !npc.navMeshAgent.isOnNavMesh)
            return;

        // If the bean is already in Hiding, do nothing.
        if (npc.state == NPC_AI.NPCState.Hiding)
            return;

        float distanceToPlayer = Vector3.Distance(npc.transform.position, GetPlayerPosition());

        if(npc.state == NPC_AI.NPCState.Hiding)
        {
            MaintainCover(npc);
        }
        else if (distanceToPlayer < npc.runRange)
        {
            if (npc.state != NPC_AI.NPCState.Running)
            {
                Debug.Log($"[EvaluateNPCState] {npc.gameObject.name} is too close to the player. Transitioning from Idle to Running.");
                npc.state = NPC_AI.NPCState.Running;
                // Release any current hiding spot assignment.
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
            return;
        }

        if (npc.state == NPC_AI.NPCState.Running)
        {
            if (distanceToPlayer > npc.runRange * 2)
            {
                Debug.Log($"[EvaluateNPCState] {npc.gameObject.name} has run far enough. Transitioning from Running to Idle.");
                npc.state = NPC_AI.NPCState.Idle;
                npc.navMeshAgent.ResetPath(); // Clear the previous escape route.
                AssignNewHidingSpot(npc, true);
            }
            return;
        }

        if (npc.state == NPC_AI.NPCState.Idle)
        {
            // If a hiding spot is assigne
            if (npc.GetHidingSpot() != null)
            {
                float distanceToSpot = Vector3.Distance(npc.transform.position, npc.GetHidingSpot().transform.position);
                
                if (distanceToSpot <= npc.navMeshAgent.stoppingDistance + 0.1f)
                {
                    Debug.Log($"[EvaluateNPCState] {npc.gameObject.name} has reached its hiding spot. Transitioning from Idle to Hiding.");
                    npc.OnReachedHidingSpot();  
                }
               
            }
            else
            {
                // No hiding spot assigned: assign one.
                Debug.Log($"[EvaluateNPCState] {npc.gameObject.name} has no hiding spot. Assigning new hiding spot.");
                AssignNewHidingSpot(npc, false);
            }
        }
    }







    public void ResetHidingTimerForNPC(NPC_AI npc)
    {

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
        int desiredHops = 4;


        List<NavNode> route = FindRandomEscapeRoute(npc, desiredHops, targetDistance);

        if (route != null && route.Count > 0)
        {
            Debug.Log($"[AIManager] {npc.gameObject.name} escape route found with {route.Count} hops (randomized).");
            StartCoroutine(FollowEscapeRoute(npc, route));
        }
        else
        {

            NavNode fallbackNode = FindFarthestNode(npc, new List<NavNode>());
            if (fallbackNode != null && Vector3.Distance(fallbackNode.transform.position, GetPlayerPosition()) >= targetDistance)
            {
                Debug.Log($"[AIManager] {npc.gameObject.name} using fallback escape route to node {fallbackNode.name}.");
                npc.MoveTo(fallbackNode.transform.position);
            }
            else
            {
                Vector3 directEscape = npc.transform.position + (npc.transform.position - GetPlayerPosition()).normalized * targetDistance;
                Debug.Log($"[AIManager] {npc.gameObject.name} using direct escape route.");
                npc.MoveTo(directEscape);
            }
        }
    }



    private List<NavNode> FindRandomEscapeRoute(NPC_AI npc, int hopCount, float targetDistance)
    {
        NavNode startNode = FindClosestNavNode(npc.transform.position);
        if (startNode == null)
        {
            Debug.LogWarning($"[FindRandomEscapeRoute] {npc.gameObject.name} has no starting NavNode!");
            return null;
        }

        List<NavNode> route = new List<NavNode> { startNode };
        Vector3 playerPos = GetPlayerPosition();


        for (int i = 0; i < hopCount; i++)
        {
            NavNode currentNode = route[route.Count - 1];
  
            List<NavNode> candidates = new List<NavNode>();
            foreach (NavNode node in currentNode.connectedNodes)
            {
                if (!route.Contains(node) && Vector3.Distance(node.transform.position, playerPos) >= npc.runRange)
                {
                    candidates.Add(node);
                }
            }

            if (candidates.Count == 0)
            {
     
                break;
            }
         
            NavNode chosen = candidates[Random.Range(0, candidates.Count)];
            route.Add(chosen);
        }

    
        if (Vector3.Distance(route[route.Count - 1].transform.position, playerPos) >= targetDistance)
        {
            return route;
        }
        else
        {
            Debug.LogWarning($"[FindRandomEscapeRoute] Final node is too close to the player.");
            return null;
        }
    }

    private IEnumerator FollowEscapeRoute(NPC_AI npc, List<NavNode> route)
    {
        foreach (NavNode node in route)
        {
            if (!npc.navMeshAgent.isOnNavMesh)
                yield break;

            npc.MoveTo(node.transform.position);
            while (npc.navMeshAgent.isOnNavMesh &&
                   (npc.navMeshAgent.pathPending || npc.navMeshAgent.remainingDistance > npc.navMeshAgent.stoppingDistance + 0.1f))
            {
                yield return null;
            }
        }
        npc.state = NPC_AI.NPCState.Idle;
        Debug.Log($"[AIManager] {npc.gameObject.name} has completed its escape route.");
        AssignNewHidingSpot(npc, true);
    }






    private List<NavNode> FindEscapeRoute(NPC_AI npc, float targetDistance)
    {
        NavNode startNode = FindClosestNavNode(npc.transform.position);
        if (startNode == null)
        {
            Debug.LogWarning($"[AIManager] {npc.gameObject.name} has no starting NavNode!");
            return null;
        }

        Queue<List<NavNode>> routesQueue = new Queue<List<NavNode>>();
        routesQueue.Enqueue(new List<NavNode> { startNode });
        Vector3 playerPos = GetPlayerPosition();

        while (routesQueue.Count > 0)
        {
            List<NavNode> currentRoute = routesQueue.Dequeue();
            NavNode currentNode = currentRoute[currentRoute.Count - 1];


            if (Vector3.Distance(currentNode.transform.position, playerPos) >= targetDistance)
            {
                return currentRoute;
            }

            foreach (NavNode connected in currentNode.connectedNodes)
            {
                // Avoid cycles.
                if (currentRoute.Contains(connected))
                    continue;

                if (Vector3.Distance(connected.transform.position, playerPos) < npc.runRange)
                    continue;

                List<NavNode> newRoute = new List<NavNode>(currentRoute) { connected };
                routesQueue.Enqueue(newRoute);
            }
        }

        // No valid route found.
        return null;
    }



    private NavNode FindFarthestNode(NPC_AI npc, List<NavNode> recentNodes)
    {
        NavNode farthestNode = null;
        float maxDistance = 0f;

        // Loop through all NavNodes, but avoid using the last 4 nodes
        foreach (NavNode node in FindAllNavNodes()) // Find all available nodes
        {
            // Skip the last 4 nodes
            if (recentNodes.Contains(node)) continue;

            // Calculate distance to player
            float distanceToPlayer = Vector3.Distance(node.transform.position, GetPlayerPosition());

            // Choose the farthest node from the player
            if (distanceToPlayer > maxDistance)
            {
                maxDistance = distanceToPlayer;
                farthestNode = node;
            }
        }

        return farthestNode;
    }









    public void ReleaseCurrentHidingSpot(NPC_AI npc)
    {
        if (npc.GetHidingSpot() != null && npcHidingAssignments.ContainsKey(npc))
        {
            Hiding_Spots currentSpot = npc.GetHidingSpot();
            currentSpot.DecrementOccupancy();
            npcHidingAssignments.Remove(npc);
            npc.SetHidingSpot(null);
        }
    }



    public void AssignNewHidingSpot(NPC_AI npc, bool run)
    {
        if (npc == null)
        {
            Debug.LogError("[AIManager] Tried to assign a hiding spot to a NULL NPC.");
            return;
        }

        if (npc.state == NPC_AI.NPCState.Running)
        {
            Debug.LogWarning($"[AIManager] Tried assigning hiding spot to {npc.gameObject.name} while RUNNING. Assignment aborted.");
            return;
        }

        ReleaseCurrentHidingSpot(npc);

        npc.state = NPC_AI.NPCState.Idle;

        if (hidingSpots == null || hidingSpots.Count == 0)
        {
            Debug.LogError("[AIManager] No hiding spots available!");
            return;
        }

        // Build an initial list of valid hiding spots.
        List<Hiding_Spots> validSpots = new List<Hiding_Spots>();

        foreach (var spot in hidingSpots)
        {
            if (spot == null || !spot.IsAvailable()) continue;
            if (IsSpotOverCapacity(spot)) continue;
            // Ensure the hiding spot itself is not too close to the player.
            if (Vector3.Distance(spot.transform.position, GetPlayerPosition()) < npc.runRange)
                continue;


            if (run == true)
            {
                if (Vector3.Distance(spot.transform.position, npc.transform.position) < npc.runRange)
                {
                    validSpots.Add(spot);
                }
                else
                {
                    continue;
                }
                continue;
            }



            validSpots.Add(spot);
            
        }

        // Remove the last used spot so the NPC doesn't immediately return there.
        Hiding_Spots lastSpot = npc.GetLastHidingSpot();
        if (lastSpot != null)
            validSpots.Remove(lastSpot);

        // Prepare a list of candidate spots that have a safe node route.
        List<Hiding_Spots> candidateSpots = new List<Hiding_Spots>();
        Dictionary<Hiding_Spots, List<NavNode>> spotRoutes = new Dictionary<Hiding_Spots, List<NavNode>>();
        float safeThreshold = npc.runRange; // Nodes must be at least this far from the player.

        foreach (var spot in validSpots)
        {
            // Find the closest node to the hiding spot.
            NavNode targetNode = FindClosestNavNode(spot.transform.position);
            // Find the NPC's current closest node.
            NavNode startNode = FindClosestNavNode(npc.transform.position);
            if (startNode == null || targetNode == null)
                continue;

            List<NavNode> safeRoute = FindSafeRoute(npc, startNode, targetNode, safeThreshold);
            // Additionally check that the final leg from the target node to the hiding spot is not blocked.
            if (safeRoute != null && !IsPathThroughPlayer(targetNode.transform.position, spot.transform.position))
            {
                candidateSpots.Add(spot);
                spotRoutes[spot] = safeRoute;
            }
        }

        if (candidateSpots.Count > 0)
        {
            // Choose one candidate hiding spot (you might prefer the one with the shortest route).
            Hiding_Spots chosenSpot = candidateSpots[Random.Range(0, candidateSpots.Count)];
            List<NavNode> chosenRoute = spotRoutes[chosenSpot];

            // Update NPC tracking.
            npc.SetLastHidingSpot(chosenSpot);
            npcHidingAssignments[npc] = chosenSpot;
            chosenSpot.IncrementOccupancy();
            npc.SetHidingSpot(chosenSpot);

            Debug.Log($"[AIManager] {npc.gameObject.name} assigned to hiding spot {chosenSpot.name} with a safe node route ({chosenRoute.Count} hops).");
            StartCoroutine(FollowHidingRoute(npc, chosenRoute, chosenSpot));
            StartCoroutine(ResolveHidingSpotConflict(npc, chosenSpot));
        }
        else
        {
            // Fallback: if no candidate safe route is found, try a direct move—but delay before reattempting to break the cycle.
            if (validSpots.Count > 0)
            {
                Hiding_Spots chosenSpot = validSpots[Random.Range(0, validSpots.Count)];
                npc.SetLastHidingSpot(chosenSpot);
                npcHidingAssignments[npc] = chosenSpot;
                chosenSpot.IncrementOccupancy();
                npc.SetHidingSpot(chosenSpot);
                Debug.LogWarning($"[AIManager] {npc.gameObject.name} has no safe node route to any hiding spot. Moving directly to {chosenSpot.name} and delaying reattempt.");
                npc.MoveTo(chosenSpot.transform.position);
                StartCoroutine(ResolveHidingSpotConflict(npc, chosenSpot));
                // Delay reassigning if the chosen spot is still blocked.
                StartCoroutine(DelayedReassign(npc, 1.0f));
            }
            else
            {
                Debug.LogWarning($"[AIManager] {npc.gameObject.name} has no valid hiding spots available.");
            }
        }
    }

    private IEnumerator DelayedReassign(NPC_AI npc, float delay)
    {
        yield return new WaitForSeconds(delay);
        // Only reassign if the NPC still isn't safely hidden.
        if (npc.state != NPC_AI.NPCState.Hiding)
            AssignNewHidingSpot(npc, false);
    }

    private IEnumerator FollowHidingRoute(NPC_AI npc, List<NavNode> route, Hiding_Spots targetSpot)
    {
        foreach (NavNode node in route)
        {
            if (!npc.navMeshAgent.isOnNavMesh)
                yield break;

            npc.MoveTo(node.transform.position);
            while (npc.navMeshAgent.isOnNavMesh &&
                   (npc.navMeshAgent.pathPending ||
                    npc.navMeshAgent.remainingDistance > npc.navMeshAgent.stoppingDistance + 0.1f))
            {
                yield return null;
            }
        }

        // Move from the last node to the hiding spot.
        npc.MoveTo(targetSpot.transform.position);
        while (npc.navMeshAgent.isOnNavMesh &&
               (npc.navMeshAgent.pathPending ||
                npc.navMeshAgent.remainingDistance > npc.navMeshAgent.stoppingDistance + 0.1f))
        {
            yield return null;
        }

        // Final check: if the direct path is blocked, reassign.
        if (IsPathThroughPlayer(npc.transform.position, targetSpot.transform.position))
        {
            Debug.LogWarning($"[AIManager] {npc.gameObject.name} reached {targetSpot.name} but the path is blocked. Reassigning after delay.");
            yield return new WaitForSeconds(0.5f);
            AssignNewHidingSpot(npc, true);
        }
        else
        {
            //npc.state = NPC_AI.NPCState.Hiding;
            Debug.Log($"[AIManager] {npc.gameObject.name} reached hiding spot {targetSpot.name} via a safe node route.");
        }
    }





    private List<NavNode> FindSafeRoute(NPC_AI npc, NavNode start, NavNode target, float safeDistance)
    {
        // Use BFS to find a route from 'start' to 'target'
        Queue<List<NavNode>> routesQueue = new Queue<List<NavNode>>();
        routesQueue.Enqueue(new List<NavNode> { start });
        Vector3 playerPos = GetPlayerPosition();

        while (routesQueue.Count > 0)
        {
            List<NavNode> currentRoute = routesQueue.Dequeue();
            NavNode currentNode = currentRoute[currentRoute.Count - 1];

            // If we've reached the target node, check the final leg:
            if (currentNode == target)
            {

                if (!IsPathThroughPlayer(currentNode.transform.position, target.transform.position))
                    return currentRoute;
            }

            // Expand the search.
            foreach (NavNode neighbor in currentNode.connectedNodes)
            {
                // Avoid cycles.
                if (currentRoute.Contains(neighbor))
                    continue;

          
                if (neighbor != target && Vector3.Distance(neighbor.transform.position, playerPos) < safeDistance)
                    continue;

                List<NavNode> newRoute = new List<NavNode>(currentRoute) { neighbor };
                routesQueue.Enqueue(newRoute);
            }
        }
        // No safe route found.
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




    private NavNode FindClosestNavNode(Vector3 position)
    {
        NavNode closestNode = null;
        float minDistance = float.MaxValue;

        foreach (NavNode node in navNodes)
        {
            float distance = Vector3.Distance(position, node.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }






    private IEnumerator ResolveHidingSpotConflict(NPC_AI npc, Hiding_Spots chosenSpot)
    {
        yield return new WaitForSeconds(0.02f); 

        List<NPC_AI> competingNPCs = new List<NPC_AI>();

     
        foreach (var assignment in npcHidingAssignments)
        {
            if (assignment.Value == chosenSpot)
            {
                competingNPCs.Add(assignment.Key);
            }
        }

        // If no NPCs remain in the competition, cancel the process
        if (competingNPCs.Count == 0)
        {
            Debug.LogWarning($"[AIManager] Conflict resolution aborted! No NPCs remaining for spot {chosenSpot.name}.");
            chosenSpot.DecrementOccupancy(); // Release reservation if unused
            yield break;
        }

        // If only one NPC wants the spot, they keep it
        if (competingNPCs.Count == 1)
        {
            NPC_AI winner = competingNPCs[0];
            npcHidingAssignments[winner] = chosenSpot;
            Debug.Log($"[AIManager] {winner.gameObject.name} confirmed hiding spot {chosenSpot.name}.");
            yield break;
        }

        // Sort NPCs by distance (closest NPC gets the spot)
        competingNPCs.Sort((a, b) =>
            Vector3.Distance(a.transform.position, chosenSpot.transform.position)
            .CompareTo(Vector3.Distance(b.transform.position, chosenSpot.transform.position))
        );

        // Ensure that there is at least one NPC in the sorted list
        if (competingNPCs.Count == 0)
        {
            Debug.LogWarning($"[AIManager] No NPCs left to assign after sorting! Aborting resolution.");
            chosenSpot.DecrementOccupancy(); // Release reservation if no one wins
            yield break;
        }

        // The closest NPC gets the spot
        NPC_AI winnerNPC = competingNPCs[0];
        npcHidingAssignments[winnerNPC] = chosenSpot;
        Debug.Log($"[AIManager] {winnerNPC.gameObject.name} won priority for hiding spot {chosenSpot.name}.");

        // All other NPCs must find a new hiding spot
        for (int i = 1; i < competingNPCs.Count; i++)
        {
            chosenSpot.DecrementOccupancy(); // Remove the reservation for losers
            AssignNewHidingSpot(competingNPCs[i], false);
        }
    }




    private bool IsSpotOverCapacity(Hiding_Spots spot)
    {
        int incomingNPCs = 0;

        // Count how many NPCs are currently heading to this spot
        foreach (var assignment in npcHidingAssignments)
        {
            if (assignment.Value == spot)
            {
                incomingNPCs++;
            }
        }

        // Print debug info for troubleshooting
        Debug.Log($"[AIManager] Checking capacity for {spot.name}: Occupancy = {spot.Occupancy}, Incoming = {incomingNPCs}, Max = {spot.MaxOccupancy}");

        // If the number of NPCs currently at the spot + incoming NPCs is >= max, it's full
        return (spot.Occupancy + incomingNPCs) >= spot.MaxOccupancy;
    }





    public void MaintainCover(NPC_AI npc)
    {
        Vector3 playerPosition = GetPlayerPosition();
        Vector3 hidingSpotPosition = npc.GetHidingSpotPosition();
        float distanceToPlayer = Vector3.Distance(npc.transform.position, playerPosition);

        // timer check
        if (!updateTimers.ContainsKey(npc))
        {
            updateTimers[npc] = Time.time + Random.Range(0f, updateInterval);
        }
        if (!hidingTimers.ContainsKey(npc))
        {
            hidingTimers[npc] = Time.time + Random.Range(hidingDurationMin * 0.5f, hidingDurationMax * 1.5f);      // Stagger hiding spot changes
        }


        if (Time.time - updateTimers[npc] < updateInterval)
        {
            return; // Skip update until interval has passed
        }

        updateTimers[npc] = Time.time; // Reset update timer



        //  Continuously adjust position to stay behind cover while hiding
        Vector3 toPlayer = (playerPosition - hidingSpotPosition).normalized;
        Vector3 idealHidingPos = hidingSpotPosition - (toPlayer * 0.35f);

        if (Vector3.Distance(npc.transform.position, idealHidingPos) > 0.2f)
        {
            if (NavMesh.SamplePosition(idealHidingPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                npc.MoveTo(hit.position);
            }
        }

        //npc.setHiding(true);

        // Ensure movment does not mess with hiding 
        if (distanceToPlayer < npc.runRange)
        {
            return; // Stay hidden if player is close
        }

        //limit number of beans that can switch
        if (!activeTimers.ContainsKey(npc) && beansToSwitch.Count < maxBeansToSwitch && !recentlySwitched.Contains(npc))
        {
            beansToSwitch.Add(npc);
            activeTimers[npc] = Time.time + Random.Range(hidingDurationMin, hidingDurationMax);
        }

        // track timers for selected beanss
        if (activeTimers.ContainsKey(npc))
        {
            if (Time.time >= activeTimers[npc])
            {
                Debug.Log($"[AIManager] {npc.gameObject.name} switching to a new hiding spot.");
                //npc.setHiding(false);
                AssignNewHidingSpot(npc, false);

                // this bean just moved
                recentlySwitched.Add(npc);

                // Remove from timers & switch list after moving
                activeTimers.Remove(npc);
                beansToSwitch.Remove(npc);

                //Remove from recently moved list after a cooldown
                StartCoroutine(RemoveFromRecentlySwitched(npc, Random.Range(5f, 15f))); // Adjust cooldown time
            }
        }
    }

    // remove beanss from the recently switched list after a cooldown
    private IEnumerator RemoveFromRecentlySwitched(NPC_AI npc, float delay)
    {
        yield return new WaitForSeconds(delay);
        recentlySwitched.Remove(npc);
    }












    private void ResetGame()
    {
        Debug.Log("[AIManager] Resetting game...");

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

        Debug.Log($"[AIManager] Spawning {count} new beans...");

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomNavMeshPosition();
            if (spawnPosition == Vector3.zero)
            {
                Debug.LogError("[AIManager] No valid spawn location found!");
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

