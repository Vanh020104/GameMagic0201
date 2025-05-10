using UnityEngine;

public class HeroListBuilder : MonoBehaviour
{
    public Transform contentParent;
    public GameObject heroItemPrefab;

    void Start()
    {
        foreach (var hero in HeroManager.Instance.allHeroes)
        {
            GameObject go = Instantiate(heroItemPrefab, contentParent);
            go.GetComponent<HeroUIItem>().Setup(hero);
        }
    }
}
