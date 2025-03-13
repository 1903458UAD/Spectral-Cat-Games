using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;
using UnityEngine.AI;



#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteInEditMode]
public class NavNode : MonoBehaviour
{
    public List<NavNode> connectedNodes = new List<NavNode>(); // Direct connections
    private float connectionDistance = 2f; // Reduced distance to connect nodes
    public Dictionary<NavNode, float> nodeDistances = new Dictionary<NavNode, float>(); // Store distance to connected nodes

    private static List<NavNode> allNodes = new List<NavNode>(); // Global list to hold all nodes in the scene

    private void Start()
    {
        if (!Application.isPlaying)
        {
            return;
        }
    }

#if UNITY_EDITOR

    private void OnValidate() 
    {
        GenerateConnections();
        PrintDiagnostics();
    }
#endif

    // This will generate connections for each node within the specified distance
    private void GenerateConnections()
    {
        connectedNodes.Clear();
        nodeDistances.Clear();
        NavNode[] allNodesArray = FindObjectsOfType<NavNode>();

        foreach (NavNode node in allNodesArray)
        {
            if (node != this)
            {
                float distance = Vector3.Distance(transform.position, node.transform.position);
                if (distance <= 2f && IsPathClear(node)) // Check if path is clear
                {
                    connectedNodes.Add(node);
                    nodeDistances[node] = distance;
                }
            }
        }

        // Store all nodes globally
        if (!allNodes.Contains(this))
        {
            allNodes.Add(this);
        }

        //Debug.Log($"[NavNode] Connections updated for {gameObject.name}");
    }


    private bool IsPathClear(NavNode targetNode)
    {
        NavMeshHit hit;
        return !NavMesh.Raycast(transform.position, targetNode.transform.position, out hit, NavMesh.AllAreas);
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

    public void PrintDiagnostics()
    {
        string diagnostics = $"[NavNode Diagnostics] Node: {gameObject.name} at position {transform.position}\n";
        NavMeshHit hit;
        bool onNavMesh = NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas);
        diagnostics += onNavMesh ? "On valid NavMesh." : "NOT on a valid NavMesh!";
        diagnostics += "\nConnected nodes:\n";
        foreach (var node in connectedNodes)
        {
            float distance = nodeDistances[node];
            bool pathClear = IsPathClear(node);
            diagnostics += $"- {node.gameObject.name} at {node.transform.position}: distance {distance:F2}, path clear: {pathClear}\n";
        }
        Debug.Log(diagnostics, this);
    }

    public static List<NavNode> GetAllNodes()
    {
        return allNodes;
    }
}

//// Checks if the path between two nodes is clear (no obstacles in the way)
//private bool IsPathClear(NavNode targetNode)
//    {
//        NavMeshHit hit;
//        bool pathBlocked = NavMesh.Raycast(transform.position, targetNode.transform.position, out hit, NavMesh.AllAreas);
//        return !pathBlocked;
//    }

//    // Draw lines between connected nodes in the scene view for debugging
//    private void OnDrawGizmos()
//    {
//        Gizmos.color = Color.green;
//        foreach (var node in connectedNodes)
//        {
//            if (node != null)
//            {
//                Gizmos.DrawLine(transform.position, node.transform.position); // Draw line between nodes
//            }
//        }
//    }

//    // Returns the closest node to the given position
//    public NavNode GetClosestNode(Vector3 position)
//    {
//        NavNode closestNode = null;
//        float closestDistance = float.MaxValue;

//        foreach (var node in connectedNodes)
//        {
//            float distance = Vector3.Distance(position, node.transform.position);
//            if (distance < closestDistance)
//            {
//                closestDistance = distance;
//                closestNode = node;
//            }
//        }

//        return closestNode;
//    }

//    // Queries the distance between this node and the target node
//    public float GetDistanceToNode(NavNode targetNode)
//    {
//        if (nodeDistances.ContainsKey(targetNode))
//        {
//            return nodeDistances[targetNode];
//        }
//        return float.MaxValue;
//    }

//    // Static method to retrieve all nodes for external use
//    public static List<NavNode> GetAllNodes()
//    {
//        return allNodes;
//    }

//    // A method to query the shortest path from this node to another (simple example using Dijkstra's algorithm)
//    public List<NavNode> FindShortestPath(NavNode targetNode)
//    {
//        // Dijkstra's algorithm or A* could be implemented here. For simplicity, let's just return direct connections.
//        List<NavNode> path = new List<NavNode>();
//        if (connectedNodes.Contains(targetNode))
//        {
//            path.Add(this);
//            path.Add(targetNode);
//        }
//        return path;
//    }
//}



// So many errors... going to keep this in case I break everything again...
// public void DebugConnections()
// {
//     foreach (var node in connectedNodes)
//     {
//         Debug.Log("Connected to: " + node.name);
//     }
// }

