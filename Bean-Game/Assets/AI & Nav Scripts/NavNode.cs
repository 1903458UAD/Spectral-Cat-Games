using System.Collections.Generic;
using UnityEngine;

public class NavNode : MonoBehaviour
{
    public List<NavNode> connectedNodes = new List<NavNode>();

    public NavNode GetClosestNode(Vector3 position)
    {
        NavNode closest = null;
        float closestDist = float.MaxValue;

        foreach (var node in connectedNodes)
        {
            float dist = Vector3.Distance(position, node.transform.position);
            if (dist < closestDist)
            {
                closest = node;
                closestDist = dist;
            }
        }

        return closest;
    }
}
