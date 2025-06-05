using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankDataManager : MonoBehaviour
{
    public static RankDataManager Instance;

    public RankDatabase rankDatabase;

    public int CurrentRankIndex { get; private set; }
    public int CurrentRankExp { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadRank();
    }

    public void AddRankExp(int amount)
    {
        if (CurrentRankIndex >= rankDatabase.ranks.Length - 1)
        {
            CurrentRankExp = 0;
            return;
        }

        CurrentRankExp += amount;

        while (CurrentRankIndex + 1 < rankDatabase.ranks.Length &&
               CurrentRankExp >= rankDatabase.ranks[CurrentRankIndex + 1].requiredExp)
        {
            CurrentRankIndex++;
            CurrentRankExp = 0;
        }

        SaveRank();
    }

    public void SaveRank()
    {
        PlayerPrefs.SetInt("PlayerRankIndex", CurrentRankIndex);
        PlayerPrefs.SetInt("PlayerRankExp", CurrentRankExp);
        PlayerPrefs.Save();
    }

    public void LoadRank()
    {
        CurrentRankIndex = PlayerPrefs.GetInt("PlayerRankIndex", 0);
        CurrentRankExp = PlayerPrefs.GetInt("PlayerRankExp", 0);

        if (CurrentRankIndex >= rankDatabase.ranks.Length)
        {
            CurrentRankIndex = rankDatabase.ranks.Length - 1;
            CurrentRankExp = 0;
        }
    }
    public int GetNextRankRequiredExp()
    {
        if (rankDatabase == null || rankDatabase.ranks == null)
            return 0;

        if (CurrentRankIndex + 1 < rankDatabase.ranks.Length)
        {
            return rankDatabase.ranks[CurrentRankIndex + 1].requiredExp;
        }

        // Nếu đã max rank
        return 0;
    }

}
