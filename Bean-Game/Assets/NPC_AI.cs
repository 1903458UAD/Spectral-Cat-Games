using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunAway : MonoBehaviour
{
    private GameObject player;
    private GameObject[] waypoints;
    private int currentWaypointIndex = 0;
    [SerializeField] private float maxRunSpeed = 0.004f; // Speed when running away from the player
    [SerializeField] private float maxWalkSpeed = 0.002f; // Speed when moving between waypoints
    [SerializeField] private float rotationSpeed = 3.0f;
    [SerializeField] private float runRange = 10f; // Range from the player that will cause the Bean to run
    [SerializeField] private float waypointProximityThreshold = 2f; // Distance to ignore the player
    [SerializeField] private float waypointSwitchInterval = 10f; // Time interval to switch waypoints
    private float waypointSwitchTimer = 0f; // Timer for periodic waypoint switching
    private bool isRunningAway = false; // Indicates if the Bean is currently running away

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // Find and store all waypoints in an array
        waypoints = new GameObject[3];
        waypoints[0] = GameObject.FindGameObjectWithTag("Waypoint1");
        waypoints[1] = GameObject.FindGameObjectWithTag("Waypoint2");
        waypoints[2] = GameObject.FindGameObjectWithTag("Waypoint3");

        // Set the first waypoint as the starting target
        currentWaypointIndex = GetFurthestValidWaypointIndex();
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].transform.position);

        // Increment the waypoint switch timer
        waypointSwitchTimer += Time.deltaTime;

        // Skip player detection if the Bean is within the proximity of the current waypoint
        if (distanceToWaypoint <= waypointProximityThreshold)
        {
            if (waypointSwitchTimer >= waypointSwitchInterval)
            {
                currentWaypointIndex = GetNextWaypointIndex(); // Periodically switch to a new waypoint
                waypointSwitchTimer = 0f; // Reset timer
            }

            MoveTowardsWaypoint(maxWalkSpeed);
            return;
        }

        // If the Bean is within the run range, run away from the player
        if (distanceToPlayer <= runRange)
        {
            isRunningAway = true;
            RunAwayFromPlayer(maxRunSpeed);
            return; // Exit to avoid waypoint logic while running away
        }

        // If the Bean is no longer in the player's range and was running away, select a new waypoint
        if (isRunningAway)
        {
            isRunningAway = false;
            currentWaypointIndex = GetFurthestValidWaypointIndex();
            waypointSwitchTimer = 0f; // Reset timer after leaving the player's range
        }

        // Switch waypoints periodically
        if (waypointSwitchTimer >= waypointSwitchInterval)
        {
            currentWaypointIndex = GetNextWaypointIndex();
            waypointSwitchTimer = 0f; // Reset timer
        }

        // Move towards the current target waypoint
        MoveTowardsWaypoint(maxWalkSpeed);
    }

    // Function to make the Bean run away from the player
    void RunAwayFromPlayer(float speed)
    {
        Vector3 directionAwayFromPlayer = transform.position - player.transform.position;
        directionAwayFromPlayer.y = 0; // Keep movement on the horizontal plane
        directionAwayFromPlayer = directionAwayFromPlayer.normalized;

        // Rotate away from the player
        Quaternion targetRotation = Quaternion.LookRotation(directionAwayFromPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move away from the player with the specified speed
        transform.Translate(directionAwayFromPlayer * speed, Space.World);
    }

    // Function to move towards the current waypoint
    void MoveTowardsWaypoint(float speed)
    {
        Vector3 targetWaypointPos = waypoints[currentWaypointIndex].transform.position;
        Vector3 directionToWaypoint = targetWaypointPos - transform.position;
        directionToWaypoint.y = 0;
        directionToWaypoint = directionToWaypoint.normalized;

        // Rotate towards the waypoint
        Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Move towards the waypoint with the specified speed
        transform.Translate(directionToWaypoint * speed, Space.World);
    }

    // Function to find the waypoint furthest from the player and not blocked by the player's range
    int GetFurthestValidWaypointIndex()
    {
        float furthestDistance = 0f;
        int furthestIndex = 0;

        for (int i = 0; i < waypoints.Length; i++)
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, waypoints[i].transform.position);

            // Check if the waypoint is furthest and its path does not intersect the player's range
            if (distanceToPlayer > furthestDistance && CanMoveToWaypoint(i))
            {
                furthestDistance = distanceToPlayer;
                furthestIndex = i;
            }
        }

        return furthestIndex;
    }

    // Function to get the next waypoint index in a cyclic order
    int GetNextWaypointIndex()
    {
        return (currentWaypointIndex + 1) % waypoints.Length;
    }

    // Function to check if the path to the waypoint intersects the player's range
    bool CanMoveToWaypoint(int waypointIndex)
    {
        Vector3 waypointPosition = waypoints[waypointIndex].transform.position;
        Vector3 playerPosition = player.transform.position;

        // Calculate the shortest distance between the player's position and the line to the waypoint
        Vector3 lineDirection = (waypointPosition - transform.position).normalized;
        Vector3 toPlayer = playerPosition - transform.position;

        float projectionLength = Vector3.Dot(toPlayer, lineDirection);
        Vector3 closestPoint;

        if (projectionLength < 0)
        {
            // Closest point is behind the Bean
            closestPoint = transform.position;
        }
        else if (projectionLength > Vector3.Distance(transform.position, waypointPosition))
        {
            // Closest point is beyond the waypoint
            closestPoint = waypointPosition;
        }
        else
        {
            // Closest point is along the line segment
            closestPoint = transform.position + lineDirection * projectionLength;
        }

        // Check if the closest point is within the player's range
        float distanceToClosestPoint = Vector3.Distance(playerPosition, closestPoint);
        return distanceToClosestPoint > runRange;
    }
}
