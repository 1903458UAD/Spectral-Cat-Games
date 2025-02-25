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
    private bool isPickedUp = false; // Track if the NPC is currently held


    private float stuckTimer = 0f;
    private const float stuckThreshold = 5f;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        navMeshAgent.autoTraverseOffMeshLink = false; //Stop auto-adjusting movement
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;
        animator.applyRootMotion = false; //Prevents animations from adjusting position



        if (navMeshAgent == null || animator == null)
        {
            Debug.LogError("[NPC_AI] Missing critical componeents!");
            enabled = false;
        }
    }

    private void Start()
    {

        //StartCoroutine(DelayedAnimatorEnable());
        //StartCoroutine(KeepBeanGrounded());
        


        navMeshAgent.acceleration = 1f;  // Lower acceleration
        navMeshAgent.speed = 1.0f;       // Reduce speed on start
        StartCoroutine(DelayedAceleration());

        //Rigidbody rb = GetComponent<Rigidbody>();
        //if (rb != null)
        //{
        //    rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        //}
        //StartCoroutine(AdjustYPosition());


        GameManager.Instance.RegisterBean(this);
        int beanLayer = LayerMask.NameToLayer("Interactable");

        if (beanLayer == -1)
        {
            Debug.LogError("Layer 'Interactable' does not exist! Please create it in the Unity Layer settings.");
            return;
        }

        Physics.IgnoreLayerCollision(beanLayer, beanLayer, true);
        Debug.Log($"[NPC_AI] Ignoring collision between beans on layer {beanLayer}");

        SelectNewHidingSpot();
    }


    private void Update()
    {
        if (isPickedUp) return; // Stop movement if the NPC is picked up

        if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
        {
            StartCoroutine(TryRepositionToNavMesh());
            return;
        }

        if (isHiding)
        {
            MaintainCover(); // Always adjust hiding position while hiding
            return;
        }

        AvoidPlayer();
    }





    private IEnumerator DelayedAceleration()
    {
        yield return new WaitForSeconds(1.0f); //Wait for physics to settle
        navMeshAgent.acceleration = 4f;  // Lower acceleration
        navMeshAgent.speed = 3f;

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
        Debug.Log($"[NPC_AI] {gameObject.name} is trying to find a hiding spot...");

        if (currentHidingSpot != null)
        {
            Debug.Log($"[NPC_AI] {gameObject.name} is leaving hiding spot {currentHidingSpot.name}");
            currentHidingSpot.DecrementOccupancy(); // Free up old spot
        }

        List<Hiding_Spots> availableSpots = GameManager.Instance.GetAvailableHidingSpots();
        if (availableSpots == null || availableSpots.Count == 0)
        {
            Debug.LogWarning($"[NPC_AI] {gameObject.name} found no available hiding spots! Moving randomly.");
            MoveToRandomNavPoint();
            return;
        }

        // Find a valid hiding spot
        currentHidingSpot = GameManager.Instance.FindBetterHidingSpot(transform.position, lastHidingSpot);

        if (currentHidingSpot == null)
        {
            Debug.LogWarning($"[NPC_AI] {gameObject.name} could not find an available hiding spot. Moving randomly.");
            MoveToRandomNavPoint();
            return;
        }

        lastHidingSpot = currentHidingSpot;
        currentHidingSpot.IncrementOccupancy();
        GameManager.Instance.RegisterNPCInSpot(this, currentHidingSpot);

        Debug.Log($"[NPC_AI] {gameObject.name} moving dynamically to {currentHidingSpot.name}");

        MoveToHidingSpot();
    }



    private void MoveToRandomNavPoint()
    {
        List<NavNode> navNodes = GameManager.Instance.GetNavNodes();

        if (navNodes == null || navNodes.Count == 0)
        {
            Debug.LogError("[NPC_AI] No NavNodes found! NPC cannot move.");
            return;
        }

        NavNode randomNode = navNodes[Random.Range(0, navNodes.Count)];

        if (randomNode != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.SetDestination(randomNode.transform.position);
            Debug.Log($"[NPC_AI] {gameObject.name} moving to random NavNode {randomNode.name}");
        }
        else
        {
            Debug.LogWarning("[NPC_AI] Could not find a valid random point to move to.");
        }
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

        if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning($"[NPC_AI] {gameObject.name} is not on NavMesh. Attempting to reposition...");
            StartCoroutine(TryRepositionToNavMesh());
            return;
        }

        if (NavMesh.SamplePosition(currentHidingSpot.transform.position, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(hit.position);
            Debug.Log($"[NPC_AI] {gameObject.name} moving to hiding spot at {hit.position}");
        }
        else
        {
            Debug.LogWarning($"[NPC_AI] Could not find a valid NavMesh position for hiding spot {currentHidingSpot.name}!");
        }

        //float distanceToSpot = Vector3.Distance(transform.position, currentHidingSpot.transform.position);

        //if (distanceToSpot < 3f) // If close, slow down to prevent overshooting
        //{
        //    navMeshAgent.speed = maxWalkSpeed; // Reduce to walking speed
        //    navMeshAgent.stoppingDistance = 0.3f; // Stop closer to the center
        //}
        //else
        //{
        //    navMeshAgent.speed = maxRunSpeed; // Normal speed otherwise
        //    navMeshAgent.stoppingDistance = 0.5f;
        //}


        //if (currentHidingSpot.hidingType == Hiding_Spots.HidingType.Small || currentHidingSpot.hidingType == Hiding_Spots.HidingType.Medium)
        //{
        //    if (distanceToSpot < 4f) // If close, slow down to prevent overshooting
        //    {

        //        navMeshAgent.speed = maxWalkSpeed * 0.8f; // Reduce speed more for these spots
        //        navMeshAgent.stoppingDistance = 0.2f; // Stop closer to the center
        //    }
        //    else
        //    {
        //        navMeshAgent.speed = maxRunSpeed; // Normal speed otherwise
        //        navMeshAgent.stoppingDistance = 0.5f;
        //    }
        //}
        //else
        //{
        //    // Normal hiding spots use standard speed settings
        //    navMeshAgent.speed = maxRunSpeed;
        //    navMeshAgent.stoppingDistance = 0.5f;
        //}


        //navMeshAgent.SetDestination(currentHidingSpot.transform.position);
    }



    private IEnumerator EnterHidingSpot()
    {
        if (currentHidingSpot == null || Vector3.Distance(transform.position, currentHidingSpot.transform.position) > 0.5f)
        {
            Debug.LogWarning($"[NPC_AI] {gameObject.name} failed to reach a valid hiding spot! Searching again...");
            SelectNewHidingSpot();
            yield break;
        }

        isHiding = true;
        navMeshAgent.isStopped = true;
        animator.enabled = false;

        switch (currentHidingSpot.hidingType)
        {
            case Hiding_Spots.HidingType.Normal:
                Debug.Log($"[NPC_AI] {gameObject.name} is now hiding in a normal spot: {currentHidingSpot.name}");
                yield return StartCoroutine(HidingCoroutine()); 
                break;

            case Hiding_Spots.HidingType.Small:
                Debug.Log($"[NPC_AI] {gameObject.name} is hiding in a small spot and will maintain cover.");
                yield return StartCoroutine(HidingCoroutine());
                break;

            case Hiding_Spots.HidingType.Medium:
                Debug.Log($"[NPC_AI] {gameObject.name} is hiding in a medium spot and will react slower.");
                yield return StartCoroutine(HidingCoroutine());
                break;
        }



        
    }





    private IEnumerator ExitHidingSpot()
    {
        if (currentHidingSpot != null)
        {
            GameManager.Instance.UnregisterNPCFromSpot(this);
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
        navMeshAgent.updatePosition = true; // Restore normal movement
        navMeshAgent.updateRotation = true;
        navMeshAgent.velocity = Vector3.zero; // Reset velocity

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
        }

        SelectNewHidingSpot();
    }






    private IEnumerator HidingCoroutine()
    {
        yield return new WaitForSeconds(stayAtWaypointDuration);

        Debug.Log("[NPC_AI] Time's up! Checking for a better hiding spot...");
        StartCoroutine(ExitHidingSpot());
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
        yield return new WaitForSeconds(1.5f); // Wait before searching

        if (!navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
        {
            Debug.LogWarning($"[NPC_AI] {gameObject.name} is not on NavMesh. Attempting to reposition...");
            StartCoroutine(TryRepositionToNavMesh());
            yield break; // Stop execution if repositioning is needed
        }

        Hiding_Spots newHidingSpot = GameManager.Instance.FindBetterHidingSpot(transform.position, currentHidingSpot);

        if (newHidingSpot != null)
        {
            if (currentHidingSpot != null)
            {
                currentHidingSpot.DecrementOccupancy();
            }

            currentHidingSpot = newHidingSpot;

            if (navMeshAgent.isOnNavMesh) 
            {
                navMeshAgent.SetDestination(currentHidingSpot.transform.position);
            }
            else
            {
                Debug.LogWarning($"[NPC_AI] {gameObject.name} is off the NavMesh! Trying to reposition...");
                StartCoroutine(TryRepositionToNavMesh());
            }
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

        // Calculate ideal position behind the hiding spot
        Vector3 toPlayer = (playerPosition - hidingSpotPosition).normalized;
        Vector3 newHidingPos = hidingSpotPosition - (toPlayer * 1.5f); // Stay behind cover

        // Check if the NPC is exposed
        if (IsExposed(newHidingPos, playerPosition))
        {
            Debug.Log($"[NPC_AI] {gameObject.name} is exposed! Moving to a better hiding position.");

            // Recalculate hiding position dynamically
            newHidingPos = hidingSpotPosition - (toPlayer * Random.Range(1.2f, 2.0f));
        }

        // Move only if necessary
        float distanceFromIdealPosition = Vector3.Distance(transform.position, newHidingPos);
        if (distanceFromIdealPosition > 0.3f) // Prevent jittery movement
        {
            if (NavMesh.SamplePosition(newHidingPos, out NavMeshHit hit, 1.5f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }
    }


    private bool IsExposed(Vector3 position, Vector3 playerPos)
    {
        Vector3 directionToPlayer = (playerPos - position).normalized;

        if (Physics.Raycast(position + Vector3.up * 1f, directionToPlayer, out RaycastHit hit, 10f))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log($"[NPC_AI] {gameObject.name} is exposed!");
                return true; // If the first thing hit is the player, NPC is exposed
            }
        }

        return false;
    }




    public void OnPickedUp()
    {
        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        isPickedUp = true; // Mark NPC as picked up
    }


    public void OnDropped()
    {
        isPickedUp = false; // Mark NPC as not being held

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;
        }
        else
        {
            Debug.LogWarning("[NPC_AI] Dropped off NavMesh! Searching for closest valid position.");
            StartCoroutine(TryRepositionToNavMesh());
        }
    }



    private IEnumerator TryRepositionToNavMesh()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to allow physics to settle

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position + Random.insideUnitSphere * 1.5f, out hit, 2.5f, NavMesh.AllAreas)) // Check within 2.5 meters
        {
            transform.position = hit.position;
            navMeshAgent.enabled = true;
            navMeshAgent.isStopped = false;

            Debug.Log($"[NPC_AI] {gameObject.name} repositioned onto NavMesh at {hit.position}");

            yield return new WaitForSeconds(Random.Range(0.2f, 1.5f)); // Add slight delay before selecting hiding spot

            SelectNewHidingSpot();
        }
        else
        {
            Debug.LogError($"[NPC_AI] {gameObject.name} could not find a valid NavMesh position nearby! Disabling movement.");
            navMeshAgent.enabled = false; // Disable the agent if no valid position is found
        }
    }



    private void OnDestroy()
    {
        GameManager.Instance.UnregisterBean(this);
    }
}




















