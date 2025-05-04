using UnityEngine;
using System.Collections.Generic;

public class PathFinder : MonoBehaviour
{
    // Assignment 2: Implement AStar
    //
    // DO NOT CHANGE THIS SIGNATURE (parameter types + return type)
    // AStar will be given the start node, destination node and the target position, and should return 
    // a path as a list of positions the agent has to traverse to reach its destination, as well as the
    // number of nodes that were expanded to find this path
    // The last entry of the path will be the target position, and you can also use it to calculate the heuristic
    // value of nodes you add to your search frontier; the number of expanded nodes tells us if your search was
    // efficient
    //
    // Take a look at StandaloneTests.cs for some test cases
    public static (List<Vector3>, int) AStar(GraphNode start, GraphNode destination, Vector3 target)
    {
        var open = new List<GraphNode> { start };
        var gScore = new Dictionary<int, float> { [start.GetID()] = 0f };
        var fScore = new Dictionary<int, float> { [start.GetID()] = Vector3.Distance(start.GetCenter(), target) };
        var cameFrom = new Dictionary<int, (int parentID, Vector3 via)>();
        var posAt = new Dictionary<int, Vector3> { [start.GetID()] = start.GetCenter() };
        int nodeNum = 0;
        while (open.Count > 0) {
            GraphNode current = open[0];
            float bestFScore = fScore[current.GetID()];
            for (int i = 1; i < open.Count; i++) {
                var node = open[i];
                float currentF = fScore[node.GetID()];
                if (currentF < bestFScore) {
                    bestFScore = currentF;
                    current = node;
                }
            }
            int currentId = current.GetID();
            if (currentId == destination.GetID()) {
                var stack = new Stack<Vector3>();
                int nodeID = currentId;
                while (cameFrom.ContainsKey(nodeID)) {
                    var info = cameFrom[nodeID];
                    stack.Push(info.via);
                    nodeID = info.parentID;
                }
                var path = new List<Vector3>();
                while (stack.Count > 0) {
                    path.Add(stack.Pop());
                }
                path.Add(target);
                return (path, nodeNum);
            }
            open.Remove(current);
            nodeNum++;
            foreach (var nbr in current.GetNeighbors()) {
                // Debug.Log(nbr);
                var next = nbr.GetNode();
                int nextId = next.GetID();
                var wallMid = nbr.GetWall().midpoint;
                // Debug.Log(next + "     " + nextId + "     " + wallMid);
                float tentativeGScore = gScore[currentId] + Vector3.Distance(posAt[currentId], wallMid);
                if (!gScore.ContainsKey(nextId) || tentativeGScore < gScore[nextId]) {
                    cameFrom[nextId] = (currentId, wallMid);
                    gScore[nextId] = tentativeGScore;
                    float h = Vector3.Distance(wallMid, target);
                    fScore[nextId] = tentativeGScore + h;
                    posAt[nextId] = wallMid;
                    if (!open.Contains(next)) {
                        open.Add(next);
                    }
                }
            }
        }

        // return path and number of nodes expanded
        return (new List<Vector3> { target }, nodeNum);

    }

    public Graph graph;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventBus.OnTarget += PathFind;
        EventBus.OnSetGraph += SetGraph;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGraph(Graph g)
    {
        graph = g;
    }

    // entry point
    public void PathFind(Vector3 target)
    {
        if (graph == null) return;

        // find start and destination nodes in graph
        GraphNode start = null;
        GraphNode destination = null;
        foreach (var n in graph.all_nodes)
        {
            if (Util.PointInPolygon(transform.position, n.GetPolygon()))
            {
                start = n;
            }
            if (Util.PointInPolygon(target, n.GetPolygon()))
            {
                destination = n;
            }
        }
        if (destination != null)
        {
            // only find path if destination is inside graph
            EventBus.ShowTarget(target);
            (List<Vector3> path, int expanded) = PathFinder.AStar(start, destination, target);

            Debug.Log("found path of length " + path.Count + " expanded " + expanded + " nodes, out of: " + graph.all_nodes.Count);
            EventBus.SetPath(path);
        }
        

    }

    

 
}
