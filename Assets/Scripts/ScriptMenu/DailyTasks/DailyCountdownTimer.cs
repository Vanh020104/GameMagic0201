using System;
using TMPro;
using UnityEngine;

public class DailyCountdownTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;

    void Update()
    {
        DateTime now = DateTime.Now;
        DateTime nextReset = now.Date.AddDays(1); 
        TimeSpan timeRemaining = nextReset - now;

        timeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
            timeRemaining.Hours, timeRemaining.Minutes, timeRemaining.Seconds);
    }
}
