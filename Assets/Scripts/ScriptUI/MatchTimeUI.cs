using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MatchTimeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    private float timeElapsed = 0f;
    private bool isRunning = true;

    private void Update()
    {
        if (!isRunning) return;

        timeElapsed += Time.deltaTime;
        UpdateText();
    }

    void UpdateText()
    {
        int m = Mathf.FloorToInt(timeElapsed / 60);
        int s = Mathf.FloorToInt(timeElapsed % 60);
        timeText.text = $"Time: {m:D2}:{s:D2}";
    }

    public void StopTimer() => isRunning = false;
    public void ResumeTimer() => isRunning = true;
}
