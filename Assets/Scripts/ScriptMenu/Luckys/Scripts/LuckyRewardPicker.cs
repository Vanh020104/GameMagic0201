using System.Collections.Generic;
using UnityEngine;

public static class LuckyRewardPicker
{
    public static List<LuckyItemData> Pick(List<LuckyItemData> source, int count)
    {
        List<LuckyItemData> pool = new();

        foreach (var item in source)
        {
            switch (item.tier)
            {
                case RewardTier.Common:
                    pool.Add(item); pool.Add(item); pool.Add(item);
                    break;
                case RewardTier.Rare:
                    pool.Add(item); pool.Add(item);
                    break;
                case RewardTier.VIP:
                    if (Random.value < 0.1f)
                        pool.Add(item);
                    break;
            }
        }

        pool.Shuffle();
        return pool.GetRange(0, Mathf.Min(count, pool.Count));
    }
}
