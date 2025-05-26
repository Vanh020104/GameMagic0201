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

    void Start()
    {
        GameObject map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
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
