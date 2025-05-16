using TMPro;
using UnityEngine;

public class NotificationPopupUI : MonoBehaviour
{
    public TMP_Text messageText;

    public void Show(string message, float duration = 2f)
    {
        gameObject.SetActive(true);
        messageText.text = message;
        CancelInvoke();
        Invoke(nameof(Hide), duration);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
