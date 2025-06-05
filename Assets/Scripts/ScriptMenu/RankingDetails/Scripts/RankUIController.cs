using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankUIController : MonoBehaviour
{
    public Image rankIcon;
    public Image rankBackgroundImage;
    public Image medalIcon;
    public TMP_Text rankText;

    public Slider rankExpSlider;
    public TMP_Text rankExpText;

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        var data = RankDataManager.Instance;
        var db = data.rankDatabase;
        int index = data.CurrentRankIndex;

        if (index >= db.ranks.Length) return;

        var rank = db.ranks[index];

        if (rankIcon != null) rankIcon.sprite = rank.rankIcon;
        if (rankBackgroundImage != null) rankBackgroundImage.sprite = rank.rankBackgroundIcon;
        if (medalIcon != null) medalIcon.sprite = rank.medalIcon;
        if (rankText != null) rankText.text = rank.displayName;

        if (index == db.ranks.Length - 1)
        {
            if (rankExpSlider != null)
                rankExpSlider.value = rankExpSlider.maxValue;

            if (rankExpText != null)
                rankExpText.text = "MAX RANK";
        }
        else
        {
            int nextExp = db.ranks[index + 1].requiredExp;
            if (rankExpSlider != null)
            {
                rankExpSlider.maxValue = nextExp;
                rankExpSlider.value = data.CurrentRankExp;
            }

            if (rankExpText != null)
                rankExpText.text = $"{data.CurrentRankExp} / {nextExp}";
        }
    }
}
