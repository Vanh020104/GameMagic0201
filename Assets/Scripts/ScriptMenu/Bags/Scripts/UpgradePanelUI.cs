using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanelUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text itemNameText;
    public TMP_Text damageText;
    public TMP_Text levelText;
    public TMP_Text upgradeCostText;
    public Image itemIcon;
    public Slider progressSlider;
    public Button upgradeButton;

    private EquipItemSO itemData;
    private int currentLevel;
    private int currentDamage;

    [SerializeField] private GameObject panelBuyGold;

    public void Setup(EquipItemSO data)
    {
        itemData = data;
        currentLevel = PlayerPrefs.GetInt($"Equip_{data.itemId}_Level", data.baseLevel);
        UpdateUI();
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnClickUpgrade);
    }

    private void UpdateUI()
    {
        itemNameText.text = itemData.itemName;
        itemIcon.sprite = itemData.icon;

        currentLevel = Mathf.Clamp(currentLevel, itemData.baseLevel, itemData.maxLevel);
        levelText.text = currentLevel.ToString();

        float t = (currentLevel - itemData.baseLevel) / (float)(itemData.maxLevel - itemData.baseLevel);
        currentDamage = Mathf.RoundToInt(Mathf.Lerp(itemData.baseDamage, itemData.maxDamage, t));
        damageText.text = $"Damage {currentDamage}";
        progressSlider.value = t;

        if (currentLevel >= itemData.maxLevel)
        {
            upgradeCostText.text = "Max";
            upgradeButton.interactable = false;
        }
        else
        {
            int cost = CalculateUpgradeCost(currentLevel);
            upgradeCostText.text = $"{cost:N0}";
            upgradeButton.interactable = true;
        }
    }

    private int CalculateUpgradeCost(int level)
    {
        return 500 + (level - itemData.baseLevel) * 1000;
    }

    private void OnClickUpgrade()
    {
        int cost = CalculateUpgradeCost(currentLevel);
        if (!GoldGemManager.Instance.SpendGold(cost))
        {
            Debug.Log("❌ Không đủ vàng để nâng cấp!");

            if (panelBuyGold != null)
                panelBuyGold.SetActive(true); 

            return;
        }

        currentLevel++;
        PlayerPrefs.SetInt($"Equip_{itemData.itemId}_Level", currentLevel);
        PlayerPrefs.Save();
        DailyTaskProgressManager.Instance.AddProgress("upgrade_equip");
        UpdateUI();

        var bag = FindObjectOfType<BagPanelController>();
        if (bag != null)
        {
            bag.ForceRefreshUI(itemData);
        }
    }

}