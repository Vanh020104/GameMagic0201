using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public void Setup(DailyTaskData task, EnergyBarManager bar)
    {
        data = task;
        energyBar = bar;

        icon.sprite = data.icon;
        descriptionText.text = data.description;
        goldText.text = data.goldReward.ToString();
        energyText.text = data.energyReward.ToString();

        currentProgress = 0;
        isClaimed = false;
        isReadyToClaim = false;

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
        if (currentProgress >= data.requiredCount)
        {
            isReadyToClaim = true;
        }
        UpdateUI();
    }

    private void OnClick()
    {
        if (!isReadyToClaim || isClaimed) return;

        // Nhận phần thưởng
        isClaimed = true;
        isReadyToClaim = false;

        int gold = PlayerPrefs.GetInt("Gold", 0);
        PlayerPrefs.SetInt("Gold", gold + data.goldReward);
        energyBar.AddEnergy(data.energyReward);

        PlayerPrefs.Save();
        UpdateUI();
    }

    private void UpdateUI()
    {
        progressText.text = $"{currentProgress} / {data.requiredCount}";

        if (isClaimed)
        {
            SetButton("DONE", Color.gray, false);
        }
        else if (isReadyToClaim)
        {
            SetButton("Receive", new Color(0.2f, 0.7f, 0.2f), true); // xanh lá
        }
        else
        {
            SetButton("GO", Color.gray, false);
        }
    }

    private void SetButton(string label, Color bgColor, bool interactable)
    {
        goButton.GetComponentInChildren<TMP_Text>().text = label;
        var image = goButton.GetComponent<Image>();
        if (image != null) image.color = bgColor;
        goButton.interactable = interactable;
    }
}
