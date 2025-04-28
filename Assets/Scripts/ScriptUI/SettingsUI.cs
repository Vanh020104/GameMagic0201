using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ResumeGame()
    {
        settingsPanel.SetActive(false);
    }

    public void QuitMatch()
    {
        Debug.Log("Thoát trận - thêm code scene sau");
    }

    public void OpenOptions()
    {
        Debug.Log("Mở menu cài đặt chi tiết");
    }
}
