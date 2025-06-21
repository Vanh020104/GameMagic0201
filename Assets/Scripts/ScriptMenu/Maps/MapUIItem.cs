using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapUIItem : MonoBehaviour
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private Button playButton;

    [SerializeField] private GameObject panelBlock;  
    [SerializeField] private TextMeshProUGUI numberConfirmText;
    private MapData currentMap;

    public void Setup(MapData data)
    {
        currentMap = data;
        thumbnail.sprite = data.thumbnail;
        mapNameText.text = data.mapName;

        int playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1); 

        bool isUnlocked = playerLevel >= data.requiredLevel;
        panelBlock.SetActive(!isUnlocked);

        if (numberConfirmText != null)
            numberConfirmText.text = $"Must be level {data.requiredLevel} to unlock!";

        playButton.interactable = isUnlocked;
        playButton.onClick.RemoveAllListeners();

        if (isUnlocked)
            playButton.onClick.AddListener(OnPlayClicked);
        else
            playButton.onClick.AddListener(() => AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxClick));
    }

    private void OnPlayClicked()
    {
        GameData.SelectedMap = currentMap;
        string selectedHeroId = PlayerPrefs.GetString("SelectedHeroId", "");
        if (!string.IsNullOrEmpty(selectedHeroId))
        {
            HeroData selectedHero = HeroManager.Instance.GetHeroById(selectedHeroId);
            GameData.SelectedHero = selectedHero;
            Debug.Log("✔️ Hero đã chọn: " + selectedHero?.heroName + " - " + selectedHero?.prefabPath);
        }


        if (GlobalLoadingController.Instance != null)
        {
            GlobalLoadingController.Instance.LoadSceneWithDelay("LayoutBattle", 2f);
        }
        else
        {

            UnityEngine.SceneManagement.SceneManager.LoadScene("LayoutBattle");
        }
    }

}
