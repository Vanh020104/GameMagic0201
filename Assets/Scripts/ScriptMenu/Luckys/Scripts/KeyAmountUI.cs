using TMPro;
using UnityEngine;

public class KeyAmountUI : MonoBehaviour
{
    [SerializeField] private TMP_Text keyText;

    private void OnEnable()
    {
        UpdateUI(); // cập nhật lần đầu
        NotificationPopupUI.KeyEvent.OnKeyChanged += UpdateUI;
    }

    private void OnDisable()
    {
        NotificationPopupUI.KeyEvent.OnKeyChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        int key = PlayerPrefs.GetInt("LuckyKey", 0);
        keyText.text = key.ToString();
    }
}
