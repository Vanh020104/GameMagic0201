using UnityEngine;

public class OpenWatchAdsPanel : MonoBehaviour
{
    [SerializeField] private GameObject watchAdsPanel;

    public void OpenPanel()
    {
        watchAdsPanel.SetActive(true);
         foreach (var btn in watchAdsPanel.GetComponentsInChildren<AdWatchLimitButton>())
        {
            btn.RefreshUI();
        }
    }

    public void ClosePanel()
    {
        watchAdsPanel.SetActive(false);
    }
}
