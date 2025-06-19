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



            GameObject zonePrefab = Resources.Load<GameObject>("ZoneManager");
            if (zonePrefab != null)
            {
                GameObject zone = Instantiate(zonePrefab);

                // dùng bound của collider map
                Collider mapCol = map.GetComponentInChildren<Collider>();
                zone.transform.position = mapCol != null ? mapCol.bounds.center : Vector3.zero;
            }

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

        int totalSpawnPoints = spawnGroup.spawnPoints.Length;

        for (int i = 0; i < totalSpawnPoints; i++)
        {
            GameObject botPrefab = botPrefabs[i % botPrefabs.Count];
            Transform spawnPos = spawnGroup.spawnPoints[i];

            GameObject bot = Instantiate(botPrefab, spawnPos.position, spawnPos.rotation);

            // Đóng bot vào container
            bot.transform.SetParent(botContainer.transform);
            var botStats = bot.GetComponent<BotStats>();
            var playerInfo = player.GetComponent<PlayerInfo>();
            if (botStats != null && playerInfo != null)
            {
                InitBotStats(botStats, playerInfo);
            }
        }

        int total = totalSpawnPoints + 1; // 1 player chính
        FindObjectOfType<KillInfoUIHandler>()?.Init(total);
    }


    private void InitBotStats(BotStats botStats, PlayerInfo playerInfo)
    {
        int playerHP = playerInfo._hpMax;
        int playerDamage = playerInfo.baseDamage;

        int botHP = Mathf.RoundToInt(Random.Range(playerHP * 0.8f, playerHP * 1.2f));
        int botDamage = Mathf.RoundToInt(Random.Range(playerDamage * 0.8f, playerDamage * 1f));
        botStats.maxHP = botHP;
        botStats.currentHP = botHP;
        botStats.baseDamage = botDamage;

        botStats.maxMana = 100;
        botStats.currentMana = 100;
    }

}
