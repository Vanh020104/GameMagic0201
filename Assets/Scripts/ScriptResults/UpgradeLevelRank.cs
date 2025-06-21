using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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
        Debug.Log("⚡ UpgradeLevelRank Start()");
        StartCoroutine(AnimateLevelThenRank());
    }
    IEnumerator AnimateLevelThenRank()
    {
        yield return AnimateLevelExp(); // Cộng Level trước
        yield return new WaitForSeconds(0.4f);

        var rankUI = FindObjectOfType<RankUIController>();
        if (rankUI != null)
        {
            yield return rankUI.AnimateRankGain(
                GameResultData.rankBefore,
                GameResultData.rankExpBefore,
                GameResultData.rankExpGained
            );
        }

        // ✅ Chỉ hiển thị panel sau khi cộng EXP rank xong
        if (GameResultData.rankAfter > GameResultData.rankBefore)
        {
            yield return new WaitForSeconds(0.3f); // delay nhẹ cho mượt

            var rankPanel = FindObjectOfType<RankUpgradeRewardManager>();
            if (rankPanel != null)
            {
                Debug.Log("🟢 Hiển thị Panel Upgrade Rank sau khi cộng xong EXP");
                rankPanel.ShowRankUpgradePanel(GameResultData.rankBefore, GameResultData.rankAfter);
            }
        }
    }

    private IEnumerator AnimateLevelExp()
    {
        int level = GameResultData.levelBefore;
        int exp = GameResultData.expBefore;
        int expToNext = GetExpToNextLevel(level);
        int gained = GameResultData.expGained;

        levelText.text = $"Level {level}";
        levelSlider.maxValue = expToNext;
        levelSlider.value = exp;

        int currentExp = exp;

        while (gained > 0)
        {
            int step = Mathf.Min(1, gained);
            currentExp += step;
            gained -= step;

            if (currentExp >= expToNext)
            {
                level++;
                currentExp = 0;
                expToNext = GetExpToNextLevel(level);
                levelSlider.maxValue = expToNext;
                levelText.text = $"Level {level}";

                // Gợi ý: thêm hiệu ứng lên cấp ở đây
            }

            levelSlider.value = currentExp;
            levelExpText.text = $"{currentExp} / {expToNext}";

            yield return new WaitForSeconds(0.01f); // ⏱ chỉnh 0.01f → 0.03f cho mượt
        }


        levelExpText.text = $"{currentExp} / {expToNext}";
        levelExpGainedText.text = $"+{GameResultData.expGained} LEVEL EXP";
        rankExpGainedText.text = $"+{GameResultData.rankExpGained} Rank EXP";
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
        return GameFormula.GetExpToNextLevel(level);
    }


    public void ClosePanelUpgradeLevelRank()
    {
        upgradeLevelRank.SetActive(false);
    }

}
