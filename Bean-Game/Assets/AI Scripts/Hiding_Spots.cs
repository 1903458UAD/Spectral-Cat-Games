using UnityEngine;

public class Hiding_Spots : MonoBehaviour
{
    private int occupancy = 0; // Tracks if a hiding spot is used

    public void IncrementOccupancy()
    {
        occupancy++;
    }

    public void DecrementOccupancy()
    {
        if (occupancy > 0) occupancy--;
    }

    public bool IsAvailable()
    {
        return occupancy == 0;
    }
}
