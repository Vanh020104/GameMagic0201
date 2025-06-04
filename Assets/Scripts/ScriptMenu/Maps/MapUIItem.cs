using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapUIItem : MonoBehaviour
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private Button playButton;

    private MapData currentMap;

    public void Setup(MapData data)
    {
        currentMap = data;
        thumbnail.sprite = data.thumbnail;
        mapNameText.text = data.mapName;

        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        GameData.SelectedMap = currentMap;

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
