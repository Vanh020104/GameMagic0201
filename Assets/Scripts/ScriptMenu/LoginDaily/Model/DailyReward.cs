using System;
using UnityEngine;

[Serializable]
public class DailyReward
{
    public int day;
    public string rewardName;
    public int amount;
    public Sprite icon;
    public RewardLoginType rewardLoginType;
}