// using System.Collections.Generic;
// using UnityEngine;

// public class NotificationBadgeManager : MonoBehaviour
// {
//     public static NotificationBadgeManager Instance;

//     private Dictionary<string, NotificationTrigger> triggerMap = new();
//     private HashSet<string> activeKeys = new(); // những key nào đang bật dấu !

//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }


//     public void RegisterTrigger(string key, NotificationTrigger trigger)
//     {
//         triggerMap[key] = trigger;
//         trigger.ShowBadge(activeKeys.Contains(key)); // cập nhật ngay nếu key đã bật
//     }

//     public void SetNotification(string key, bool isActive)
// {
//     Debug.Log($"[NotificationBadgeManager] SetNotification {key} = {isActive}");

//     if (isActive)
//         activeKeys.Add(key);
//     else
//         activeKeys.Remove(key);

//     if (triggerMap.TryGetValue(key, out var trigger))
//     {
//         trigger.ShowBadge(isActive);
//     }
//     else
//     {
//         Debug.LogWarning($"[NotificationBadgeManager] Key {key} chưa được RegisterTrigger!");
//     }
// }


//     public bool IsNotificationActive(string key)
//     {
//         return activeKeys.Contains(key);
//     }
// }

using System.Collections.Generic;
using UnityEngine;

public class NotificationBadgeManager : MonoBehaviour
{
    public static NotificationBadgeManager Instance;

    private Dictionary<string, NotificationTrigger> triggerMap = new();
    private HashSet<string> activeKeys = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterTrigger(string key, NotificationTrigger trigger)
    {
        triggerMap[key] = trigger;
        trigger.ShowBadge(IsNotificationActive(key));
    }

    public void UnregisterTrigger(string key, NotificationTrigger trigger)
    {
        if (triggerMap.TryGetValue(key, out var registeredTrigger) && registeredTrigger == trigger)
        {
            triggerMap.Remove(key);
        }
    }

    public void SetNotification(string key, bool isActive)
    {
        Debug.Log($"[NotificationBadgeManager] SetNotification {key} = {isActive}");

        if (isActive)
            activeKeys.Add(key);
        else
            activeKeys.Remove(key);

        if (triggerMap.TryGetValue(key, out var trigger))
        {
            if (trigger != null)
            {
                trigger.ShowBadge(isActive);
            }
            else
            {
                Debug.LogWarning($"[NotificationBadgeManager] Trigger for {key} is null (destroyed)");
                triggerMap.Remove(key); // dọn rác
            }
        }
        else
        {
            Debug.Log($"[NotificationBadgeManager] Chưa có trigger active cho key {key}, sẽ hiển thị khi Register");
        }
    }

    public bool IsNotificationActive(string key)
    {
        return activeKeys.Contains(key);
    }
}
