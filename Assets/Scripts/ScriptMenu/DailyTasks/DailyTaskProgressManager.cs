
using System.Collections.Generic;
using UnityEngine;

public class DailyTaskProgressManager : MonoBehaviour
{
    public static DailyTaskProgressManager Instance;

    private Dictionary<string, int> progressMap = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ giữ lại khi load scene mới
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetProgress(string taskId)
    {
        if (!progressMap.ContainsKey(taskId))
        {
            int saved = PlayerPrefs.GetInt($"DailyTaskProgress_{taskId}", 0);
            progressMap[taskId] = saved;
        }

        return progressMap[taskId];
    }


    public void AddProgress(string taskId, int amount = 1)
    {
        if (!progressMap.ContainsKey(taskId))
            progressMap[taskId] = 0;

        progressMap[taskId] += amount;

        PlayerPrefs.SetInt($"DailyTaskProgress_{taskId}", progressMap[taskId]);
        PlayerPrefs.Save();
    }

    public void ResetAllProgress()
    {
        foreach (var key in progressMap.Keys)
        {
            PlayerPrefs.DeleteKey($"DailyTaskProgress_{key}");
            PlayerPrefs.DeleteKey($"DailyTaskClaimed_{key}");
        }

        progressMap.Clear();
    }


}
