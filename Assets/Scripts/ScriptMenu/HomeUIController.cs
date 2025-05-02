using UnityEngine;

public class HomeUIController : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private ShopTabController shopTabController; 
    [SerializeField] private GameObject settingPanel;
    // goi mở thanh rename
    [SerializeField] private GameObject renamePanel;

    // gọi nhạc ở đâyđây
    private void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmHome);
        settingPanel.SetActive(false);
        renamePanel.SetActive(false);
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

    // open rename
    public void OpenRename(){
        renamePanel.SetActive(true);
    }

    // close rename
    public void CloseRename(){
        renamePanel.SetActive(false);
    }
}
