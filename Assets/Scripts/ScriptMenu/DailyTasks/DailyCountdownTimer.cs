// using System;
// using TMPro;
// using UnityEngine;

// public class DailyCountdownTimer : MonoBehaviour
// {
//     [SerializeField] private TMP_Text timeText;

//     void Update()
//     {
//         DateTime now = DateTime.Now;
//         DateTime nextReset = now.Date.AddDays(1); 
//         TimeSpan timeRemaining = nextReset - now;

//         timeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
//             timeRemaining.Hours, timeRemaining.Minutes, timeRemaining.Seconds);
//     }
// }
using System;
using TMPro;
using UnityEngine;

public class DailyCountdownTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;

    private string lastResetDate;
    private bool hasResetToday;

    void Start()
    {
        lastResetDate = DateTime.Now.ToString("yyyyMMdd");
        hasResetToday = false;
    }

    void Update()
    {
        DateTime now = DateTime.Now;
        DateTime nextReset = now.Date.AddDays(1); 
        TimeSpan timeRemaining = nextReset - now;

        timeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
            timeRemaining.Hours, timeRemaining.Minutes, timeRemaining.Seconds);

        string today = now.ToString("yyyyMMdd");
        if (today != lastResetDate && !hasResetToday)
        {
            hasResetToday = true;
            lastResetDate = today;

            // 🔥 Gọi reset nhiệm vụ mới
            if (DailyTaskManager.Instance != null)
            {
                DailyTaskManager.Instance.ResetDailyTasks();
            }
        }
    }
}
