using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ReviveManager : MonoBehaviour
{
    public GameObject revivePanel;
    public TMP_Text countdownText;
    public Button gemButton;
    public Button adsButton;
    public TextMeshProUGUI gemButtonText;
    public TextMeshProUGUI notifiText;
    public TextMeshProUGUI checkGem;

    private PlayerInfo player;
    private bool isCounting = false;

    private int reviveCount = 0; // tổng lượt revive (gem + ads)
    private int gemReviveCount = 0; // số lượt dùng gem
    private readonly int[] gemCosts = { 5, 10, 20 };
    private const int maxRevivesPerMatch = 3;

    public void TriggerRevive(PlayerInfo targetPlayer)
    {
        if (notifiText != null) notifiText.gameObject.SetActive(false);
        if (checkGem != null) checkGem.gameObject.SetActive(false);

        player = targetPlayer;

        int alive = FindObjectOfType<KillInfoUIHandler>().GetAliveCount();
        if (alive <= 3)
        {
            Debug.Log("⛔ Không thể hồi sinh khi còn ≤ 3 người.");
            return;
        }

        revivePanel.SetActive(true);

        if (reviveCount >= maxRevivesPerMatch)
        {
            adsButton.gameObject.SetActive(false);
            gemButton.gameObject.SetActive(false);
            if (notifiText != null)
            {
                notifiText.gameObject.SetActive(true);
                notifiText.text = "YOUR REVIVE TURN IS OVER!";
            }
        }
        else
        {
            adsButton.gameObject.SetActive(true);
            gemButton.gameObject.SetActive(true);

            int currentCost = gemCosts[Mathf.Min(gemReviveCount, gemCosts.Length - 1)];
            gemButtonText.text = $"{currentCost}";

            bool canAfford = GoldGemManager.Instance.GetGem() >= currentCost;
            gemButton.interactable = canAfford;
            if (!canAfford && checkGem != null)
            {
                checkGem.gameObject.SetActive(true);
                checkGem.text = "NOT ENOUGH GEMS!";
            }
            else if (checkGem != null)
            {
                checkGem.gameObject.SetActive(false);
            }

            adsButton.interactable = AdManager.Instance != null && AdManager.Instance.HasInterstitialReady();
        }

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
            player.FinishDeath();
        }
    }

    public void ReviveWithGem()
    {
        gemButton.interactable = false;

        if (reviveCount >= maxRevivesPerMatch)
        {
            Debug.Log("⚠️ Đã dùng hết lượt hồi sinh.");
            return;
        }

        int cost = gemCosts[Mathf.Min(gemReviveCount, gemCosts.Length - 1)];
        if (GoldGemManager.Instance.SpendGem(cost))
        {
            gemReviveCount++;
            CompleteRevive();
            Debug.Log($"💎 Hồi sinh bằng gem thành công! (Cost: {cost})");
        }
        else
        {
            Debug.Log("❌ Không đủ gem để hồi sinh.");
            gemButton.interactable = true;
        }
    }

    public void ReviveWithAds()
    {
        adsButton.interactable = false;

        if (reviveCount >= maxRevivesPerMatch)
        {
            Debug.Log("⚠️ Đã dùng hết lượt hồi sinh.");
            return;
        }

        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowRewardedAd(() =>
            {
                CompleteRevive();
                Debug.Log("🎥 Hồi sinh sau khi xem quảng cáo thành công!");
            });
        }
        else
        {
            Debug.Log("❌ AdManager.Instance null.");
            adsButton.interactable = true;
        }
    }

    private void CompleteRevive()
    {
        reviveCount++;
        isCounting = false;
        revivePanel.SetActive(false);
        player.ReviveFromDeath();
    }

    public void CloseRevivedPanel() => revivePanel.SetActive(false);

    public void ResetReviveCount()
    {
        reviveCount = 0;
        gemReviveCount = 0;
    }
}
