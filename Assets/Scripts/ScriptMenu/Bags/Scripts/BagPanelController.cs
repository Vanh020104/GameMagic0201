using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BagPanelController : MonoBehaviour
{
    [Header("Database")]
    public EquipDatabaseSO database;

    [Header("Scroll View")]
    public Transform contentParent;
    public GameObject itemPrefab;

    [Header("Detail Info")]
    public Image itemIconDisplay;
    public TMP_Text itemNameText;
    public TMP_Text itemLevelText;
    public TMP_Text itemDamageText;

    [Header("Equipped Slots")]
    public Image armorSlot;
    public Image hatSlot;
    public Image bootsSlot;

    private EquipType currentTab;
    private Dictionary<EquipType, EquipItemSO> equippedItems = new();
    private EquipItemUI currentSelectedUI;

    [SerializeField] private TMP_Text tabTitleText;

    private void Start()
    {
        foreach (EquipType type in System.Enum.GetValues(typeof(EquipType)))
        {
            var list = database.GetByType(type);
            if (list.Length > 0)
            {
                equippedItems[type] = list[0];
                SetSlotImage(type, list[0].icon);
            }
        }

        ShowEquipTab((int)EquipType.Armor);
    }

    private string GetEquipTypeDisplayName(EquipType type)
    {
        return type switch
        {
            EquipType.Armor => "Chestplate",
            EquipType.Hat => "Helmet",
            EquipType.Boots => "Boots",
            _ => "Equipment"
        };
    }


    public void ShowEquipTab(int typeIndex)
    {
        currentTab = (EquipType)typeIndex;
        UpdateList();
        if (tabTitleText != null)
            tabTitleText.text = GetEquipTypeDisplayName(currentTab);
    }

    private void UpdateList()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        var list = database.GetByType(currentTab);
        EquipItemSO equippedItem = null;
        equippedItems.TryGetValue(currentTab, out equippedItem);
        EquipItemUI toSelectUI = null;

        foreach (var item in list)
        {
            if (PlayerPrefs.GetInt($"Equip_{item.itemId}_Unlocked", 0) == 1)
            {
                var go = Instantiate(itemPrefab, contentParent);
                var ui = go.GetComponent<EquipItemUI>();
                ui.Setup(item, OnItemSelected);

                if (equippedItem != null && item.itemId == equippedItem.itemId)
                {
                    toSelectUI = ui;
                }
            }
        }
        if (equippedItem != null && toSelectUI != null)
        {
            OnItemSelected(equippedItem, toSelectUI);
        }
    }


    private void OnItemSelected(EquipItemSO data, EquipItemUI ui)
    {
        itemIconDisplay.sprite = data.icon;
        itemNameText.text = data.itemName;
        itemLevelText.text = $"Level {data.baseLevel}";
        itemDamageText.text = $"Damage {data.baseDamage}";

        equippedItems[data.type] = data;
        SetSlotImage(data.type, data.icon);

        if (currentSelectedUI != null)
            currentSelectedUI.SetSelected(false);

        currentSelectedUI = ui;
        currentSelectedUI.SetSelected(true);
    }

    private void SetSlotImage(EquipType type, Sprite icon)
    {
        switch (type)
        {
            case EquipType.Armor: armorSlot.sprite = icon; break;
            case EquipType.Hat: hatSlot.sprite = icon; break;
            case EquipType.Boots: bootsSlot.sprite = icon; break;
        }
    }

    public void OnClickArmorSlot() => ShowEquipTab((int)EquipType.Armor);
    public void OnClickHatSlot() => ShowEquipTab((int)EquipType.Hat);
    public void OnClickBootSlot() => ShowEquipTab((int)EquipType.Boots);
}
