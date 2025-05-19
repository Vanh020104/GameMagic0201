using System;
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


    // tự động refesh sau khi chọn hero và bagbag
    public static class HeroEvents
    {
        public static Action<string> OnHeroSelected;
    }


    public static class BagEvent
    {
        public static event System.Action OnItemBought;

        public static void InvokeItemBought()
        {
            OnItemBought?.Invoke();
        }
    }

}
