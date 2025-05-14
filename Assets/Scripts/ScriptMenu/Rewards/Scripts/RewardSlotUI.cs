using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSlotUI : MonoBehaviour {
    public Image icon;
    public TMP_Text amountText;
    public GameObject lockIcon;
    public GameObject checkIcon;
    public Image imageBg;
    public Image imageBorder;

    public void Setup(RewardData data, bool isUnlocked) {
        icon.sprite = data.icon;
        amountText.text = data.quantity.ToString();
        lockIcon.SetActive(!isUnlocked);
        checkIcon.SetActive(false); 
        
        // Gán nền
        if (data.background != null)
            imageBg.sprite = data.background;

        // Gán border
        if (data.border != null)
            imageBorder.sprite = data.border;
    }
}
