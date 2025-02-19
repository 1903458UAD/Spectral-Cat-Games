using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

public class NPC_AI : MonoBehaviour
{
    private GameObject player;
    private Hiding_Spots currentHidingSpot;
    private Hiding_Spots previousHidingSpot;

    [Header("Movement Settings")]
    public float maxRunSpeed = 3f;
    public float maxWalkSpeed = 1.5f;
    public float rotationSpeed = 3.0f;
    public float runRange = 10f;
    public float stayAtWaypointDuration = 10f;

    private float waypointArrivalTime;
    private bool isRunningAway;
    private bool isHiding;
    private bool needsNewHidingSpot;

    private UnityEngine.AI.NavMeshAgent navMeshAgent;
    private Rigidbody npcRigidbody;
    private Animator animator;

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        StartCoroutine(DelayedStart(UnityEngine.Random.Range(0f, 1f)));
    }

    private IEnumerator DelayedStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        SelectNewHidingSpot();
    }

    private void InitializeComponents()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] Plyaer not found in scene. NPC will be disabled.");
            enabled = false;
            return;
        }

        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navMeshAgent == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] NavMeshAgent not found. NPC cannot move.");
            enabled = false;
            return;
        }

        npcRigidbody = GetComponent<Rigidbody>();
        if (npcRigidbody == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] Rigidbody not found. NPC physics will not behave correctly.");
            enabled = false;
            return;
        }

        animator = GetComponent<Animator>();
        if (animator == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] Animator not found. NPC animations will not play.");
            enabled = false;
            return;
        }

        navMeshAgent.speed = maxWalkSpeed;
        navMeshAgent.avoidancePriority = UnityEngine.Random.Range(30, 60);
        navMeshAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        navMeshAgent.autoBraking = false;
        navMeshAgent.autoRepath = true;
        navMeshAgent.stoppingDistance = 0.5f;
    }

    private void Update()
    {
        if (isHiding) return;

        AvoidPlayer();

        if (needsNewHidingSpot)
        {
            SelectNewHidingSpot();
            needsNewHidingSpot = false;
        }

        if (currentHidingSpot != null && !isHiding && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending)
        {
            EnterHidingSpot();
        }
    }

    private void AvoidPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= runRange)
        {
            Vector3 directionAway = (transform.position - player.transform.position).normalized;
            Vector3 newDestination = transform.position + directionAway * runRange;

            if (UnityEngine.AI.NavMesh.SamplePosition(newDestination, out UnityEngine.AI.NavMeshHit hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }

            if (!isRunningAway)
            {
                isRunningAway = true;
                //Debug.Log("[NPC_AI] Player detected, fleeing!");
            }
        }
        else
        {
            if (isRunningAway)
            {
                isRunningAway = false;
                needsNewHidingSpot = true;
                //Debug.Log("[NPC_AI] Safe distance from player, searching for new hiding spot.");
            }

            if (currentHidingSpot != null && !isRunningAway)
            {
                navMeshAgent.SetDestination(currentHidingSpot.transform.position);
            }
        }
    }

    private void EnterHidingSpot()
    {
        if (!currentHidingSpot.IsAvailable())
        {
            UnityEngine.Debug.Log("[NPC_AI] Hiding spot already full upon arival. Selecting new spot.");
            SelectNewHidingSpot();
            return;
        }

        transform.position = currentHidingSpot.transform.position;
        transform.rotation = currentHidingSpot.transform.rotation;

        currentHidingSpot.IncrementOccupancy();
        navMeshAgent.isStopped = true;
        npcRigidbody.constraints = RigidbodyConstraints.FreezeAll;
        animator.enabled = false;
        isHiding = true;
        waypointArrivalTime = Time.time;

        StartCoroutine(HidingCoroutine());
    }

    private IEnumerator HidingCoroutine()
    {
        yield return new WaitForSeconds(stayAtWaypointDuration);
        ExitHidingSpot();
    }

    private void ExitHidingSpot()
    {
        isHiding = false;
        npcRigidbody.constraints = RigidbodyConstraints.None;
        animator.enabled = true;
        navMeshAgent.isStopped = false;
        currentHidingSpot.DecrementOccupancy();
        previousHidingSpot = currentHidingSpot;
        SelectNewHidingSpot();
    }

    private void SelectNewHidingSpot()
    {
        List<Hiding_Spots> availableSpots = GameManager.Instance.GetAvailableHidingSpots();
        List<Hiding_Spots> bestSpots = new List<Hiding_Spots>();
        float highestScore = float.MinValue;

        foreach (var spot in availableSpots)
        {
            if (spot == previousHidingSpot || !spot.IsAvailable()) continue;

            float distanceToPlayer = Vector3.Distance(spot.transform.position, player.transform.position);
            bool isVisible = GameManager.Instance.IsSpotVisibleToPlayer(spot);

            float randomnessFactor = UnityEngine.Random.Range(0f, 50f);
            float score = distanceToPlayer + randomnessFactor;
            if (isVisible) score -= 50f;

            if (score > highestScore)
            {
                highestScore = score;
                bestSpots.Clear();
                bestSpots.Add(spot);
            }
            else if (score == highestScore)
            {
                bestSpots.Add(spot);
            }
        }

        if (bestSpots.Count > 0)
        {
            currentHidingSpot = bestSpots[UnityEngine.Random.Range(0, bestSpots.Count)];
            navMeshAgent.SetDestination(currentHidingSpot.transform.position);
        }
        else
        {
            UnityEngine.Debug.LogError("[NPC_AI] No suitable hiding spot foud.");
        }
    }

    private void OnDrawGizmos()
    {
        if (currentHidingSpot != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentHidingSpot.transform.position, 1.0f);
        }
    }
}
