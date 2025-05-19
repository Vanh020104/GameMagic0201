using UnityEngine;

[CreateAssetMenu(fileName = "EquipDatabase", menuName = "Equip/Create Equip Database")]
public class EquipDatabaseSO : ScriptableObject
{
    public EquipItemSO[] allItems;

    public EquipItemSO[] GetByType(EquipType type)
    {
        return System.Array.FindAll(allItems, e => e.type == type);
    }
}
