using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_AI : MonoBehaviour
{
    private GameObject player; // Reference to the player
    private List<Hiding_Spots> hidingSpots; // List of all hiding spots
    private Hiding_Spots currentHidingSpot; // Currently assigned hiding spot

    [SerializeField] private float maxRunSpeed = 0.004f;
    [SerializeField] private float maxWalkSpeed = 0.002f;
    [SerializeField] private float rotationSpeed = 3.0f;
    [SerializeField] private float runRange = 10f;
    [SerializeField] private float waypointProximityThreshold = 2f;
    [SerializeField] private float stayAtWaypointDuration = 10f; // Time spent at current hiding spot

    private float waypointArrivalTime = 0f;
    private bool isRunningAway = false;
    private float timeSpentAtSpot = 0f; // Timer to track time spent at hiding spot

    private NavMeshAgent navMeshAgent;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            enabled = false;
            return;
        }

        // Find all hiding spots
        hidingSpots = new List<Hiding_Spots>(FindObjectsOfType<Hiding_Spots>());
        if (hidingSpots.Count == 0)
        {
            enabled = false;
            return;
        }

        // Shuffle hiding spots to randomize the selection
        ShuffleHidingSpots();

        // Select a random hiding spot from the shuffled list
        currentHidingSpot = hidingSpots[UnityEngine.Random.Range(0, hidingSpots.Count)];

        // Set up the NavMeshAgent for pathfinding
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            enabled = false;
            return;
        }

        navMeshAgent.speed = maxWalkSpeed; // Start with walking speed

        // Set destination to the initial hiding spot
        navMeshAgent.SetDestination(currentHidingSpot.transform.position);
    }

    void Update()
    {
        // Calculate the distance to the player and to the current hiding spot
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float distanceToHidingSpot = Vector3.Distance(transform.position, currentHidingSpot.transform.position);

        // Check if the NPC has arrived at the hiding spot
        if (distanceToHidingSpot <= waypointProximityThreshold)
        {
            // If we just arrived, start the timer to stay for 'stayAtWaypointDuration' seconds
            if (waypointArrivalTime == 0f)
                waypointArrivalTime = Time.time;

            // If enough time has passed, check if the NPC can switch hiding spots
            if (Time.time - waypointArrivalTime >= stayAtWaypointDuration)
            {
                if (distanceToPlayer > runRange)
                {
                    // Only move to a new spot if the player is out of range
                    currentHidingSpot.DecrementOccupancy();
                    currentHidingSpot = hidingSpots[UnityEngine.Random.Range(0, hidingSpots.Count)];
                    currentHidingSpot.IncrementOccupancy();
                    navMeshAgent.SetDestination(currentHidingSpot.transform.position); // Move to new hiding spot
                    waypointArrivalTime = 0f; // Reset the arrival time
                    timeSpentAtSpot = 0f; // Reset the timer
                }
                else
                {
                    // Stay at the current spot if the player is in range
                    UnityEngine.Debug.Log("Player is still in range. Staying at the hiding spot.");
                }
            }
            return; // Do nothing else if we are at the hiding spot
        }

        // If the player is within range, run away from the player
        if (distanceToPlayer <= runRange)
        {
            isRunningAway = true;
            RunAwayFromPlayer(maxRunSpeed);
            return;
        }

        if (isRunningAway)
        {
            isRunningAway = false;
            waypointArrivalTime = Time.time;
        }

        // Keep moving towards the current hiding spot
        navMeshAgent.SetDestination(currentHidingSpot.transform.position);
    }

    void RunAwayFromPlayer(float speed)
    {
        Vector3 directionAway = (transform.position - player.transform.position).normalized;
        directionAway.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionAway);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        navMeshAgent.SetDestination(transform.position + directionAway * runRange);
    }

    void ShuffleHidingSpots()
    {
        // Shuffle the hiding spots to ensure random selection
        for (int i = 0; i < hidingSpots.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, hidingSpots.Count);
            Hiding_Spots temp = hidingSpots[i];
            hidingSpots[i] = hidingSpots[randomIndex];
            hidingSpots[randomIndex] = temp;
        }
    }
}
