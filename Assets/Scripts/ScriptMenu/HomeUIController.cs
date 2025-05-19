using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý UI màn hình chính: tên người chơi, level, exp, panel shop, setting, rename, v.v.
/// </summary>
public class HomeUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private ShopTabController shopTabController;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject renamePanel;
    [SerializeField] private GameObject playerDetails;
    [SerializeField] private GameObject gift;
    [SerializeField] private GameObject dailyTasks;
    [SerializeField] private GameObject panelBuyGold;
    [SerializeField] private GameObject welcomeRewardPanel;

    [Header("UI Texts & Slider")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text levelNumberText;
    [SerializeField] private Slider levelSlider;
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private NotificationPopupUI notificationPopup;
    [SerializeField] private GameObject luckyPanel;

    [SerializeField] private GameObject rankingPanel;

    [Header("UI Rank Panel - Player Info")]
    [SerializeField] private TMP_Text playerNameInRankPanel;
    [SerializeField] private TMP_Text levelTextInRankPanel;
    [SerializeField] private Slider expSliderInRankPanel;

    [SerializeField] private GameObject bagPanel;
    [SerializeField] private GameObject upgradePanel;



    /// <summary>
    /// Khởi tạo dữ liệu mặc định cho người chơi mới (chỉ chạy 1 lần).
    /// </summary>
    private void Awake()
    {
        if (!PlayerPrefs.HasKey("IsFirstLogin"))
        {
            // PlayerPrefs.SetInt("Gold", 500);
            // PlayerPrefs.SetInt("Gem", 20);
            // PlayerPrefs.SetInt("IsFirstLogin", 1);

            // int randomNum = Random.Range(1000, 9999);
            // string randomName = "Player" + randomNum;
            // PlayerPrefs.SetString("PlayerName", randomName);

            // int startingLevel = 1;
            // int giftExp = Mathf.FloorToInt(GetRequiredExp(startingLevel) * 0.3f); // 🎁 Tặng 30% exp ban đầu
            // PlayerPrefs.SetInt("PlayerLevel", startingLevel);
            // PlayerPrefs.SetInt("PlayerExp", giftExp);

            // PlayerPrefs.Save();

            if (welcomeRewardPanel != null)
                welcomeRewardPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Khởi động UI khi vào scene.
    /// </summary>
    private void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmHome);

        string name = PlayerPrefs.GetString("PlayerName", "PlayerXXXX");
        playerNameText.text = name;

        UpdateLevelUI();
        UpdateRankPanelUI();
        // Tắt các panel mặc định
        settingPanel.SetActive(false);
        renamePanel.SetActive(false);
        playerDetails.SetActive(false);
        gift.SetActive(false);
        dailyTasks.SetActive(false);
        panelBuyGold.SetActive(false);
        luckyPanel.SetActive(false);
        rankingPanel.SetActive(false);
        bagPanel.SetActive(false);
        upgradePanel.SetActive(false);
    }

    /// <summary>
    /// Cập nhật UI level + thanh exp người chơi.
    /// </summary>
    private void UpdateLevelUI()
    {
        int level = PlayerPrefs.GetInt("PlayerLevel", 1);
        int exp = PlayerPrefs.GetInt("PlayerExp", 0);
        int requiredExp = GetRequiredExp(level);

        levelNumberText.text = level.ToString();
        levelSlider.maxValue = requiredExp;
        levelSlider.value = exp;
    }

    /// <summary>
    /// Tính số EXP cần thiết để lên level tiếp theo.
    /// </summary>
    private int GetRequiredExp(int level)
    {
        return Mathf.FloorToInt(100 * Mathf.Pow(level, 1.5f));
    }

    ///
    /// code rename
    /// 
    /// 

    public void ConfirmRename()
    {
        string newName = usernameInputField.text.Trim();

        if (string.IsNullOrEmpty(newName))
        {
            notificationPopup.Show("Name cannot be blank!");
            return;
        }

        if (newName.Length < 1 || newName.Length > 10)
        {
            notificationPopup.Show("Name must be between 1 and 10 characters!");
            return;
        }

        PlayerPrefs.SetString("PlayerName", newName);
        PlayerPrefs.Save();
        // notificationPopup.Show("Rename successful!");

        playerNameText.text = newName;
        UpdateRankPanelUI();
        CloseRename();
    }


    // hien thong tin player trong details
    void UpdateRankPanelUI()
    {
        int level = PlayerPrefs.GetInt("PlayerLevel", 1);
        int exp = PlayerPrefs.GetInt("PlayerExp", 0);
        int requiredExp = GetRequiredExp(level);
        string name = PlayerPrefs.GetString("PlayerName", "PlayerXXXX");

        if (playerNameInRankPanel != null)
            playerNameInRankPanel.text = name;

        if (levelTextInRankPanel != null)
            levelTextInRankPanel.text = level.ToString();

        if (expSliderInRankPanel != null)
        {
            expSliderInRankPanel.maxValue = requiredExp;
            expSliderInRankPanel.value = exp;
        }
    }

    // lên level 
    public void AddExp(int amount)
    {
        int level = PlayerPrefs.GetInt("PlayerLevel", 1);
        int exp = PlayerPrefs.GetInt("PlayerExp", 0);

        exp += amount;

        while (exp >= GetRequiredExp(level))
        {
            exp -= GetRequiredExp(level);
            level++;
        }

        PlayerPrefs.SetInt("PlayerLevel", level);
        PlayerPrefs.SetInt("PlayerExp", exp);
        PlayerPrefs.Save();

        UpdateLevelUI();         // 🔁 cập nhật ở UI chính
        UpdateRankPanelUI();     // 🔁 cập nhật chỗ ảnh (rank panel)
    }



    /// <summary> Mở shop </summary>
    public void OpenShop() => shopPanel.SetActive(true);

    /// <summary> Mở shop và chọn tab cụ thể </summary>
    public void OpenShopAndSelectTab(int tabIndex)
    {
        shopPanel.SetActive(true);
        shopTabController.SelectTab(tabIndex);
    }

    /// <summary> Đóng shop </summary>
    public void CloseShop() => shopPanel.SetActive(false);

    /// <summary> Mở setting </summary>
    public void OpenSetting() => settingPanel.SetActive(true);

    /// <summary> Đóng setting </summary>
    public void CloseSetting() => settingPanel.SetActive(false);

    /// <summary> Mở rename panel </summary>
    public void OpenRename() => renamePanel.SetActive(true);

    /// <summary> Đóng rename panel </summary>
    public void CloseRename() => renamePanel.SetActive(false);

    /// <summary> Mở chi tiết người chơi </summary>
    public void OpenPlayerDetails() => playerDetails.SetActive(true);

    /// <summary> Đóng chi tiết người chơi </summary>
    public void ClosePlayerDetails() => playerDetails.SetActive(false);

    /// <summary> Mở gift panel </summary>
    public void OpenGiftSpecial() => gift.SetActive(true);

    /// <summary> Đóng gift panel </summary>
    public void CloseGiftSpecial() => gift.SetActive(false);

    /// <summary> Mở daily tasks </summary>
    public void OpenDailyTasks() => dailyTasks.SetActive(true);

    /// <summary> Đóng daily tasks </summary>
    public void CloseDailyTasks() => dailyTasks.SetActive(false);

    /// <summary> Mở panel mua vàng </summary>
    public void OpenPanelBuyGold() => panelBuyGold.SetActive(true);

    /// <summary> Đóng panel mua vàng </summary>
    public void ClosePanelBuyGold() => panelBuyGold.SetActive(false);

    /// <summary> Đóng popup quà chào mừng </summary>
    public void CloseWellcomReward() => welcomeRewardPanel.SetActive(false);

    /// mở lucky panel
    public void OpenLuckPanel() => luckyPanel.SetActive(true);

    /// dong lucky panel
    public void CloseLuckPanel() => luckyPanel.SetActive(false);

    public void OpenRankingPanel() => rankingPanel.SetActive(true);
    public void CloseRankingPanel() => rankingPanel.SetActive(false);

    public void OpenBagPanel() => bagPanel.SetActive(true);
    public void CloseBagPanel() => bagPanel.SetActive(false);
    public void OpenUpgradePanel() => upgradePanel.SetActive(true);
    public void CloseUpgradePanel() => upgradePanel.SetActive(false);
}
