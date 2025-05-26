using System.Collections.Generic;
using UnityEngine;

public class BotSpawnPointGroup : MonoBehaviour
{
    public Transform[] spawnPoints;

    void Awake()
    {
        // Tự động lấy các child làm spawn points nếu chưa gán
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
}
