using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationPopupUI : MonoBehaviour
{
    public static NotificationPopupUI Instance; 

    public TMP_Text messageText;
    public Image backgroundImage;
    public Sprite successSprite;
    public Sprite errorSprite;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameObject.SetActive(false); 
    }

    public void Show(string message, bool isSuccess = true, float duration = 2f)
    {
        gameObject.SetActive(true);
        messageText.text = message;

        if (backgroundImage != null)
        {
            backgroundImage.sprite = isSuccess ? successSprite : errorSprite;
        }

        CancelInvoke();
        Invoke(nameof(Hide), duration);
    }



    private void Hide()
    {
        gameObject.SetActive(false);
    }

    public static class HeroEvents
    {
        public static Action<string> OnHeroSelected;
        public static Action<string> OnHeroBought;
    }

    public static class BagEvent
    {
        public static event Action OnItemBought;

        public static void InvokeItemBought()
        {
            OnItemBought?.Invoke();
        }
    }
}
