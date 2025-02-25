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

    public int Occupancy => occupancy; // Read-only property to get current occupancy

    //Check if the hiding spot is still open for NPCs
    public bool IsAvailable()
    {
        if (occupancy < MaxOccupancy)
        {
            Debug.Log($"[Hiding_Spots] {gameObject.name} is available! Occupancy: {occupancy}/{MaxOccupancy}");
            return true;
        }

        Debug.Log($"[Hiding_Spots] {gameObject.name} is FULL! Occupancy: {occupancy}/{MaxOccupancy}");
        return false;
    }


    //Increase the number of NPCs hiding here
    public void IncrementOccupancy()
    {
        if (occupancy < MaxOccupancy)
        {
            occupancy++;
            Debug.Log($"[Hiding_Spots] {gameObject.name} occupancy increased: {occupancy}/{MaxOccupancy}");
        }
        else
        {
            Debug.LogWarning($"[Hiding_Spots] {gameObject.name} is already at max occupancy!");
        }
    }

    //Decrease the number of NPCs hiding here
    public void DecrementOccupancy()
    {
        if (occupancy > 0)
        {
            occupancy--;
            Debug.Log($"[Hiding_Spots] {gameObject.name} occupancy decreased: {occupancy}/{MaxOccupancy}");
        }
        else
        {
            Debug.LogWarning($"[Hiding_Spots] {gameObject.name} occupancy is already zero!");
        }
    }

}
