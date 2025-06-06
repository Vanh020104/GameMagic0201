// ------------------------------
// 🔧 Bước 1: Update RankUIController.cs
// ------------------------------

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    public IEnumerator AnimateRankGain(int beforeIndex, int beforeExp, int gainedExp)
    {
        var db = RankDataManager.Instance.rankDatabase;
        int currentIndex = beforeIndex;
        int currentExp = beforeExp;

        while (gainedExp > 0 && currentIndex < db.ranks.Length - 1)
        {
            currentExp++;
            gainedExp--;

            int expToNext = db.ranks[currentIndex + 1].requiredExp;

            if (currentExp >= expToNext)
            {
                currentIndex++;
                currentExp = 0;
            }

            UpdateAnimatedUI(currentIndex, currentExp);
            yield return new WaitForSeconds(0.01f); // Tốc độ tăng Rank EXP
        }


        UpdateAnimatedUI(currentIndex, currentExp);
    }

    private void UpdateAnimatedUI(int index, int exp)
    {
        var db = RankDataManager.Instance.rankDatabase;
        var rank = db.ranks[index];

        if (rankIcon != null) rankIcon.sprite = rank.rankIcon;
        if (rankBackgroundImage != null) rankBackgroundImage.sprite = rank.rankBackgroundIcon;
        if (medalIcon != null) medalIcon.sprite = rank.medalIcon;
        if (rankText != null) rankText.text = rank.displayName;

        if (index == db.ranks.Length - 1)
        {
            rankExpSlider.value = rankExpSlider.maxValue;
            rankExpText.text = "MAX RANK";
        }
        else
        {
            int nextExp = db.ranks[index + 1].requiredExp;
            rankExpSlider.maxValue = nextExp;
            rankExpSlider.value = exp;
            rankExpText.text = $"{exp} / {nextExp}";
        }
    }
}
