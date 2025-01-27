using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hiding_Spots : MonoBehaviour
{
    public int occupancyLimit = 4;
    private int currentOccupancy = 0;

    public float priority = 100f; // Higher value means higher priority
    public float priorityReductionAmount = 50f; // Amount to reduce priority if the player is visible
    public Transform player; // Reference to the player's Transform
    public LayerMask raycastLayerMask; // Layer mask for raycasting (to include Player and Wall)

    public bool CanAcceptNPC()
    {
        return currentOccupancy < occupancyLimit;
    }

    public void IncrementOccupancy()
    {
        currentOccupancy++;
    }

    public void DecrementOccupancy()
    {
        if (currentOccupancy > 0)
            currentOccupancy--;
    }

    private void Update()
    {
        CheckPlayerVisibility();
    }

    private void CheckPlayerVisibility()
    {
        // Calculate direction and distance to the player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Perform a raycast with the LayerMask
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer, raycastLayerMask))
        {
            UnityEngine.Debug.Log($"Raycast hit: {hit.collider.name} on layer {hit.collider.gameObject.layer}");

            if (hit.collider.CompareTag("Player"))
            {
                // Raycast hit the player, lower priority
                priority -= priorityReductionAmount;
                priority = Mathf.Clamp(priority, 0, 100f);
                UnityEngine.Debug.Log($"Player is visible from {gameObject.name}. Priority lowered to {priority}");
            }
            else
            {
                // Raycast hit something else (e.g., a wall)
                UnityEngine.Debug.Log($"Raycast blocked by {hit.collider.name}");
            }
        }
        else
        {
            UnityEngine.Debug.Log($"No hit detected by raycast.");
            priority = 100f; // Reset priority if nothing blocks the ray
        }
    }

    private void OnDrawGizmos()
    {
        if (player != null)
        {
            // Simulate raycast direction
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Perform a debug raycast for visualization
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer, raycastLayerMask))
            {
                // If the raycast hits something, draw a line to the hit point
                Gizmos.color = hit.collider.CompareTag("Player") ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.2f); // Visualize the hit point
            }
            else
            {
                // If nothing is hit, draw the ray in red to the player's position
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }
    }
}
