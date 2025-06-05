using UnityEngine;
using TMPro;

public class MatchStatistics : MonoBehaviour
{
    public TMP_Text killText, timeText, goldText, gemText, keyText,heroLevelText;
    void Start()
    {
        killText.text = GameResultData.killCount.ToString();
        timeText.text = FormatTime(GameResultData.matchTime);
        goldText.text = GameResultData.gold.ToString();
        gemText.text = GameResultData.gem.ToString();
        keyText.text = GameResultData.key.ToString();
        heroLevelText.text =GameResultData.levelAfter.ToString();
    }

    string FormatTime(float t)
    {
        int min = Mathf.FloorToInt(t / 60);
        int sec = Mathf.FloorToInt(t % 60);
        return $"{min:D2}:{sec:D2}";
    }
}
