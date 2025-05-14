using UnityEngine;

public class BattlePassRenderer : MonoBehaviour {
    public BattlePassDatabase database;
    public Transform premiumParent;
    public Transform freeParent;
    public GameObject rewardSlotPrefab;

    private void Start() {
        int playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        foreach (var level in database.levels) {
            bool unlocked = playerLevel >= level.level;

            // FREE SLOT
            var free = Instantiate(rewardSlotPrefab, freeParent);
            free.GetComponent<RewardSlotUI>().Setup(level.freeReward, unlocked);

            // PREMIUM SLOT
            var premium = Instantiate(rewardSlotPrefab, premiumParent);
            premium.GetComponent<RewardSlotUI>().Setup(level.premiumReward, unlocked);
        }
    }
}
