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

    private void InitTasks()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");
        string savedDate = PlayerPrefs.GetString(DateKey, "");

        if (savedDate != today)
        {
            PlayerPrefs.DeleteKey(DailyKey);
            PlayerPrefs.SetString(DateKey, today);
            energyBar.ResetEnergyAndKeyProgress();
            DailyTaskProgressManager.Instance.ResetAllProgress();
        }

        List<DailyTaskData> selectedTasks = GetTodayTasks();
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var task in selectedTasks)
        {
            var go = Instantiate(itemDailyPrefab, contentParent);
            var ui = go.GetComponent<DailyTaskItemUI>();
            ui.Setup(task, energyBar);
            currentTaskUIs.Add(ui);
        }
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
            ui.RefreshUIManually(); // tạo mới hàm này trong DailyTaskItemUI
        }
    }

}
