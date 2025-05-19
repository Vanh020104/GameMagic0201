using UnityEngine;

[CreateAssetMenu(fileName = "EquipItem", menuName = "Equip/Create New Equip Item")]
public class EquipItemSO : ScriptableObject
{
    public string itemId;
    public string itemName;
    public EquipType type;
    public Sprite icon;

    [Header("Level Settings")]
    public int baseLevel = 1;
    public int maxLevel = 10;

    [Header("Damage Settings")]
    public int baseDamage = 10;
    public int maxDamage = 50;
}
