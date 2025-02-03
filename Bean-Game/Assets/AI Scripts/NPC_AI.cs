using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_AI : MonoBehaviour
{
    private GameObject player;
    private Hiding_Spots currentHidingSpot;

    [Header("Movement Settings")]
    [SerializeField] private float maxRunSpeed = 3f;
    [SerializeField] private float maxWalkSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 3.0f;
    [SerializeField] private float runRange = 10f;
    [SerializeField] private float waypointProximityThreshold = 1.5f;
    [SerializeField] private float stayAtWaypointDuration = 10f;

    private float waypointArrivalTime = 0f;
    private bool isRunningAway = false;
    private bool isHiding = false; // ✅ NEW: Prevents NPC from running when actively hiding
    private NavMeshAgent navMeshAgent;

    void Start()
    {
        if (!enabled)
        {
            enabled = true;
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] No Player found in the scene!");
            enabled = false;
            return;
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] No NavMeshAgent component found!");
            enabled = false;
            return;
        }

        if (GameManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] GameManager.Instance is NULL!");
            enabled = false;
            return;
        }

        navMeshAgent.speed = maxWalkSpeed;
        SelectNewHidingSpot();
    }

    void Update()
    {
        if (currentHidingSpot == null)
        {
            SelectNewHidingSpot();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float distanceToHidingSpot = Vector3.Distance(transform.position, currentHidingSpot.transform.position);

        // ✅ FIXED: Stop NPC when it reaches the hiding spot and prevent it from running
        if (distanceToHidingSpot <= waypointProximityThreshold && !isHiding)
        {
            UnityEngine.Debug.Log("[NPC_AI] NPC " + gameObject.name + " is hiding at: " + currentHidingSpot.gameObject.name);

            navMeshAgent.isStopped = true;
            isHiding = true; // ✅ NPC is now in hiding mode
            waypointArrivalTime = Time.time;
        }

        // ✅ NPC stays hidden and ignores player detection during hiding period
        if (isHiding)
        {
            if (Time.time - waypointArrivalTime >= stayAtWaypointDuration)
            {
                isHiding = false; // ✅ NPC is no longer hiding
                navMeshAgent.isStopped = false;
                currentHidingSpot.DecrementOccupancy();
                SelectNewHidingSpot();
            }
            return; // ✅ Ensures NPC does nothing else while hiding
        }

        // ✅ FIXED: Make sure NPCs only run from the player when NOT hiding
        if (distanceToPlayer <= runRange && !isHiding)
        {
            if (!isRunningAway)
            {
                UnityEngine.Debug.Log("[NPC_AI] Player detected! Running away.");
            }
            isRunningAway = true;
            RunAwayFromPlayer();
        }
        else if (isRunningAway && !isHiding)
        {
            isRunningAway = false;
            waypointArrivalTime = Time.time;
            navMeshAgent.speed = maxWalkSpeed;
            navMeshAgent.SetDestination(currentHidingSpot.transform.position);
        }
    }

    void RunAwayFromPlayer()
    {
        Vector3 directionAway = (transform.position - player.transform.position).normalized;
        directionAway.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionAway);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        navMeshAgent.speed = maxRunSpeed;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(transform.position + directionAway * runRange);
        UnityEngine.Debug.Log("[NPC_AI] Running away from player!");
    }

    void SelectNewHidingSpot()
    {
        currentHidingSpot = GameManager.Instance.GetRandomHidingSpot();

        if (currentHidingSpot == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] No available hiding spots from GameManager!");
            return;
        }

        UnityEngine.Debug.Log("[NPC_AI] Moving to new hiding spot: " + currentHidingSpot.gameObject.name);
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(currentHidingSpot.transform.position);
    }
}