//IEnumerator AdjustYPosition()
//{
//    yield return new WaitForSeconds(0.5f);  // Wait for physics to settle

//    Vector3 position = transform.position;
//    position.y = 0.1f;  // Adjust this to the correct ground level
//    transform.position = position;

//    Debug.Log("[NPC_AI] Adjusted Y position to prevent jumping.");
//}


//private IEnumerator DelayedAnimatorEnable()
//{
//    animator.enabled = false;
//    yield return new WaitForSeconds(0.5f); //Wait for physics to settle
//    animator.enabled = true;
//}


//private IEnumerator KeepBeanGrounded()
//{
//    yield return new WaitForSeconds(0.55f); // Wait until after Animator reactivates

//    Vector3 startPos = transform.position;
//    startPos.y = GetGroundYPosition(startPos);
//    transform.position = startPos; // Snap to ground to counter any unwanted movement
//}

//private float GetGroundYPosition(Vector3 position)
//{
//    RaycastHit hit;
//    if (Physics.Raycast(position + Vector3.up * 1f, Vector3.down, out hit, 2f, LayerMask.GetMask("Ground")))
//    {
//        return hit.point.y; // Return the exact Y position of the ground
//    }
//    return position.y; // Default to current position if no ground found
//}



//private void HidingLogic()
//{
//    
//}





