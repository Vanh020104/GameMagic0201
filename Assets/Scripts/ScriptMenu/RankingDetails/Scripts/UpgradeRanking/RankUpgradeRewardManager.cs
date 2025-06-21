using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankUpgradeRewardManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelUpgradeRanking;
    public TMP_Text textSeason;
    public TMP_Text textGold, textGem;
    public Image rankIcon;
    public Image frameIcon;
    public Image medalIcon;

    [Header("Data")]
    public RankDatabase rankDatabase;
    public RankReward[] rankRewards = new RankReward[9]; // cấu hình theo số lượng rank

    public void ShowRankUpgradePanel(int oldRank, int newRank)
    {
        if (newRank <= oldRank) return;

        panelUpgradeRanking.SetActive(true);

        int season = PlayerPrefs.GetInt("CurrentSeason", 1);
        textSeason.text = $"RANK SEASON #{season}";

        RankInfo newRankData = rankDatabase.ranks[newRank];

        // Gán icon
        rankIcon.sprite = newRankData.rankIcon;
        frameIcon.sprite = newRankData.rankBackgroundIcon;
        medalIcon.sprite = newRankData.medalIcon;

        // Gán thưởng
        RankReward reward = rankRewards[newRank];
        int gold = Random.Range(reward.minGold, reward.maxGold + 1);
        int gem = Random.Range(reward.minGem, reward.maxGem + 1);

        textGold.text = gold.ToString();
        textGem.text = gem.ToString();

        GoldGemManager.Instance.AddGold(gold);
        GoldGemManager.Instance.AddGem(gem);
    }
    public void ClosePanel()
    {
        panelUpgradeRanking.SetActive(false);
    }

}