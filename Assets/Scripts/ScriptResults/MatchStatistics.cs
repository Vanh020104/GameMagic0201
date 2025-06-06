using UnityEngine;
using TMPro;

public class MatchStatistics : MonoBehaviour
{
    public TMP_Text killText, timeText, goldText, gemText, keyText, battleLevelText, resultMessageText, topText;
    void Start()
    {
        killText.text = GameResultData.killCount.ToString();
        timeText.text = FormatTime(GameResultData.matchTime);
        goldText.text = GameResultData.gold.ToString();
        gemText.text = GameResultData.gem.ToString();
        keyText.text = GameResultData.key.ToString();
        // heroLevelText.text = GameResultData.levelAfter.ToString();
        battleLevelText.text = GameResultData.battleLevel.ToString();
        topText.text = GetRankWithSuffix(GameResultData.playerRank);
        ShowRankMessage();
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
