using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text amountText;
    public GameObject lockIcon;
    public GameObject checkIcon;
    public Image imageBg;
    public Image imageBorder;
    public Button claimButton;
    private bool isClaimed;

    public void Setup(RewardData data, bool isUnlocked, string claimKey, bool isPremium = false)
    {
        icon.sprite = data.icon;
        amountText.text = data.quantity.ToString();

        isClaimed = PlayerPrefs.GetInt(claimKey, 0) == 1;

        bool canClaim = isUnlocked && !isClaimed;
        if (isPremium && !BattlePassManager.IsActivated())
        {
            canClaim = false; // ❌ Chặn nếu chưa mua
        }

        lockIcon.SetActive(!isUnlocked);
        checkIcon.SetActive(isClaimed);
        claimButton.gameObject.SetActive(canClaim);

        if (data.background != null) imageBg.sprite = data.background;
        if (data.border != null) imageBorder.sprite = data.border;
    }


    public void SetClaimCallback(System.Action onClaim)
    {
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(() =>
        {
            onClaim?.Invoke();
            claimButton.gameObject.SetActive(false);
            checkIcon.SetActive(true);
        });
    }

}
