using UnityEngine;
using System.Collections.Generic;

public class DeliveryManager : MonoBehaviour
{
    public Transform player;

    public DeliveryPoint pickupPoint; // ponto A
    public List<DeliveryPoint> deliveryPoints;

    public GPSRenderer gps;

    public float reachDistance = 1.5f;

    DeliveryPoint currentTarget;
    bool carryingPackage = false;

    public System.Action<Vector3> OnNewTarget;

    void Start()
    {
        SetNextTarget();
    }

    void Update()
    {
        if (currentTarget == null) return;

        float dist = Vector2.Distance(player.position, currentTarget.transform.position);

        if (dist < reachDistance)
        {
            if (!carryingPackage)
            {
                // pegou pacote
                carryingPackage = true;
                Debug.Log("Recebe");
            }
            else
            {
                // entregou
                carryingPackage = false;
                Debug.Log("Entrega");
            }

            SetNextTarget();
        }
    }

    void SetNextTarget()
    {
        if (!carryingPackage)
        {
            currentTarget = pickupPoint;
        }
        else
        {
            currentTarget = deliveryPoints[Random.Range(0, deliveryPoints.Count)];
        }

        OnNewTarget?.Invoke(currentTarget.transform.position);

        gps.SetTarget(currentTarget.transform);

        Debug.Log("New target pos!");
    }
}