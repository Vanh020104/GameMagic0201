using UnityEngine;
using UnityEngine.UI;
using static NotificationPopupUI;

public class PlayerImageUpdater : MonoBehaviour
{
    public Image playerImage; // Gán PlayerImage
    private string currentHeroId;


    // đồng bộ skill ra home
    public Image[] skillIcons = new Image[3];

    void Start()
    {
        UpdateImage();
        UpdateSkillIcons();
    }

    public void UpdateImage()
    {
        string selectedId = PlayerPrefs.GetString("SelectedHeroId", "");

        if (!string.IsNullOrEmpty(selectedId))
        {
            HeroData selectedHero = HeroManager.Instance.GetHeroById(selectedId);
            if (selectedHero != null && selectedHero.heroIcon != null)
            {
                playerImage.sprite = selectedHero.heroIcon;
                currentHeroId = selectedHero.heroId;
            }
        }
    }

    public void UpdateSkillIcons()
    {
        string selectedId = PlayerPrefs.GetString("SelectedHeroId", "");
        if (string.IsNullOrEmpty(selectedId)) return;

        HeroData selectedHero = HeroManager.Instance.GetHeroById(selectedId);
        if (selectedHero == null || selectedHero.skills == null) return;

        for (int i = 0; i < skillIcons.Length; i++)
        {
            if (i < selectedHero.skills.Length && selectedHero.skills[i] != null)
            {
                skillIcons[i].sprite = selectedHero.skills[i].skillIcon;
                skillIcons[i].enabled = true;
            }
            else
            {
                skillIcons[i].enabled = false;
            }
        }
    }

    void OnEnable()
    {
        HeroEvents.OnHeroSelected += OnHeroSelected;
    }
    void OnDisable()
    {
        HeroEvents.OnHeroSelected -= OnHeroSelected;
    }

    private void OnHeroSelected(string heroId)
    {
        UpdateImage();
        UpdateSkillIcons();
    }

}
