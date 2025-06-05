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

void Start()
{
    ShowLevelInfo();
    // Không cần gọi ShowRankInfo() nữa
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
