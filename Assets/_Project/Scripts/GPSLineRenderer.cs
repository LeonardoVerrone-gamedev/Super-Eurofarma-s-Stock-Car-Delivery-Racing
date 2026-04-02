using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class GPSRenderer : MonoBehaviour
{
    public Transform player;
    public Transform target;

    LineRenderer line;
    WaypointPathfinder pathfinder = new WaypointPathfinder();

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (player == null || target == null) return;

        var graph = WaypointGraph.Instance;

        Waypoint start = graph.GetClosest(player.position);
        Waypoint end = graph.GetClosest(target.position);

        if (start == null || end == null) return;

        List<Waypoint> path = pathfinder.FindPath(start, end);

        if (path == null || path.Count == 0)
        {
            line.positionCount = 0;
            return;
        }

        // inclui player e destino real
        List<Vector3> finalPath = new List<Vector3>();

        finalPath.Add(player.position);

        foreach (var w in path)
            finalPath.Add(w.transform.position);

        finalPath.Add(target.position);

        line.positionCount = finalPath.Count;

        for (int i = 0; i < finalPath.Count; i++)
            line.SetPosition(i, finalPath[i]);
    }
}