using UnityEngine;
using System.Collections.Generic;

public class AStar : MonoBehaviour
{
    public Grid2D grid;

    int currentSearchId = 0;
    List<Node> neighbors = new();

    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        currentSearchId++;

        Node start = grid.NodeFromWorld(startPos);
        Node target = grid.NodeFromWorld(targetPos);

        if (start == null || target == null) return null;

        MinHeap openSet = new();

        start.gCost = 0;
        start.hCost = GetDistance(start, target);
        start.searchId = currentSearchId;

        openSet.Add(start);
        start.inOpenSet = true;

        while (openSet.Count > 0)
        {
            Node current = openSet.RemoveFirst();
            current.inOpenSet = false;
            current.closedId = currentSearchId;

            if (current == target)
                return Retrace(start, target);

            grid.GetNeighbors(current, neighbors);

            foreach (var n in neighbors)
            {
                if (!n.walkable || n.closedId == currentSearchId)
                    continue;

                if (n.searchId != currentSearchId)
                {
                    n.gCost = int.MaxValue;
                    n.searchId = currentSearchId;
                    n.inOpenSet = false;
                }

                int newCost = current.gCost + GetDistance(current, n);

                if (newCost < n.gCost)
                {
                    n.gCost = newCost;
                    n.hCost = GetDistance(n, target);
                    n.parent = current;

                    if (!n.inOpenSet)
                    {
                        openSet.Add(n);
                        n.inOpenSet = true;
                    }
                    else
                    {
                        openSet.UpdateItem(n);
                    }
                }
            }
        }

        return null;
    }

    List<Vector2> Retrace(Node start, Node end)
    {
        List<Vector2> path = new();
        Node current = end;

        while (current != start)
        {
            path.Add(current.worldPos);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        return dx > dy
            ? 14 * dy + 10 * (dx - dy)
            : 14 * dx + 10 * (dy - dx);
    }
}