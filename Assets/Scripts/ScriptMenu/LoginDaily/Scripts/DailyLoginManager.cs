using System;
using UnityEngine;
using UnityEngine.UI;
using static NotificationPopupUI;

public class DailyLoginManager : MonoBehaviour
{
    public DailyLoginRewardDatabase rewardDatabase;

    private const string LastLoginDateKey = "LastLoginDate";
    private const string LastLoginDayKey = "LastLoginDay";
    private const string ExtraClaimedKey = "DailyLogin_ExtraClaimed";

    public DailyLoginPopupUI popupUI;
    [SerializeField] private GameObject loginDailyPanel;
    [SerializeField] private Button watchAdsButton;

    private int todayRewardDay;

    private void Start()
    {
        Debug.Log("🟢 DailyLoginManager Start Called!");
        CheckLoginReward();
    }


    // private void CheckLoginReward()
    // {
    //     PlayerPrefs.SetInt(ExtraClaimedKey, 0); // reset flag mỗi ngày

    //     DateTime now = DateTime.UtcNow.Date;
    //     string lastLoginStr = PlayerPrefs.GetString(LastLoginDateKey, now.AddDays(-2).ToString());
    //     DateTime lastLogin = DateTime.Parse(lastLoginStr);
    //     int lastDay = PlayerPrefs.GetInt(LastLoginDayKey, 0);

    //     bool isSameDay = (now - lastLogin).Days == 0;
    //     bool isConsecutive = (now - lastLogin).Days == 1;

    //     if (isSameDay)
    //     {
    //         todayRewardDay = lastDay;
    //     }
    //     else if (isConsecutive)
    //     {
    //         todayRewardDay = lastDay + 1;
    //     }
    //     else
    //     {
    //         // ❌ Bỏ lỡ ngày → reset chuỗi
    //         todayRewardDay = 1;
    //         PlayerPrefs.SetInt(LastLoginDayKey, 0);
    //     }

    //     if (todayRewardDay > rewardDatabase.rewards.Count)
    //         todayRewardDay = 1;

    //     int lastClaimedDay = PlayerPrefs.GetInt(LastLoginDayKey, 0);

    //     // ✅ Chỉ hiển thị nếu chưa nhận hôm nay
    //     if (lastClaimedDay < todayRewardDay)
    //     {
    //         ShowRewardPopup(todayRewardDay);
    //     }

    //     // Cập nhật nút xem ads
    //     watchAdsButton.interactable = PlayerPrefs.GetInt(ExtraClaimedKey, 0) == 0;
    // }
    private void CheckLoginReward()
    {
        PlayerPrefs.SetInt(ExtraClaimedKey, 0); // reset flag mỗi ngày

        // 💡 Chuẩn hóa theo UTC.Date toàn cầu
        DateTime nowUtcDate = DateTime.UtcNow.Date;

        string lastLoginStr = PlayerPrefs.GetString(LastLoginDateKey, nowUtcDate.AddDays(-2).ToString());
        DateTime lastLoginDate = DateTime.Parse(lastLoginStr).Date;
        int lastDay = PlayerPrefs.GetInt(LastLoginDayKey, 0);

        bool isSameDay = (nowUtcDate - lastLoginDate).Days == 0;
        bool isConsecutive = (nowUtcDate - lastLoginDate).Days == 1;

        if (isSameDay)
        {
            todayRewardDay = lastDay;
        }
        else if (isConsecutive)
        {
            todayRewardDay = lastDay + 1;
        }
        else
        {
            // ❌ Bỏ lỡ ngày → reset chuỗi
            todayRewardDay = 1;
            PlayerPrefs.SetInt(LastLoginDayKey, 0);
        }

        if (todayRewardDay > rewardDatabase.rewards.Count)
            todayRewardDay = 1;

        int lastClaimedDay = PlayerPrefs.GetInt(LastLoginDayKey, 0);

        if (lastClaimedDay < todayRewardDay)
        {
            ShowRewardPopup(todayRewardDay);
        }

        watchAdsButton.interactable = PlayerPrefs.GetInt(ExtraClaimedKey, 0) == 0;
    }



