using UnityEngine;
using TMPro;

public class TestBattlePassPurchase : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    public void OnClickBuyPass()
    {
        int requiredKey = 200;

        if (GoldGemManager.Instance != null && GoldGemManager.Instance.SpendKey(requiredKey))
        {
            BattlePassManager.Activate();
            NotificationPopupUI.Instance?.Show("Battle Pass activated!");
            UpdateStatus();
            RefreshBattlePassUI();
        }
        else
        {
            NotificationPopupUI.Instance?.Show(" Not enough Lucky Keys!", false);
        }
    }


    private void UpdateStatus()
    {
        if (statusText != null)
        {
            statusText.text = BattlePassManager.IsActivated() ? "Unlocked" : "Locked";
        }
    }

    private void RefreshBattlePassUI()
    {
        var renderer = FindObjectOfType<BattlePassRenderer>();
        if (renderer != null)
        {
            renderer.RefreshAllSlots(); // 👈 gọi hàm tự làm mới reward slots
        }
    }
    public void OnClickResetPass()
    {
        BattlePassManager.Reset();
        NotificationPopupUI.Instance?.Show("Battle Pass reset to locked.");
        UpdateStatus();
        RefreshBattlePassUI();
    }


    private void Start()
    {
        UpdateStatus();
    }
}
