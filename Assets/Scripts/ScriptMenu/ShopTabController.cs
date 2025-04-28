using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopTabController : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button button;         
        public GameObject content;  
        public Image buttonImage;   
    }

    [SerializeField] private List<Tab> tabs = new List<Tab>();

    [Header("Button Sprites")]
    public Sprite selectedSprite;
    public Sprite normalSprite;   

    [Header("Button Scale Settings")]
    public Vector3 selectedScale = new Vector3(1.2f, 1f, 1f); 
    public Vector3 normalScale = Vector3.one;          

    private int currentTabIndex = 0;

    private void Start()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            tabs[i].button.onClick.AddListener(() => OnTabClicked(index));
        }
        UpdateTabUI();
    }

    private void OnTabClicked(int index)
    {
        currentTabIndex = index;
        UpdateTabUI();
    }

    private void UpdateTabUI()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            bool isSelected = (i == currentTabIndex);
            tabs[i].content.SetActive(isSelected);
            if (tabs[i].buttonImage != null)
            {
                tabs[i].buttonImage.sprite = isSelected ? selectedSprite : normalSprite;
            }
            tabs[i].button.transform.localScale = isSelected ? selectedScale : normalScale;
        }
    }
    public void SelectTab(int index)
    {
        if (index >= 0 && index < tabs.Count)
        {
            currentTabIndex = index;
            UpdateTabUI();
        }
    }

}
