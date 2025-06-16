using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class ExpSpawner : MonoBehaviour
{
    public GameObject expGemPrefab;
    public int initialGemCount = 300;
    public Vector2 mapSize = new Vector2(100, 100);
    public float height = 1.5f;
    public Transform gemContainer;

    private List<GameObject> activeGems = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < initialGemCount; i++)
        {
            SpawnRandomGem();
        }
    }

    public void SpawnRandomGem()
    {
        for (int i = 0; i < 10; i++) // thử tối đa 10 lần
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-mapSize.x / 2, mapSize.x / 2),
                height,
                Random.Range(-mapSize.y / 2, mapSize.y / 2)
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPos, out hit, 2f, NavMesh.AllAreas))
            {
                Vector3 spawnPos = hit.position + Vector3.up * 0.2f;
                GameObject gem = Instantiate(expGemPrefab, spawnPos, Quaternion.identity, gemContainer);
                gem.GetComponent<ExpGem>().Init(this);
                activeGems.Add(gem);
                return;
            }
        }

        Debug.LogWarning("❌ Không tìm được vị trí hợp lệ trên NavMesh để spawn gem.");
    }

    public void RespawnAfterDelay(float delay)
    {
        StartCoroutine(RespawnCoroutine(delay));
    }

    private IEnumerator RespawnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnRandomGem();
    }
}
