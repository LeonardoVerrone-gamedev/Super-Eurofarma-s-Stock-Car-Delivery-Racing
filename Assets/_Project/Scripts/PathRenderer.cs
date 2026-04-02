using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class PathRenderer : MonoBehaviour
{
    public Transform player;
    public AStar pathfinder;
    public Grid2D grid;

    LineRenderer line;
    Vector2 target;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void SetTarget(Vector2 newTarget)
    {
        target = newTarget;

        CancelInvoke();
        InvokeRepeating(nameof(UpdatePath), 0f, 0.4f); // ↓ menos frequência
    }

    void UpdatePath()
    {
        Vector2 toTarget = target - (Vector2)player.position;

        float dist = toTarget.magnitude;
        float maxDist = grid.gridWorldSize.x * 0.4f;

        Vector2 clampedTarget = (Vector2)player.position
            + toTarget.normalized * Mathf.Min(dist, maxDist);

        var path = pathfinder.FindPath(player.position, clampedTarget);

        if (path == null || path.Count == 0)
        {
            line.positionCount = 0;
            return;
        }

        path.Insert(0, player.position);
        path[path.Count - 1] = target;

        line.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
            line.SetPosition(i, path[i]);
    }
}