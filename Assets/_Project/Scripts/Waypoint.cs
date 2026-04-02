using UnityEngine;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{
    public List<Waypoint> neighbors = new List<Waypoint>();

    public bool blocked;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);

        Gizmos.color = Color.white;
        foreach (var n in neighbors)
        {
            if (n != null)
                Gizmos.DrawLine(transform.position, n.transform.position);
        }
    }
}