using UnityEngine;

public class DailyTaskBridge : MonoBehaviour
{
    public static DailyTaskBridge Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // sống xuyên scenes
        }
        else Destroy(gameObject);
    }

    public void TryAddProgress(string taskId, int amount = 1)
    {
        DailyTaskProgressManager.Instance?.AddProgress(taskId, amount);

        // Nếu đang ở scene có UI (Home), thì cập nhật UI luôn
        if (DailyTaskManager.Instance != null)
        {
            DailyTaskManager.Instance.RefreshAllTasksUI();
        }
    }
}
