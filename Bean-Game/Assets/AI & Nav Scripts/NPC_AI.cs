using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class NPC_AI : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private bool isHiding;
    private Hiding_Spots currentHidingSpot;
    public enum HidingType { Normal, Small, Medium }


    [Header("Movement Settings")]
    public float maxRunSpeed = 3f;
    public float maxWalkSpeed = 1.5f;
    public float runRange = 10f;
    public float rotationSpeed = 3.0f;
    public float stayAtWaypointDuration = 10f;
    private Hiding_Spots lastHidingSpot;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (navMeshAgent == null || animator == null)
        {
            Debug.LogError("[NPC_AI] Missing critical componeents!");
            enabled = false;
        }
    }

    private void Start()
    {
        GameManager.Instance.RegisterBean(this);
        SelectNewHidingSpot();
    }

    //private void OldHidingLogic()
    //{
    //    Debug.Log("Old hiding logic was here but was removed due to better implementation.");
    //}

    private void FindNewHidingSpot()
    {
        List<Hiding_Spots> availableSpots = GameManager.Instance.GetAvailableHidingSpots();
        if (availableSpots.Count > 0)
        {
            currentHidingSpot = availableSpots[UnityEngine.Random.Range(0, availableSpots.Count)];
            navMeshAgent.SetDestination(currentHidingSpot.transform.position);
        }
        else
        {
            Debug.LogWarning("[NPC_AI] No avalable hiding spots found.");
        }
    }

    private IEnumerator RetryHiding()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("[NPC_AI] Retrying to find a hiding spot...");
        SelectNewHidingSpot();
    }

    public void SelectNewHidingSpot()
    {
        if (currentHidingSpot != null)
        {
            currentHidingSpot.DecrementOccupancy(); // Free up old spot
        }

        // Let GM assign a spot instead of choosing independently
        currentHidingSpot = GameManager.Instance.FindBetterHidingSpot(transform.position, lastHidingSpot);

        if (currentHidingSpot == null || !currentHidingSpot.IsAvailable())
        {
            Debug.LogWarning($"[NPC_AI] {gameObject.name} could not find an available hiding spot. Retrying...");
            StartCoroutine(RetryHiding());
            return;
        }

        lastHidingSpot = currentHidingSpot; // Save last used spot
        currentHidingSpot.IncrementOccupancy(); // Reserve the spot

        // ✅ Correctly register the NPC with the GM
        GameManager.Instance.RegisterNPCInSpot(this, currentHidingSpot);

        Debug.Log($"[NPC_AI] {gameObject.name} moving to {currentHidingSpot.name}");
        MoveToHidingSpot();
    }






    private void MoveToHidingSpot()
    {
        if (currentHidingSpot == null)
        {
            Debug.LogWarning("[NPC_AI] No valid hiding spot assigned! Trying again...");
            SelectNewHidingSpot();
            return;
        }

        Debug.Log($"[NPC_AI] Moving to hiding spot: {currentHidingSpot.name}");
        navMeshAgent.SetDestination(currentHidingSpot.transform.position);
    }

    private void EnterHidingSpot()
    {
        if (currentHidingSpot == null || Vector3.Distance(transform.position, currentHidingSpot.transform.position) > 1f)
        {
            Debug.LogWarning($"[NPC_AI] {gameObject.name} failed to reach a valid hiding spot! Searching again...");
            SelectNewHidingSpot(); // Try again
            return;
        }

        isHiding = true;
        navMeshAgent.isStopped = true;
        animator.enabled = false;

        switch (currentHidingSpot.hidingType)
        {
            case Hiding_Spots.HidingType.Normal:
                Debug.Log($"[NPC_AI] {gameObject.name} is now hiding in a normal spot: {currentHidingSpot.name}");
                StartCoroutine(HidingCoroutine());
                break;

            case Hiding_Spots.HidingType.Small:
                Debug.Log($"[NPC_AI] {gameObject.name} is hiding in a small spot and will maintain cover.");
                StartCoroutine(SmallHidingBehavior());
                break;

            case Hiding_Spots.HidingType.Medium:
                Debug.Log($"[NPC_AI] {gameObject.name} is hiding in a medium spot and will react slower.");
                StartCoroutine(MediumHidingBehavior());
                break;
        }
    }


    private IEnumerator ExitHidingSpot()
    {
        if (currentHidingSpot != null)
        {
            GameManager.Instance.UnregisterNPCFromSpot(this); // ✅ Correctly unregister from GM
            Debug.Log($"[NPC_AI] {gameObject.name} leaving hiding spot: {currentHidingSpot.name}");

            while (Vector3.Distance(transform.position, GameManager.Instance.GetPlayerPosition()) < 3.5f)
            {
                Debug.Log("[NPC_AI] Player is too close, staying hidden...");
                yield return new WaitForSeconds(1f);
            }

            yield return new WaitForSeconds(1.5f);
            currentHidingSpot = null;
        }

        isHiding = false;
        animator.enabled = true;
        navMeshAgent.isStopped = false;

        SelectNewHidingSpot();
    }





    private IEnumerator HidingCoroutine()
    {
        yield return new WaitForSeconds(stayAtWaypointDuration);

        Debug.Log("[NPC_AI] Time's up! Checking for a better hiding spot...");
        StartCoroutine(ExitHidingSpot());
    }

    private void Update()
    {
        if (isHiding)
        {
            float playerDistance = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerPosition());

            if (currentHidingSpot != null &&
                (currentHidingSpot.hidingType == Hiding_Spots.HidingType.Small ||
                 currentHidingSpot.hidingType == Hiding_Spots.HidingType.Medium))
            {
                // Continuously update NPC position to stay behind cover
                MaintainCover();

                // If the player gets too close and can see the NPC, they should run
                if (playerDistance < 2.5f && !IsSpotHidingMe())
                {
                    Debug.Log($"[NPC_AI] {gameObject.name} is exposed! Running!");
                    StartCoroutine(ExitHidingSpot());
                }
                return;
            }

            // Normal hiding spots: Freeze if the player is near
            if (playerDistance < 3.5f)
            {
                return;
            }

            if (currentHidingSpot == null)
            {
                StartCoroutine(ExitHidingSpot());
            }
            return;
        }

        AvoidPlayer();

        if (currentHidingSpot != null && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending)
        {
            EnterHidingSpot();
        }
    }


    private void AvoidPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerPosition());

        if (distanceToPlayer <= runRange)
        {
            List<NavNode> navNodes = GameManager.Instance.GetNavNodes();
            if (navNodes == null || navNodes.Count == 0)
            {
                Debug.LogError("[NPC_AI] No NavNodes found! NPC is stuck.");
                return;
            }

            NavNode closestNode = GetClosestNode(navNodes);
            if (closestNode == null || closestNode.connectedNodes.Count == 0)
            {
                Debug.LogError("[NPC_AI] Could not find a valid NavNode or escape route.");
                return;
            }

            NavNode bestEscapeNode = null;
            float maxDistance = 0f;

            foreach (NavNode node in closestNode.connectedNodes)
            {
                float distanceToPlayerFromNode = Vector3.Distance(node.transform.position, GameManager.Instance.GetPlayerPosition());

                if (distanceToPlayerFromNode > maxDistance)
                {
                    maxDistance = distanceToPlayerFromNode;
                    bestEscapeNode = node;
                }
            }

            if (bestEscapeNode != null)
            {
                navMeshAgent.SetDestination(bestEscapeNode.transform.position);
            }
            else
            {
                Debug.LogWarning("[NPC_AI] No good escape route found, picking a random connected node.");
                if (closestNode.connectedNodes.Count > 0)
                {
                    navMeshAgent.SetDestination(closestNode.connectedNodes[Random.Range(0, closestNode.connectedNodes.Count)].transform.position);
                }
                else
                {
                    Debug.LogError("[NPC_AI] No valid escape routes! NPC is stuck.");
                }
            }

            StartCoroutine(RequestBetterHidingSpot());
        }
    }

    private IEnumerator RequestBetterHidingSpot()
    {
        yield return new WaitForSeconds(1.5f);

        Hiding_Spots newHidingSpot = GameManager.Instance.FindBetterHidingSpot(transform.position, currentHidingSpot);

        if (newHidingSpot != null)
        {
            if (currentHidingSpot != null)
            {
                currentHidingSpot.DecrementOccupancy();
            }

            currentHidingSpot = newHidingSpot;
            navMeshAgent.SetDestination(currentHidingSpot.transform.position);
        }
    }

    private NavNode GetClosestNode(List<NavNode> nodes)
    {
        NavNode closestNode = null;
        float closestDistance = float.MaxValue;

        foreach (NavNode node in nodes)
        {
            float distance = Vector3.Distance(transform.position, node.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    private void MaintainCover()
    {
        if (currentHidingSpot == null) return;

        Vector3 playerPosition = GameManager.Instance.GetPlayerPosition();
        Vector3 hidingSpotPosition = currentHidingSpot.transform.position;

        // Calculate position behind cover
        Vector3 toPlayer = (playerPosition - hidingSpotPosition).normalized;
        Vector3 newHidingPos = hidingSpotPosition - (toPlayer * 1.2f); // Stay behind cover

        // Only move if necessary
        float distanceFromIdealPosition = Vector3.Distance(transform.position, newHidingPos);
        if (distanceFromIdealPosition < 0.3f) return; // Prevent jittery movement

        if (NavMesh.SamplePosition(newHidingPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
        }
    }





    private bool IsSpotHidingMe()
    {
        Vector3 playerPosition = GameManager.Instance.GetPlayerPosition();
        Vector3 toPlayer = (playerPosition - transform.position).normalized;

        if (Physics.Raycast(transform.position, toPlayer, out RaycastHit hit))
        {
            return hit.collider.CompareTag("HidingSpot"); // Still hidden if cover blocks the view
        }

        return false; // No cover, NPC is exposed!
    }


    private IEnumerator SmallHidingBehavior()
{
    yield return new WaitForSeconds(1.0f); // Allow time to "settle" into hiding

    while (isHiding)
    {
        MaintainCover(); // Continuously reposition to stay hidden

        if (!IsSpotHidingMe()) // If exposed, they bolt
        {
            Debug.Log($"[NPC_AI] {gameObject.name} is exposed in a small spot! Running!");
            StartCoroutine(ExitHidingSpot());
            yield break;
        }

        yield return new WaitForSeconds(0.1f); // React very quickly
    }
}


    private IEnumerator MediumHidingBehavior()
    {
        while (isHiding)
        {
            MaintainCover();

            if (!IsSpotHidingMe())
            {
                Debug.Log($"[NPC_AI] {gameObject.name} is exposed in a medium spot! Running!");
                StartCoroutine(ExitHidingSpot());
                yield break;
            }

            yield return new WaitForSeconds(0.6f); // Slower reaction time than small spots
        }
    }




    private void OnDestroy()
    {
        GameManager.Instance.UnregisterBean(this);
    }
}
