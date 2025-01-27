using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hiding_Spots : MonoBehaviour
{
    public int occupancyLimit = 4; // Max NPCs allowed in this spot
    private int currentOccupancy = 0; // Current NPCs occupying the spot
    private int incomingNPCs = 0; // Number of NPCs heading to this spot

    public float priority = 100f; // Higher value = higher priority
    public float priorityReductionAmount = 50f; // Reduction if player is visible
    public Transform player; // Reference to the player's Transform
    public LayerMask raycastLayerMask; // Layer mask for raycasting (Player + Walls)

    public bool CanAcceptNPC()
    {
        bool canAccept = currentOccupancy + incomingNPCs < occupancyLimit;
        UnityEngine.Debug.Log($"{gameObject.name}: CanAcceptNPC = {canAccept} (Current = {currentOccupancy}, Incoming = {incomingNPCs}, Limit = {occupancyLimit})");
        return canAccept;
    }

    public void IncrementOccupancy()
    {
        currentOccupancy++;
        UnityEngine.Debug.Log($"{gameObject.name}: IncrementOccupancy. Current = {currentOccupancy}");
    }

    public void DecrementOccupancy()
    {
        if (currentOccupancy > 0)
            currentOccupancy--;

        UnityEngine.Debug.Log($"{gameObject.name}: DecrementOccupancy. Current = {currentOccupancy}");
    }

    public void IncrementIncomingNPCs()
    {
        incomingNPCs++;
        UnityEngine.Debug.Log($"{gameObject.name}: IncrementIncomingNPCs. Incoming = {incomingNPCs}");
    }

    public void DecrementIncomingNPCs()
    {
        if (incomingNPCs > 0)
            incomingNPCs--;

        UnityEngine.Debug.Log($"{gameObject.name}: DecrementIncomingNPCs. Incoming = {incomingNPCs}");
    }

    public float GetPriority()
    {
        // Adjust priority based on current and incoming occupancy
        float adjustedPriority = priority - ((currentOccupancy + incomingNPCs) * 10f);
        adjustedPriority = Mathf.Max(adjustedPriority, 10); // Ensure priority is not negative
        UnityEngine.Debug.Log($"{gameObject.name}: GetPriority = {adjustedPriority}");
        return adjustedPriority;
    }

    private void Update()
    {
        CheckPlayerVisibility();
    }

    private void CheckPlayerVisibility()
    {
        if (player == null) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Raycast to check if player is visible
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer, raycastLayerMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                priority -= priorityReductionAmount;
                priority = Mathf.Clamp(priority, 10, 100f); // Min priority is 10
                UnityEngine.Debug.Log($"{gameObject.name}: Player visible. Priority reduced to {priority}");
            }
        }
        else
        {
            // Gradually restore priority when the player is not visible
            priority += priorityReductionAmount * Time.deltaTime;
            priority = Mathf.Clamp(priority, 10, 100f); // Restore to max 100
            UnityEngine.Debug.Log($"{gameObject.name}: Player not visible. Priority recovering to {priority}");
        }
    }
}
