using TMPro;
using UnityEngine;

public class KillInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI enemiesLeftText;

    public void SetKillCount(int count)
    {
        killCountText.text = $"{count}";
    }

    public void SetEnemiesLeft(int count)
    {
        enemiesLeftText.text = $"{count}";
    }
}
