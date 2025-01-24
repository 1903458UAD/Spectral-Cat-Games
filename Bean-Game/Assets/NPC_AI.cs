// NPC Script (NPC_AI)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_AI : MonoBehaviour
{
    private GameObject player;
    private List<GameObject> waypoints;
    private int currentWaypointIndex = 0;
    [SerializeField] private float maxRunSpeed = 0.004f;
    [SerializeField] private float maxWalkSpeed = 0.002f;
    [SerializeField] private float rotationSpeed = 3.0f;
    [SerializeField] private float runRange = 10f;
    [SerializeField] private float waypointProximityThreshold = 2f;
    [SerializeField] private float stayAtWaypointDuration = 10f;

    private float waypointArrivalTime = 0f;
    private bool isRunningAway = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            enabled = false;
            return;
        }

        waypoints = new List<GameObject>(GameObject.FindGameObjectsWithTag("Waypoint"));
        if (waypoints.Count == 0)
        {
            enabled = false;
            return;
        }

        ShuffleWaypoints();
        currentWaypointIndex = GetFurthestWaypointIndex();
    }

    void Update()
    {
        if (currentWaypointIndex < 0 || currentWaypointIndex >= waypoints.Count)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].transform.position);

        if (distanceToWaypoint <= waypointProximityThreshold)
        {
            if (waypointArrivalTime == 0f)
                waypointArrivalTime = Time.time;

            if (Time.time - waypointArrivalTime >= stayAtWaypointDuration)
            {
                currentWaypointIndex = GetNextWaypointIndex();
            }

            MoveTowardsWaypoint(maxWalkSpeed);
            return;
        }

        waypointArrivalTime = 0f;

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

        MoveTowardsWaypoint(maxWalkSpeed);
    }

    void RunAwayFromPlayer(float speed)
    {
        Vector3 directionAway = (transform.position - player.transform.position).normalized;
        directionAway.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionAway);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.Translate(directionAway * speed, Space.World);
    }

    void MoveTowardsWaypoint(float speed)
    {
        Vector3 targetPosition = waypoints[currentWaypointIndex].transform.position;
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.Translate(direction * speed, Space.World);
    }

    int GetFurthestWaypointIndex()
    {
        float furthestDistance = 0f;
        int furthestIndex = -1;

        for (int i = 0; i < waypoints.Count; i++)
        {
            float distanceToPlayer = Vector3.Distance(player.transform.position, waypoints[i].transform.position);

            if (distanceToPlayer > furthestDistance)
            {
                furthestDistance = distanceToPlayer;
                furthestIndex = i;
            }
        }

        return furthestIndex == -1 ? 0 : furthestIndex;
    }

    int GetNextWaypointIndex()
    {
        return (currentWaypointIndex + 1) % waypoints.Count;
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
}