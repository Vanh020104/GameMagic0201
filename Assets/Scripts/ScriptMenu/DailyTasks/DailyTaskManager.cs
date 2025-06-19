using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DailyTaskManager : MonoBehaviour
{
    public DailyTaskDatabase database;
    public GameObject itemDailyPrefab;
    public Transform contentParent;
    public EnergyBarManager energyBar;

    private const int MaxDailyTasks = 7;
    private const string DailyKey = "SelectedDailyTasks";
    private const string DateKey = "DailyDate";
    private List<DailyTaskItemUI> currentTaskUIs = new();
    public static DailyTaskManager Instance;
    public List<DailyTaskItemUI> GetCurrentTaskUIs() => currentTaskUIs;

    void Start()
    {
        InitTasks();
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    public bool IsTaskActive(string taskId)
    {
        return currentTaskUIs.Any(ui => ui != null && ui.GetTaskId() == taskId);
    }

    private void InitTasks()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");
        string savedDate = PlayerPrefs.GetString(DateKey, "");

        if (savedDate != today)
        {
            ResetDailyTasks(); // ✨ dùng hàm tách riêng
        }
        else
        {
            LoadExistingTasks(); // ✨ load lại UI nếu chưa cần reset
        }
    }

    public void CheckAndUpdateNotificationBadge()
    {
        bool hasUnclaimed = currentTaskUIs.Any(t => t.IsReadyToClaim());
        NotificationBadgeManager.Instance.SetNotification("mission", hasUnclaimed);
    }


    private List<DailyTaskData> GetTodayTasks()
    {
        if (PlayerPrefs.HasKey(DailyKey))
        {
            string[] ids = PlayerPrefs.GetString(DailyKey).Split(',');
            return database.tasks.Where(t => ids.Contains(t.id)).ToList();
        }
        else
        {
            var random = database.tasks.OrderBy(t => Guid.NewGuid()).Take(MaxDailyTasks).ToList();
            PlayerPrefs.SetString(DailyKey, string.Join(",", random.Select(t => t.id)));
            PlayerPrefs.Save();
            return random;
        }
    }

    public void RefreshAllTasksUI()
    {
        foreach (var ui in currentTaskUIs)
        {
            ui.RefreshUIManually();
        }
        CheckAndUpdateNotificationBadge();
    }
    public void ResetDailyTasks()
    {
        PlayerPrefs.DeleteKey(DailyKey);
        PlayerPrefs.SetString(DateKey, DateTime.Now.ToString("yyyyMMdd"));
        energyBar.ResetEnergyAndKeyProgress();
        DailyTaskProgressManager.Instance.ResetAllProgress();

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
        currentTaskUIs.Clear();

        List<DailyTaskData> selectedTasks = GetTodayTasks();
        foreach (var task in selectedTasks)
        {
            var go = Instantiate(itemDailyPrefab, contentParent);
            var ui = go.GetComponent<DailyTaskItemUI>();
            ui.Setup(task, energyBar);
            currentTaskUIs.Add(ui);
        }

        CheckAndUpdateNotificationBadge();
    }
    private void LoadExistingTasks()
    {
        currentTaskUIs.Clear();
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        List<DailyTaskData> selectedTasks = GetTodayTasks();
        foreach (var task in selectedTasks)
        {
            var go = Instantiate(itemDailyPrefab, contentParent);
            var ui = go.GetComponent<DailyTaskItemUI>();
            ui.Setup(task, energyBar);
            currentTaskUIs.Add(ui);
        }

        CheckAndUpdateNotificationBadge();
    }
    public void TryAddProgress(string taskId, int amount = 1)
    {
        if (IsTaskActive(taskId))
        {
            DailyTaskProgressManager.Instance.AddProgress(taskId, amount);
        }
    }

}
