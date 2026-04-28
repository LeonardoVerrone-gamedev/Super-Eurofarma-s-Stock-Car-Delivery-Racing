using UnityEngine;

public class DeliveryRunManager : MonoBehaviour
{
    public int totalDeliveries = 5;

    private int completed;
    private float totalTime;
    private bool running;

    void Start()
    {
        StartRun();
    }

    public void StartRun()
    {
        totalTime = 0f;
        completed = 0;
        running = true;
    }

    void Update()
    {
        if (!running) return;

        totalTime += Time.deltaTime;
    }

    public void CompleteDelivery()
    {
        completed++;

        if (completed >= totalDeliveries)
        {
            FinishRun();
        }
    }

    void FinishRun()
    {
        running = false;

        float bestLocal = PlayerPrefs.GetFloat("BestRun", float.MaxValue);

        if (totalTime < bestLocal)
        {
            PlayerPrefs.SetFloat("BestRun", totalTime);
            Debug.Log("Novo recorde local!");
        }

        SupabaseDeliveryManager.Instance.SubmitRun(totalTime);

        Debug.Log("Tempo final: " + totalTime);
    }
}