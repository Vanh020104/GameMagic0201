using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("FX")]
    public GameObject smokeFxInstance;

    private Coroutine smokeRoutine;



    private void Awake() => Instance = this;

    private void Start()
    {
        upgradeButton.onClick.AddListener(OnUpgradeClicked);

        string savedHeroId = PlayerPrefs.GetString("SelectedHeroId", "");
        
        // ✅ Nếu chưa có Hero được chọn → chọn hero đầu tiên
        if (string.IsNullOrEmpty(savedHeroId))
        {
            if (HeroManager.Instance.allHeroes.Count > 0)
            {
                HeroData firstHero = HeroManager.Instance.allHeroes[0];
                savedHeroId = firstHero.heroId;
                PlayerPrefs.SetString("SelectedHeroId", savedHeroId);
                PlayerPrefs.Save();
            }
        }

        // ✅ Hiển thị Hero đã chọn (dù là mới chọn hay đã lưu trước đó)
        HeroData selected = HeroManager.Instance.GetHeroById(savedHeroId);
        if (selected != null)
        {
            ShowHero(selected);
        }
    }


    public void ShowHero(HeroData data)
    {
        if (smokeFxInstance != null)
        {
            if (smokeRoutine != null)
                StopCoroutine(smokeRoutine);

            smokeRoutine = StartCoroutine(PlaySmoke());
        }
            
        currentData = data;
        int savedLevel = PlayerPrefs.GetInt($"HeroLevel_{data.heroId}", data.defaultLevel);
        currentPlayerHero = new HeroPlayerData(data.heroId, savedLevel);

        bigHeroImage.sprite = data.heroIcon;
        nameText.text = data.heroName;

        int savedHealth = PlayerPrefs.GetInt($"HeroHealth_{data.heroId}", data.baseHealth);
        int savedDamage = PlayerPrefs.GetInt($"HeroDamage_{data.heroId}", data.baseDamage);
        int savedSpeed  = PlayerPrefs.GetInt($"HeroSpeed_{data.heroId}", data.baseSpeed);

        healthText.text = savedHealth.ToString();
        damageText.text = savedDamage.ToString();
        speedText.text = savedSpeed.ToString();

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

        // Ẩn nút nâng cấp nếu đạt max level
        if (currentPlayerHero.currentLevel >= currentData.maxLevel)
        {
            upgradeButton.gameObject.SetActive(false);
        }
        else
        {
            upgradeButton.gameObject.SetActive(true);
        }

    }
private IEnumerator PlaySmoke()
{
    smokeFxInstance.SetActive(true); // bật lên

    // CHẮC CHẮN phát lại ParticleSystem
    var ps = smokeFxInstance.GetComponent<ParticleSystem>();
    if (ps != null)
    {
        ps.Clear(); // Xoá hạt cũ
        ps.Play();  // Phát lại
    }

    yield return new WaitForSeconds(2f);
    smokeFxInstance.SetActive(false); // ẩn sau khi xong
}



    private int GetUpgradeCost(int level)
    {
        return 500 + (int)Mathf.Pow(level, 2) * 100;
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

            int currentHealth = PlayerPrefs.GetInt($"HeroHealth_{currentData.heroId}", currentData.baseHealth);
            int currentDamage = PlayerPrefs.GetInt($"HeroDamage_{currentData.heroId}", currentData.baseDamage);
            int currentSpeed  = PlayerPrefs.GetInt($"HeroSpeed_{currentData.heroId}",  currentData.baseSpeed);

            int healthBonus = Random.Range(3, 6); // 3–5
            int damageBonus = Random.Range(0, 3); // 0–2
            int speedBonus  = Random.Range(0, 4); // 0–3

            currentHealth += healthBonus;
            currentDamage += damageBonus;
            currentSpeed  += speedBonus;

            PlayerPrefs.SetInt($"HeroLevel_{currentData.heroId}", currentPlayerHero.currentLevel);
            PlayerPrefs.SetInt($"HeroHealth_{currentData.heroId}", currentHealth);
            PlayerPrefs.SetInt($"HeroDamage_{currentData.heroId}", currentDamage);
            PlayerPrefs.SetInt($"HeroSpeed_{currentData.heroId}", currentSpeed);
            PlayerPrefs.Save();

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
            var home = FindObjectOfType<HomeUIController>();
            if(home != null){
                home.OpenPanelBuyGold();
            }
        }
    }
}