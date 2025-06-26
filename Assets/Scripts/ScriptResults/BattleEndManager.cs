using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEndManager : MonoBehaviour
{
    public bool isWin = false;
    public float timer = 0f;
    public int killCount = 0;

    private RankDataManager rankData;
    private LevelUI levelUI;

    void Awake()
    {
        rankData = RankDataManager.Instance;
        levelUI = FindObjectOfType<LevelUI>();

        if (rankData == null)
            Debug.LogError("❌ Không tìm thấy RankDataManager.");
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    public void EndMatch()
    {
        // ========== 1. Match Stats ==========
        GameResultData.matchTime = timer;
        var killUI = FindObjectOfType<KillInfoUIHandler>();
        if (killUI != null)
        {
            GameResultData.killCount = killUI != null ? int.Parse(killUI.killText.text) : 0;
        }


        // ========== 2. Cập nhật Battle Level từ UI ==========
        LevelUI levelUI = FindObjectOfType<LevelUI>();
        if (levelUI != null)
        {
            GameResultData.battleLevel = levelUI.CurrentBattleLevel;

        }
        else
        {
            GameResultData.battleLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
            Debug.LogWarning("⚠️ Không tìm thấy LevelUI (Prefab chưa spawn?) => Gán battleLevel từ PlayerLevel.");
        }

        // ========== 3. Tính EXP cho PlayerLevel & Rank ==========
        int levelExp = 0;
        int rankExp = 0;

        // EXP theo thời gian sống
        if (timer >= 20f)
        {
            int timeExp = Mathf.FloorToInt(timer * 0.6f);
            levelExp += timeExp;
            rankExp += Mathf.FloorToInt(timeExp * 0.5f);
        }

        // EXP theo số kill
        if (killCount > 0)
        {
            int killExp = killCount * 12;
            levelExp += killExp;
            rankExp += Mathf.FloorToInt(killExp * 0.6f);
        }

        // EXP thưởng nếu thắng
        if (isWin)
        {
            levelExp += 40;
            rankExp += 50;
        }
        Debug.Log($"[EXP] Level: {levelExp}, Rank: {rankExp}");

        // ========== 4. Cập nhật Level ==========
        int level = PlayerPrefs.GetInt("PlayerLevel", 1);
        int expBefore = PlayerPrefs.GetInt("PlayerExp", 0);
        int expToNext = GameFormula.GetExpToNextLevel(level);

        int newExp = expBefore + levelExp;
        bool leveledUp = false;

        while (newExp >= expToNext)
        {
            newExp -= expToNext;
            level++;
            expToNext = GameFormula.GetExpToNextLevel(level);
            leveledUp = true;
        }

        PlayerPrefs.SetInt("PlayerLevel", level);
        PlayerPrefs.SetInt("PlayerExp", newExp);
        PlayerPrefs.Save();

        GameResultData.levelBefore = level - (leveledUp ? 1 : 0);
        GameResultData.levelAfter = level;
        GameResultData.expBefore = expBefore;
        GameResultData.expGained = levelExp;
        GameResultData.expToNext = expToNext;

        // ========== 5. Cập nhật Rank ==========
        if (rankData != null)
        {
            GameResultData.rankBefore = rankData.CurrentRankIndex;
            GameResultData.rankExpBefore = rankData.CurrentRankExp;

            rankData.AddRankExp(rankExp);

            GameResultData.rankAfter = rankData.CurrentRankIndex;
            GameResultData.rankExpToNext = rankData.GetNextRankRequiredExp();
            GameResultData.rankExpGained = rankExp;
        }

        // ========== 6. Reward ==========
        int timeBonus = Mathf.FloorToInt(timer * 1.5f);
        int gold = 200 + killCount * 30 + timeBonus;

        int gem = 0;
        if (killCount >= 3 && timer >= 60)
            gem = Random.Range(1, 4);
        else if (killCount >= 1)
            gem = 1;
        if (isWin)
        {
            gem += 5;
            Debug.Log("🎉 Win bonus: +5 GEM!");
        }

        int key = 0;
        if (killCount >= 5 || timer >= 90)
            key = Random.Range(2, 6);
        else if (killCount >= 2)
            key = 2;

        GameResultData.gold = gold;
        GameResultData.gem = gem;
        GameResultData.key = key;
        // 🎁 Thưởng nếu Top 1 (đã được gán sẵn ở KillInfoUIHandler)
        if (GameResultData.playerRank == 1)
        {
            GameResultData.key += 5;
            GameResultData.gem += 5;
            Debug.Log("🎉 TOP 1 BONUS: +5 key, +5 gem!");
        }

        // ========== 8. Load Result Scene ==========
        SceneManager.LoadScene("Scene_Result");
    }
    public void ForceEndMatchByQuit()
    {
        Debug.Log("🏳️ Người chơi thoát trận giữa chừng → EndMatch()");

        isWin = false;
        EndMatch(); // Tái sử dụng logic hiện tại
    }

}
