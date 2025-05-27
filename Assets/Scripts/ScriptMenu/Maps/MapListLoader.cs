using UnityEngine;

public class MapListLoader : MonoBehaviour
{
    [SerializeField] private MapDatabase database;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject mapItemPrefab;

    private void Start()
    {
        foreach (var map in database.allMaps)
        {
            var item = Instantiate(mapItemPrefab, contentParent);
            item.GetComponent<MapUIItem>().Setup(map);
        }
    }
}
