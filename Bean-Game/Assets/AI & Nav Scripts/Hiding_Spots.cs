using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;


public class Hiding_Spots : MonoBehaviour
{
    public enum HidingType { Normal, Small, Medium }

    [Header("Hiding Spot Settings")]
    public HidingType hidingType = HidingType.Normal;
    public int MaxOccupancy;  // Maximum NPCs allowed in this spot
    private int occupancy = 0;    // Current number of NPCs in this spot
    public int occ = 0;

    public int Occupancy => occupancy; // Read-only property to get current occupancy

    
    public void ReserveSpot()
    {
        if (occupancy < MaxOccupancy)
        {
            occupancy++;  //Reserve the spot immediately
            occ = occupancy;
        }
    }

    public void ReleaseSpot()
    {
        if (occupancy > 0)
        {
            occupancy--;  //Release the spot if an NPC leaves
            occ = occupancy;
        }
    }

    public bool IsAvailable()
    {
        return occupancy < MaxOccupancy;
    }


    public void IncrementOccupancy()
    {
        if (occupancy < MaxOccupancy)
        {
            occupancy++;
            occ = occupancy;
            Debug.Log($"[Hiding_Spots] {gameObject.name} occupancy increased: {occupancy}/{MaxOccupancy}");
        }
        else
        {
            Debug.LogWarning($"[Hiding_Spots] {gameObject.name} is already at max occupancy!");
        }
    }

    public void DecrementOccupancy()
    {
        if (occupancy > 0)
        {
            occupancy--;
            occ = occupancy;
            Debug.Log($"[Hiding_Spots] {gameObject.name} occupancy decreased: {occupancy}/{MaxOccupancy}");
        }
        else
        {
            Debug.LogWarning($"[Hiding_Spots] {gameObject.name} occupancy is already zero!");
        }
    }

    public void ResetHidingSpot()
    {
        occupancy = 0;  // Clear any NPCs marked inside
        occ = occupancy;
        Debug.Log($"[Hiding_Spots] {gameObject.name} reset to empty.");
    }



}
