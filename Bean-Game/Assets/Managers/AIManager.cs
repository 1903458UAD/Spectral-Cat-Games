using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [Header("AI Elements")]
    private List<NPC_AI> npcList = new List<NPC_AI>();
    private List<Hiding_Spots> hidingSpots = new List<Hiding_Spots>();
    private Dictionary<NPC_AI, Hiding_Spots> npcHidingAssignments = new Dictionary<NPC_AI, Hiding_Spots>();

    private List<NavNode> navNodes = new List<NavNode>();

    private float decisionInterval = 0.5f;  // Every NPC updates decisions every 0.5s
    private Dictionary<NPC_AI, float> nextDecisionTimes = new Dictionary<NPC_AI, float>();


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
    }

    private void Update()
    {
        foreach (NPC_AI npc in npcList)
        {
            if (npc.IsPickedUp()) continue;

            // Esure each NPC has an individual timer
            if (!nextDecisionTimes.ContainsKey(npc))
            {
                nextDecisionTimes[npc] = Time.time + Random.Range(0.2f, 1.0f);  // Randomized delay per NPC
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

                //Asign new random decision time to prevent synchronized movement
                nextDecisionTimes[npc] = Time.time + Random.Range(0.5f, 2.0f);
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
        float distanceToPlayer = Vector3.Distance(npc.transform.position, GetPlayerPosition());

        if (distanceToPlayer < npc.runRange)
        {
            AssignEscapeRoute(npc);
        }
        else
        {
            AssignNewHidingSpot(npc);
        }
    }

    private void AssignEscapeRoute(NPC_AI npc)
    {
        NavNode bestEscapeNode = null;
        float maxDistance = 0f;

        foreach (NavNode node in navNodes)
        {
            float distance = Vector3.Distance(node.transform.position, GetPlayerPosition());
            if (distance > maxDistance)
            {
                maxDistance = distance;
                bestEscapeNode = node;
            }
        }

        if (bestEscapeNode != null)
        {
            npc.MoveTo(bestEscapeNode.transform.position);
        }
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

        Hiding_Spots bestSpot = null;
        float bestScore = float.MaxValue;

        foreach (var spot in hidingSpots)
        {
            if (spot == null) continue;  // Skip null spots
            if (!spot.IsAvailable()) continue;  // Skip full spots

            float distanceToPlayer = Vector3.Distance(spot.transform.position, GetPlayerPosition());
            float distanceToNPC = Vector3.Distance(spot.transform.position, npc.transform.position);
            float randomFactor = Random.Range(-5f, 5f); // Introduce randomness
            float score = distanceToNPC - (distanceToPlayer * 0.5f) + randomFactor;

            if (score < bestScore)
            {
                bestScore = score;
                bestSpot = spot;
            }
        }

        if (bestSpot == null)
        {
            Debug.LogWarning("[AIManager] No valid hiding spot found! Assigning escape route instead.");
            AssignEscapeRoute(npc);
            return;
        }

        //nsure hiding spot assignment is properly tracked
        npcHidingAssignments[npc] = bestSpot;
        npc.SetHidingSpot(bestSpot);
        npc.MoveTo(bestSpot.transform.position);
        bestSpot.IncrementOccupancy();
        Debug.Log($"[AIManager] Assigned NPC {npc.gameObject.name} to hiding spot {bestSpot.name}.");
    }




    private void MaintainCover(NPC_AI npc)
    {
        Vector3 playerPosition = GetPlayerPosition();
        Vector3 hidingSpotPosition = npc.GetHidingSpotPosition();
        Vector3 toPlayer = (playerPosition - hidingSpotPosition).normalized;
        Vector3 newHidingPos = hidingSpotPosition - (toPlayer * 1.5f);

        if (NavMesh.SamplePosition(newHidingPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
        {
            npc.MoveTo(hit.position);
        }
    }

    public Vector3 GetPlayerPosition()
    {
        return player != null ? player.transform.position : Vector3.zero;
    }
}
