using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattlePassPurchasePanel : MonoBehaviour
{
    public GameObject panelRoot;

    [SerializeField] private Button buttonUnlock;
    [SerializeField] private TMP_Text unlockButtonText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;

    private void Start()
    {
        Hide();
    }

    public void Show()
    {
        UpdateUI();
        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    private void UpdateUI()
    {
        bool isUnlocked = BattlePassManager.IsActivated();

        if (isUnlocked)
        {
            titleText.text = "BATTLE PASS ALREADY UNLOCKED";
            subtitleText.text = "You already own the Premium Battle Pass";
            buttonUnlock.interactable = false;
            unlockButtonText.text = "UNLOCKED";
        }
        else
        {
            titleText.text = "UNLOCK BATTLE PASS";
            subtitleText.text = "Unlock Premium Battle Pass\nFor 2.99$";
            buttonUnlock.interactable = true;
            unlockButtonText.text = "UNLOCK";
        }
    }
}
