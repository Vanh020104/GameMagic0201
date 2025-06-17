using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NotificationPopupUI;

public class EnergyBarManager : MonoBehaviour
{
    public Slider energySlider;
    public Image[] keyIcons;
    public int maxEnergy = 200;

    private int currentEnergy;
    private readonly Dictionary<int, int> keyRewardMap = new()
    {
        { 40, 1 },
        { 120, 2 },
        { 200, 3 }
    };

    public TMP_Text energyText;

    private void Start()
    {
        currentEnergy = PlayerPrefs.GetInt("DailyEnergy", 0);
        UpdateUI();
    }

    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        PlayerPrefs.SetInt("DailyEnergy", currentEnergy);
        PlayerPrefs.Save();
        UpdateUI();
        CheckRewardMilestones();
    }

    private void UpdateUI()
    {
        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;

        if (energyText != null)
            energyText.text = $"{currentEnergy}/{maxEnergy}";

        int i = 0;
        foreach (var milestone in keyRewardMap.Keys)
        {
            if (i >= keyIcons.Length) break;

            keyIcons[i].enabled = true;
            bool unlocked = currentEnergy >= milestone;
            keyIcons[i].color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.3f);
            i++;
        }
    }

    private void CheckRewardMilestones()
    {
        foreach (var pair in keyRewardMap)
        {
            int milestone = pair.Key;
            int keyReward = pair.Value;

            string key = $"DailyKey_{milestone}";

            if (currentEnergy >= milestone && PlayerPrefs.GetInt(key, 0) == 0)
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();

                int luckyKey = PlayerPrefs.GetInt("LuckyKey", 0);
                luckyKey += keyReward;
                PlayerPrefs.SetInt("LuckyKey", luckyKey);
                PlayerPrefs.Save();
                KeyEvent.InvokeKeyChanged();
                NotificationPopupUI.Instance?.Show($" You've received {keyReward} Lucky Key for reaching {milestone} energy!");
                NotificationBadgeManager.Instance.SetNotification("lucky", true);
            }
        }
    }

    public void ResetEnergyAndKeyProgress()
    {
        currentEnergy = 0;
        PlayerPrefs.SetInt("DailyEnergy", 0);

        foreach (var milestone in keyRewardMap.Keys)
            PlayerPrefs.SetInt($"DailyKey_{milestone}", 0);

        PlayerPrefs.Save();
        UpdateUI();
    }
}
