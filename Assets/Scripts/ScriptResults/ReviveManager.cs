using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ReviveManager : MonoBehaviour
{
    public GameObject revivePanel;
    public TMP_Text countdownText;
    public Button gemButton;

    private PlayerInfo player;
    private bool isCounting = false;

    public void TriggerRevive(PlayerInfo targetPlayer)
    {
        player = targetPlayer;
        revivePanel.SetActive(true);
        StartCoroutine(CountdownCoroutine());
    }

    IEnumerator CountdownCoroutine()
    {
        isCounting = true;
        int countdown = 5;
        while (countdown >= 0)
        {
            countdownText.text = countdown.ToString();
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        if (isCounting)
        {
            player.FinishDeath(); // gọi kết thúc trận
        }
    }

    public void ReviveWithGem()
    {
        if (GoldGemManager.Instance.SpendGem(5))
        {
            isCounting = false;
            revivePanel.SetActive(false);
            player.ReviveFromDeath();
            Debug.Log("💎 Hồi sinh bằng gem thành công!");
        }
        else
        {
            Debug.Log("❌ Không đủ gem để hồi sinh.");
        }
    }

    public void CloseRevivedPanel() => revivePanel.SetActive(false);
}
