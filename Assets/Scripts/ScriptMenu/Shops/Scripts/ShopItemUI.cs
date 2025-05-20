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
        if (data.id.StartsWith("Hero"))
        {
            isOwned = PlayerPrefs.GetInt($"HeroUnlocked_{data.id}", 0) == 1;
        }
        else
        {
            isOwned = PlayerPrefs.GetInt($"Equip_{data.id}_Unlocked", 0) == 1;
        }
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
        if (data.id.StartsWith("gold"))
        {
            GoldGemManager.Instance.AddGold(data.amount);
            NotificationPopupUI.Instance?.Show("Purchase successful!");
            return;
        }

        if (data.id.StartsWith("gem"))
        {
            GoldGemManager.Instance.AddGem(data.amount);
            NotificationPopupUI.Instance?.Show("Purchase successful!");
            return;
        }

        if (data.id.StartsWith("Hero"))
        {
            if (GoldGemManager.Instance.SpendGem(int.Parse(data.priceText)))
            {
                PlayerPrefs.SetInt($"HeroUnlocked_{data.id}", 1);
                PlayerPrefs.Save();
                NotificationPopupUI.Instance?.Show("Hero unlocked!");
                if (lockPanel != null) lockPanel.SetActive(true);
                buttonBuy.interactable = false;
                textPrice.alpha = 0.5f;
                HeroEvents.OnHeroBought?.Invoke(data.id);
            }
            else
            {
                NotificationPopupUI.Instance?.Show("Not enough Gems!", false);
            }
            return;
        }

        if (!int.TryParse(data.priceText, out int price)) return;

        if (PlayerPrefs.GetInt($"Equip_{data.id}_Unlocked", 0) == 1) return;

        if (GoldGemManager.Instance.SpendGold(price))
        {
            PlayerPrefs.SetInt($"Equip_{data.id}_Unlocked", 1);
            PlayerPrefs.SetInt($"Equip_{data.id}_Level", 1);
            PlayerPrefs.Save();
            NotificationPopupUI.Instance?.Show("Item purchased!");
            isOwned = true;
            UpdateUI();
            BagEvent.InvokeItemBought();
        }
        else
        {
            NotificationPopupUI.Instance?.Show("Not enough Gold!", false);
        }
    }
}
