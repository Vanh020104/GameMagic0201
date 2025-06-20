using UnityEngine;
using static NotificationPopupUI;

public class BattlePassRenderer : MonoBehaviour
{
    public BattlePassDatabase database;
    public Transform premiumParent;
    public Transform freeParent;
    public GameObject rewardSlotPrefab;

    private void Start()
    {
        int playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);

        for (int i = 0; i < database.levels.Count; i++)
        {
            var levelData = database.levels[i];
            bool unlocked = playerLevel >= levelData.level;

            var free = Instantiate(rewardSlotPrefab, freeParent);
            var freeSlot = free.GetComponent<RewardSlotUI>();
            string freeKey = $"Claimed_BattlePass_{levelData.level}_Free";
            freeSlot.Setup(levelData.freeReward, unlocked, freeKey);

            var premium = Instantiate(rewardSlotPrefab, premiumParent);
            var premiumSlot = premium.GetComponent<RewardSlotUI>();
            string premiumKey = $"Claimed_BattlePass_{levelData.level}_Premium";
            premiumSlot.Setup(levelData.premiumReward, unlocked, premiumKey, true);
            // Debug.Log($"Rendering level {levelData.level} | PlayerLevel = {playerLevel} | unlocked = {unlocked}");

            freeSlot.SetClaimCallback(() =>
            {
                ClaimReward(levelData.freeReward);
                PlayerPrefs.SetInt(freeKey, 1);
                PlayerPrefs.Save();
                CheckAndUpdateNotification();

            });

            premiumSlot.SetClaimCallback(() =>
            {
                ClaimReward(levelData.premiumReward);
                PlayerPrefs.SetInt(premiumKey, 1);
                PlayerPrefs.Save();
                CheckAndUpdateNotification();

            });
        }
        CheckAndUpdateNotification();

    }
    private void ClaimReward(RewardData data)
    {
        switch (data.type)
        {
            case RewardTypeItem.Gold:
                GoldGemManager.Instance.AddGold(data.quantity);
                break;

            case RewardTypeItem.Gem:
                GoldGemManager.Instance.AddGem(data.quantity);
                break;

            case RewardTypeItem.LuckyKey:
                int key = PlayerPrefs.GetInt("LuckyKey", 0);
                PlayerPrefs.SetInt("LuckyKey", key + data.quantity);
                PlayerPrefs.Save();
                KeyEvent.InvokeKeyChanged();
                NotificationBadgeManager.Instance.SetNotification("lucky", true);
                break;

            case RewardTypeItem.Item:
                string id = data.itemId ?? data.rewardName; 
                PlayerPrefs.SetInt($"Equip_{id}_Unlocked", 1);
                PlayerPrefs.SetInt($"Equip_{id}_Level", 1);
                PlayerPrefs.Save();
                BagEvent.InvokeItemBought();
                break;


            case RewardTypeItem.Hero:
                PlayerPrefs.SetInt($"HeroUnlocked_{data.rewardName}", 1);
                PlayerPrefs.Save();
                HeroEvents.OnHeroBought?.Invoke(data.rewardName);
                break;
        }

        NotificationPopupUI.Instance?.Show($"You claimed: {data.rewardName} x{data.quantity}");
    }
    public void RefreshAllSlots()
    {
        // Xoá toàn bộ slot cũ
        foreach (Transform child in freeParent) Destroy(child.gameObject);
        foreach (Transform child in premiumParent) Destroy(child.gameObject);

        // Vẽ lại toàn bộ
        Start(); // 👈 gọi lại Start() là đủ
    }
    public void CheckAndUpdateNotification()
    {
        int playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);

        foreach (var levelData in database.levels)
        {
            bool unlocked = playerLevel >= levelData.level;

            string freeKey = $"Claimed_BattlePass_{levelData.level}_Free";
            if (unlocked && PlayerPrefs.GetInt(freeKey, 0) == 0)
            {
                NotificationBadgeManager.Instance.SetNotification("battlepass", true);
                return;
            }

            if (BattlePassManager.IsActivated())
            {
                string premiumKey = $"Claimed_BattlePass_{levelData.level}_Premium";
                if (unlocked && PlayerPrefs.GetInt(premiumKey, 0) == 0)
                {
                    NotificationBadgeManager.Instance.SetNotification("battlepass", true);
                    return;
                }
            }
        }

        NotificationBadgeManager.Instance.SetNotification("battlepass", false);
    }

}
