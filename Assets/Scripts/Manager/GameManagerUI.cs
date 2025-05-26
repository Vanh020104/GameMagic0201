using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerUI : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;
    public GameObject mapPrefab;
    public List<GameObject> botPrefabs;



    void Start()
    {
        GameObject map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        Camera.main.GetComponent<CameraFollow>().target = player.transform;

        // Tìm BotSpawnPointGroup trong map
        BotSpawnPointGroup spawnGroup = map.GetComponentInChildren<BotSpawnPointGroup>();
        if (spawnGroup == null || spawnGroup.spawnPoints.Length == 0)
        {
            Debug.LogWarning("⚠️ Không tìm thấy điểm spawn bot!");
            return;
        }

        int totalSpawnPoints = spawnGroup.spawnPoints.Length;

        // Spawn bot tại mỗi điểm, dùng prefab luân phiên
        for (int i = 0; i < totalSpawnPoints; i++)
        {
            GameObject botPrefab = botPrefabs[i % botPrefabs.Count]; // quay vòng 3 prefab
            Transform spawnPos = spawnGroup.spawnPoints[i];

            Instantiate(botPrefab, spawnPos.position, spawnPos.rotation);
        }
    }
}