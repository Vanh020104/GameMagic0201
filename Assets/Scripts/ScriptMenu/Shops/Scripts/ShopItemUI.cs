using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NotificationPopupUI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text textAmount;
    public TMP_Text textPrice;
    public Button buttonBuy;
    [SerializeField] private GameObject lockPanel;

    private ShopItemData data;
    private bool isOwned;
    public void Setup(ShopItemData itemData)
    {
        data = itemData;

        icon.sprite = data.icon;
        textAmount.text = data.amount.ToString("N0");
        textPrice.text = data.priceText;

        isOwned = PlayerPrefs.GetInt($"Equip_{data.id}_Unlocked", 0) == 1;

        UpdateUI();
    }
    private void UpdateUI()
    {
        if (isOwned)
        {
            buttonBuy.interactable = false;
            textPrice.alpha = 0.5f;
            if (lockPanel != null) lockPanel.SetActive(true);
        }
        else
        {
            buttonBuy.interactable = true;
            textPrice.alpha = 1f;
            if (lockPanel != null) lockPanel.SetActive(false);

            buttonBuy.onClick.RemoveAllListeners();
            buttonBuy.onClick.AddListener(OnBuyClick);
        }
    }

    private void OnBuyClick()
    {
        Debug.Log($"🛒 Buying item: {data.itemName} | ID: {data.id} | Price: {data.priceText}");

        // Nếu là gói mua GOLD
        if (data.id.StartsWith("gold"))
        {
            GoldGemManager.Instance.AddGold(data.amount);
            Debug.Log($"✅ Đã cộng {data.amount} gold!");
            return;
        }

        // Nếu là gói mua GEM
        if (data.id.StartsWith("gem"))
        {
            GoldGemManager.Instance.AddGem(data.amount);
            Debug.Log($"✅ Đã cộng {data.amount} gem!");
            return;
        }

        // Nếu là gói mua ITEM
        if (!int.TryParse(data.priceText, out int price)) return;

        if (PlayerPrefs.GetInt($"Equip_{data.id}_Unlocked", 0) == 1)
        {
            Debug.Log("⚠️ Item đã được sở hữu, không thể mua lại.");
            return;
        }

        if (GoldGemManager.Instance.SpendGold(price))
        {
            PlayerPrefs.SetInt($"Equip_{data.id}_Unlocked", 1);
            PlayerPrefs.SetInt($"Equip_{data.id}_Level", 1);
            PlayerPrefs.Save();

            isOwned = true;
            UpdateUI();

            BagEvent.InvokeItemBought();
        }
        else
        {
            Debug.LogWarning("❌ Không đủ vàng để mua!");
        }
    }

}
