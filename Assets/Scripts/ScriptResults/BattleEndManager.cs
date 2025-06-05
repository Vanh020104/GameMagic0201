using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEndManager : MonoBehaviour
{
    public bool isWin = false;
    public float timer = 0f;
    public int killCount = 0;

    private RankDataManager rankData;

    void Awake()
    {
        rankData = RankDataManager.Instance;

        if (rankData == null)
        {
            Debug.LogError("❌ Không tìm thấy RankDataManager. Đảm bảo nó tồn tại ở scene Home và có DontDestroyOnLoad.");
        }
    }

    void Update() => timer += Time.deltaTime;

    public void EndMatch()
    {
        // ------------------------
        // 1. Match Stats
        // ------------------------
        GameResultData.matchTime = timer;
        GameResultData.killCount = killCount;

        // ------------------------
        // 2. Level & EXP
        // ------------------------
        int level = PlayerPrefs.GetInt("PlayerLevel", 1);
        int expBefore = PlayerPrefs.GetInt("PlayerExp", 0);
        int expToNext = GetExpToNextLevel(level);

        int expGained = 50 + killCount * 10 + Mathf.FloorToInt(timer * 1.5f);
        int newExp = expBefore + expGained;
        bool leveledUp = false;

        while (newExp >= expToNext)
        {
            newExp -= expToNext;
            level++;
            expToNext = GetExpToNextLevel(level);
            leveledUp = true;
        }

        PlayerPrefs.SetInt("PlayerLevel", level);
        PlayerPrefs.SetInt("PlayerExp", newExp);
        PlayerPrefs.Save();

        GameResultData.levelBefore = level - (leveledUp ? 1 : 0);
        GameResultData.levelAfter = level;
        GameResultData.expBefore = expBefore;
        GameResultData.expGained = expGained;
        GameResultData.expToNext = expToNext;

        // ------------------------
        // 3. Rank & Rank EXP
        // ------------------------
        if (rankData != null)
        {
            GameResultData.rankBefore = rankData.CurrentRankIndex;
            GameResultData.rankExpBefore = rankData.CurrentRankExp;

            rankData.AddRankExp(expGained);

            GameResultData.rankAfter = rankData.CurrentRankIndex;
            GameResultData.rankExpToNext = rankData.GetNextRankRequiredExp();
            GameResultData.rankExpGained = expGained;
        }
        else
        {
            Debug.LogWarning("⚠️ Không thể cập nhật Rank vì thiếu RankDataManager.");
        }

        // ------------------------
        // 4. Reward: Gold / Gem / Key
        // ------------------------
        int timeBonus = Mathf.FloorToInt(timer * 1.5f);
        int gold = 100 + killCount * 30 + timeBonus;

        int gem = 0;
        if (killCount >= 3 && timer >= 60)
            gem = Random.Range(1, 4);
        else if (killCount >= 1)
            gem = 1;

        if (isWin)
        {
            gem += 5;
            Debug.Log("🎉 Thắng trận, cộng thêm 5 GEM bonus!");
        }

        int key = 0;
        if (killCount >= 5 || timer >= 90)
            key = Random.Range(1, 4);
        else if (killCount >= 2)
            key = 1;

        GameResultData.gold = gold;
        GameResultData.gem = gem;
        GameResultData.key = key;

        // ------------------------
        // 5. Cộng vào tài khoản
        // ------------------------
        GoldGemManager.Instance?.AddGold(gold);
        GoldGemManager.Instance?.AddGem(gem);

        int currentKey = PlayerPrefs.GetInt("LuckyKey", 0);
        PlayerPrefs.SetInt("LuckyKey", currentKey + key);
        PlayerPrefs.Save();

        // ------------------------
        // 6. Load Result Scene
        // ------------------------
        SceneManager.LoadScene("Scene_Result");
    }

    private int GetExpToNextLevel(int level)
    {
        return Mathf.RoundToInt(100 * Mathf.Pow(1.2f, level - 1));
    }
    
}
