using UnityEngine;
using static NotificationPopupUI;

public class HeroListBuilder : MonoBehaviour
{
    public Transform contentParent;
    public GameObject heroItemPrefab;

    void Start()
    {
        foreach (var hero in HeroManager.Instance.allHeroes)
        {
            bool isUnlocked = hero.heroId == "Hero01" || PlayerPrefs.GetInt($"HeroUnlocked_{hero.heroId}", 0) == 1;
            if (!isUnlocked) continue;

            GameObject go = Instantiate(heroItemPrefab, contentParent);
            go.GetComponent<HeroUIItem>().Setup(hero);
        }
    }
    
    private void OnEnable()
    {
        HeroEvents.OnHeroBought += OnHeroBought;
    }

    private void OnDisable()
    {
        HeroEvents.OnHeroBought -= OnHeroBought;
    }

    private void OnHeroBought(string heroId)
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);
        Start(); // Load lại danh sách
    }
}
