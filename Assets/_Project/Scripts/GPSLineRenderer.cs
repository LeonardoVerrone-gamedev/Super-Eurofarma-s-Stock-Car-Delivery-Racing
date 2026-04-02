using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class GPSRenderer : MonoBehaviour
{
    public Transform player;
    public Transform target;
    public LayerMask obstacleLayer;

    LineRenderer line;
    WaypointPathfinder pathfinder = new WaypointPathfinder();
    List<Vector3> lastValidRender = new List<Vector3>();

    const int maxAttempts = 3;

    HashSet<(Waypoint, Waypoint)> blockedEdges = new();

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

        List<Waypoint> path = GetValidPath(start, end);

        RenderPath(path);

        // limpa só depois de usar
        blockedEdges.Clear();
    }

    List<Waypoint> GetValidPath(Waypoint start, Waypoint end)
    {
        List<Waypoint> bestComplete = null;
        float bestCompleteDist = Mathf.Infinity;

        List<Waypoint> bestPartial = null;
        float bestPartialProgress = 0f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var path = pathfinder.FindPath(start, end, blockedEdges);

            if (path == null)
                break;

            float progress;
            var partial = GetValidPartialPath(path, out progress);

            if (partial == null || partial.Count == 0)
            {
                BlockInvalidEdge(path);
                continue;
            }

            bool isComplete = progress >= 0.999f;

            if (isComplete)
            {
                float dist = GetPathDistance(partial);

                if (dist < bestCompleteDist)
                {
                    bestCompleteDist = dist;
                    bestComplete = partial;
                }
            }
            else
            {
                // fallback: melhor progresso
                if (progress > bestPartialProgress)
                {
                    bestPartialProgress = progress;
                    bestPartial = partial;
                }
            }

            BlockInvalidEdge(path);
        }

        if (bestComplete != null)
            return bestComplete;

        return bestPartial;
    }

    List<Waypoint> GetValidPartialPath(List<Waypoint> path, out float progress)
    {
        List<Waypoint> valid = new();

        Vector2 current = player.position;
        float total = path.Count;
        float reached = 0;

        foreach (var w in path)
        {
            if (!IsSegmentValid(current, w.transform.position))
                break;

            valid.Add(w);
            current = w.transform.position;
            reached++;
        }

        progress = total > 0 ? reached / total : 0f;
        return valid;
    }

    void BlockInvalidEdge(List<Waypoint> path)
    {
        Vector2 currentPos = player.position;
        Waypoint prev = WaypointGraph.Instance.GetClosest(currentPos);

        foreach (var w in path)
        {
            if (!IsSegmentValid(currentPos, w.transform.position))
            {
                if (prev != null)
                    blockedEdges.Add((prev, w));

                return;
            }

            currentPos = w.transform.position;
            prev = w;
        }
    }

    void RenderPath(List<Waypoint> path)
    {
        List<Vector3> finalPath = new();

        Vector2 current = player.position;
        finalPath.Add(current);

        int startIndex = 0;

        // CORTE APENAS NO INÍCIO
        if (path != null && path.Count > 0)
        {
            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (IsSegmentValid(current, path[i].transform.position))
                {
                    startIndex = i;
                    break;
                }
            }

            // vai direto pro melhor ponto visível
            Vector2 firstTarget = path[startIndex].transform.position;
            finalPath.Add(firstTarget);
            current = firstTarget;

            for (int i = startIndex + 1; i < path.Count; i++)
            {
                Vector2 next = path[i].transform.position;

                if (!IsSegmentValid(current, next))
                    break;

                finalPath.Add(next);
                current = next;
            }
        }

        // tenta ir direto pro target
        if (IsSegmentValid(current, target.position))
        {
            finalPath.Add(target.position);
        }

        bool isValid = finalPath.Count > 1;

        if (isValid)
        {
            lastValidRender.Clear();
            lastValidRender.AddRange(finalPath);
        }
        else if (lastValidRender.Count > 1)
        {
            //reconstrói começando do player
            finalPath.Clear();

            Vector2 _current = player.position;
            finalPath.Add(current);

            // reaproveita o resto da linha antiga
            for (int i = 1; i < lastValidRender.Count; i++)
            {
                Vector2 next = lastValidRender[i];

                // opcional: valida ainda
                if (!IsSegmentValid(current, next))
                    break;

                finalPath.Add(next);
                current = next;
            }
        }
        else
        {
            finalPath.Add(current + Vector2.right * 0.1f);
        }

        // RENDER
        line.positionCount = finalPath.Count;

        for (int i = 0; i < finalPath.Count; i++)
            line.SetPosition(i, finalPath[i]);
    }

    bool IsSegmentValid(Vector2 a, Vector2 b)
    {
        Vector2 dir = (b - a);
        float dist = dir.magnitude;

        if (dist <= 0.01f) return true;

        RaycastHit2D hit = Physics2D.Raycast(
            a + dir.normalized * 0.1f,
            dir.normalized,
            dist - 0.1f,
            obstacleLayer
        );

        return hit.collider == null;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    float GetPathDistance(List<Waypoint> path)
    {
        if (path == null || path.Count == 0) return Mathf.Infinity;

        float dist = 0f;
        Vector2 current = player.position;

        foreach (var w in path)
        {
            dist += Vector2.Distance(current, w.transform.position);
            current = w.transform.position;
        }

        dist += Vector2.Distance(current, target.position);

        return dist;
    }
}