using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunAway : MonoBehaviour
{
    private GameObject player;
    private List<GameObject> waypoints; // List of dynamically assigned waypoints
    private int currentWaypointIndex = 0;
    [SerializeField] private float maxRunSpeed = 0.004f;
    [SerializeField] private float maxWalkSpeed = 0.002f;
    [SerializeField] private float rotationSpeed = 3.0f;
    [SerializeField] private float runRange = 10f;
    [SerializeField] private float waypointProximityThreshold = 2f;
    [SerializeField] private float stayAtWaypointDuration = 10f;

    private float waypointArrivalTime = 0f; // Time when the Bean arrived at the waypoint
    private bool isRunningAway = false;

    // Static dictionary to track Beans at each waypoint
    private static Dictionary<GameObject, int> waypointOccupancy = new Dictionary<GameObject, int>();

    void Start()
    {
        // Find the player GameObject
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            enabled = false; // Disable the script if no player is found
            return;
        }

        // Find all waypoints and initialize the waypointOccupancy dictionary
        waypoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("Waypoint"));
        if (waypoints.Count == 0)
        {
            enabled = false; // Disable the script if no waypoints are found
            return;
        }

        foreach (var waypoint in waypoints)
        {
            if (!waypointOccupancy.ContainsKey(waypoint))
            {
                waypointOccupancy[waypoint] = 0;
            }
        }

        // Shuffle the waypoints and set the first target
        ShuffleWaypoints();
        currentWaypointIndex = GetFurthestValidWaypointIndex();
        IncrementWaypointOccupancy(currentWaypointIndex);
    }

    void Update()
    {
        if (currentWaypointIndex < 0 || currentWaypointIndex >= waypoints.Count)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].transform.position);

        // Handle proximity to waypoint
        if (distanceToWaypoint <= waypointProximityThreshold)
        {
            if (waypointArrivalTime == 0f)
            {
                waypointArrivalTime = Time.time; // Mark arrival time when reaching the waypoint
            }

            // Wait for the required duration before switching waypoints
            if (Time.time - waypointArrivalTime >= stayAtWaypointDuration)
            {
                int nextWaypoint = GetNextWaypointIndex();

                // Allow moving to the next waypoint, even if crowded, as a fallback
                if (nextWaypoint != currentWaypointIndex)
                {
                    DecrementWaypointOccupancy(currentWaypointIndex);
                    currentWaypointIndex = nextWaypoint;
                    IncrementWaypointOccupancy(currentWaypointIndex);
                    waypointArrivalTime = 0f; // Reset the arrival timer
                }
            }

            MoveTowardsWaypoint(maxWalkSpeed);
            return;
        }

        // Reset arrival time if no longer near the waypoint
        waypointArrivalTime = 0f;

        // Handle running away from the player
        if (distanceToPlayer <= runRange)
        {
            isRunningAway = true;
            RunAwayFromPlayer(maxRunSpeed);
            return;
        }

        if (isRunningAway)
        {
            isRunningAway = false;
            waypointArrivalTime = Time.time; // Reset timer after escaping player
        }

        MoveTowardsWaypoint(maxWalkSpeed);
    }

    void RunAwayFromPlayer(float speed)
    {
        Vector3 directionAwayFromPlayer = transform.position - player.transform.position;
        directionAwayFromPlayer.y = 0;
        directionAwayFromPlayer = directionAwayFromPlayer.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(directionAwayFromPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.Translate(directionAwayFromPlayer * speed, Space.World);
    }

    void MoveTowardsWaypoint(float speed)
    {
        Vector3 targetWaypointPos = waypoints[currentWaypointIndex].transform.position;
        Vector3 directionToWaypoint = targetWaypointPos - transform.position;
        directionToWaypoint.y = 0;
        directionToWaypoint = directionToWaypoint.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.Translate(directionToWaypoint * speed, Space.World);
    }

    int GetFurthestValidWaypointIndex()
    {
        float furthestDistance = 0f;
        int furthestIndex = -1;

        for (int i = 0; i < waypoints.Count; i++)
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, waypoints[i].transform.position);

            // Ensure the waypoint is not overcrowded
            if (distanceToPlayer > furthestDistance && waypointOccupancy[waypoints[i]] < 4 && CanMoveToWaypoint(i))
            {
                furthestDistance = distanceToPlayer;
                furthestIndex = i;
            }
        }

        return furthestIndex == -1 ? 0 : furthestIndex;
    }

    int GetNextWaypointIndex()
    {
        for (int i = 1; i <= waypoints.Count; i++)
        {
            int nextIndex = (currentWaypointIndex + i) % waypoints.Count;

            // Allow moving to the least crowded waypoint
            if (waypointOccupancy[waypoints[nextIndex]] < 4)
            {
                return nextIndex;
            }
        }

        // Fallback: return current waypoint if no others are valid
        return currentWaypointIndex;
    }

    bool CanMoveToWaypoint(int waypointIndex)
    {
        Vector3 waypointPosition = waypoints[waypointIndex].transform.position;
        Vector3 playerPosition = player.transform.position;

        Vector3 lineDirection = (waypointPosition - transform.position).normalized;
        Vector3 toPlayer = playerPosition - transform.position;

        float projectionLength = Vector3.Dot(toPlayer, lineDirection);
        Vector3 closestPoint;

        if (projectionLength < 0)
        {
            closestPoint = transform.position;
        }
        else if (projectionLength > Vector3.Distance(transform.position, waypointPosition))
        {
            closestPoint = waypointPosition;
        }
        else
        {
            closestPoint = transform.position + lineDirection * projectionLength;
        }

        float distanceToClosestPoint = Vector3.Distance(playerPosition, closestPoint);
        return distanceToClosestPoint > runRange;
    }

    void ShuffleWaypoints()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, waypoints.Count);
            GameObject temp = waypoints[i];
            waypoints[i] = waypoints[randomIndex];
            waypoints[randomIndex] = temp;
        }
    }

    void IncrementWaypointOccupancy(int waypointIndex)
    {
        waypointOccupancy[waypoints[waypointIndex]]++;
    }

    void DecrementWaypointOccupancy(int waypointIndex)
    {
        waypointOccupancy[waypoints[waypointIndex]]--;
    }
}
