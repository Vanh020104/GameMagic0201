using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AdWatchLimiter : MonoBehaviour
{
    [System.Serializable]
    public class AdWatchInfo
    {
        public string adType;
        public List<string> timestamps = new List<string>(); // ISO datetime
    }

    public List<AdWatchInfo> adHistories = new List<AdWatchInfo>();
    private const int MaxViewsPerDay = 5;
    private const int CooldownMinutes = 2;
    public static AdWatchLimiter Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        LoadHistory();
    }

    public bool CanWatch(string type)
    {
        var now = DateTime.Now;
        var info = GetOrCreateInfo(type);
        PurgeOldEntries(info);

        if (info.timestamps.Count >= MaxViewsPerDay) return false;

        if (info.timestamps.Count > 0)
        {
            DateTime lastTime = DateTime.Parse(info.timestamps[^1]);
            if ((now - lastTime).TotalMinutes < CooldownMinutes) return false;
        }

        return true;
    }

    public void RecordWatch(string type)
    {
        var info = GetOrCreateInfo(type);
        info.timestamps.Add(DateTime.Now.ToString("o")); // ISO format
        SaveHistory();
    }

    private AdWatchInfo GetOrCreateInfo(string type)
    {
        var info = adHistories.Find(a => a.adType == type);
        if (info == null)
        {
            info = new AdWatchInfo { adType = type };
            adHistories.Add(info);
        }
        return info;
    }

    private void PurgeOldEntries(AdWatchInfo info)
    {
        DateTime now = DateTime.Now;
        info.timestamps.RemoveAll(t => DateTime.Parse(t).Date != now.Date);
    }

    private void SaveHistory()
    {
        PlayerPrefs.SetString("AdHistory", JsonUtility.ToJson(this));
    }

    private void LoadHistory()
    {
        string json = PlayerPrefs.GetString("AdHistory", "");
        if (!string.IsNullOrEmpty(json))
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}
