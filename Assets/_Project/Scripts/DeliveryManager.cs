using UnityEngine;
using System.Collections.Generic;

public class DeliveryManager : MonoBehaviour
{
    public Transform player;

    public DeliveryPoint pickupPoint;
    public List<DeliveryPoint> deliveryPoints;

    public GPSRenderer gps;

    public float reachDistance = 1.5f;

    [Header("Run Settings")]
    public int maxDeliveries = 5;

    private int completedDeliveries = 0;

    DeliveryPoint currentTarget;
    bool carryingPackage = false;

    public System.Action<Vector3> OnNewTarget;
    public System.Action OnRunFinished;

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
                // PEGOU PACOTE
                carryingPackage = true;
                Debug.Log("📦 Pegou pacote");
            }
            else
            {
                // ENTREGOU
                carryingPackage = false;
                completedDeliveries++;

                Debug.Log($"✅ Entrega {completedDeliveries}/{maxDeliveries}");

                // chama sistema de run
                FindObjectOfType<DeliveryRunManager>()?.CompleteDelivery();

                // terminou tudo?
                if (completedDeliveries >= maxDeliveries)
                {
                    FinishRun();
                    return;
                }
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
    }

    void FinishRun()
    {
        Debug.Log("TODAS ENTREGAS COMPLETAS!");

        currentTarget = null;

        OnRunFinished?.Invoke();
    }
}