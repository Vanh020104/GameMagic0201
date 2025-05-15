using UnityEngine;
using UnityEngine.UI;

public class EnergyBarManager : MonoBehaviour
{
    public Slider energySlider;
    public Image[] keyIcons;
    public int maxEnergy = 200;

    private int currentEnergy;
    private readonly int[] keyMilestones = { 40, 120, 200 };

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

        for (int i = 0; i < keyMilestones.Length; i++)
        {
            keyIcons[i].enabled = true;
            bool unlocked = currentEnergy >= keyMilestones[i];
            keyIcons[i].color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.3f); // sáng hoặc mờ
        }
    }

    private void CheckRewardMilestones()
    {
        for (int i = 0; i < keyMilestones.Length; i++)
        {
            string key = $"DailyKey_{keyMilestones[i]}";
            if (currentEnergy >= keyMilestones[i] && PlayerPrefs.GetInt(key, 0) == 0)
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
                Debug.Log($"🎉 Nhận KEY mốc {keyMilestones[i]}");
            }
        }
    }

    public void ResetEnergyAndKeyProgress()
    {
        currentEnergy = 0;
        PlayerPrefs.SetInt("DailyEnergy", 0);

        foreach (var milestone in keyMilestones)
            PlayerPrefs.SetInt($"DailyKey_{milestone}", 0);

        PlayerPrefs.Save();
        UpdateUI();
    }
}
