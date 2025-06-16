using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardButtonWatch : MonoBehaviour
{
    [Header("Reward Settings")]
    public bool isGoldReward = true;
    public TextMeshProUGUI amountText;
    public AdManager adManager;

    [Header("Cooldown UI")]
    public GameObject lockPanel; // UI che nút
    public TMP_Text countdownText;

    private Button button;
    private GoldGemManager goldGemManager;

    private const string KeyLastAdTime = "LastRewardAdTime";
    private const int cooldownSeconds = 30; 

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    private void Start()
    {
        if (adManager == null)
        adManager = AdManager.Instance;

        goldGemManager = FindObjectOfType<GoldGemManager>();
        InvokeRepeating(nameof(UpdateCooldownUI), 0f, 1f);
    }

    private void UpdateCooldownUI()
    {
        float remain = GetRemainingCooldown();
        bool canWatch = remain <= 0;

        button.interactable = canWatch;

        if (lockPanel != null) lockPanel.SetActive(!canWatch);
        if (countdownText != null)
        {
            if (canWatch)
                countdownText.text = "";
            else
            {
                TimeSpan t = TimeSpan.FromSeconds(remain);
                countdownText.text = $"{t.Minutes:D2}:{t.Seconds:D2}";
            }
        }
    }

    private void OnButtonClicked()
    {
        if (!CanWatchAdNow())
        {
            Debug.Log("⏳ Ads đang cooldown...");
            return;
        }

        // GÁN lại nếu thiếu
        if (adManager == null) adManager = AdManager.Instance;
        if (goldGemManager == null) goldGemManager = FindObjectOfType<GoldGemManager>();

        if (goldGemManager == null || amountText == null || adManager == null)
        {
            Debug.LogWarning("❌ RewardButtonWatch thiếu thành phần!");
            return;
        }

        adManager.ShowRewardedAd(() =>
        {
            if (int.TryParse(amountText.text.Replace(",", ""), out int amount))
            {
                if (isGoldReward)
                    goldGemManager.AddGold(amount);
                else
                    goldGemManager.AddGem(amount);

                RecordAdWatched();
                DailyTaskProgressManager.Instance.AddProgress("watch_ad");
                DailyTaskProgressManager.Instance.AddProgress("watch_ad_3");
            }
        });
    }


    private bool CanWatchAdNow()
    {
        string timeStr = PlayerPrefs.GetString(KeyLastAdTime, "");
        if (string.IsNullOrEmpty(timeStr)) return true;

        DateTime lastTime = DateTime.Parse(timeStr);
        return (DateTime.Now - lastTime).TotalSeconds >= cooldownSeconds;
    }

    private float GetRemainingCooldown()
    {
        string timeStr = PlayerPrefs.GetString(KeyLastAdTime, "");
        if (string.IsNullOrEmpty(timeStr)) return 0;

        DateTime lastTime = DateTime.Parse(timeStr);
        float remain = cooldownSeconds - (float)(DateTime.Now - lastTime).TotalSeconds;
        return Mathf.Max(remain, 0);
    }

    private void RecordAdWatched()
    {
        PlayerPrefs.SetString(KeyLastAdTime, DateTime.Now.ToString());
        PlayerPrefs.Save();
    }
}
