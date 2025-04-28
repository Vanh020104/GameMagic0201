using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerUI : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;
    public GameObject mapPrefab;


    void Start()
    {
        Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        Camera.main.GetComponent<CameraFollow>().target = player.transform;
            
    }
}
