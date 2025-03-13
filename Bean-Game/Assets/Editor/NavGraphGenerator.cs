#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class NavGraphGenerator : EditorWindow
{
    [MenuItem("Tools/Generate NavGraph")]
    public static void GenerateGraph()
    {
        // Find all NavNode components in the scene.
        NavNode[] allNodes = FindObjectsOfType<NavNode>();
        NavGraph navGraph = ScriptableObject.CreateInstance<NavGraph>();

        Dictionary<NavNode, int> nodeIndexMap = new Dictionary<NavNode, int>();
        for (int i = 0; i < allNodes.Length; i++)
        {
            nodeIndexMap[allNodes[i]] = i;
        }

        // Populate node data.
        foreach (NavNode node in allNodes)
        {
            node.PrintDiagnostics();  // (Optional) Print diagnostics for debugging.

            NavGraph.NavNodeData nodeData = new NavGraph.NavNodeData
            {
                position = node.transform.position,
                connectedNodeIndices = new List<int>(),
                distances = new List<float>()
            };

            foreach (var connectedNode in node.connectedNodes)
            {
                if (nodeIndexMap.ContainsKey(connectedNode))
                {
                    nodeData.connectedNodeIndices.Add(nodeIndexMap[connectedNode]);
                    nodeData.distances.Add(node.nodeDistances[connectedNode]);
                }
            }

            navGraph.nodes.Add(nodeData);
        }

        // Precompute routes between every pair of nodes.
        navGraph.precomputedRoutes = new List<NavGraph.RouteData>();
        int nodeCount = navGraph.nodes.Count;
        int maxRoutesPerPair = 5; // Adjust this number as needed.
        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = 0; j < nodeCount; j++)
            {
                if (i == j)
                    continue;

                List<List<int>> routes = ComputeRoutes(i, j, navGraph, maxRoutesPerPair);
                if (routes != null && routes.Count > 0)
                {
                    NavGraph.RouteData routeData = new NavGraph.RouteData
                    {
                        sourceIndex = i,
                        destinationIndex = j,
                        pathIndices = routes
                    };
                    navGraph.precomputedRoutes.Add(routeData);
                }
            }
        }

        AssetDatabase.CreateAsset(navGraph, "Assets/NavGraph.asset");
        AssetDatabase.SaveAssets();
        Debug.Log("[NavGraphGenerator] NavGraph asset created with " +
                  navGraph.nodes.Count + " nodes and " +
                  navGraph.precomputedRoutes.Count + " route entries!");
    }

    private static List<List<int>> ComputeRoutes(int start, int destination, NavGraph navGraph, int maxRoutes)
    {
        List<List<int>> routes = new List<List<int>>();
        int maxDepth = 10; // Aggressive maximum depth limit

        // DFS function with iterative deepening behavior.
        void DFS(List<int> currentPath)
        {
            if (routes.Count >= maxRoutes)
                return;

            int last = currentPath[currentPath.Count - 1];
            if (last == destination)
            {
                routes.Add(new List<int>(currentPath));
                return;
            }
            if (currentPath.Count >= maxDepth)
                return;

            // Get neighbors and sort them by distance to destination.
            List<int> neighbors = new List<int>(navGraph.nodes[last].connectedNodeIndices);
            neighbors.Sort((a, b) =>
            {
                float da = Vector3.Distance(navGraph.nodes[a].position, navGraph.nodes[destination].position);
                float db = Vector3.Distance(navGraph.nodes[b].position, navGraph.nodes[destination].position);
                return da.CompareTo(db);
            });

            // Optionally limit the number of neighbors considered.
            int neighborLimit = 3;
            int count = 0;
            foreach (int neighbor in neighbors)
            {
                if (count++ >= neighborLimit)
                    break;
                if (!currentPath.Contains(neighbor))
                {
                    currentPath.Add(neighbor);
                    DFS(currentPath);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
            }
        }

        DFS(new List<int> { start });
        // Optionally, sort routes by hop count or total distance.
        routes.Sort((a, b) => a.Count.CompareTo(b.Count));
        return routes;
    }
}

#endif