    private void ShowRewardPopup(int day)
    {
        int lastClaimedDay = PlayerPrefs.GetInt(LastLoginDayKey, 0);
        Debug.Log($"[ShowPopup] Today = {day}, LastClaimed = {lastClaimedDay}");

        // 👉 Luôn hiển thị Panel, UI sẽ xử lý nút
        loginDailyPanel.SetActive(true);
        popupUI.Setup(rewardDatabase.rewards, day, lastClaimedDay, this);
    }

    public void ClaimReward()
    {
        PlayerPrefs.SetInt(LastLoginDayKey, todayRewardDay);
        PlayerPrefs.SetString(LastLoginDateKey, DateTime.UtcNow.Date.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();

        DailyReward reward = rewardDatabase.rewards.Find(r => r.day == todayRewardDay);
        if (reward != null)
        {
            Debug.Log($"✅ Đã nhận: {reward.rewardName} - {reward.amount}");

            switch (reward.rewardLoginType)
            {
                case RewardLoginType.Gold:
                    GoldGemManager.Instance.AddGold(reward.amount);
                    break;
                case RewardLoginType.Gem:
                    GoldGemManager.Instance.AddGem(reward.amount);
                    break;
                case RewardLoginType.Key:
                    AddLuckyKey(reward.amount);
                    break;
            }
        }
    }


    public void OnCancelClick()
    {
        loginDailyPanel.SetActive(false);
    }

    public void WatchAdsLogin()
    {
        if (PlayerPrefs.GetInt(ExtraClaimedKey, 0) == 1)
        {
            Debug.LogWarning("⚠️ Đã nhận từ Ads hôm nay rồi!");
            return;
        }

        AdManager.Instance.ShowRewardedAd(() =>
        {
            Debug.Log("✅ Người chơi đã xem quảng cáo thành công!");
            ClaimExtraRewardFromAds();
        });
    }

    public void ClaimExtraRewardFromAds()
    {
        int nextDay = todayRewardDay + 1;
        if (nextDay > rewardDatabase.rewards.Count)
            nextDay = 1;

        todayRewardDay = nextDay;

        PlayerPrefs.SetInt(ExtraClaimedKey, 1);
        PlayerPrefs.SetInt(LastLoginDayKey, todayRewardDay - 1);
        PlayerPrefs.SetString(LastLoginDateKey, DateTime.UtcNow.ToString());
        PlayerPrefs.Save();

        ShowRewardPopup(todayRewardDay);
        watchAdsButton.interactable = false;

        Debug.Log($"🎁 Đã mở khóa ngày {todayRewardDay}, chờ người chơi nhấn nhận!");
    }


    private void AddLuckyKey(int amount)
    {
        GoldGemManager.Instance.AddKey(amount);
        KeyEvent.InvokeKeyChanged();

        NotificationPopupUI.Instance?.Show("You have received a new key!", true);
        NotificationBadgeManager.Instance.SetNotification("lucky", true);
    }

    // ----------------- Debug Tools ----------------------
    [ContextMenu("🧪 Test Fake Next Day")]
    public void DebugFakeNextDay()
    {
        int lastDay = PlayerPrefs.GetInt(LastLoginDayKey, 0);
        PlayerPrefs.SetInt(LastLoginDayKey, lastDay);
        PlayerPrefs.SetString(LastLoginDateKey, DateTime.UtcNow.AddDays(-1).ToString());
        PlayerPrefs.Save();

        Debug.Log($"🧪 Fake sang ngày {lastDay + 1} thành công!");
    }

    [ContextMenu("🧪 Test Fake Skip 1 Day (should reset)")]
    public void DebugFakeSkip1Day()
    {
        PlayerPrefs.SetInt(LastLoginDayKey, 1);
        PlayerPrefs.SetString(LastLoginDateKey, DateTime.UtcNow.AddDays(-2).ToString());
        PlayerPrefs.Save();

        Debug.Log("🧪 Fake skip 1 day complete → Next login should RESET to Day 1");
    }
}
