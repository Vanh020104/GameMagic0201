using UnityEngine;
using UnityEngine.UI;
using static NotificationPopupUI;

public class SimpleHeroImageUpdater : MonoBehaviour
{
    [SerializeField] private Image playerImage;

    void Start()
    {
        UpdateImage();
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
    }

    public void UpdateImage()
    {
        string selectedId = PlayerPrefs.GetString("SelectedHeroId", "");
        if (!string.IsNullOrEmpty(selectedId))
        {
            HeroData hero = HeroManager.Instance.GetHeroById(selectedId);
            if (hero != null && hero.heroIcon != null)
            {
                playerImage.sprite = hero.heroIcon;
            }
        }
    }
}
