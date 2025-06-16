using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPointGroup : MonoBehaviour
{
    public Transform[] spawnPoints;

    void Awake()
    {
        // Tự lấy các con nếu chưa gán thủ công
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            List<Transform> list = new List<Transform>();
            foreach (Transform child in transform)
            {
                list.Add(child);
            }
            spawnPoints = list.ToArray();
        }
    }

    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
