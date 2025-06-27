using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;
   
public class DailyTaskItemUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text descriptionText;
    public TMP_Text goldText;
    public TMP_Text energyText;
    public TMP_Text progressText;
    public Button goButton;

    private DailyTaskData data;
    private EnergyBarManager energyBar;

    private int currentProgress;
    private bool isClaimed;
    private bool isReadyToClaim;
    public bool IsReadyToClaim() => !isClaimed && currentProgress >= data.requiredCount;
    public string GetTaskId() => data.id;
    private LocalizedString localizedDescription;

    public void Setup(DailyTaskData task, EnergyBarManager bar)
    {
        data = task;
        energyBar = bar;

        icon.sprite = data.icon;
        localizedDescription = new LocalizedString("LanguageVanh", data.descriptionKey);
        localizedDescription.StringChanged += (localizedValue) =>
        {
            if (descriptionText != null)
                descriptionText.text = localizedValue;
        };

        goldText.text = data.goldReward.ToString();
        energyText.text = data.energyReward.ToString();

        isClaimed = PlayerPrefs.GetInt($"DailyTaskClaimed_{data.id}", 0) == 1;

        UpdateUI();

        goButton.onClick.RemoveAllListeners();
        goButton.onClick.AddListener(OnClick);
    }


    /// <summary>
    /// Gọi từ bên ngoài khi người chơi làm được 1 tiến trình
    /// </summary>
    public void AddProgress(int amount)
    {
        if (isClaimed) return;

        currentProgress = Mathf.Min(currentProgress + amount, data.requiredCount);

        // 👉 Lưu lại
        PlayerPrefs.SetInt($"DailyTaskProgress_{data.id}", currentProgress);
        PlayerPrefs.Save();

        if (currentProgress >= data.requiredCount)
        {
            isReadyToClaim = true;
        }

        UpdateUI();
        DailyTaskManager.Instance.CheckAndUpdateNotificationBadge();

    }


    private void OnClick()
    {
        if (!isReadyToClaim || isClaimed) return;

        // Nhận phần thưởng
        isClaimed = true;
        isReadyToClaim = false;

        GoldGemManager.Instance.AddGold(data.goldReward);
        energyBar.AddEnergy(data.energyReward);
        PlayerPrefs.SetInt($"DailyTaskClaimed_{data.id}", 1);
        PlayerPrefs.Save();
        UpdateUI();

        DailyTaskManager.Instance.CheckAndUpdateNotificationBadge();

    }

    private void UpdateUI()
    {
        currentProgress = DailyTaskProgressManager.Instance.GetProgress(data.id, data.requiredCount);
        progressText.text = $"{currentProgress} / {data.requiredCount}";
        if (isClaimed)
        {
            SetButton("DONE", Color.gray, false);
        }
        else if (currentProgress >= data.requiredCount)
        {
            isReadyToClaim = true;
            Color receiveColor = new Color32(0, 255, 113, 255);
            SetButton("Receive", receiveColor, true);
        }
        else
        {
            SetButton("GO", Color.gray, false);
        }

        // ✅ Luôn gán lại listener sau khi SetButton xong
        goButton.onClick.RemoveAllListeners();
        goButton.onClick.AddListener(OnClick);
    }



    private void SetButton(string label, Color bgColor, bool interactable)
    {
        var text = goButton.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = label;

        var image = goButton.GetComponent<Image>();
        if (image != null) image.color = bgColor;

        goButton.interactable = interactable;
    }

    public void RefreshUIManually()
    {
        UpdateUI();
    }

    private bool CheckAnyUnclaimedTasks()
    {
        foreach (var ui in DailyTaskManager.Instance.GetCurrentTaskUIs())
        {
            if (ui.IsReadyToClaim()) return true;
        }
        return false;
    }

}
