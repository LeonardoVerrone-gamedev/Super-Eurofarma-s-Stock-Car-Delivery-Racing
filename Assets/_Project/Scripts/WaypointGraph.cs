using System.Collections.Generic;
using UnityEngine;

public class WaypointGraph : MonoBehaviour
{
    public static WaypointGraph Instance;

    public List<Waypoint> waypoints = new List<Waypoint>();

    void Awake()
    {
        Instance = this;
        waypoints = new List<Waypoint>(FindObjectsOfType<Waypoint>());
    }

    public Waypoint GetClosest(Vector2 pos)
    {
        Waypoint best = null;
        float bestDist = Mathf.Infinity;

        foreach (var w in waypoints)
        {
            float d = Vector2.Distance(pos, w.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = w;
            }
        }

        return best;
    }
}