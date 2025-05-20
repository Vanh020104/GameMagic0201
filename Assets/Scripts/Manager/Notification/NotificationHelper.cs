using UnityEngine;

public static class NotificationHelper
{
    /// <summary>
    /// Hiển thị thông báo với màu mặc định (thành công).
    /// </summary>
    public static void Show(string message, float duration = 2f)
    {
        Show(message, true, duration);
    }

    /// <summary>
    /// Hiển thị thông báo với trạng thái (true = thành công, false = lỗi).
    /// </summary>
    public static void Show(string message, bool isSuccess, float duration = 2f)
    {
        var instance = Object.FindObjectOfType<NotificationPopupUI>();
        if (instance != null)
        {
            instance.Show(message, isSuccess, duration);
        }
        else
        {
            Debug.LogWarning("⚠️ Không tìm thấy NotificationPopupUI trong scene!");
        }
    }
}
