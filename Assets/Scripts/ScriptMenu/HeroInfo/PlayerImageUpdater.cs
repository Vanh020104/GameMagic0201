using UnityEngine;
using UnityEngine.UI;

public class PlayerImageUpdater : MonoBehaviour
{
    public Image playerImage; // Gán PlayerImage
    private string currentHeroId;

    void Start()
    {
        UpdateImage();
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
}
