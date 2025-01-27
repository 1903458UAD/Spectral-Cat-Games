using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_AI : MonoBehaviour
{
    private GameObject player; // Reference to the player
    private List<Hiding_Spots> hidingSpots; // List of all hiding spots
    private Hiding_Spots currentHidingSpot; // Currently assigned hiding spot

    [SerializeField] private float maxRunSpeed = 4f; // Speed when running
    [SerializeField] private float maxWalkSpeed = 2f; // Speed when walking
    [SerializeField] private float runRange = 10f; // Detection range for the player
    [SerializeField] private float hidingSpotReevaluationInterval = 1f; // Time between reevaluations
    [SerializeField] private float timeToWaitBeforeSwitching = 10f; // Time to wait before considering moving to a new hiding spot

    private NavMeshAgent navMeshAgent; // Reference to the NavMeshAgent
    private bool isRunningAway = false; // Whether NPC is running from the player
    private bool isHeadingToHidingSpot = false; // Whether NPC is heading to a hiding spot
    private float lastReevaluationTime = 0f; // Timer for reevaluations
    private float timeSpentAtSpot = 0f; // Time spent at current hiding spot

    void Start()
    {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            UnityEngine.Debug.LogError($"{name}: Player not found! Ensure the player is tagged 'Player'.");
            enabled = false;
            return;
        }

        // Find all hiding spots
        hidingSpots = new List<Hiding_Spots>(FindObjectsOfType<Hiding_Spots>());
        if (hidingSpots.Count == 0)
        {
            UnityEngine.Debug.LogError($"{name}: No hiding spots found! Add hiding spots to the scene.");
            enabled = false;
            return;
        }

        // Get the NavMeshAgent
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            UnityEngine.Debug.LogError($"{name}: NavMeshAgent not found! Add a NavMeshAgent component.");
            enabled = false;
            return;
        }

        navMeshAgent.speed = maxWalkSpeed; // Start with walking speed
    }

    void Update()
    {
        // Calculate the distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        UnityEngine.Debug.Log($"{name}: Distance to Player = {distanceToPlayer}, RunRange = {runRange}");

        // Constantly check if player is within run range
        if (distanceToPlayer <= runRange && !isHeadingToHidingSpot)
        {
            // Player is within range, run away from the player
            if (!isRunningAway)
            {
                isRunningAway = true;
                navMeshAgent.speed = maxRunSpeed; // Switch to running speed
                UnityEngine.Debug.Log($"{name}: Player detected! Switching to running mode.");
            }

            // Run away from the player
            RunAwayFromPlayer();
        }
        else if (distanceToPlayer > runRange && !isHeadingToHidingSpot)
        {
            if (isRunningAway)
            {
                isRunningAway = false;
                navMeshAgent.speed = maxWalkSpeed; // Switch back to walking speed
                UnityEngine.Debug.Log($"{name}: Player out of range. Stopping run.");
                FindBestHidingSpot(); // Move to hiding spot when out of range
            }
        }

        // If already heading to a hiding spot, check if we need to interrupt
        if (isHeadingToHidingSpot)
        {
            if (distanceToPlayer <= runRange)
            {
                // If the player comes back into range, interrupt the waiting and run away
                UnityEngine.Debug.Log($"{name}: Player detected while heading to hiding spot! Running away.");
                isHeadingToHidingSpot = false;
                isRunningAway = true;
                navMeshAgent.speed = maxRunSpeed; // Switch to running speed
                RunAwayFromPlayer();
            }
            else
            {
                timeSpentAtSpot += Time.deltaTime;
                if (timeSpentAtSpot >= timeToWaitBeforeSwitching)
                {
                    UnityEngine.Debug.Log($"{name}: Waiting for {timeSpentAtSpot}s at hiding spot. Ready to re-evaluate.");
                    CheckForSpotSwitch(); // Re-evaluate the hiding spot after waiting
                }
            }
        }

        // Check if NPC has reached the assigned hiding spot
        if (currentHidingSpot != null && IsAtHidingSpot())
        {
            UnityEngine.Debug.Log($"{name}: Reached hiding spot {currentHidingSpot.name}");
            currentHidingSpot.DecrementIncomingNPCs();
            currentHidingSpot.IncrementOccupancy();

            // Prevent switching spots if player is in range
            if (distanceToPlayer <= runRange)
            {
                UnityEngine.Debug.Log($"{name}: Player is in range, staying at the hiding spot.");
                return; // Do nothing if player is detected
            }

            // Check if we need to switch hiding spots
            CheckForSpotSwitch();
        }
    }

    void RunAwayFromPlayer()
    {
        // Calculate direction away from the player
        Vector3 directionAway = (transform.position - player.transform.position).normalized;
        directionAway.y = 0; // Ensure no vertical movement

        // Move the NPC away from the player
        navMeshAgent.SetDestination(transform.position + directionAway * runRange); // Move to a location far from the player
    }

    void FindBestHidingSpot()
    {
        if (currentHidingSpot != null)
        {
            // Release the current hiding spot
            currentHidingSpot.DecrementIncomingNPCs();
            UnityEngine.Debug.Log($"{name}: Leaving hiding spot {currentHidingSpot.name}");
        }

        // Find the first available hiding spot
        Hiding_Spots bestSpot = null;
        foreach (Hiding_Spots spot in hidingSpots)
        {
            if (spot.CanAcceptNPC()) // Pick the first available hiding spot
            {
                bestSpot = spot;
                break; // Exit the loop once a valid spot is found
            }
        }

        if (bestSpot != null)
        {
            currentHidingSpot = bestSpot;
            currentHidingSpot.IncrementIncomingNPCs();
            navMeshAgent.SetDestination(bestSpot.transform.position); // Update destination
            isHeadingToHidingSpot = true; // Mark that the NPC is heading to a hiding spot
            timeSpentAtSpot = 0f; // Reset the waiting timer
            UnityEngine.Debug.Log($"{name}: Assigned to hiding spot {bestSpot.name}, Destination = {bestSpot.transform.position}");
        }
        else
        {
            UnityEngine.Debug.LogWarning($"{name}: No valid hiding spots available!");
        }
    }

    bool IsAtHidingSpot()
    {
        return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending;
    }

    // Check if the NPC should switch hiding spots
    void CheckForSpotSwitch()
    {
        if (currentHidingSpot != null && !currentHidingSpot.CanAcceptNPC())
        {
            // If the current hiding spot is full, find another spot
            UnityEngine.Debug.Log($"{name}: Current hiding spot {currentHidingSpot.name} is full. Finding a new spot.");
            isHeadingToHidingSpot = false; // Stop heading to the current spot
            FindBestHidingSpot(); // Recalculate hiding spot
        }
    }
}
