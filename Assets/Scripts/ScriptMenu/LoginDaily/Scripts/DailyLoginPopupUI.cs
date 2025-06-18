using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyLoginPopupUI : MonoBehaviour
{
    public Transform slotParent; // CenterElement
    public DailyRewardSlotUI slotPrefab;

    public Button receiveButton;    // ButtonOK
    public GameObject buttonGroup;  // GameObject chứa ButtonCancel & ButtonAds

    private DailyLoginManager manager;
    private int currentDay;
    private DailyRewardSlotUI currentSlot; // ô hiện tại

    public void Setup(List<DailyReward> rewards, int currentDay, int lastClaimedDay, DailyLoginManager mgr)
    {
        manager = mgr;
        this.currentDay = currentDay;

        foreach (Transform child in slotParent)
            Destroy(child.gameObject);

        foreach (var reward in rewards)
        {
            var slot = Instantiate(slotPrefab, slotParent);
            bool isToday = reward.day == currentDay;
            bool isClaimed = reward.day < currentDay && reward.day <= lastClaimedDay;

            slot.Setup(reward, isToday, isClaimed);

            if (isToday)
                currentSlot = slot;
        }

        receiveButton.gameObject.SetActive(true);
        buttonGroup.SetActive(false);

        receiveButton.onClick.RemoveAllListeners();
        receiveButton.onClick.AddListener(() =>
        {
            manager.ClaimReward();
            currentSlot.ShowTick();
            receiveButton.gameObject.SetActive(false);
            buttonGroup.SetActive(true);
        });
    }
}
