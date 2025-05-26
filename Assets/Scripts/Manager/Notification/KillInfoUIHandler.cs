using TMPro;
using UnityEngine;

public class KillInfoUIHandler : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI aliveText;
    public TextMeshProUGUI killText;

    private int totalPlayers = 0;
    private int kills = 0;

    public void Init(int total)
    {
        totalPlayers = total;
        kills = 0;
        UpdateUI();
    }

    public void AddKill()
    {
        kills++;
        UpdateUI();
    }

    public void PlayerDied()
    {
        totalPlayers--;
        UpdateUI();
    }

    private void UpdateUI()
    {
        aliveText.text = totalPlayers.ToString();
        killText.text = kills.ToString();
    }
}
