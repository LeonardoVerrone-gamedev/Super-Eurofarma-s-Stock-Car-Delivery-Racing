using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class SupabaseDeliveryManager : MonoBehaviour
{
    public static SupabaseDeliveryManager Instance;

    public string url = "https://dxlnnnzsfdizyjnpexlx.supabase.co";
    public string apiKey = "sb_publishable_Zz_93lVw09pKslI7J5CISQ_BhK3ISye";

    string playerId;
    string playerName = "Player";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (PlayerPrefs.HasKey("PlayerId"))
            playerId = PlayerPrefs.GetString("PlayerId");
        else
        {
            playerId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("PlayerId", playerId);
        }
    }

    // =========================
    // SUBMIT RUN
    // =========================
    public void SubmitRun(float totalTime)
    {
        StartCoroutine(SubmitCoroutine(totalTime));
    }

    IEnumerator SubmitCoroutine(float totalTime)
    {
        DeliveryPayload payload = new DeliveryPayload
        {
            p_player_id = playerId,
            p_player_name = playerName,
            p_total_time = totalTime
        };

        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest req = new UnityWebRequest(url + "/rest/v1/rpc/submit_delivery_run", "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);

            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("apikey", apiKey);
            req.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                Debug.Log(" Run enviada (se top 3)");
            else
                Debug.LogError("Erro: " + req.error);
        }
    }

    // =========================
    // GET TOP 3
    // =========================
    public void GetTop3(System.Action<string> callback)
    {
        StartCoroutine(GetTop3Coroutine(callback));
    }

    IEnumerator GetTop3Coroutine(System.Action<string> callback)
    {
        string fullUrl = $"{url}/rest/v1/delivery_runs?select=player_id,player_name,total_time&order=total_time.asc&limit=3";

        using (UnityWebRequest req = UnityWebRequest.Get(fullUrl))
        {
            req.SetRequestHeader("apikey", apiKey);
            req.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                callback?.Invoke(req.downloadHandler.text);
            else
                Debug.LogError("Erro fetch: " + req.error);
        }
    }
}

[System.Serializable]
public class DeliveryPayload
{
    public string p_player_id;
    public string p_player_name;
    public float p_total_time;
}