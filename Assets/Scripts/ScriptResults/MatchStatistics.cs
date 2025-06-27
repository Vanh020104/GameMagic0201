using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Localization;

public class MatchStatistics : MonoBehaviour
{
    public TMP_Text killText, timeText, goldText, gemText, keyText, battleLevelText, resultMessageText, topText;
    public Button doubleRewardButton;
    private bool hasClaimedDouble = false;
    public GameObject upgradeLevelRankPanel;
    private LocalizedString topLocalized;
    private LocalizedString quoteLocalized;

    void Start()
    {
        // Hiển thị kết quả
        killText.text = GameResultData.killCount.ToString();
        timeText.text = FormatTime(GameResultData.matchTime);
        goldText.text = GameResultData.gold.ToString();
        gemText.text = GameResultData.gem.ToString();
        keyText.text = GameResultData.key.ToString();
        battleLevelText.text = $"{GameResultData.battleLevel}";

        // Cộng phần thưởng (chỉ 1 lần duy nhất ở đây)
        GoldGemManager.Instance.AddGold(GameResultData.gold);
        GoldGemManager.Instance.AddGem(GameResultData.gem);

        GoldGemManager.Instance.AddKey(GameResultData.key);


        // Gắn sự kiện x2 nếu có
        doubleRewardButton.onClick.AddListener(HandleDoubleRewardAd);
        // Ẩn panel UpgradeLevelRank nếu có
        if (upgradeLevelRankPanel != null)
            upgradeLevelRankPanel.SetActive(false);

        DailyTaskBridge.Instance?.TryAddProgress("kill_1", GameResultData.killCount);
        DailyTaskBridge.Instance?.TryAddProgress("kill_10", GameResultData.killCount);

        if (GameResultData.battleLevel >= 5)
            DailyTaskBridge.Instance?.TryAddProgress("reach_level_5", 1);

        if (GameResultData.battleLevel >= 10)
            DailyTaskBridge.Instance?.TryAddProgress("reach_level_10", 1);


        // Nếu đã xem quảng cáo x2
        if (hasClaimedDouble)
            DailyTaskBridge.Instance?.TryAddProgress("watch_ads");

        // Nếu người chơi đã đạt Top 1
        if (GameResultData.playerRank == 1)
        {
            DailyTaskBridge.Instance?.TryAddProgress("win_match");
            DailyTaskBridge.Instance?.TryAddProgress("win_3_match");
        }

        // 🎯 Nhiệm vụ sống sót theo thời gian
        int matchSeconds = Mathf.FloorToInt(GameResultData.matchTime);

        if (matchSeconds >= 60)
            DailyTaskBridge.Instance?.TryAddProgress("survive_60s", matchSeconds);

        if (matchSeconds >= 180)
            DailyTaskBridge.Instance?.TryAddProgress("survive_180s", matchSeconds);

        ShowRankMessage(); // hiển thị quote dựa trên playerRank
        topText.text = GetRankWithSuffix(GameResultData.playerRank); // hiển thị TOP 1, TOP 2...


        // Bắt đầu coroutine delay 1s → show panel
        StartCoroutine(ShowUpgradePanelDelayed());
    }

