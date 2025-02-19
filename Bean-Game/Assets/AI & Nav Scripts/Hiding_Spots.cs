using UnityEngine;

public class Hiding_Spots : MonoBehaviour
{
    [Header("Occupancy Settings")]
    public int maxOccupancy = 1; // Maximum number of NPCs allowed in this hiding spot

    private int currentOccupancy = 0; // Tracks current number of NPCs in the hiding spot

    public void IncrementOccupancy()
    {
        if (currentOccupancy < maxOccupancy)
        {
            currentOccupancy++;
        }
    }

    public void DecrementOccupancy()
    {
        if (currentOccupancy > 0)
        {
            currentOccupancy--;
        }
    }

    public bool IsAvailable()
    {
        return currentOccupancy < maxOccupancy; // Returns true if there's room for more NPCs
    }
}
