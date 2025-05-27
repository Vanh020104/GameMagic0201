using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "Game/Map Data")]
public class MapData : ScriptableObject
{
    public string mapId;
    public string mapName;
    public Sprite thumbnail;

    [Header("Path trong Resources (ví dụ: Prefabs/GameBattle/Maps/Map_01)")]
    public string prefabPath;
}
