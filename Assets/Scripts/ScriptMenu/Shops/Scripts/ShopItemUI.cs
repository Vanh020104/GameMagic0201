using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text textAmount;
    public TMP_Text textPrice;
    public Button buttonBuy;

    private ShopItemData data;
    [SerializeField] private NotificationPopupUI notificationPopupUI;

    public void Setup(ShopItemData itemData)
    {
        data = itemData;
        icon.sprite = data.icon;
        textAmount.text = data.amount.ToString("N0");
        textPrice.text = data.priceText;

        buttonBuy.onClick.RemoveAllListeners();
        buttonBuy.onClick.AddListener(OnBuyClick);
    }

    private void OnBuyClick()
    {
        Debug.Log($"🛒 Buying item: {data.itemName} | ID: {data.id} | Price: {data.priceText}");

        if (data.id.StartsWith("gold"))
        {
            GoldGemManager.Instance.AddGold(data.amount);
        }
        else if (data.id.StartsWith("gem"))
        {
            GoldGemManager.Instance.AddGem(data.amount);
        }
        else
        {
            if (!int.TryParse(data.priceText, out int price))
            {
                return;
            }
            if (GoldGemManager.Instance.SpendGold(price))
            {
                Debug.Log($"✅ Mua thành công item {data.id}");
                PlayerPrefs.SetInt($"Equip_{data.id}_Unlocked", 1);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogWarning("❌ Không đủ vàng để mua!");
            }
        }
    }

}
