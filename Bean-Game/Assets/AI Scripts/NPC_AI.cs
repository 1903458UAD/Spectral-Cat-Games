using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_AI : MonoBehaviour
{
    private GameObject player; // Reference to the player object
    private Hiding_Spots currentHidingSpot; // Current hiding spot selected by the NPC
    private Hiding_Spots previousHidingSpot; // Last hiding spot to avoid reuse

    [Header("Movement Settings")]
    public float maxRunSpeed = 3f; // Maximum speed while running
    public float maxWalkSpeed = 1.5f; // Speed while walking
    public float rotationSpeed = 3.0f; // Rotation speed when changing direction
    public float runRange = 10f; // Range within which NPC detects the player and runs
    public float stayAtWaypointDuration = 10f; // Time to stay at a hiding spot before moving

    private float waypointArrivalTime; // Time when NPC arrived at hiding spot
    private bool isRunningAway; // Tracks if NPC is currently fleeing
    private bool isHiding; // Tracks if NPC is currently hiding
    private bool needsNewHidingSpot; // Flags if NPC should find a new hiding spot after running

    private NavMeshAgent navMeshAgent; // Reference to NavMeshAgent component for pathfinding
    private Rigidbody npcRigidbody; // Rigidbody for applying physics constraints
    private Animator animator; // Animator for controlling NPC animations

    private void Awake()
    {
        InitializeComponents(); // Initialize necessary components and references
    }

    private void Start()
    {
        // Add a slight random delay to stagger movement starts
        StartCoroutine(DelayedStart(UnityEngine.Random.Range(0f, 1f)));
    }

    private IEnumerator DelayedStart(float delay)
    {
        yield return new WaitForSeconds(delay);
        SelectNewHidingSpot(); // Pick an initial hiding spot after a random delay
    }

    private void InitializeComponents()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] Player not found in scene. NPC will be disabled.");
            enabled = false;
            return;
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
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

        navMeshAgent.speed = maxWalkSpeed; // Set initial speed
        navMeshAgent.avoidancePriority = UnityEngine.Random.Range(30, 60); // Randomize obstacle avoidance priority
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance; // Set avoidance type
        navMeshAgent.autoBraking = false; // Disable auto-braking to prevent corner sticking
        navMeshAgent.autoRepath = true; // Enable automatic replanning if the path becomes invalid
    }

    private void Update()
    {
        if (isHiding) return; // Skip update if NPC is hiding

        AvoidPlayer();

        if (needsNewHidingSpot)
        {
            SelectNewHidingSpot();
            needsNewHidingSpot = false;
        }

        // Ensure NPC fully enters hiding spot before considering it occupied
        if (currentHidingSpot != null && !isHiding && Vector3.Distance(transform.position, currentHidingSpot.transform.position) < 1.0f)
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

            if (NavMesh.SamplePosition(newDestination, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }
        else
        {
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
            UnityEngine.Debug.Log("[NPC_AI] Hiding spot already full upon arrival. Selecting new spot.");
            SelectNewHidingSpot();
            return;
        }

        currentHidingSpot.IncrementOccupancy(); // Mark spot as occupied
        navMeshAgent.isStopped = true;
        npcRigidbody.constraints = RigidbodyConstraints.FreezeAll; // Freeze position completely
        animator.enabled = false; // Stop animations
        isHiding = true;
        waypointArrivalTime = Time.time;
        StartCoroutine(HidingCoroutine()); // Start timer for hiding
    }

    private IEnumerator HidingCoroutine()
    {
        yield return new WaitForSeconds(stayAtWaypointDuration);
        ExitHidingSpot(); // Exit hiding after duration ends
    }

    private void ExitHidingSpot()
    {
        isHiding = false;
        npcRigidbody.constraints = RigidbodyConstraints.None; // Unfreeze movement
        animator.enabled = true;
        navMeshAgent.isStopped = false;
        currentHidingSpot.DecrementOccupancy(); // Mark spot as available again
        previousHidingSpot = currentHidingSpot;
        SelectNewHidingSpot(); // Pick a new spot
    }

    private void SelectNewHidingSpot()
    {
        List<Hiding_Spots> availableSpots = GameManager.Instance.GetAvailableHidingSpots();
        List<Hiding_Spots> bestSpots = new List<Hiding_Spots>();
        float highestScore = float.MinValue;

        foreach (var spot in availableSpots)
        {
            if (spot == previousHidingSpot || !spot.IsAvailable()) continue; // Skip the previous or over-occupied hiding spots

            float distanceToPlayer = Vector3.Distance(spot.transform.position, player.transform.position);
            bool isVisible = GameManager.Instance.IsSpotVisibleToPlayer(spot);

            float randomnessFactor = UnityEngine.Random.Range(0f, 50f); // Increased randomness to diversify spot selection
            float score = distanceToPlayer + randomnessFactor;
            if (isVisible) score -= 50f; // Penalize spots visible to the player

            if (score > highestScore)
            {
                highestScore = score;
                bestSpots.Clear(); // Clear previous best spots
                bestSpots.Add(spot); // Add new best spot
            }
            else if (score == highestScore)
            {
                bestSpots.Add(spot); // Add to list of equally scored spots
            }
        }

        if (bestSpots.Count > 0)
        {
            currentHidingSpot = bestSpots[UnityEngine.Random.Range(0, bestSpots.Count)]; // Randomly choose from best spots
            navMeshAgent.SetDestination(currentHidingSpot.transform.position);
        }
        else
        {
            UnityEngine.Debug.LogError("[NPC_AI] No suitable hiding spot found.");
        }
    }
}
