using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MatchStatistics : MonoBehaviour
{
    public TMP_Text killText, timeText, goldText, gemText, keyText, battleLevelText, resultMessageText, topText;
    public Button doubleRewardButton;
    private bool hasClaimedDouble = false;
    public GameObject upgradeLevelRankPanel;
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

        int currentKey = PlayerPrefs.GetInt("LuckyKey", 0);
        PlayerPrefs.SetInt("LuckyKey", currentKey + GameResultData.key);
        PlayerPrefs.Save();

        // Gắn sự kiện x2 nếu có
        doubleRewardButton.onClick.AddListener(HandleDoubleRewardAd);
          // Ẩn panel UpgradeLevelRank nếu có
        if (upgradeLevelRankPanel != null)
            upgradeLevelRankPanel.SetActive(false);


        // Daily Task Progress (chạy sau trận)
        DailyTaskProgressManager.Instance.AddProgress("win_match");
        DailyTaskProgressManager.Instance.AddProgress("win_3_match");

        DailyTaskProgressManager.Instance.AddProgress("kill_1", GameResultData.killCount);
        DailyTaskProgressManager.Instance.AddProgress("kill_10", GameResultData.killCount);

        DailyTaskProgressManager.Instance.AddProgress("reach_level_5", GameResultData.levelAfter);
        DailyTaskProgressManager.Instance.AddProgress("reach_level_10", GameResultData.levelAfter);

        // Nếu đã xem quảng cáo x2
        if (hasClaimedDouble)
            DailyTaskProgressManager.Instance.AddProgress("watch_ads");

        // Nếu người chơi đã đạt Top 1
        if (GameResultData.playerRank == 1)
            DailyTaskProgressManager.Instance.AddProgress("reach_top_1");

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

            int currentKey = PlayerPrefs.GetInt("LuckyKey", 0);
            PlayerPrefs.SetInt("LuckyKey", currentKey + GameResultData.key);
            PlayerPrefs.Save();

            // Cập nhật lại UI sau khi cộng x2
            goldText.text = (GameResultData.gold * 2).ToString();
            gemText.text = (GameResultData.gem * 2).ToString();
            keyText.text = (GameResultData.key * 2).ToString();

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

        string[] top1Msgs = {
        "CHIẾN THẮNG! GÀ ĐÃ VÀO NỒI!",
        "Top 1 không ai cản nổi!",
        "Đỉnh của chóp, trận này là của bạn!",
        "Gánh team không một vết xước!",
    };

        string[] top2to5Msgs = {
        "Suýt nữa thôi... Gà gần tới tay!",
        "Top cao thật đó, nhưng chưa đủ gắt!",
        "Gục trước cửa thiên đường...",
        "Thiếu chút may mắn, nhưng trình thì có!",
        "Được đấy! Nhưng còn phải luyện thêm!"
    };

        string[] top6to10Msgs = {
        "Khá ổn, nhưng vẫn bị out trình!",
        "Bắn ngon nhưng vẫn gãy, tiếc ghê!",
        "Top giữa... Ừ thì cũng không tệ!",
        "Chơi vậy là được rồi, nhưng chưa nổi bật!",
    };

        string[] top11to20Msgs = {
        "Vào game làm nền à?",
        "Vẫn đang khởi động thôi mà đúng không?",
        "Vừa loot được cây súng thì... die!",
        "Gà chưa kịp gáy, người đã toang!",
        "Trận sau nhớ bật aim nha bạn ơi!"
    };

        if (rank == 1)
        {
            resultMessageText.text = GetRandom(top1Msgs);
            resultMessageText.color = new Color32(0, 255, 0, 255); // Xanh lá
        }
        else if (rank <= 5)
        {
            resultMessageText.text = GetRandom(top2to5Msgs);
            resultMessageText.color = new Color32(255, 204, 0, 255); // Vàng cam
        }
        else if (rank <= 10)
        {
            resultMessageText.text = GetRandom(top6to10Msgs);
            resultMessageText.color = new Color32(255, 136, 0, 255); // Cam sáng
        }
        else
        {
            resultMessageText.text = GetRandom(top11to20Msgs);
            resultMessageText.color = new Color32(255, 68, 68, 255); // Đỏ hồng
        }
    }

    string GetRandom(string[] messages)
    {
        return messages[Random.Range(0, messages.Length)];
    }


    public void RetryCurrentMap()
    {
        if (GameData.SelectedMap != null)
        {
            Debug.Log($"🔁 Retry map: {GameData.SelectedMap.mapName}");

            // Dùng Loading nếu có
            if (GlobalLoadingController.Instance != null)
                GlobalLoadingController.Instance.LoadSceneWithDelay("LayoutBattle", 2f);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("LayoutBattle");
        }
        else
        {
            Debug.LogWarning("⚠️ GameData.SelectedMap is null. Can't retry!");
        }
    }


    //     void ShowRankMessage()
    // {
    //     int rank = GameResultData.playerRank;

    //     string[] top1Msgs = {
    //         "VICTORY! Dinner is served!",
    //         "Top 1! Nobody even came close!",
    //         "Absolutely legendary performance!",
    //         "Carried the whole game without a scratch!",
    //         "Don’t ask why you won. Ask why they even tried!"
    //     };

    //     string[] top2to5Msgs = {
    //         "So close… but no chicken!",
    //         "Great effort, but not quite enough!",
    //         "Knocked out at heaven’s gate!",
    //         "You’ve got the skills, just need a bit of luck!",
    //         "Nice one! But greatness takes more!"
    //     };

    //     string[] top6to10Msgs = {
    //         "Decent, but still outplayed!",
    //         "Solid game, but not enough fire!",
    //         "Middle of the pack… not bad!",
    //         "Could've been better. Could've been worse.",
    //         "You're improving, but you're not there yet!"
    //     };

    //     string[] top11to20Msgs = {
    //         "What was that? A warm-up match?",
    //         "Didn't even get to loot properly!",
    //         "Blink and you're dead!",
    //         "Were you even trying?",
    //         "Tip: The trigger goes *pew*, not *panic*."
    //     };

    //     if (rank == 1)
    //     {
    //         resultMessageText.text = GetRandom(top1Msgs);
    //         resultMessageText.color = new Color32(0, 255, 0, 255); // Green
    //     }
    //     else if (rank <= 5)
    //     {
    //         resultMessageText.text = GetRandom(top2to5Msgs);
    //         resultMessageText.color = new Color32(255, 204, 0, 255); // Gold
    //     }
    //     else if (rank <= 10)
    //     {
    //         resultMessageText.text = GetRandom(top6to10Msgs);
    //         resultMessageText.color = new Color32(255, 136, 0, 255); // Orange
    //     }
    //     else
    //     {
    //         resultMessageText.text = GetRandom(top11to20Msgs);
    //         resultMessageText.color = new Color32(255, 68, 68, 255); // Red
    //     }
    // }

    // string GetRandom(string[] messages)
    // {
    //     return messages[Random.Range(0, messages.Length)];
    // }

}
