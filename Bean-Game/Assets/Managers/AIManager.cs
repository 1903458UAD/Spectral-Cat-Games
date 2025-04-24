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



    private float decisionInterval = 0.5f;  // Every NPC updates decisions every 0.5s
    private Dictionary<NPC_AI, float> nextDecisionTimes = new Dictionary<NPC_AI, float>();

    private bool isResetting = false;

    private bool start = false;

    public int beansLow = 4;
    public int beanRestock = 6;

    public float minDistanceFromShelf = .01f;

    private Dictionary<NPC_AI, float> lastSwitchTime = new Dictionary<NPC_AI, float>();


    public float maxRunTime = 8f; // Maximum timea bean is allowed to run continuously
    private Dictionary<NPC_AI, float> runStartTimes = new Dictionary<NPC_AI, float>();



    private List<Collider> shelfColliders = new List<Collider>();

    private EventInstance beanFootsteps;



    [Header("AI Behavior Settings")]
    public float updateInterval = 0.1f; // Adjust this for difficulty


    public float hidingDurationMin = 5f; // Value to track the min hiding time 



    public float hidingDurationMax = 10f; // track the max hiding time

    public float switchPeriod = 10f;
    private float switchElapsed = 0f;


    private Dictionary<NPC_AI, float> updateTimers = new Dictionary<NPC_AI, float>();

    public int maxBeansToSwitch = 3; // Num beans that can switch spots at the same time
    //private List<NPC_AI> beansToSwitch = new List<NPC_AI>(); // beans chosen to switch

    //private Dictionary<NPC_AI, float> activeTimers = new Dictionary<NPC_AI, float>(); // Only track selected beans time
    //private HashSet<NPC_AI> recentlySwitched = new HashSet<NPC_AI>(); // Track recently moved beans

    //private List<Transform> shelfPositions = new List<Transform>();


    //private float globalSwitchCycleTimer = 0f;
    //private float lastGlobalCountdownDebug = 0f;



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

   
        FindAllHidingSpots();


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
            //Debug.LogWarning("[AIManager] Bean count low! Triggering reset...");
            StartCoroutine(ResetGameCoroutine());
        }



        // Process each NPC in the list.
        foreach (NPC_AI npc in npcList)
        {
            if (npc == null)
            {
                continue;
            }


            if (npc.IsPickedUp())
            {
                //npc.PlayBeanMoveSound(false);
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

        bool anyRunning = npcList.Any(b => b.state == NPC_AI.NPCState.Running);
        if (!anyRunning)
        {
            switchElapsed += Time.deltaTime;
        }



        if (switchElapsed >= switchPeriod)
        {

            var candidates = npcList.Where(b => b.state == NPC_AI.NPCState.Hiding
                             && Vector3.Distance(b.transform.position, GetPlayerPosition()) >= b.runRange)
                            .ToList();


            for (int i = 0; i < maxBeansToSwitch && candidates.Count > 0; i++)
            {
                int indx = Random.Range(0, candidates.Count);
                var bean = candidates[indx];
                candidates.RemoveAt(indx);


                ReleaseCurrentHidingSpot(bean);
                AssignNewHidingSpot(bean, false);
            }


            switchElapsed = 0f;

        }


    }

    public Vector3 GetRandomSpawnPositionUsingNodes()
    {
        // Get all NavNode objects in the scene.
        NavNode[] nodes = FindObjectsOfType<NavNode>();

        if (nodes.Length > 0)
        {
            // random node.
            NavNode randomNode = nodes[Random.Range(0, nodes.Length)];
            NavMeshHit hit;
            // Ensure valid on the NavMesh.
            if (NavMesh.SamplePosition(randomNode.transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
                
            return randomNode.transform.position;
        }

        // Fallback point in a defined area.
        float range = 20f;

        Vector3 randomPoint = new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));

        NavMeshHit fallbackHit;

        if (NavMesh.SamplePosition(randomPoint, out fallbackHit, 10f, NavMesh.AllAreas))
        {
            return fallbackHit.position;
        }

        return Vector3.zero;
    }

    public Vector3 GetClosestNavNode(Vector3 targetPosition)
    {
        NavNode[] nodes = FindObjectsOfType<NavNode>();

        NavNode closest = null;

        float minDistance = Mathf.Infinity;

        foreach (var node in nodes)
        {
            float dist = Vector3.Distance(targetPosition, node.transform.position);

            if (dist < minDistance)
            {
                minDistance = dist;
                closest = node;
            }
        }


        if (closest != null)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(closest.transform.position, out hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return closest.transform.position;
        }

        return targetPosition; // Fallback to original if no nodes found.
    }

 
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
            hidingTimers.Remove(npc);
            nextDecisionTimes.Remove(npc);
            updateTimers.Remove(npc);
            lastSwitchTime.Remove(npc);
            runStartTimes.Remove(npc);
            npcHidingAssignments.Remove(npc);
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

        if (npc.isFrozen) return;

        float distanceToPlayer = Vector3.Distance(npc.transform.position, GetPlayerPosition());

        // If the bean is already in Hiding, do nothing.
        if (npc.state == NPC_AI.NPCState.Hiding)
        {
            // For non-shelf hiding spots (and now for shelf as well), simply check if the bean is near its target.
            if (npc.GetHidingSpot() != null)
            {
                float distanceToSpot = Vector3.Distance(npc.transform.position, npc.GetHidingSpot().transform.position);
                if (distanceToSpot <= npc.navMeshAgent.stoppingDistance + 0.3f)
                {
                    npc.OnReachedHidingSpot();
                }
                // Otherwise, let MaintainCover drive the bean back.
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
        //else if (distanceToPlayer > npc.runRange * 2)
        //{
        //    if (npc.state == NPC_AI.NPCState.Running)
        //    {
        //       // Debug.Log($"[EvaluateNPCState] {npc.gameObject.name} has run far enough. Transitioning from Running to Idle.");
        //        npc.state = NPC_AI.NPCState.Idle;
        //        npc.navMeshAgent.ResetPath(); // Clear the previous escape route.
        //        AssignNewHidingSpot(npc, true);
        //    }
        //}
        
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

        float minDistance = npc.runRange * 3f;
        float clearanceThreshold = npc.runRange;
        Vector3 nodeTarget = GetRandomEscapeNode(npc.transform.position, GetPlayerPosition(), minDistance, clearanceThreshold);

        npc.MoveTo(nodeTarget);

        StartCoroutine(WaitForDestinationAndTransition(npc));
    }




    private IEnumerator WaitForDestinationAndTransition(NPC_AI npc)
    {

        while (npc.navMeshAgent != null &&
               npc.navMeshAgent.enabled &&
               npc.navMeshAgent.isOnNavMesh &&
               (npc.navMeshAgent.pathPending ||
                npc.navMeshAgent.remainingDistance > npc.navMeshAgent.stoppingDistance))
        {
            yield return null;
        }

        if (npc.navMeshAgent == null || !npc.navMeshAgent.isOnNavMesh)
            yield break;


        npc.state = NPC_AI.NPCState.Idle;
        AIManager.Instance.AssignNewHidingSpot(npc, true);
    }


    private IEnumerator WaitForDestination(NPC_AI npc)
    {
        while (npc.navMeshAgent.pathPending || npc.navMeshAgent.remainingDistance > npc.navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        npc.state = NPC_AI.NPCState.Idle;
        AssignNewHidingSpot(npc, true);
    }


    public Vector3 GetRandomEscapeNode(Vector3 npcPos, Vector3 playerPos, float minDistance, float clearanceThreshold)
    {
        // Find all NavNodes in the scene.
        NavNode[] nodes = FindObjectsOfType<NavNode>();

        List<NavNode> validNodes = new List<NavNode>();

        List<NavNode> fallbackNodes = new List<NavNode>();

        foreach (var node in nodes)
        {
            float distToPlayer = Vector3.Distance(node.transform.position, playerPos);
            if (distToPlayer >= minDistance)
            {
                fallbackNodes.Add(node); // meets minimum distance
                float pathClearance = DistanceFromPointToLineSegment(playerPos, npcPos, node.transform.position);
                if (pathClearance >= clearanceThreshold)
                {
                    validNodes.Add(node); // passes clearance check
                }
            }
        }

        if (validNodes.Count > 0)
        {
            return validNodes[Random.Range(0, validNodes.Count)].transform.position;
        }
        else if (fallbackNodes.Count > 0)
        {
            return fallbackNodes[Random.Range(0, fallbackNodes.Count)].transform.position;
        }
        // Fallback: return npcPos (or you could generate a random point)
        return npcPos;
    }


    public float DistanceFromPointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 segment = lineEnd - lineStart;
        if (segment.sqrMagnitude == 0)
        {
            return Vector3.Distance(point, lineStart);
        }

        float t = Vector3.Dot(point - lineStart, segment) / segment.sqrMagnitude;

        t = Mathf.Clamp01(t);

        Vector3 projection = lineStart + t * segment;

        return Vector3.Distance(point, projection);
    }




    





    public void ReleaseCurrentHidingSpot(NPC_AI npc)
    {
        if (npc == null)
        {
            return;
        }
            

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


        if (npc == null || npc.state == NPC_AI.NPCState.Running)
        {
            return;
        }

        ReleaseCurrentHidingSpot(npc);
        npc.state = NPC_AI.NPCState.Idle;

        if (hidingSpots == null || hidingSpots.Count == 0)
        {
            return;
        }

        List<Hiding_Spots> validSpots = new List<Hiding_Spots>();


        foreach (var spot in hidingSpots)
        {

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
        }

        Hiding_Spots lastSpot = npc.GetLastHidingSpot();


        if (lastSpot != null)
        {
            validSpots.Remove(lastSpot);
        }


        if (validSpots.Count == 0)
        {

            validSpots = hidingSpots;
        }


        Hiding_Spots chosenSpot = validSpots[Random.Range(0, validSpots.Count)];
        npc.SetLastHidingSpot(chosenSpot);
        npcHidingAssignments[npc] = chosenSpot;
        npc.SetHidingSpot(chosenSpot);

        // Directly move the NPC to the chosen hiding spot.
        npc.MoveTo(chosenSpot.transform.position);

        StartCoroutine(ResolveHidingSpotConflict(npc, chosenSpot));
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

        if (npcList.Any(b => b.navMeshAgent.hasPath && b.state == NPC_AI.NPCState.Running))
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
        Vector3 targetPosition = hidingSpotPosition - toPlayer * 0.25f;

        

        switch (hidingSpot.hidingType)
        {
            case Hiding_Spots.HidingType.BehindCover:
                {

                    targetPosition = hidingSpotPosition - toPlayer * 0.25f;
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
            default:
                {
                    
                    targetPosition = hidingSpotPosition - toPlayer * 0.25f;
                    break;
                }
        }

        // If the bean is not near the target and the spot is not a cage, move toward the target.
        if (Vector3.Distance(npc.transform.position, targetPosition) > 0.2f && !hidingSpot.IsCage())
        {
            NavMeshHit hit;
            float maxVerticalDifference = 0.5f; // adjust this threshold as needed



            if (NavMesh.SamplePosition(targetPosition, out hit, 2.0f, NavMesh.AllAreas))
            {
                
                if (Mathf.Abs(hit.position.y - targetPosition.y) <= maxVerticalDifference)
                {
                    npc.MoveTo(hit.position);
                }
                else
                {
            
                    Vector3 adjustedTarget = new Vector3(hit.position.x, targetPosition.y, hit.position.z);
                    npc.MoveTo(adjustedTarget);
                }
            }
        }
        else if (hidingSpot.IsCage())
        {
            npc.MoveTo(hidingSpot.transform.position);
        }

        //// If the player is very close stay hidden.
        //if (distanceToPlayer < npc.runRange)
        //{
        //    return;
        //}

        //if (hidingSpot.IsTrap())
        //{
        //    if (activeTimers.ContainsKey(npc) && Time.time > activeTimers[npc])
        //    {
        //        activeTimers.Remove(npc);
        //    }
        //    if (!activeTimers.ContainsKey(npc))
        //    {
        //        beansToSwitch.Add(npc);
        //        activeTimers[npc] = Time.time + 10f;  // 10 second interval for traps
        //    }
        //}
        //else if (!hidingSpot.IsCage())
        //{

        //    if (Time.time >= globalSwitchCycleTimer)
        //    {
                
        //        List<NPC_AI> beansToProcess = new List<NPC_AI>();

        //        foreach (NPC_AI bean in beansToSwitch.ToArray())
        //        {
                    
        //            if (Vector3.Distance(bean.transform.position, GetPlayerPosition()) >= bean.runRange)
        //            {
        //                beansToProcess.Add(bean);
        //            }
        //            else
        //            {
        //                // bean too close select diffrent bean
        //                beansToSwitch.Remove(bean);
        //            }
        //        }


        //        while (beansToProcess.Count < maxBeansToSwitch)
        //        {

        //            NPC_AI candidate = npcList.FirstOrDefault(n =>
        //                !beansToSwitch.Contains(n) &&
        //                !recentlySwitched.Contains(n) &&
        //                n.state == NPC_AI.NPCState.Idle &&
        //                Vector3.Distance(n.transform.position, GetPlayerPosition()) >= n.runRange);

        //            if (candidate == null)
        //            {
                        
        //                break;
        //            }
        //            beansToProcess.Add(candidate);
        //            beansToSwitch.Add(candidate);
        //        }


        //        foreach (NPC_AI bean in beansToProcess)
        //        {
        //            ReleaseCurrentHidingSpot(bean);       
        //            AssignNewHidingSpot(bean, false);   
        //            lastSwitchTime[bean] = Time.time;
        //            recentlySwitched.Add(bean);



        //        }

        //        beansToSwitch.Clear();

        //        // Restart the global cycle.
        //        globalSwitchCycleTimer = Time.time + (10f + Random.Range(hidingDurationMin, hidingDurationMax));
        //        recentlySwitched.Clear();
        //        Debug.Log($"[Cycle] New cycle started; next cycle at: {globalSwitchCycleTimer:F2}");
        //        lastGlobalCountdownDebug = Time.time;
        //    }
        //    else
        //    {
        //        // Add this NPC for switching if it qualifies and we haven't yet reached the max number.
        //        if (beansToSwitch.Count < maxBeansToSwitch && !recentlySwitched.Contains(npc))
        //        {
        //            if (!beansToSwitch.Contains(npc))
        //            {
        //                beansToSwitch.Add(npc);
        //            }
        //        }

        //        // Log a countdown once per second.
        //        if (Time.time - lastGlobalCountdownDebug >= 1f)
        //        {
        //            lastGlobalCountdownDebug = Time.time;
        //            float countdown = globalSwitchCycleTimer - Time.time;
        //            string selectedBeans = string.Join(", ", beansToSwitch.Select(b => b.gameObject.name).ToArray());
        //            Debug.Log($"[Cycle Countdown] {Mathf.Ceil(countdown)} sec until next switch. Selected beans: {selectedBeans}");
        //        }
        //    }
        //}
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





    //// remove beanss from the recently switched list after a cooldown
    //private IEnumerator RemoveFromRecentlySwitched(NPC_AI npc, float delay)
    //{
    //    yield return new WaitForSeconds(delay);
    //    recentlySwitched.Remove(npc);
    //}












    private void ResetGame()
    {
        //Debug.Log("[AIManager] Resetting game...");

        // Clear NPC List and Assignments
        npcList.Clear();
        npcHidingAssignments.Clear();
        hidingTimers.Clear();
        nextDecisionTimes.Clear();



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
            Vector3 spawnPosition = AIManager.Instance.GetRandomSpawnPositionUsingNodes();
            if (spawnPosition == Vector3.zero)
            {
               // Debug.LogError("[AIManager] No valid spawn location found!");
                continue;
            }

            // Use GameManager's beanPrefab instead of Resources.Load()
            GameObject beanPrefab = GameManager.Instance.beanPrefab;
          
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


