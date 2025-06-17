// using System.Collections;
// using UnityEngine;

// public class NotificationTrigger : MonoBehaviour
// {
//     public GameObject badgeIcon;
//     public string notificationKey; // Ví dụ: "mission", "reward", "bag", "lucky"

//     private IEnumerator Start()
//     {
//         yield return null;

//         NotificationBadgeManager.Instance.RegisterTrigger(notificationKey, this);
//         bool state = NotificationBadgeManager.Instance.IsNotificationActive(notificationKey);
//         ShowBadge(state);
//     }


//     public void ShowBadge(bool show)
//     {
//         if (badgeIcon != null)
//             badgeIcon.SetActive(show);
//         else
//             Debug.LogWarning($"[NotificationTrigger] badgeIcon is NULL on {name}");
           
//     }

// }

using System.Collections;
using UnityEngine;

public class NotificationTrigger : MonoBehaviour
{
    public GameObject badgeIcon;
    public string notificationKey;

    private void OnDestroy()
    {
        if (NotificationBadgeManager.Instance != null)
        {
            NotificationBadgeManager.Instance.UnregisterTrigger(notificationKey, this);
        }
    }

    private IEnumerator Start()
    {
        yield return null;

        if (NotificationBadgeManager.Instance != null)
        {
            NotificationBadgeManager.Instance.RegisterTrigger(notificationKey, this);
            bool state = NotificationBadgeManager.Instance.IsNotificationActive(notificationKey);
            ShowBadge(state);
        }
    }

    public void ShowBadge(bool show)
    {
        if (badgeIcon != null)
        {
            badgeIcon.SetActive(show);
        }
        else
        {
            Debug.LogWarning($"[NotificationTrigger] badgeIcon is NULL on GameObject: {gameObject.name} | Path: {GetFullPath(transform)}");
        }
    }

    private string GetFullPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetFullPath(t.parent) + "/" + t.name;
    }
}
