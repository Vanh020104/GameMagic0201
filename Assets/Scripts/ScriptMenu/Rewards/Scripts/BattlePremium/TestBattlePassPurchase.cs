using UnityEngine;
using TMPro;

public class TestBattlePassPurchase : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;

    public void OnClickBuyPass()
    {
        if (BattlePassManager.IsActivated())
        {
            NotificationPopupUI.Instance?.Show("You already own the Battle Pass.");
            return;
        }

        const int requiredKey = 500;

        // ✅ Kiểm tra người chơi có đủ key chưa
        int playerKey = GoldGemManager.Instance?.GetKey() ?? 0;

        if (playerKey < requiredKey)
        {
            NotificationPopupUI.Instance?.Show($" Not enough Lucky Keys!", false);
            return;
        }

        // ✅ Trừ key và mở khóa
        bool success = GoldGemManager.Instance.SpendKey(requiredKey);
        if (success)
        {
            BattlePassManager.Activate();
            NotificationPopupUI.Instance?.Show($" Battle Pass unlocked!");
            UpdateStatus();
            RefreshBattlePassUI();
        }
        else
        {
            // Phòng khi có lỗi nào đó
            NotificationPopupUI.Instance?.Show("⚠️ Failed to spend keys. Please try again.");
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
