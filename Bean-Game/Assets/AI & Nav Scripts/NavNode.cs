using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using UnityEngine.AI;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavNode : MonoBehaviour
{
    public List<NavNode> connectedNodes = new List<NavNode>(); // Direct connections
    private float connectionDistance = 2f; // Reduced distance to connect nodes
    public Dictionary<NavNode, float> nodeDistances = new Dictionary<NavNode, float>(); // Store distance to connected nodes

    private static List<NavNode> allNodes = new List<NavNode>(); // Global list to hold all nodes in the scene

    private void Start()
    {
        GenerateConnections();
    }

    // This will generate connections for each node within the specified distance
    private void GenerateConnections()
    {
        connectedNodes.Clear();
        NavNode[] allNodesArray = FindObjectsOfType<NavNode>(); // Get all nodes in the scene

        foreach (NavNode node in allNodesArray)
        {
            if (node != this) // Avoid self-connection
            {
                float distance = Vector3.Distance(transform.position, node.transform.position);
                if (distance <= connectionDistance && IsPathClear(node)) // Check if it's within range and clear of obstacles
                {
                    connectedNodes.Add(node);
                    nodeDistances[node] = distance;

                    if (!node.connectedNodes.Contains(this)) // Ensure the connection is bidirectional
                    {
                        node.connectedNodes.Add(this);
                        node.nodeDistances[this] = distance;
                    }
                    Debug.Log($"Node {gameObject.name} successfully connected to {node.gameObject.name}. Distance: {distance}");
                }
            }
        }

        // Add this node to the global node list
        if (!allNodes.Contains(this))
        {
            allNodes.Add(this);
        }
    }

    // Checks if the path between two nodes is clear (no obstacles in the way)
    private bool IsPathClear(NavNode targetNode)
    {
        NavMeshHit hit;
        bool pathBlocked = NavMesh.Raycast(transform.position, targetNode.transform.position, out hit, NavMesh.AllAreas);
        return !pathBlocked;
    }

    // Draw lines between connected nodes in the scene view for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach (var node in connectedNodes)
        {
            if (node != null)
            {
                Gizmos.DrawLine(transform.position, node.transform.position); // Draw line between nodes
            }
        }
    }

    // Returns the closest node to the given position
    public NavNode GetClosestNode(Vector3 position)
    {
        NavNode closestNode = null;
        float closestDistance = float.MaxValue;

        foreach (var node in connectedNodes)
        {
            float distance = Vector3.Distance(position, node.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    // Queries the distance between this node and the target node
    public float GetDistanceToNode(NavNode targetNode)
    {
        if (nodeDistances.ContainsKey(targetNode))
        {
            return nodeDistances[targetNode];
        }
        return float.MaxValue;
    }

    // Static method to retrieve all nodes for external use
    public static List<NavNode> GetAllNodes()
    {
        return allNodes;
    }

    // A method to query the shortest path from this node to another (simple example using Dijkstra's algorithm)
    public List<NavNode> FindShortestPath(NavNode targetNode)
    {
        // Dijkstra's algorithm or A* could be implemented here. For simplicity, let's just return direct connections.
        List<NavNode> path = new List<NavNode>();
        if (connectedNodes.Contains(targetNode))
        {
            path.Add(this);
            path.Add(targetNode);
        }
        return path;
    }
}



// So many errors... going to keep this in case I break everything again...
// public void DebugConnections()
// {
//     foreach (var node in connectedNodes)
//     {
//         Debug.Log("Connected to: " + node.name);
//     }
// }

