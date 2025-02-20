using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class NavNode : MonoBehaviour
{
    public List<NavNode> connectedNodes = new List<NavNode>();

    // This function finds the nearest node based on distance
    public NavNode GetClosestNode(Vector3 position)
    {
        NavNode closestNode = null;
        float closestDistance = float.MaxValue; // Just using a big number, should be fine

        foreach (NavNode node in connectedNodes)
        {
            float distance = Vector3.Distance(position, node.transform.position);

            //
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        // This should never return null, but just in case...
        if (closestNode == null)
        {
            Debug.LogWarning("[NavNode] No closest node found, returning self."); // should never happen
            return this;
        }

        return closestNode;
    }

    // So many errors... going to keep this in case I break everything again...
    // public void DebugConnections()
    // {
    //     foreach (var node in connectedNodes)
    //     {
    //         Debug.Log("Connected to: " + node.name);
    //     }
    // }
}
