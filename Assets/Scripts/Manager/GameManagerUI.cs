using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerUI : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;
    public GameObject mapPrefab;
    public List<GameObject> botPrefabs;


    [Header("Spawn Bot")]
    public int totalCharacter = 20;
    public Vector2 mapSize = new Vector2(200f, 200f);
    private Transform botContainer;
    void Start()
    {
        Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        Camera.main.GetComponent<CameraFollow>().target = player.transform;

// Tạo object chứa bot
        GameObject botRoot = new GameObject("BotContainer");
        botContainer = botRoot.transform;

        // Spawn 3 bot chính
        foreach (var bot in botPrefabs)
        {
            Vector3 pos = GetRandomSpawnPosition();
            GameObject newBot = Instantiate(bot, pos, Quaternion.identity);
            newBot.transform.SetParent(botContainer);
        }

        // Spawn thêm bot random
        int remainingBotCount = totalCharacter - 1 - botPrefabs.Count;
        for (int i = 0; i < remainingBotCount; i++)
        {
            int rand = Random.Range(0, botPrefabs.Count);
            Vector3 pos = GetRandomSpawnPosition();
            GameObject newBot = Instantiate(botPrefabs[rand], pos, Quaternion.identity);
            newBot.transform.SetParent(botContainer);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-mapSize.x / 2, mapSize.x / 2);
        float z = Random.Range(-mapSize.y / 2, mapSize.y / 2);
        return new Vector3(x, 0, z);
    }
}