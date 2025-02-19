using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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


    [Header("Movement Settings")]
    public float maxRunSpeed = 3f;
    public float maxWalkSpeed = 1.5f;
    public float runRange = 10f;
    public float rotationSpeed = 3.0f;
    public float stayAtWaypointDuration = 10f;


    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (navMeshAgent == null || animator == null)
        {
            UnityEngine.Debug.LogError("[NPC_AI] Missing critical components!");
            enabled = false;
        }
    }

    private void Start()
    {
        GameManager.Instance.RegisterBean(this);
        SelectNewHidingSpot();
    }




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
            Debug.LogWarning("[NPC_AI] No available hiding spots found.");
        }
    }

    private void SelectNewHidingSpot()
    {
        currentHidingSpot = GameManager.Instance.FindBestHidingSpot(transform.position);

        if (currentHidingSpot != null)
        {
            navMeshAgent.SetDestination(currentHidingSpot.transform.position);
        }
    }

    private void EnterHidingSpot()
    {
        isHiding = true;
        navMeshAgent.isStopped = true;
        animator.enabled = false;

        StartCoroutine(HidingCoroutine());
    }

    private void ExitHidingSpot()
    {
        isHiding = false;
        animator.enabled = true;
        navMeshAgent.isStopped = false;
        SelectNewHidingSpot();
    }

    private IEnumerator HidingCoroutine()
    {
        yield return new WaitForSeconds(stayAtWaypointDuration);
        ExitHidingSpot();
    }

    private void MoveUsingNavNodes()
{
    List<NavNode> navNodes = GameManager.Instance.GetNavNodes();
    if (navNodes.Count > 0)
    {
        NavNode closestNode = GetClosestNode(navNodes);
        navMeshAgent.SetDestination(closestNode.transform.position);
    }
}

    private void Update()
    {
        if (isHiding) return;

        AvoidPlayer();

        if (currentHidingSpot != null && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance && !navMeshAgent.pathPending)
        {
            EnterHidingSpot();
        }
    }



    private NavNode GetClosestNode(List<NavNode> nodes)
    {
        if (nodes.Count == 0) return null;

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

    private void AvoidPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, GameManager.Instance.GetPlayerPosition());

        if (distanceToPlayer <= runRange)
        {
            Vector3 directionAway = (transform.position - GameManager.Instance.GetPlayerPosition()).normalized;
            Vector3 newDestination = transform.position + directionAway * runRange;

            if (NavMesh.SamplePosition(newDestination, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }
    }



    private void OnDestroy()
    {
        GameManager.Instance.UnregisterBean(this);
    }
}
