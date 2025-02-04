using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles NPC movement, hiding behavior, and reactions to player presence.
/// </summary>
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
    private bool isHiding = false; // Prevents NPC from running when actively hiding
    private NavMeshAgent navMeshAgent;

    private void Start()
    {
        if (!enabled)
        {
            enabled = true;
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] No Player found in the scene! Disabling NPC.");
            enabled = false;
            return;
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] No NavMeshAgent component found! NPC cannot move.");
            enabled = false;
            return;
        }

        if (GameManager.Instance == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] GameManager.Instance is NULL! Disabling NPC.");
            enabled = false;
            return;
        }

        navMeshAgent.speed = maxWalkSpeed;
        SelectNewHidingSpot();
    }

    private void Update()
    {
        if (currentHidingSpot == null)
        {
            SelectNewHidingSpot();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        float distanceToHidingSpot = Vector3.Distance(transform.position, currentHidingSpot.transform.position);

        // Check if NPC has reached its hiding spot
        if (distanceToHidingSpot <= waypointProximityThreshold && !isHiding)
        {
            UnityEngine.Debug.Log($"[NPC_AI] {gameObject.name} has reached hiding spot: {currentHidingSpot.gameObject.name}");

            navMeshAgent.isStopped = true;
            isHiding = true;
            waypointArrivalTime = Time.time;
        }

        // NPC remains hidden for a set duration
        if (isHiding)
        {
            if (Time.time - waypointArrivalTime >= stayAtWaypointDuration)
            {
                isHiding = false;
                navMeshAgent.isStopped = false;
                currentHidingSpot.DecrementOccupancy();
                SelectNewHidingSpot();
            }
            return; // Prevents further updates while hiding
        }

        // NPC reacts to player's proximity (runs away if detected)
        if (distanceToPlayer <= runRange && !isHiding)
        {
            if (!isRunningAway)
            {
                UnityEngine.Debug.Log("[NPC_AI] Player detected! NPC is running away.");
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

    /// <summary>
    /// Moves NPC away from the player.
    /// </summary>
    private void RunAwayFromPlayer()
    {
        Vector3 directionAway = (transform.position - player.transform.position).normalized;
        directionAway.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(directionAway);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        navMeshAgent.speed = maxRunSpeed;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(transform.position + directionAway * runRange);

        UnityEngine.Debug.Log($"[NPC_AI] {gameObject.name} is running away from player.");
    }

    /// <summary>
    /// Selects a new hiding spot from GameManager's available locations.
    /// </summary>
    private void SelectNewHidingSpot()
    {
        currentHidingSpot = GameManager.Instance.GetRandomHidingSpot();

        if (currentHidingSpot == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] No available hiding spots found! NPC will idle.");
            return;
        }

        UnityEngine.Debug.Log($"[NPC_AI] {gameObject.name} moving to new hiding spot: {currentHidingSpot.gameObject.name}");
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(currentHidingSpot.transform.position);
    }
}
