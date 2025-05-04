using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class NavMesh : MonoBehaviour
{
    // implement NavMesh generation here:
    //    the outline are Walls in counterclockwise order
    //    iterate over them, and if you find a reflex angle
    //    you have to split the polygon into two
    //    then perform the same operation on both parts
    //    until no more reflex angles are present
    //
    //    when you have a number of polygons, you will have
    //    to convert them into a graph: each polygon is a node
    //    you can find neighbors by finding shared edges between
    //    different polygons (or you can keep track of this while 
    //    you are splitting)
    public Graph MakeNavMesh(List<Wall> outline)
    {
        Graph g = new Graph {
            all_nodes = new List<GraphNode>(),
            outline = outline
        };
        var polygons = new List<List<Wall>>{
            new List<Wall>(outline)
        };
        bool done = false;
        while (!done) {
            done = true;
            for (int i = 0; i < polygons.Count; i++) {
                int nc = NonConvex(polygons[i]);
                if (nc >= 0) {
                    int split = FindSplitPoint(polygons[i], nc);
                    if (split >= 0) {
                        var (A, B) = SplitPolygon(polygons[i], nc, split);
                        polygons.RemoveAt(i);
                        polygons.Add(A);
                        polygons.Add(B);
                        done = false;
                    }
                    break;
                }
            }
        }
        for (int i = 0; i < polygons.Count; i++) {
            g.all_nodes.Add(new GraphNode(i, polygons[i]));
        }
        for (int i = 0; i < g.all_nodes.Count; i++) {
            var iNode = g.all_nodes[i];
            var iPolygon = iNode.GetPolygon();
            for (int v = i + 1; v < g.all_nodes.Count; v++) {
                var vNode = g.all_nodes[v];
                var vPolygon = vNode.GetPolygon();
                for (int x = 0; x < iPolygon.Count; x++) {
                    for (int z = 0; z < vPolygon.Count; z++) {
                        if (iPolygon[x].Same(vPolygon[z])) {
                            iNode.AddNeighbor(vNode, x);
                            vNode.AddNeighbor(iNode, z);
                        }
                    }
                }
            }
        }
        return g;
    }
    
    private int NonConvex(List<Wall> polygon) {
        int polygonCount = polygon.Count;
        var vertices = new List<Vector3>(polygonCount);
        foreach (var wall in polygon) {
            vertices.Add(wall.start);
        }
        float area = 0;
        for (int i = 0; i < polygonCount; i++) {
            area += (vertices[i].x * vertices[(i + 1) % polygonCount].z - vertices[(i + 1) % polygonCount].x * vertices[i].z);
        }
        float orient = Mathf.Sign(area);
        for (int i = 0; i < polygonCount; i++) {
            var prev = vertices[(i - 1 + polygonCount) % polygonCount];
            var curr = vertices[i];
            var next = vertices[(i + 1) % polygonCount];
            var a = curr - prev;
            var b = next - curr;
            if ((a.x * b.z - a.z * b.x) * orient < 0) {
                return i;
            }
        }
        return -1;
    }
    
    private int FindSplitPoint(List<Wall> polygon, int id) {
        int polygonCount = polygon.Count;
        var vertices = new List<Vector3>(polygonCount);
        foreach (var wall in polygon) {
            vertices.Add(wall.start);
        }
        int prev = (id - 1 + polygonCount) % polygonCount;
        int next = (id + 1) % polygonCount;
        for (int i = 0; i < polygonCount; i++) {
            if (i == id || i == prev || i == next) {
                continue;
            }
            bool breakOut = false;
            for (int v = 0; v < polygonCount; v++) {
                if (v == prev || v == id || v == ((i - 1 + polygonCount) % polygonCount) || v == i) {
                    continue;
                }
                if (polygon[v].Crosses(vertices[id], vertices[i])) {
                    breakOut = true;
                    break;
                }
            }
            if (breakOut) {
                continue;
            }
            if (PointInPolygon((vertices[id] + vertices[i]) * 0.5f, vertices)) {
                return i;
            }
        }
        return -1;
    }

    private bool PointInPolygon(Vector3 point, List<Vector3> vertices) {
        bool inside = false;
        for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++) {
            if (((vertices[i].z > point.z) != (vertices[j].z > point.z)) && (point.x < (vertices[j].x - vertices[i].x) * (point.z - vertices[i].z) / (vertices[j].z - vertices[i].z) + vertices[i].x)) {
                inside = !inside;
            }
        }
        return inside;
    }
    
    private (List<Wall> A, List<Wall> B) SplitPolygon(List<Wall> polygons, int nc, int split) {
        int polygonCount = polygons.Count;
        var vertices = new List<Vector3>(polygonCount);
        foreach (var wall in polygons) {
            vertices.Add(wall.start);
        }
        var A = new List<Wall>();
        for (int i = nc; ; i = (i + 1) % polygonCount) {
            if (i == split) {
                break;
            }
            A.Add(new Wall(vertices[i], vertices[(i + 1) % polygonCount]));
        }
        A.Add(new Wall(vertices[split], vertices[nc]));
        var B = new List<Wall>();
        for (int k = split; ; k = (k + 1) % polygonCount) {
            if (k == nc) {
                break;
            }
            B.Add(new Wall(vertices[k], vertices[(k + 1) % polygonCount]));
        }
        B.Add(new Wall(vertices[nc], vertices[split]));
        return (A, B);
    }

    List<Wall> outline;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventBus.OnSetMap += SetMap;
    }

    // Update is called once per frame
    void Update()
    {
       

    }

    public void SetMap(List<Wall> outline)
    {
        Graph navmesh = MakeNavMesh(outline);
        if (navmesh != null)
        {
            Debug.Log("got navmesh: " + navmesh.all_nodes.Count);
            EventBus.SetGraph(navmesh);
        }
    }

    


    
}
