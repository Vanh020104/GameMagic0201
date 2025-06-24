using System.Collections;
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
        GameResultData.killCount = kills;
        UpdateUI();
    }

    public void PlayerDied()
    {
        totalPlayers--;
        UpdateUI();
        if (totalPlayers == 1 && FindObjectOfType<PlayerInfo>()?.isLocalPlayer == true)
        {
            GameResultData.playerRank = 1;

            var player = FindObjectOfType<PlayerInfo>();
            player?.PlayVictoryAnimation();

            var endManager = FindObjectOfType<BattleEndManager>();
            if (endManager != null)
            {
                endManager.isWin = true;
                endManager.StartCoroutine(DelayEndMatch(endManager, 4f));
            }
        }

    }
    private IEnumerator DelayEndMatch(BattleEndManager endManager, float delay)
    {
        yield return new WaitForSeconds(delay);
        endManager.EndMatch();
    }


    private void UpdateUI()
    {
        aliveText.text = totalPlayers.ToString();
        killText.text = kills.ToString();
    }
    public int GetAliveCount()
    {
        return totalPlayers;
    }

}
