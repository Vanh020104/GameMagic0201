using UnityEngine;

public class HomeUIController : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private ShopTabController shopTabController; 
    [SerializeField] private GameObject settingPanel;

    // gọi nhạc ở đâyđây
    private void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmHome);
        settingPanel.SetActive(false);
    }
                                    
    public void OpenShop()
    {
        shopPanel.SetActive(true);
    }

    public void OpenShopAndSelectTab(int tabIndex)
    {
        shopPanel.SetActive(true);
        shopTabController.SelectTab(tabIndex);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }
    public void OpenSetting()
    {
        settingPanel.SetActive(true); 
    }

    public void CloseSetting()
    {
        settingPanel.SetActive(false); 
    }
}
