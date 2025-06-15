using UnityEngine;

[System.Serializable]
public class RewardData
{
    public Sprite icon;
    public int quantity;
    public string rewardName;
    public Sprite background;
    public Sprite border;
    public RewardTypeItem type;
}

public enum RewardTypeItem
{
    Gold,
    Gem,
    LuckyKey,
    Item,
    Hero
}
