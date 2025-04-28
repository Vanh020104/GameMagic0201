using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExpSpawner : MonoBehaviour
{
    public GameObject expGemPrefab;
    public int initialGemCount = 800; 
    public Vector2 mapSize = new Vector2(400, 400);
    public float height = 1.5f;

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
        Vector3 pos = new Vector3(
            Random.Range(-mapSize.x / 2, mapSize.x / 2),
            height,
            Random.Range(-mapSize.y / 2, mapSize.y / 2)
        );

        GameObject gem = Instantiate(expGemPrefab, pos, Quaternion.identity);
        gem.GetComponent<ExpGem>().Init(this);
        activeGems.Add(gem);
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