//private IEnumerator DelayedFreezePosition()
//{
//    yield return new WaitForSeconds(1f);  // Wait for physics to settle

//    Rigidbody rb = GetComponent<Rigidbody>();
//    if (rb != null)
//    {
//        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
//        Debug.Log($"[NPC_AI] {gameObject.name} Y-axis frozen after delay.");
//    }
//}





//private bool IsSpotHidingMe()
//{
//    Vector3 playerPosition = GameManager.Instance.GetPlayerPosition();
//    Vector3 toPlayer = (playerPosition - transform.position).normalized;

//    if (Physics.Raycast(transform.position, toPlayer, out RaycastHit hit))
//    {
//        return hit.collider.CompareTag("HidingSpot"); // Still hidden if cover blocks the view
//    }

//    return false; // No cover, NPC is exposed!
//}


//private IEnumerator SmallHidingBehavior()
//{
//    yield return new WaitForSeconds(1.0f); // Allow time to "settle" into hiding

//    while (isHiding)
//    {
//        MaintainCover(); // Continuously reposition to stay hidden

//        //if (!IsSpotHidingMe()) // If exposed, they bolt
//        //{
//        //    Debug.Log($"[NPC_AI] {gameObject.name} is exposed in a small spot! Running!");
//        //    StartCoroutine(ExitHidingSpot());
//        //    yield break;
//        //}

//        yield return new WaitForSeconds(0.1f); // React very quickly
//    }
//}


//private IEnumerator MediumHidingBehavior()
//{
//    while (isHiding)
//    {
//        MaintainCover();

//        //if (!IsSpotHidingMe())
//        //{
//        //    Debug.Log($"[NPC_AI] {gameObject.name} is exposed in a medium spot! Running!");
//        //    StartCoroutine(ExitHidingSpot());
//        //    yield break;
//        //}

//        yield return new WaitForSeconds(0.6f); // Slower reaction time than small spots
//    }
//}

