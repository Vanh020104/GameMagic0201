using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ReviveManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject revivePanel;
    public TMP_Text countdownText;
    public Button gemButton;
    public Button adsButton;
    public TextMeshProUGUI gemButtonText;
    public TextMeshProUGUI notifiText;
    public TextMeshProUGUI checkGem;

    private PlayerInfo player;
    private bool isCounting = false;
    private bool isReviveBlocked = false;

    private int reviveCount = 0; // tổng lượt revive (gem + ads)
    private int gemReviveCount = 0; // số lượt dùng gem
    private readonly int[] gemCosts = { 5, 10, 20 };
    private const int maxRevivesPerMatch = 3;

    public void TriggerRevive(PlayerInfo targetPlayer)
    {
        player = targetPlayer;

        // Xác định điều kiện không cho revive
        int alive = FindObjectOfType<KillInfoUIHandler>().GetAliveCount();
        isReviveBlocked = alive <= 3;

        // Hiện panel + cập nhật giao diện tương ứng
        revivePanel.SetActive(true);
        UpdateReviveUI();

        // Bắt đầu đếm ngược
        StartCoroutine(CountdownCoroutine());
    }

    private void UpdateReviveUI()
    {
        // Ẩn mặc định
        notifiText?.gameObject.SetActive(false);
        checkGem?.gameObject.SetActive(false);

        if (isReviveBlocked)
        {
            gemButton.gameObject.SetActive(false);
            adsButton.gameObject.SetActive(false);
            if (notifiText != null)
            {
                notifiText.gameObject.SetActive(true);
                notifiText.text = "YOU CANNOT RESURRECT!";
            }
            return;
        }

        if (reviveCount >= maxRevivesPerMatch)
        {
            gemButton.gameObject.SetActive(false);
            adsButton.gameObject.SetActive(false);
            if (notifiText != null)
            {
                notifiText.gameObject.SetActive(true);
                notifiText.text = "YOUR REVIVE TURN IS OVER!";
            }
            return;
        }

        // Nếu hợp lệ → bật 2 nút revive
        gemButton.gameObject.SetActive(true);
        adsButton.gameObject.SetActive(true);

        int currentCost = gemCosts[Mathf.Min(gemReviveCount, gemCosts.Length - 1)];
        gemButtonText.text = $"{currentCost}";

        bool canAfford = GoldGemManager.Instance.GetGem() >= currentCost;
        gemButton.interactable = canAfford;

        if (!canAfford && checkGem != null)
        {
            checkGem.gameObject.SetActive(true);
            checkGem.text = "NOT ENOUGH GEMS!";
        }

        adsButton.interactable = AdManager.Instance != null && AdManager.Instance.HasInterstitialReady();
    }

    private IEnumerator CountdownCoroutine()
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
