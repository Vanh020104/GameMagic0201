using UnityEngine;
using TMPro;
using System;

public class RankSeasonResetManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text seasonNumberText;

    [Header("Season Config")]
    [SerializeField] private int seasonDurationDays = 15; // 15 ngày

    private DateTime fixedSeasonStart;      // Thời điểm bắt đầu mùa đầu tiên
    private DateTime nextResetTime;         // Thời điểm reset tiếp theo
    private int currentSeasonIndex;
    private bool hasResetThisCycle = false;

    void Start()
    {
        // ✅ Set fixed ngày bắt đầu mùa đầu tiên (VD: 1/6/2025)
        fixedSeasonStart = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        UpdateNextResetTime();
    }

    void Update()
    {
        TimeSpan remaining = nextResetTime - DateTime.UtcNow;

        if (remaining.TotalSeconds <= 0 && !hasResetThisCycle)
        {
            Debug.Log("✅ Đủ thời gian → Reset rank season");
            ResetRankSeason();
            UpdateNextResetTime();
            hasResetThisCycle = true;
        }
        else if (remaining.TotalSeconds > 0)
        {
            hasResetThisCycle = false;

            if (countdownText != null)
                countdownText.text = FormatTime(remaining);
        }
    }

    private void UpdateNextResetTime()
    {
        TimeSpan seasonDuration = TimeSpan.FromDays(seasonDurationDays);
        nextResetTime = fixedSeasonStart;
        currentSeasonIndex = 1;

        while (nextResetTime <= DateTime.UtcNow)
        {
            nextResetTime = nextResetTime.Add(seasonDuration);
            currentSeasonIndex++;
        }

        // Debug.Log($"📆 Cập nhật thời gian reset tiếp theo: {nextResetTime} | Season #{currentSeasonIndex}");

        if (seasonNumberText != null)
            seasonNumberText.text = $"Season Rank #{currentSeasonIndex}";
    }

    private string FormatTime(TimeSpan time)
    {
        return $"Season ends in: {time.Days}d {time.Hours:D2}h{time.Minutes:D2}m{time.Seconds:D2}s";
    }

    private void ResetRankSeason()
    {
        Debug.Log("🔁 Reset mùa rank → quay lại Bronze I");

        // Xóa rank cũ
        PlayerPrefs.DeleteKey("PlayerRankIndex");
        PlayerPrefs.DeleteKey("PlayerRankExp");

        // Load lại rank từ đầu
        if (RankDataManager.Instance != null)
        {
            RankDataManager.Instance.LoadRank();
        }

        // Cập nhật UI
        var ui = FindObjectOfType<RankUIController>();
        if (ui != null)
        {
            ui.UpdateUI();
        }
    }
}
