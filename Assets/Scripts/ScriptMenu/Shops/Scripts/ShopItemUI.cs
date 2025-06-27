using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NotificationPopupUI;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class ShopItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text textAmount;
    public TMP_Text textPrice;
    public Button buttonBuy;
    [SerializeField] private GameObject lockPanel;

    private ShopItemData data;
    private bool isOwned;
    [SerializeField] private GameObject levelBlockPanel;
    [SerializeField] private TMP_Text levelBlockText;

    public void Setup(ShopItemData itemData)
    {
        data = itemData;
        icon.sprite = data.icon;
        textAmount.text = data.amount.ToString("N0");
        if (data.isKeyPurchase)
        {
            textPrice.text = $"{data.priceText}";
        }
        else if (int.TryParse(data.priceText, out int parsedPrice))
        {
            textPrice.text = parsedPrice.ToString("N0");
        }
        else
        {
            textPrice.text = data.priceText;
        }


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
        int playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        bool levelRequirementMet = playerLevel >= data.requiredLevel;

        if (isOwned)
        {
            buttonBuy.interactable = false;
            textPrice.alpha = 0.5f;
            if (lockPanel != null) lockPanel.SetActive(true);
            if (levelBlockPanel != null) levelBlockPanel.SetActive(false);
            return;
        }

        if (!levelRequirementMet)
        {
            buttonBuy.interactable = false;
            textPrice.alpha = 0.5f;

            if (levelBlockPanel != null) levelBlockPanel.SetActive(true);
            var loc = new LocalizedString("LanguageVanh", "need_level_to_unlock");
            loc.Arguments = new object[] { data.requiredLevel };
            loc.StringChanged += val => levelBlockText.text = val;


            if (lockPanel != null) lockPanel.SetActive(false);
            return;
        }


        // Có thể mua
        buttonBuy.interactable = true;
        textPrice.alpha = 1f;
        if (lockPanel != null) lockPanel.SetActive(false);
        if (levelBlockPanel != null) levelBlockPanel.SetActive(false);

        buttonBuy.onClick.RemoveAllListeners();
        buttonBuy.onClick.AddListener(OnBuyClick);
    }


    private void OnBuyClick()
    {
        // --- MUA BẰNG LUCKY KEY ---
        if (data.isKeyPurchase)
        {
            if (!int.TryParse(data.priceText, out int keyCost))
            {
                Debug.LogWarning($"❌ Invalid key cost: {data.priceText}");
                return;
            }

            int keys = PlayerPrefs.GetInt("LuckyKey", 0);
            if (keys < keyCost)
            {
                var keyNeeded = new LocalizedString("LanguageVanh", "not_enough_keys");
                keyNeeded.Arguments = new object[] { keyCost };
                keyNeeded.StringChanged += val => NotificationPopupUI.Instance?.Show(val);

                return;
            }

            PlayerPrefs.SetInt("LuckyKey", keys - keyCost);
            PlayerPrefs.Save();

            if (data.id.StartsWith("gold"))
                GoldGemManager.Instance.AddGold(data.amount);
            else if (data.id.StartsWith("gem"))
                GoldGemManager.Instance.AddGem(data.amount);

            var loc = new LocalizedString("LanguageVanh", "spent_keys_get_item");
            loc.Arguments = new object[] { keyCost, data.amount };
            loc.StringChanged += val => NotificationPopupUI.Instance?.Show(val);


            // ❌ Không disable nút mua để cho mua tiếp
            return;
        }

        // --- ADS (nếu vẫn còn giữ) ---
        if (data.isRewardedAd)
        {
            if (!CanWatchAdNow(out int remaining, out float wait))
            {
                TimeSpan t = TimeSpan.FromSeconds(wait);
                string timeString = $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";

                var loc = new LocalizedString("LanguageVanh", "come_back_after_time");
                loc.Arguments = new object[] { timeString };
                loc.StringChanged += val => NotificationPopupUI.Instance?.Show(val, false);

                return;
            }

            AdManager.Instance.ShowRewardedAd(() =>
            {
                if (data.id.StartsWith("gold"))
                    GoldGemManager.Instance.AddGold(data.amount);
                else if (data.id.StartsWith("gem"))
                    GoldGemManager.Instance.AddGem(data.amount);

                var loc = new LocalizedString("LanguageVanh", "reward_received_left");
                loc.Arguments = new object[] { remaining - 1 };
                loc.StringChanged += val => NotificationPopupUI.Instance?.Show(val, true);


                string countKey = $"AdLimit_{data.id}_WatchedCount";
                string resetKey = $"AdLimit_{data.id}_LastReset";
                int count = PlayerPrefs.GetInt(countKey, 0);
                PlayerPrefs.SetInt(countKey, count + 1);

                if (count == 0)
                    PlayerPrefs.SetString(resetKey, DateTime.UtcNow.Ticks.ToString());

                PlayerPrefs.Save();
                buttonBuy.interactable = false;
                Invoke(nameof(EnableBuyButton), 3f);
            });

            return;
        }

        // --- GÓI GOLD/GEM BÌNH THƯỜNG (không key, không ad) ---
        if (data.id.StartsWith("gold"))
        {
            GoldGemManager.Instance.AddGold(data.amount);
            var loc = new LocalizedString("LanguageVanh", "purchase_successful");
            loc.StringChanged += val => NotificationPopupUI.Instance?.Show(val);
            return;
        }

        if (data.id.StartsWith("gem"))
        {
            GoldGemManager.Instance.AddGem(data.amount);
            var loc = new LocalizedString("LanguageVanh", "purchase_successful");
            loc.StringChanged += val => NotificationPopupUI.Instance?.Show(val);
            return;
        }

        // --- GÓI HERO (mua 1 lần duy nhất) ---
        if (data.id.StartsWith("Hero"))
        {
            if (int.TryParse(data.priceText, out int gemPrice) && GoldGemManager.Instance.SpendGem(gemPrice))
            {
                PlayerPrefs.SetInt($"HeroUnlocked_{data.id}", 1);
                PlayerPrefs.Save();
                var loc = new LocalizedString("LanguageVanh", "hero_unlocked");
                loc.StringChanged += val => NotificationPopupUI.Instance?.Show(val);

                if (lockPanel != null) lockPanel.SetActive(true);
                buttonBuy.interactable = false;
                textPrice.alpha = 0.5f;
                HeroEvents.OnHeroBought?.Invoke(data.id);
                NotificationBadgeManager.Instance.SetNotification("character", true);
            }
            else
            {
                var notEnoughGems = new LocalizedString("LanguageVanh", "not_enough_gems");
                notEnoughGems.StringChanged += val => NotificationPopupUI.Instance?.Show(val, false);

            }
            return;
        }

        // --- GÓI ITEM (mua 1 lần duy nhất) ---
        if (!int.TryParse(data.priceText, out int price)) return;

        if (PlayerPrefs.GetInt($"Equip_{data.id}_Unlocked", 0) == 1) return;

        if (GoldGemManager.Instance.SpendGold(price))
        {
            PlayerPrefs.SetInt($"Equip_{data.id}_Unlocked", 1);
            PlayerPrefs.SetInt($"Equip_{data.id}_Level", 1);
            PlayerPrefs.Save();
            var loc = new LocalizedString("LanguageVanh", "item_purchased");
            loc.StringChanged += val => NotificationPopupUI.Instance?.Show(val);

            isOwned = true;
            UpdateUI();
            BagEvent.InvokeItemBought();
            NotificationBadgeManager.Instance.SetNotification("bag", true);
        }
        else
        {
            var loc = new LocalizedString("LanguageVanh", "not_enough_gold");
            loc.StringChanged += val => NotificationPopupUI.Instance?.Show(val, false);

        }
    }

    private bool CanWatchAdNow(out int remainingCount, out float waitSeconds)
    {
        string countKey = $"AdLimit_{data.id}_WatchedCount";
        string resetKey = $"AdLimit_{data.id}_LastReset";

        int watchedCount = PlayerPrefs.GetInt(countKey, 0);
        long lastResetTicks = long.Parse(PlayerPrefs.GetString(resetKey, "0"));
        DateTime lastReset = new DateTime(lastResetTicks);
        TimeSpan sinceReset = DateTime.UtcNow - lastReset;

        // Nếu quá 10 tiếng -> reset
        if (sinceReset.TotalHours >= 10)
        {
            PlayerPrefs.SetInt(countKey, 0);
            PlayerPrefs.SetString(resetKey, DateTime.UtcNow.Ticks.ToString());
            PlayerPrefs.Save();
            watchedCount = 0;
        }

        remainingCount = 3 - watchedCount;

        // Chặn nếu đã xem đủ 3 lần
        if (watchedCount >= 3)
        {
            waitSeconds = (float)(TimeSpan.FromHours(10) - sinceReset).TotalSeconds;
            return false;
        }

        waitSeconds = 0f;
        return true;
    }
    private void EnableBuyButton()
    {
        buttonBuy.interactable = true;
    }

}
