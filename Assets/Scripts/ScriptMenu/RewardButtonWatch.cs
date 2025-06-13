// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;


// public class RewardButtonWatch : MonoBehaviour
// {
//   [Header("Reward Settings")]
//     public bool isGoldReward = true;          
//     public TextMeshProUGUI amountText;     
//     public AdManager adManager;
//     private Button button;
//     private GoldGemManager goldGemManager;

//     private void Awake()
//     {
//         button = GetComponent<Button>();
//         button.onClick.AddListener(OnButtonClicked);
//     }

//     private void Start()
//     {
//         goldGemManager = FindObjectOfType<GoldGemManager>();
//     }

//     private void OnButtonClicked()
//     {
//         if (goldGemManager == null || amountText == null)
//         {
//             Debug.LogWarning("RewardButton setup thiếu component!");
//             return;
//         }

//         // Parse số lượng từ Text
//          adManager.ShowRewardedAd(() =>
//         {
//             if (int.TryParse(amountText.text.Replace(",", ""), out int amount))
//             {
//                 if (isGoldReward)
//                     goldGemManager.AddGold(amount);
//                 else
//                     goldGemManager.AddGem(amount);
//             }
//         });
//     }
// }


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
    private const int cooldownSeconds = 600; // 10 phút

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    private void Start()
    {
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
