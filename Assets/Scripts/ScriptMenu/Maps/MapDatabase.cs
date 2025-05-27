using UnityEngine;

[CreateAssetMenu(fileName = "MapDatabase", menuName = "Game/Map Database")]
public class MapDatabase : ScriptableObject
{
    public MapData[] allMaps;
}
