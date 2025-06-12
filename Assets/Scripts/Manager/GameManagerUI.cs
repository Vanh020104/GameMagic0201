using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerUI : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;
    public GameObject mapPrefab;
    public List<GameObject> botPrefabs;

    private GameObject botContainer;
    private GameObject player;
    void Start()
    {
        GameObject map;

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
                player = Instantiate(loaded, spawnPoint.position, Quaternion.identity);
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
            // fallback nếu chưa chọn hero
            player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
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
        }

        int total = totalSpawnPoints + 1; // 1 player chính
        FindObjectOfType<KillInfoUIHandler>()?.Init(total);
    }
}
