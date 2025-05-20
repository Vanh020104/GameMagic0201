using UnityEngine;

[System.Serializable]
public class LuckyItemData
{
    public string id;
    public string rewardName;
    public Sprite rewardIcon;
    public int amount;
    public RewardTier tier;
    public RewardType rewardType;
}
