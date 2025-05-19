using UnityEngine;
using UnityEngine.UI;

public class EquipItemUI : MonoBehaviour
{
    public Image icon;
    public GameObject selectedFrame; 

    private EquipItemSO itemData;
    private System.Action<EquipItemSO, EquipItemUI> onClick;

    public void Setup(EquipItemSO data, System.Action<EquipItemSO, EquipItemUI> callback)
    {
        itemData = data;
        icon.sprite = data.icon;
        onClick = callback;
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke(itemData, this));

        SetSelected(false); // reset khi init
    }

    public void SetSelected(bool isSelected)
    {
        if (selectedFrame != null)
            selectedFrame.SetActive(isSelected);
    }
}
