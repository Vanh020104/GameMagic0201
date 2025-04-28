using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;

    void Start()
    {
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        Camera.main.GetComponent<CameraFollow>().target = player.transform;
    }
}
