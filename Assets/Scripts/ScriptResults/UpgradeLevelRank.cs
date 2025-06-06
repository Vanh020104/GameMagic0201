using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeLevelRank : MonoBehaviour
{
    [Header("LEVEL UI")]
    public TMP_Text levelText;
    public Slider levelSlider;
    public TMP_Text levelExpText;

    [SerializeField] private GameObject upgradeLevelRank;
    [SerializeField] private TMP_Text levelExpGainedText;
    [SerializeField] private TMP_Text rankExpGainedText;

    void Start()
    {
        ShowLevelInfo();
    }

    void ShowLevelInfo()
    {
        int level = PlayerPrefs.GetInt("PlayerLevel", 1);
        int exp = PlayerPrefs.GetInt("PlayerExp", 0);
        int expToNext = GetExpToNextLevel(level);

        levelText.text = $"Level {level}";
        levelSlider.maxValue = expToNext;
        levelSlider.value = exp;
        levelExpText.text = $"{exp} / {expToNext}";
        levelExpGainedText.text = $"+{GameResultData.expGained} EXP";
        rankExpGainedText.text = $"+{GameResultData.rankExpGained} Rank EXP";

    }

    int GetExpToNextLevel(int level)
    {
        return Mathf.FloorToInt(100 * Mathf.Pow(1.5f, level));
    }

    public void ClosePanelUpgradeLevelRank()
    {
        upgradeLevelRank.SetActive(false);
    }

}
