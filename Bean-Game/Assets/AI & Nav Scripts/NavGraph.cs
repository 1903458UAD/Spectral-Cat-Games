using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NavGraph", menuName = "AI/NavGraph", order = 1)]
public class NavGraph : ScriptableObject
{
    public List<NavNodeData> nodes = new List<NavNodeData>();

    [System.Serializable]
    public class NavNodeData
    {
        public Vector3 position;
        public List<int> connectedNodeIndices;
        public List<float> distances;
    }

    [System.Serializable]
    public class RouteData
    {
        public int sourceIndex;
        public int destinationIndex;
        // List of routes; each route is a list of node indices.
        public List<List<int>> pathIndices;
    }

    public List<RouteData> precomputedRoutes = new List<RouteData>();
}
