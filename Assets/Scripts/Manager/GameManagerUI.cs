using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerUI : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject mapPrefab;
    public List<GameObject> botPrefabs;

    private GameObject botContainer;
    private GameObject player;
    public static int currentHealItemCount = 0;
    public static int maxHealItems = 10;
    void Start()
    {
        GameObject map;
        currentHealItemCount = 0;
        if (GameData.SelectedMap != null)
        {
            var loaded = Resources.Load<GameObject>(GameData.SelectedMap.prefabPath);
            if (loaded != null)
            {
                map = Instantiate(loaded, Vector3.zero, Quaternion.identity);

            }
            else
            {
                Debug.LogError("❌ Không tìm thấy prefab map: " + GameData.SelectedMap.prefabPath);
                return;
            }
        }
        else
        {
            // fallback: dùng map mặc định nếu chưa chọn (tránh lỗi)
            map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
            Debug.LogWarning("⚠️ Chưa chọn map, dùng map mặc định.");
        }

        if (GameData.SelectedHero != null)
        {
            var loaded = Resources.Load<GameObject>(GameData.SelectedHero.prefabPath);
            if (loaded != null)
            {
                PlayerSpawnPointGroup playerSpawnGroup = map.GetComponentInChildren<PlayerSpawnPointGroup>();
                if (playerSpawnGroup == null || playerSpawnGroup.spawnPoints.Length == 0)
                {
                    Debug.LogError("❌ Không tìm thấy điểm spawn cho Hero trong map!");
                    return;
                }

                Transform spawnPoint = playerSpawnGroup.GetRandomSpawnPoint();
                player = Instantiate(loaded, spawnPoint.position, spawnPoint.rotation);

                var info = player.GetComponent<PlayerInfo>();
                if (info != null && GameData.SelectedHero != null)
                {
                    string heroId = GameData.SelectedHero.heroId;

                    // Lấy máu và damage từ HeroData hoặc PlayerPrefs (nếu có hệ thống nâng cấp)
                    int savedHealth = PlayerPrefs.GetInt($"HeroHealth_{heroId}", GameData.SelectedHero.baseHealth);
                    int savedDamage = PlayerPrefs.GetInt($"HeroDamage_{heroId}", GameData.SelectedHero.baseDamage);

                    info._hp = savedHealth;
                    info._hpMax = savedHealth;
                    info.baseDamage = savedDamage;

                    Debug.Log($"✅ Hero [{heroId}] gán máu: {savedHealth}, damage: {savedDamage}");
                }

            }
            else
            {
                Debug.LogError("❌ Không tìm thấy prefab Hero: " + GameData.SelectedHero.prefabPath);
                return;
            }
        }
        else
        {
            Transform spawnPoint = map.GetComponentInChildren<PlayerSpawnPointGroup>()?.GetRandomSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogError("❌ Không tìm thấy điểm spawn mặc định cho Hero!");
                return;
            }
            player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

            Debug.LogWarning("⚠️ Chưa chọn Hero → dùng mặc định");
        }
        Camera.main.GetComponent<CameraFollow>().target = player.transform;
        player.GetComponent<PlayerInfo>().playerName = PlayerPrefs.GetString("PlayerName", "Player");
        player.GetComponent<PlayerInfo>().isLocalPlayer = true;

        // Tạo container chứa bot cho gọn hierarchy
        botContainer = new GameObject("BotContainer");

        BotSpawnPointGroup spawnGroup = map.GetComponentInChildren<BotSpawnPointGroup>();
        if (spawnGroup == null || spawnGroup.spawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ Không tìm thấy điểm spawn bot!");
            return;
        }


        /// Spawn Bot
        int totalSpawnPoints = spawnGroup.spawnPoints.Length;

        int playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);

        // 🧠 Xác định tỷ lệ bot theo từng mốc level rõ ràng
        float weakRate, mediumRate, strongRate;

        if (playerLevel <= 2)
        {
            weakRate = 0.8f; mediumRate = 0.2f; strongRate = 0f;
        }
        else if (playerLevel <= 4)
        {
            weakRate = 0.6f; mediumRate = 0.4f; strongRate = 0f;
        }
        else if (playerLevel == 5)
        {
            weakRate = 0.4f; mediumRate = 0.5f; strongRate = 0.1f;
        }
        else if (playerLevel <= 7)
        {
            weakRate = 0.3f; mediumRate = 0.5f; strongRate = 0.2f;
        }
        else if (playerLevel <= 9)
        {
            weakRate = 0.2f; mediumRate = 0.5f; strongRate = 0.3f;
        }
        else if (playerLevel <= 12)
        {
            weakRate = 0.1f; mediumRate = 0.4f; strongRate = 0.5f;
        }
        else
        {
            weakRate = 0.1f; mediumRate = 0.3f; strongRate = 0.6f;
        }

        Debug.Log($"[SpawnBot] Level {playerLevel} → Yếu: {weakRate:P0}, Trung: {mediumRate:P0}, Mạnh: {strongRate:P0}");

        for (int i = 0; i < totalSpawnPoints; i++)
        {
            float roll = Random.value;
            int botLevel;

            if (roll < weakRate)
                botLevel = 0;
            else if (roll < weakRate + mediumRate)
                botLevel = 1;
            else
                botLevel = 2;

            SpawnBot(spawnGroup.spawnPoints[i], player.GetComponent<PlayerInfo>(), botLevel);
        }


        int total = totalSpawnPoints + 1; // 1 player chính
        FindObjectOfType<KillInfoUIHandler>()?.Init(total);
    }

    private void SpawnBot(Transform spawnPoint, PlayerInfo playerInfo, int botLevel)
    {
        GameObject botPrefab = botPrefabs[Random.Range(0, botPrefabs.Count)];
        GameObject bot = Instantiate(botPrefab, spawnPoint.position, spawnPoint.rotation);
        bot.transform.SetParent(botContainer.transform);

        var botStats = bot.GetComponent<BotStats>();
        if (botStats != null)
        {
            InitBotStats(botStats, playerInfo, botLevel);
        }
    }

    private void InitBotStats(BotStats botStats, PlayerInfo playerInfo, int botLevel)
    {
        int playerHP = playerInfo._hpMax;
        int playerDamage = playerInfo.baseDamage;

        float hpMultiplier, dmgMultiplier;

        switch (botLevel)
        {
            case 0: // yếu
                hpMultiplier = Random.Range(0.8f, 1.0f);
                dmgMultiplier = Random.Range(0.7f, 0.9f);
                break;
            case 1: // trung bình
                hpMultiplier = Random.Range(1.0f, 1.3f);
                dmgMultiplier = Random.Range(0.9f, 1.1f);
                break;
            case 2: // mạnh
                hpMultiplier = Random.Range(1.3f, 1.6f);
                dmgMultiplier = Random.Range(1.1f, 1.4f);
                break;
            default:
                hpMultiplier = 1f;
                dmgMultiplier = 1f;
                break;
        }

        int botHP = Mathf.RoundToInt(playerHP * hpMultiplier);
        int botDamage = Mathf.RoundToInt(playerDamage * dmgMultiplier);

        botStats.maxHP = botHP;
        botStats.currentHP = botHP;
        botStats.baseDamage = botDamage;

        botStats.maxMana = 100;
        botStats.currentMana = 100;
    }


}