    private IEnumerator ShowUpgradePanelDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        if (upgradeLevelRankPanel != null)
            upgradeLevelRankPanel.SetActive(true);
    }


    private void HandleDoubleRewardAd()
    {
        if (hasClaimedDouble) return;

        AdManager.Instance.ShowRewardedAd(() =>
        {
            GoldGemManager.Instance.AddGold(GameResultData.gold);
            GoldGemManager.Instance.AddGem(GameResultData.gem);
            GoldGemManager.Instance.AddKey(GameResultData.key);


            // Cập nhật lại UI sau khi cộng x2
            goldText.text = (GameResultData.gold * 2).ToString();
            gemText.text = (GameResultData.gem * 2).ToString();
            keyText.text = (GameResultData.key * 1).ToString();

            doubleRewardButton.interactable = false;
            hasClaimedDouble = true;
        });
    }




    string GetRankWithSuffix(int rank)
    {
        if (rank <= 0) return "-";

        if (rank % 100 >= 11 && rank % 100 <= 13)
            return $"TOP {rank}";

        switch (rank % 10)
        {
            case 1: return $"TOP {rank}";
            case 2: return $"TOP {rank}";
            case 3: return $"TOP {rank}";
            default: return $"TOP {rank}";
        }
    }

    string FormatTime(float t)
    {
        int min = Mathf.FloorToInt(t / 60);
        int sec = Mathf.FloorToInt(t % 60);
        return $"{min:D2}:{sec:D2}";
    }

    void ShowRankMessage()
    {
        int rank = GameResultData.playerRank;

        string[] top1Keys = { "match_victory_1", "match_victory_2", "match_victory_3", "match_victory_4" };
        string[] top2to5Keys = { "match_rank_2to5_1", "match_rank_2to5_2", "match_rank_2to5_3", "match_rank_2to5_4", "match_rank_2to5_5" };
        string[] top6to10Keys = { "match_rank_6to10_1", "match_rank_6to10_2", "match_rank_6to10_3", "match_rank_6to10_4" };
        string[] top11to20Keys = { "match_rank_11to20_1", "match_rank_11to20_2", "match_rank_11to20_3", "match_rank_11to20_4", "match_rank_11to20_5" };

        string selectedKey = null;

        if (rank == 1)
        {
            selectedKey = top1Keys[Random.Range(0, top1Keys.Length)];
            resultMessageText.color = new Color32(0, 255, 0, 255); // Green
        }
        else if (rank <= 5)
        {
            selectedKey = top2to5Keys[Random.Range(0, top2to5Keys.Length)];
            resultMessageText.color = new Color32(255, 204, 0, 255); // Orange
        }
        else if (rank <= 10)
        {
            selectedKey = top6to10Keys[Random.Range(0, top6to10Keys.Length)];
            resultMessageText.color = new Color32(255, 136, 0, 255); // Light Orange
        }
        else
        {
            selectedKey = top11to20Keys[Random.Range(0, top11to20Keys.Length)];
            resultMessageText.color = new Color32(255, 68, 68, 255); // Red
        }

        quoteLocalized = new LocalizedString("LanguageVanh", selectedKey);
        quoteLocalized.StringChanged += val => resultMessageText.text = val;
    }


    string GetRandom(string[] messages)
    {
        return messages[Random.Range(0, messages.Length)];
    }
    public void RetryCurrentMap()
    {
        if (GameData.SelectedMap == null)
        {
            Debug.LogWarning("⚠️ GameData.SelectedMap is null. Can't retry!");
            return;
        }

        // Lấy số lần đã chơi lại
        int retryCounter = PlayerPrefs.GetInt("RetryCounter", 0);
        retryCounter++;
        PlayerPrefs.SetInt("RetryCounter", retryCounter);
        PlayerPrefs.Save();

        Debug.Log($"🔁 Retry count: {retryCounter}");

        // Cứ mỗi 2 lần thì bắt xem quảng cáo
        if (retryCounter % 2 == 0)
        {
            Debug.Log("📺 Show ad before retrying...");

            AdManager.Instance.ShowRewardedAd(() =>
            {
                ReloadBattleScene();
                DailyTaskBridge.Instance?.TryAddProgress("watch_ads");
            });
        }
        else
        {
            ReloadBattleScene();
        }
    }
    private void ReloadBattleScene()
    {
        Debug.Log($"🔁 Reloading map: {GameData.SelectedMap.mapName}");

        if (GlobalLoadingController.Instance != null)
            GlobalLoadingController.Instance.LoadSceneWithDelay("LayoutBattle", 2f);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("LayoutBattle");
    }

}
