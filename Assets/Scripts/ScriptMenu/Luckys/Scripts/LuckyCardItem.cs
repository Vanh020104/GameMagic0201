using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LuckyCardItem : MonoBehaviour
{
    [Header("Card Faces")]
    public GameObject backFace;  // GameObject "Back"
    public GameObject frontFace; // GameObject "Front"

    [Header("UI Elements")]
    public Image iconLucky;                 // GameObject: Front/IconLucky
    public TextMeshProUGUI luckyNameText;   // GameObject: Front/LuckyName
    public TextMeshProUGUI rewardAmountText;// GameObject: Front/NumberReward

    private LuckyItemData rewardData;

    public void SetReward(LuckyItemData data)
    {
        rewardData = data;

        if (iconLucky) iconLucky.sprite = data.rewardIcon;
        if (luckyNameText) luckyNameText.text = data.rewardName;
        if (rewardAmountText) rewardAmountText.text = $"x{data.amount}";
    }

    public LuckyItemData GetReward()
    {
        return rewardData;
    }

    public void FlipToFront()
    {
        frontFace.SetActive(true);
        backFace.SetActive(false);
    }

    public void FlipToBack()
    {
        frontFace.SetActive(false);
        backFace.SetActive(true);
    }
}
