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


    public int beansLow = 4;
    public int beanRestock = 6;


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

        
        if (npcList.Count == beansLow && !isResetting)
        {
            Debug.LogWarning("[AIManager] All beans are destroyed! Triggering reset...");
            StartCoroutine(ResetGameCoroutine());
        }



        foreach (NPC_AI npc in npcList)
        {
            if (npc.IsPickedUp()) continue;

            


            float distanceToPlayer = Vector3.Distance(npc.transform.position, GetPlayerPosition());
           
            // Validate NPC before modifying NavMeshAgent
            if (npc.navMeshAgent == null || !npc.navMeshAgent.enabled || !npc.navMeshAgent.isOnNavMesh)
            {
                Debug.LogWarning($"[AIManager] {npc.gameObject.name} is not on a valid NavMesh. Skipping update.");
                continue; // Skip this NPC to avoid errors
            }


            // Ensure NPCs NEVER freeze
            if (npc.navMeshAgent.isStopped)
            {
                npc.navMeshAgent.isStopped = false;
            }

            if (npc.IsPickedUp()) continue;

            // Ensure the NPC has an entry in hidingTimers
            if (!hidingTimers.ContainsKey(npc))
            {
                hidingTimers[npc] = Time.time;
            }

            // float distanceToPlayer = Vector3.Distance(npc.transform.position, GetPlayerPosition());

            if (npc.IsHiding())
            {
                MaintainCover(npc);
            }
            else
            {
                EvaluateNPCState(npc);
            }





            // Only run if they are NOT hiding & the player is too close
            if (distanceToPlayer < npc.runRange && !npc.IsHiding())
            {
                Debug.Log($"[AIManager] {npc.gameObject.name} is TOO CLOSE to the player! FORCING ESCAPE...");

                // Remove any assigned hiding spot
                if (npcHidingAssignments.ContainsKey(npc))
                {
                    Hiding_Spots lastSpot = npcHidingAssignments[npc];
                    lastSpot.DecrementOccupancy(); // Release hiding spot
                    npcHidingAssignments.Remove(npc);
                    npc.SetHidingSpot(null);
                }

                // Ensure NPC is NOT stopped before running
                npc.navMeshAgent.ResetPath();
                npc.navMeshAgent.isStopped = false;
                npc.navMeshAgent.velocity = Vector3.zero;

                AssignEscapeRoute(npc); // Immediately make them run
                continue;
            }

            // Ensure each NPC has an individual decision timer
            if (!nextDecisionTimes.ContainsKey(npc))
            {
                nextDecisionTimes[npc] = Time.time + Random.Range(0.2f, 1.0f);
            }

            if (Time.time >= nextDecisionTimes[npc])
            {
                if (npc.IsHiding())
                {
                    MaintainCover(npc);
                }
                else
                {
                    EvaluateNPCState(npc);
                }

                // Assign new random decision time to prevent synchronized movement
                nextDecisionTimes[npc] = Time.time + Random.Range(0.5f, 2.0f);
            }

            if (npc.navMeshAgent.velocity.x >= 0.5 || npc.navMeshAgent.velocity.z >= 0.5 || npc.navMeshAgent.velocity.x <= -0.5 || npc.navMeshAgent.velocity.z <= -0.5)
            {
                npc.PlayBeanMoveSound(true);
            }
            else
            {
                npc.PlayBeanMoveSound(false);
            }
        }
    }





    private void FindAllNavNodes()
    {
        navNodes.Clear();
        navNodes.AddRange(FindObjectsOfType<NavNode>());

        if (navNodes.Count == 0)
        {
            Debug.LogError("[AIManager] No NavNodes found in the scene! NPCs cannot move.");
        }
        else
        {
            Debug.Log($"[AIManager] Found {navNodes.Count} NavNodes.");
        }
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



    private void EvaluateNPCState(NPC_AI npc)
    {
        if (npc == null || !npc.navMeshAgent.enabled || !npc.navMeshAgent.isOnNavMesh)
            return;

        float distanceToPlayer = Vector3.Distance(npc.transform.position, GetPlayerPosition());

        // Ensure NPCs will run if they are too close to the player
        if (distanceToPlayer < npc.runRange)
        {
            Debug.Log($"[AIManager] {npc.gameObject.name} is TOO CLOSE to the player! Running away...");

            // Remove their hiding spot status so they don't stay in hiding mode
            if (npcHidingAssignments.ContainsKey(npc))
            {
                Hiding_Spots lastSpot = npcHidingAssignments[npc];
                lastSpot.DecrementOccupancy(); // Release the hiding spot
                npcHidingAssignments.Remove(npc); // Remove them from assignments
                npc.SetHidingSpot(null);
            }

            npc.navMeshAgent.ResetPath(); // Clear current movement to force a new action
            AssignEscapeRoute(npc); // Force them to run
            return;
        }

        // If NPC is not hiding and not in danger, assign a new hiding spot
        if (!npc.IsHiding())
        {
            AssignNewHidingSpot(npc);
        }
    }






    private void AssignEscapeRoute(NPC_AI npc)
    {
        if (npc == null) return;

        npc.SetHidingSpot(null); // Ensure they are not assigned a hiding spot

        // Ensure NPC is moving before assigning a route
        npc.navMeshAgent.isStopped = false;

        // Find the closest NavNode to the NPC
        NavNode currentNode = FindClosestNavNode(npc.transform.position);
        if (currentNode == null || currentNode.connectedNodes.Count == 0)
        {
            Debug.LogWarning($"[AIManager] {npc.gameObject.name} has no valid NavNodes! Running in a straight line.");
            AssignDirectEscape(npc); // Fallback to direct escape if no nodes are found
            return;
        }

        // Find the best escape node (farthest from the player)
        NavNode bestEscapeNode = null;
        float maxDistance = 0f;

        foreach (NavNode node in currentNode.connectedNodes)
        {
            float distanceToPlayer = Vector3.Distance(node.transform.position, GetPlayerPosition());

            // If the node is farther from the player than the current best, select it
            if (distanceToPlayer > maxDistance)
            {
                maxDistance = distanceToPlayer;
                bestEscapeNode = node;
            }
        }

        // If trapped (no good escape node), ignore the player and just run
        if (bestEscapeNode == null && currentNode.connectedNodes.Count > 0)
        {
            Debug.Log($"[AIManager] {npc.gameObject.name} is cornered! Ignoring player and running to any NavNode.");
            bestEscapeNode = currentNode.connectedNodes[Random.Range(0, currentNode.connectedNodes.Count)];
        }

        // Assign the escape route
        if (bestEscapeNode != null)
        {
            Debug.Log($"[AIManager] {npc.gameObject.name} is escaping via NavNode {bestEscapeNode.name}");

            //  Ensure NPC is NOT stopped before assigning movement
            npc.navMeshAgent.isStopped = false;
            npc.MoveTo(bestEscapeNode.transform.position);
        }
        else
        {
            Debug.LogWarning($"[AIManager] {npc.gameObject.name} has no valid escape route! Assigning direct path.");
            AssignDirectEscape(npc); // Fallback to direct escape
        }
    }






    private IEnumerator TryFindHidingSpotAfterEscape(NPC_AI npc)
    {
        yield return new WaitForSeconds(Random.Range(3f, 6f)); // Wait 3-6 seconds before hiding again
        Debug.Log($"[AIManager] {npc.gameObject.name} is searching for a new hiding spot after escaping.");
        AssignNewHidingSpot(npc);
    }





    public void AssignNewHidingSpot(NPC_AI npc)
    {
        if (npc == null)
        {
            Debug.LogError("[AIManager] Tried to assign a hiding spot to a NULL NPC.");
            return;
        }

        if (hidingSpots == null || hidingSpots.Count == 0)
        {
            Debug.LogError("[AIManager] No hiding spots available!");
            return;
        }

        List<Hiding_Spots> validSpots = hidingSpots.FindAll(spot => spot != null && spot.IsAvailable());

        if (validSpots.Count == 0)
        {
            Debug.LogWarning($"[AIManager] {npc.gameObject.name} has no valid hiding spots. Skipping...");
            return;
        }

        Hiding_Spots lastSpot = npc.GetLastHidingSpot();

        // Release the last hiding spot before picking a new one
        if (lastSpot != null)
        {
            lastSpot.DecrementOccupancy();
            Debug.Log($"[AIManager] {npc.gameObject.name} released spot {lastSpot.name}");
        }



        foreach (var spot in hidingSpots)
        {
            if (spot == null) continue;
            if (!spot.IsAvailable()) continue;
            if (IsSpotOverCapacity(spot)) continue;
            if (Vector3.Distance(spot.transform.position, GetPlayerPosition()) < npc.runRange) continue;

            //  Allow returning to last spot only if all other spots are full
            if (spot == lastSpot && validSpots.Count > 0) continue;

            validSpots.Add(spot);
        }

        if (validSpots.Count == 0)
        {
            Debug.LogWarning($"[AIManager] {npc.gameObject.name} has no valid hiding spots. Re-enabling last spot.");
            validSpots.Add(lastSpot); // Allow returning if no other options exist
        }

        Hiding_Spots chosenSpot = validSpots[Random.Range(0, validSpots.Count)];

        npc.SetLastHidingSpot(chosenSpot);
        npcHidingAssignments[npc] = chosenSpot;
        chosenSpot.IncrementOccupancy(); //Properly increment the new spot
        npc.SetHidingSpot(chosenSpot);
        npc.MoveTo(chosenSpot.transform.position);

        Debug.Log($"[AIManager] {npc.gameObject.name} assigned to hiding spot {chosenSpot.name}.");
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

    private void AssignDirectEscape(NPC_AI npc)
    {
        Vector3 directionAwayFromPlayer = (npc.transform.position - GetPlayerPosition()).normalized;
        Vector3 escapeTarget = npc.transform.position + directionAwayFromPlayer * 10f; // Move 10 units away

        if (NavMesh.SamplePosition(escapeTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            Debug.Log($"[AIManager] {npc.gameObject.name} is escaping in a direct line.");
            npc.navMeshAgent.isStopped = false;
            npc.MoveTo(hit.position);
        }
        else
        {
            Debug.LogWarning($"[AIManager] {npc.gameObject.name} could not find a valid escape position!");
        }
    }




    private IEnumerator ResolveHidingSpotConflict(NPC_AI npc, Hiding_Spots chosenSpot)
    {
        yield return new WaitForSeconds(0.02f); // Slightly longer wait to reduce simultaneous picks

        List<NPC_AI> competingNPCs = new List<NPC_AI>();

        // Find all NPCs that have chosen the same spot
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
            AssignNewHidingSpot(competingNPCs[i]);
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




    private bool IsSpotAlreadyChosen(Hiding_Spots spot)
    {
        foreach (var assignment in npcHidingAssignments)
        {
            if (assignment.Value == spot)
            {
                return true; // Someone is already heading to this spot
            }
        }
        return false;
    }




    private void MaintainCover(NPC_AI npc)
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

        // Ensure movment does not mess with hiding 
        if (distanceToPlayer < npc.runRange)
        {
            return; // Stay hidden if player is close
        }

        //limit number of beans that can switch
        if (!activeTimers.ContainsKey(npc) && beansToSwitch.Count < maxBeansToSwitch)
        {
            // recently switched beans can't be re-picked
            if (!recentlySwitched.Contains(npc))
            {
                beansToSwitch.Add(npc);
                activeTimers[npc] = Time.time + Random.Range(hidingDurationMin, hidingDurationMax);
            }
        }

        // track timers for selected beanss
        if (activeTimers.ContainsKey(npc))
        {
            if (Time.time >= activeTimers[npc])
            {
                Debug.Log($"[AIManager] {npc.gameObject.name} switching to a new hiding spot.");
                AssignNewHidingSpot(npc);

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










    //if (Time.time >= hidingTimers[npc])
    //{
    //    // only select number of beans move at a time
    //    if (!beansToSwitch.Contains(npc))
    //    {
    //        beansToSwitch.Add(npc);
    //    }

    //    // Only allow maxBeansToSwitch beans to switch at the same time
    //    if (beansToSwitch.Count <= maxBeansToSwitch)
    //    {
    //        Debug.Log($"[AIManager] {npc.gameObject.name} switching to a new hiding spot.");
    //        AssignNewHidingSpot(npc);

    //        // Reset hiding timer with a new randomized time
    //        hidingTimers[npc] = Time.time + Random.Range(hidingDurationMin, hidingDurationMax);

    //        // Remove bean from the list after switching
    //        beansToSwitch.Remove(npc);
    //    }
    //}





    //// switching spots each bean independent
    ////float randomHidingDuration = Random.Range(hidingDurationMin, hidingDurationMax);

    //if (Time.time >= hidingTimers[npc])
    //{
    //    Debug.Log($"[AIManager] {npc.gameObject.name} switching to a new hiding spot.");

    //    AssignNewHidingSpot(npc);

    //    // Ensure random hiding time for each NPC
    //    hidingTimers[npc] = Time.time + Random.Range(hidingDurationMin, hidingDurationMax);
    //}








    //private bool IsInLineOfSight(NPC_AI npc, Vector3 playerPosition)
    //{
    //    Vector3 npcHeadPosition = npc.transform.position + Vector3.up * 1.5f; // Adjust height for head level
    //    Vector3 directionToPlayer = (playerPosition - npcHeadPosition).normalized;

    //    RaycastHit hit;
    //    if (Physics.Raycast(npcHeadPosition, directionToPlayer, out hit))
    //    {
    //        return hit.collider.CompareTag("Player"); // NPC is exposed if the ray hits the player
    //    }
    //    return false;
    //}





    private void ResetGame()
    {
        Debug.Log("[AIManager] Resetting game...");

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



//private void MaintainCover(NPC_AI npc)
//{
//    Vector3 playerPosition = GetPlayerPosition();
//    Vector3 hidingSpotPosition = npc.GetHidingSpotPosition();
//    float distanceToPlayer = Vector3.Distance(npc.transform.position, playerPosition);



//    // Reaction time
//    if (!hidingTimers.ContainsKey(npc))
//    {
//        hidingTimers[npc] = Time.time;
//    }



//    // If the player sees the NPC, start counting the reaction time
//    if (IsInLineOfSight(npc, playerPosition))
//    {
//        if (Time.time - hidingTimers[npc] >= reactionTime)
//        {
//            Debug.Log($"[AIManager] {npc.gameObject.name} detected! Reacting after {reactionTime} seconds.");

//            // Move back into cover
//            Vector3 toPlayer = (playerPosition - hidingSpotPosition).normalized;
//            Vector3 newHidingPos = hidingSpotPosition - (toPlayer * 0.35f);

//            if (NavMesh.SamplePosition(newHidingPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
//            {
//                npc.MoveTo(hit.position);
//            }
//        }
//    }
//    else
//    {
//        // Reset the timer if the NPC is not in the player's line of sight
//        hidingTimers[npc] = Time.time;
//    }

//    //// Keep moving around slightly in the hiding spot
//    //Vector3 toPlayer = (playerPosition - hidingSpotPosition).normalized;
//    //Vector3 newHidingPos = hidingSpotPosition - (toPlayer * 0.35f);

//    //if (NavMesh.SamplePosition(newHidingPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
//    //{
//    //    npc.MoveTo(hit.position);
//    //}

//    //  If the player is nearby, stay in the hiding spot
//    if (distanceToPlayer < npc.runRange)
//    {
//        hidingTimers[npc] = Time.time; // Reset timer if player is close
//        return;
//    }

//    // Ensure the hiding timer resets after each hiding spot move
//    if (!hidingTimers.ContainsKey(npc))
//    {
//        hidingTimers[npc] = Time.time + Random.Range(5f, 10f);
//    }

//    float randomHidingDuration = hidingDuration + Random.Range(-3f, 3f);

//    if (Time.time - hidingTimers[npc] >= randomHidingDuration)
//    {
//        Debug.Log($"[AIManager] {npc.gameObject.name} is moving to a new hiding spot after {randomHidingDuration} seconds.");
//        AssignNewHidingSpot(npc); // Move to a new hiding spot
//        hidingTimers[npc] = Time.time + Random.Range(5f, 10f); // Reset movement cooldown
//    }
//}


//private bool IsInLineOfSight(NPC_AI npc, Vector3 playerPosition)
//{
//    Vector3 direction = (playerPosition - npc.transform.position).normalized;
//    RaycastHit hit;

//    if (Physics.Raycast(npc.transform.position, direction, out hit))
//    {
//        return hit.collider.CompareTag("Player");
//    }
//    return false;
//}