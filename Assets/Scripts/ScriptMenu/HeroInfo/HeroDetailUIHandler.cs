using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroDetailUIHandler : MonoBehaviour
{
    public static HeroDetailUIHandler Instance;

    public Image bigHeroImage;
    public TMP_Text nameText;
    public TMP_Text healthText, damageText, speedText, levelText;
    public Slider levelSlider;
    public TMP_Text upgradeCostText;
    public Button upgradeButton;
    public TMP_Text[] skillNames;
    public Image[] skillIcons;

    private HeroData currentData;
    private HeroPlayerData currentPlayerHero;

    public TMP_Text levelTopLeftText;


    private void Awake() => Instance = this;

    private void Start()
    {
        upgradeButton.onClick.AddListener(OnUpgradeClicked);

        string savedHeroId = PlayerPrefs.GetString("SelectedHeroId", "");
        if (!string.IsNullOrEmpty(savedHeroId))
        {
            HeroData selected = HeroManager.Instance.GetHeroById(savedHeroId);
            if (selected != null)
                ShowHero(selected);
        }
    }

    public void ShowHero(HeroData data)
    {
        currentData = data;
        int savedLevel = PlayerPrefs.GetInt($"HeroLevel_{data.heroId}", data.defaultLevel);
        currentPlayerHero = new HeroPlayerData(data.heroId, savedLevel);

        bigHeroImage.sprite = data.heroIcon;
        nameText.text = data.heroName;
        healthText.text = data.baseHealth.ToString();
        damageText.text = data.baseDamage.ToString();
        speedText.text = data.baseSpeed.ToString();

        levelText.text = $"{currentPlayerHero.currentLevel}/{data.maxLevel}";
        levelSlider.maxValue = data.maxLevel;
        levelSlider.value = currentPlayerHero.currentLevel;
        levelTopLeftText.text = currentPlayerHero.currentLevel.ToString();

        for (int i = 0; i < skillIcons.Length; i++)
        {
            skillIcons[i].sprite = data.skills[i].skillIcon;
            skillNames[i].text = data.skills[i].skillName;
        }

        upgradeCostText.text = GetUpgradeCost(currentPlayerHero.currentLevel).ToString();
    }

    private int GetUpgradeCost(int level)
    {
        return 700 + (int)Mathf.Pow(level, 2) * 100;
    }

    private void OnUpgradeClicked()
    {
        if (currentPlayerHero.currentLevel >= currentData.maxLevel)
        {
            Debug.Log("Hero đã đạt cấp tối đa");
            return;
        }

        int cost = GetUpgradeCost(currentPlayerHero.currentLevel);
        if (GoldGemManager.Instance != null && GoldGemManager.Instance.SpendGold(cost))
        {
            currentPlayerHero.currentLevel++;
            PlayerPrefs.SetInt($"HeroLevel_{currentData.heroId}", currentPlayerHero.currentLevel);
            PlayerPrefs.Save();


            // Cập nhật UI các HeroItem dưới danh sách
            HeroUIItem[] allItems = FindObjectsOfType<HeroUIItem>();
            foreach (var item in allItems)
            {
                if (item != null && item.GetHeroId() == currentData.heroId)
                {
                    item.RefreshLevelDisplay();
                    break;
                }
            }

            ShowHero(currentData);
        }
        else
        {
            Debug.Log("Không đủ vàng để nâng cấp");
        }
    }
}
