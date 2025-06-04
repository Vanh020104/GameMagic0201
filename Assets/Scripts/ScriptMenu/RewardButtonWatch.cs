using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class RewardButtonWatcj : MonoBehaviour
{
  [Header("Reward Settings")]
    public bool isGoldReward = true;          
    public TextMeshProUGUI amountText;     
    public AdManager adManager;
    private Button button;
    private GoldGemManager goldGemManager;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    private void Start()
    {
        goldGemManager = FindObjectOfType<GoldGemManager>();
    }

    private void OnButtonClicked()
    {
        if (goldGemManager == null || amountText == null)
        {
            Debug.LogWarning("RewardButton setup thiếu component!");
            return;
        }

        // Parse số lượng từ Text
         adManager.ShowRewardedAd(() =>
        {
            if (int.TryParse(amountText.text.Replace(",", ""), out int amount))
            {
                if (isGoldReward)
                    goldGemManager.AddGold(amount);
                else
                    goldGemManager.AddGem(amount);
            }
        });
    }
}