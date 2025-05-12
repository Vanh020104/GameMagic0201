using TMPro;
using UnityEngine;

public class HomeUIController : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private ShopTabController shopTabController; 
    [SerializeField] private GameObject settingPanel;
    // goi mở thanh rename
    [SerializeField] private GameObject renamePanel;

    // goi mở Player details
    [SerializeField] private GameObject playerDetails;

    // dong mo gift special
    [SerializeField] private GameObject gift;

    // goi dong mo daily tasks
    [SerializeField] private GameObject dailyTasks;
    [SerializeField] private GameObject panelBuyGold;
    [SerializeField] private GameObject welcomeRewardPanel;
    [SerializeField] private TMP_Text playerNameText;


    // gọi nhạc ở đâyđây
    private void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmHome);
        string name = PlayerPrefs.GetString("PlayerName", "PlayerXXXX");
        playerNameText.text = name;

        settingPanel.SetActive(false);
        renamePanel.SetActive(false);
        playerDetails.SetActive(false);
        gift.SetActive(false);
        dailyTasks.SetActive(false);
        panelBuyGold.SetActive(false);
    }
    private void Awake()
    {
        if (!PlayerPrefs.HasKey("IsFirstLogin"))
        {
            PlayerPrefs.SetInt("Gold", 500);
            PlayerPrefs.SetInt("Gem", 20);
            PlayerPrefs.SetInt("IsFirstLogin", 1);
            int randomNum = Random.Range(1000, 9999);
            string randomName = "Player" + randomNum;
            PlayerPrefs.SetString("PlayerName", randomName);
            PlayerPrefs.Save();

            if (welcomeRewardPanel != null)
                welcomeRewardPanel.SetActive(true);
        }
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

    // goi dong mo playerDetails

    public void OpenPlayerDetails(){
        playerDetails.SetActive(true);
    }

    public void ClosePlayerDetails(){
        playerDetails.SetActive(false);
    }

    // dong mo Gift Special
    public void OpenGiftSpecial(){
        gift.SetActive(true);
    }
    public void CloseGiftSpecial(){
        gift.SetActive(false);
    }

    // dong mo daily tasks
    public void OpenDailyTasks(){
        dailyTasks.SetActive(true);
    }

    public void CloseDailyTasks(){
        dailyTasks.SetActive(false);
    }
     // dong mo panelBuyGold
    public void OpenPanelBuyGold(){
        panelBuyGold.SetActive(true);
    }

    public void ClosePanelBuyGold(){
        panelBuyGold.SetActive(false);
    }
    public void CloseWellcomReward(){
        welcomeRewardPanel.SetActive(false);
    }
}
