using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NotificationPopupUI;

public class HeroUIItem : MonoBehaviour
{
    public Image icon;
    public Button button;
    public TMP_Text levelText;

    private HeroData heroData;

    public void Setup(HeroData data)
    {
        heroData = data;
        icon.sprite = data.heroIcon;
        RefreshLevelDisplay();

        button.onClick.AddListener(OnClick);
    }

    public void RefreshLevelDisplay()
    {
        int savedLevel = PlayerPrefs.GetInt($"HeroLevel_{heroData.heroId}", heroData.defaultLevel);
        levelText.text = savedLevel.ToString();
    }

    void OnClick()
    {
        PlayerPrefs.SetString("SelectedHeroId", heroData.heroId);
        PlayerPrefs.Save(); 
        HeroEvents.OnHeroSelected?.Invoke(heroData.heroId);


        HeroDetailUIHandler.Instance?.ShowHero(heroData);

        // 👉 Gọi cập nhật ảnh player ngoài màn Home
        var updater = FindObjectOfType<PlayerImageUpdater>();
        if (updater != null)
        updater.UpdateImage();

        // update skill
        var skillDisplay = FindObjectOfType<PlayerImageUpdater>();
        if (skillDisplay != null) skillDisplay.UpdateSkillIcons();
    }

    public string GetHeroId()
    {
        return heroData.heroId;
    }
}