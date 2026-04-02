using System.Collections.Generic;
using UnityEngine;

public class WaypointPathfinder
{
    public List<Waypoint> FindPath(
        Waypoint start,
        Waypoint target,
        HashSet<(Waypoint, Waypoint)> blockedEdges
    )
    {
        var open = new List<Waypoint>();
        var closed = new HashSet<Waypoint>();

        var gCost = new Dictionary<Waypoint, float>();
        var parent = new Dictionary<Waypoint, Waypoint>();

        open.Add(start);
        gCost[start] = 0;

        while (open.Count > 0)
        {
            Waypoint current = open[0];

            foreach (var node in open)
            {
                float f = gCost[node] + Heuristic(node, target);
                float fCurrent = gCost[current] + Heuristic(current, target);

                if (f < fCurrent)
                    current = node;
            }

            if (current == target)
                return Retrace(parent, start, target);

            open.Remove(current);
            closed.Add(current);

            foreach (var n in current.neighbors)
            {
                if (n == null || closed.Contains(n))
                    continue;

                // 🔥 IGNORA ARESTAS BLOQUEADAS
                if (blockedEdges.Contains((current, n)))
                    continue;

                float newCost = gCost[current] + Vector2.Distance(
                    current.transform.position,
                    n.transform.position
                );

                if (!gCost.ContainsKey(n) || newCost < gCost[n])
                {
                    gCost[n] = newCost;
                    parent[n] = current;

                    if (!open.Contains(n))
                        open.Add(n);
                }
            }
        }

        return null;
    }

    List<Waypoint> Retrace(Dictionary<Waypoint, Waypoint> parent, Waypoint start, Waypoint end)
    {
        List<Waypoint> path = new List<Waypoint>();
        Waypoint current = end;

        while (current != start)
        {
            path.Add(current);
            current = parent[current];
        }

        path.Add(start);
        path.Reverse();

        return path;
    }

    float Heuristic(Waypoint a, Waypoint b)
    {
        return Vector2.Distance(a.transform.position, b.transform.position);
    }
}