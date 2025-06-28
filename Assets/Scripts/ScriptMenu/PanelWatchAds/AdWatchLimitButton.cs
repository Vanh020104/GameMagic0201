using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

public class AdWatchLimitButton : MonoBehaviour
{
    public enum AdType { Gold, Gem, Key }

    [Header("Config")]
    public AdType adType;
    public Button watchButton;
    public TMP_Text counterText;
    public float cooldownMinutes = 2f;
    public int maxViewsPerDay = 5;

    private string KeyPrefix => "AdLimit_" + adType.ToString();
    private string LastTimeKey => KeyPrefix + "_LastTime";
    private string CountKey => KeyPrefix + "_Count";
    private string DateKey => KeyPrefix + "_Date";
    private Coroutine countdownRoutine;
    [SerializeField] private GameObject cooldownPanel;
    private void Start()
    {
        if (watchButton != null)
            watchButton.onClick.AddListener(OnClickWatchAd);

        RefreshUI();
    }

    public void RefreshUI()
{
    if (countdownRoutine != null)
        StopCoroutine(countdownRoutine);

    if (GetTodayCount() >= maxViewsPerDay)
    {
        watchButton.interactable = false;
        counterText.text = ""; // Ẩn hết
        return;
    }

     if (IsCooldownActive())
    {
        watchButton.interactable = false;
        cooldownPanel.SetActive(true); // bật đè text lên
        countdownRoutine = StartCoroutine(UpdateCountdownUI());
    }
    else if (GetTodayCount() >= maxViewsPerDay)
    {
        watchButton.interactable = false;
        cooldownPanel.SetActive(false);
        counterText.text = "";
    }
    else
    {
        watchButton.interactable = true;
        cooldownPanel.SetActive(false); // ẩn text đè
        counterText.text = "";
    }
}

    private IEnumerator UpdateCountdownUI()
    {
        while (true)
        {
            float remaining = RemainingCooldown();
            if (remaining <= 0f)
            {
                RefreshUI();
                yield break;
            }

            TimeSpan time = TimeSpan.FromSeconds(remaining);
            counterText.text = $"{time.Minutes:D2}:{time.Seconds:D2}"; // ví dụ: 00:07

            yield return new WaitForSeconds(1f);
        }
    }

    private void OnClickWatchAd()
    {
        watchButton.interactable = false;

        if (!AdManager.Instance.HasRewardedAdReady())
        {
            NotificationPopupUI.Instance?.Show("Ad not ready!", false);
            RefreshUI();
            return;
        }

        AdManager.Instance.ShowRewardedAd(() =>
        {
            PlayerPrefs.SetString(LastTimeKey, DateTime.Now.ToString("o"));
            PlayerPrefs.SetInt(CountKey, GetTodayCount() + 1);
            PlayerPrefs.SetString(DateKey, DateTime.Now.Date.ToString());

            GrantReward(adType);
            RefreshUI(); // 👈 bật lại nếu đủ điều kiện
        });
    }


    private void GrantReward(AdType type)
    {
        switch (type)
        {
            case AdType.Gold:
                GoldGemManager.Instance.AddGold(5500);
            NotificationPopupUI.Instance?.Show("Reward claimed!!");

                break;
            case AdType.Gem:
                GoldGemManager.Instance.AddGem(6);
            NotificationPopupUI.Instance?.Show("Reward claimed!!");

                break;
            case AdType.Key:
                GoldGemManager.Instance.AddKey(3);
            NotificationPopupUI.Instance?.Show("Reward claimed!!");

                break;
        }
    }

    private bool IsCooldownActive()
    {
        string lastTimeStr = PlayerPrefs.GetString(LastTimeKey, "");
        if (DateTime.TryParse(lastTimeStr, out DateTime lastTime))
        {
            return (DateTime.Now - lastTime).TotalMinutes < cooldownMinutes;
        }
        return false;
    }

    private float RemainingCooldown()
    {
        string lastTimeStr = PlayerPrefs.GetString(LastTimeKey, "");
        if (DateTime.TryParse(lastTimeStr, out DateTime lastTime))
        {
            float secondsLeft = Mathf.Max(0, (float)((cooldownMinutes * 60) - (DateTime.Now - lastTime).TotalSeconds));
            return secondsLeft;
        }
        return 0;
    }

    private int GetTodayCount()
    {
        string savedDate = PlayerPrefs.GetString(DateKey, "");
        if (savedDate != DateTime.Now.Date.ToString())
        {
            PlayerPrefs.SetString(DateKey, DateTime.Now.Date.ToString());
            PlayerPrefs.SetInt(CountKey, 0);
        }
        return PlayerPrefs.GetInt(CountKey, 0);
    }
}
