using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class Hiding_Spots : MonoBehaviour
{
    [Header("Hiding Spot Settings")]
    public int MaxOccupancy = 1;  // Maximum NPCs allowed in this spot
    private int occupancy = 0;   // Current number of NPCs in this spot

    public int Occupancy => occupancy; // Read-only property to get current occupancy

    // This function is to check if the hiding spot is still open for NPCs
    public bool IsAvailable()
    {
        return occupancy < MaxOccupancy; // Spot is available if not full
    }

    public void IncrementOccupancy()
    {
        if (occupancy < MaxOccupancy)
        {
            occupancy++;
        }
        else
        {
            Debug.LogWarning($"[Hiding_Spots] {gameObject.name} is already at max occupency!"); 
        }
    }

    public void DecrementOccupancy()
    {
        if (occupancy > 0)
        {
            occupancy--;
        }
        else
        {
            Debug.LogWarning($"[Hiding_Spots] {gameObject.name} ocupancy is already zero!"); 
        }
    }

    public int GetOccupancy()
    {
        return Occupancy; // Redundant getter but useful if we ever need to override it
    }

    
    //Left in case we want to log all hiding spots when I break the code later...
    //public void DebugOccupancy()
    //{
        //Debug.Log($"{gameObject.name} has {occupancy} NPC(s) hiding in it.");
    //}
    

}
