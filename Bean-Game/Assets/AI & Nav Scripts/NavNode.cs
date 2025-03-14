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
        //PrintDiagnostics();
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

  

    public static List<NavNode> GetAllNodes()
    {
        return allNodes;
    }
}
