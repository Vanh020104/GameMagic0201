using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static NotificationPopupUI;

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

    [SerializeField] private TMP_Text tabTitleText;

    private EquipType currentTab;
    private Dictionary<EquipType, EquipItemSO> equippedItems = new();
    private EquipItemUI currentSelectedUI;
    [SerializeField] private UpgradePanelUI upgradePanelUI;
    [SerializeField] private GameObject upgradePanelGO;

    private void Start()
    {
        foreach (EquipType type in System.Enum.GetValues(typeof(EquipType)))
        {
            string selectedId = PlayerPrefs.GetString($"Equip_{type}_SelectedId", "");
            var list = database.GetByType(type);

            EquipItemSO selectedItem = null;
            foreach (var item in list)
            {
                if (item.itemId == selectedId)
                {
                    selectedItem = item;
                    break;
                }
            }

            if (selectedItem == null && list.Length > 0)
            {
                selectedItem = list[0];
                PlayerPrefs.SetString($"Equip_{type}_SelectedId", selectedItem.itemId);
                PlayerPrefs.SetInt($"Equip_{selectedItem.itemId}_Unlocked", 1);
                PlayerPrefs.SetInt($"Equip_{selectedItem.itemId}_Level", selectedItem.baseLevel);
                PlayerPrefs.Save();
            }

            if (selectedItem != null)
            {
                equippedItems[type] = selectedItem;
                SetSlotImage(type, selectedItem.icon);
            }
        }

        ShowEquipTab((int)EquipType.Armor);
        UpdateTotalEquipDamage();
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
        string selectedItemId = PlayerPrefs.GetString($"Equip_{currentTab}_SelectedId", "");
        EquipItemUI toSelectUI = null;

        foreach (var item in list)
        {
            if (PlayerPrefs.GetInt($"Equip_{item.itemId}_Unlocked", 0) == 1)
            {
                var go = Instantiate(itemPrefab, contentParent);
                var ui = go.GetComponent<EquipItemUI>();
                ui.Setup(item, OnItemSelected);

                if (item.itemId == selectedItemId)
                    toSelectUI = ui;
            }
        }

        if (toSelectUI != null)
        {
            OnItemSelected(toSelectUI.GetData(), toSelectUI);
        }
    }

    private void OnItemSelected(EquipItemSO data, EquipItemUI ui)
    {
        int level = PlayerPrefs.GetInt($"Equip_{data.itemId}_Level", data.baseLevel);
        float t = (level - data.baseLevel) / (float)(data.maxLevel - data.baseLevel);
        int damage = Mathf.RoundToInt(Mathf.Lerp(data.baseDamage, data.maxDamage, t));

        itemIconDisplay.sprite = data.icon;
        itemNameText.text = data.itemName;
        itemLevelText.text = $"Level {level}";
        itemDamageText.text = $"Damage {damage}";

        equippedItems[data.type] = data;
        SetSlotImage(data.type, data.icon);

        PlayerPrefs.SetString($"Equip_{data.type}_SelectedId", data.itemId);
        PlayerPrefs.Save();

        if (currentSelectedUI != null)
            currentSelectedUI.SetSelected(false);

        currentSelectedUI = ui;
        currentSelectedUI.SetSelected(true);

        UpdateTotalEquipDamage();
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

    private void UpdateTotalEquipDamage()
    {
        int total = 0;
        foreach (var kvp in equippedItems)
        {
            EquipItemSO item = kvp.Value;
            int level = PlayerPrefs.GetInt($"Equip_{item.itemId}_Level", item.baseLevel);
            float t = (level - item.baseLevel) / (float)(item.maxLevel - item.baseLevel);
            int damage = Mathf.RoundToInt(Mathf.Lerp(item.baseDamage, item.maxDamage, t));
            total += damage;
        }

        PlayerPrefs.SetInt("TotalEquipDamage", total);
        PlayerPrefs.Save();
    }

    private void OnEnable()
    {
        BagEvent.OnItemBought += OnItemBought;
    }

    private void OnDisable()
    {
        BagEvent.OnItemBought -= OnItemBought;
    }

    private void OnItemBought()
    {
        UpdateList();
    }

    public void OnClickOpenUpgrade()
    {
        if (currentSelectedUI == null) return;
        EquipItemSO selectedItem = currentSelectedUI.GetData();
        upgradePanelGO.SetActive(true);
        upgradePanelUI.Setup(selectedItem);
    }

    public void ForceRefreshUI(EquipItemSO data)
    {
        if (currentSelectedUI != null && currentSelectedUI.GetData().itemId == data.itemId)
        {
            OnItemSelected(data, currentSelectedUI);
        }

        UpdateTotalEquipDamage();
    }

    public void OnClickArmorSlot() => ShowEquipTab((int)EquipType.Armor);
    public void OnClickHatSlot() => ShowEquipTab((int)EquipType.Hat);
    public void OnClickBootSlot() => ShowEquipTab((int)EquipType.Boots);
}
