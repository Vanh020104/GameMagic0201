using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ShopCategory
{
    public string name;
    public ShopCategoryData data;
    public GameObject prefab;
    public Transform parent;
}

public class ShopLoader : MonoBehaviour
{
    public List<ShopCategory> categories;

    private void Start()
    {
        foreach (var category in categories)
        {
            foreach (Transform child in category.parent)
                Destroy(child.gameObject);

            foreach (var item in category.data.items)
            {
                GameObject go = Instantiate(category.prefab, category.parent);
                go.GetComponent<ShopItemUI>().Setup(item);
            }
        }
    }
}
