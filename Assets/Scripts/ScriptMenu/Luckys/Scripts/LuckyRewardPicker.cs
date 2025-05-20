using System.Collections.Generic;
using UnityEngine;

public static class LuckyRewardPicker
{
    public static List<LuckyItemData> Pick(List<LuckyItemData> source, int count)
    {
        List<LuckyItemData> pool = new();

        foreach (var item in source)
        {
            if (item.id.StartsWith("Item") && PlayerPrefs.GetInt($"Equip_{item.id}_Unlocked", 0) == 1)
                continue;

            if (item.id.StartsWith("Hero") && PlayerPrefs.GetInt($"HeroUnlocked_{item.id}", 0) == 1)
                continue;


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
