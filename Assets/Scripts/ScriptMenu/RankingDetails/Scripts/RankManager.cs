using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankManager : MonoBehaviour
{
    [Header("Database")]
    public RankDatabase rankDatabase;

    [Header("UI")]
    public Image rankIcon;
    public Image rankBackgroundImage;
    public Image medalIcon;
    public TMP_Text rankText;

    [Header("UI - Rank EXP Progress")]
    public Slider rankExpSlider;
    public TMP_Text rankExpText;

    private int currentRankIndex;
    private int currentRankExp;

    const string RANK_INDEX_KEY = "PlayerRankIndex";
    const string RANK_EXP_KEY = "PlayerRankExp";

    void Start()
    {
        currentRankIndex = PlayerPrefs.GetInt(RANK_INDEX_KEY, 0);
        currentRankExp = PlayerPrefs.GetInt(RANK_EXP_KEY, 0);
        UpdateRankUI();
    }

    public void AddRankExp(int amount)
    {
        currentRankExp += amount;

        while (currentRankIndex + 1 < rankDatabase.ranks.Length &&
               currentRankExp >= rankDatabase.ranks[currentRankIndex + 1].requiredExp)
        {
            currentRankIndex++;
            currentRankExp = 0;
            Debug.Log($"🎉 Lên rank: {rankDatabase.ranks[currentRankIndex].displayName}");
        }

        PlayerPrefs.SetInt(RANK_INDEX_KEY, currentRankIndex);
        PlayerPrefs.SetInt(RANK_EXP_KEY, currentRankExp);
        PlayerPrefs.Save();

        UpdateRankUI();
    }

    private int GetNextRankRequiredExp()
    {
        if (currentRankIndex + 1 < rankDatabase.ranks.Length)
        {
            return rankDatabase.ranks[currentRankIndex + 1].requiredExp;
        }

        return rankDatabase.ranks[currentRankIndex].requiredExp;
    }

    public void UpdateRankUI()
    {
        var rank = rankDatabase.ranks[currentRankIndex];
        var nextRankExp = GetNextRankRequiredExp();

        if (rankIcon != null) rankIcon.sprite = rank.rankIcon;
        if (rankBackgroundImage != null) rankBackgroundImage.sprite = rank.rankBackgroundIcon;
        if (medalIcon != null) medalIcon.sprite = rank.medalIcon;
        if (rankText != null) rankText.text = rank.displayName;

        if (rankExpSlider != null)
        {
            rankExpSlider.maxValue = nextRankExp;
            rankExpSlider.value = currentRankExp;
        }

        if (rankExpText != null)
        {
            rankExpText.text = $"{currentRankExp} / {nextRankExp}";
        }
    }

    public void ResetToDefaultRank()
    {
        currentRankIndex = 0;
        currentRankExp = 0;

        PlayerPrefs.SetInt(RANK_INDEX_KEY, currentRankIndex);
        PlayerPrefs.SetInt(RANK_EXP_KEY, currentRankExp);
        PlayerPrefs.Save();

        UpdateRankUI();
    }

    public void Debug_AddRankExp()
    {
        AddRankExp(300);
    }
}
