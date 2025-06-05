using UnityEngine;
using TMPro;
using System;

public class RankSeasonResetManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text seasonNumberText;
    private int currentSeasonIndex;

    [Header("Season Config")]
    [SerializeField] private int seasonDurationDays = 15;

    private DateTime fixedSeasonStart => new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc);
    private DateTime nextResetTime;
    private bool hasResetThisCycle = false;
    void Start()
    {
        UpdateNextResetTime();
    }

    void Update()
    {
        TimeSpan remaining = nextResetTime - DateTime.UtcNow;

        if (remaining.TotalSeconds <= 0 && !hasResetThisCycle)
        {
            ResetRankSeason();
            UpdateNextResetTime();
            hasResetThisCycle = true;
        }
        else if (remaining.TotalSeconds > 0)
        {
            hasResetThisCycle = false;
            countdownText.text = FormatTime(remaining);
        }
    }

    private void UpdateNextResetTime()
    {
        TimeSpan sinceStart = DateTime.UtcNow - fixedSeasonStart;
        currentSeasonIndex = Mathf.FloorToInt((float)(sinceStart.TotalDays / seasonDurationDays)) + 1;
        nextResetTime = fixedSeasonStart.AddDays(currentSeasonIndex * seasonDurationDays);

        if (seasonNumberText != null)
            seasonNumberText.text = $"Season Rank #{currentSeasonIndex}";
    }

    private string FormatTime(TimeSpan time)
    {
        return $"Season ends in: {time.Days}d {time.Hours:D2}h{time.Minutes:D2}m{time.Seconds}s";
    }

    private void ResetRankSeason()
    {
        Debug.Log("🔁 Reset mùa rank → quay lại Bronze I");

        PlayerPrefs.DeleteKey("PlayerRankIndex");
        PlayerPrefs.DeleteKey("PlayerRankExp");

        if (RankDataManager.Instance != null)
        {
            RankDataManager.Instance.LoadRank();
        }
    }
}
