using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DailyRewardSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject highlightFrame;
    public TMP_Text dayText;
    public Image icon;
    public TMP_Text amountText;
    public GameObject tick;

    public void Setup(DailyReward reward, bool isToday, bool isClaimed)
    {
        dayText.text = $"Day {reward.day}";
        icon.sprite = reward.icon;
        amountText.text = reward.amount.ToString();
        highlightFrame.SetActive(isToday);
        tick.SetActive(isClaimed);
    }
    public void ShowTick()
    {
        tick.SetActive(true);
    }

}
