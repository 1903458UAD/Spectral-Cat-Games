// Waypoint Script (Hiding_Spots)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hiding_Spots : MonoBehaviour
{
    public int occupancyLimit = 4;
    private int currentOccupancy = 0;

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
}
