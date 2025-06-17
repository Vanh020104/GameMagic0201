using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        // foreach (var category in categories)
        // {
        //     foreach (Transform child in category.parent)
        //         Destroy(child.gameObject);

        //     foreach (var item in category.data.items)
        //     {
        //         GameObject go = Instantiate(category.prefab, category.parent);
        //         go.GetComponent<ShopItemUI>().Setup(item);
        //     }
        // }

        foreach (var category in categories)
        {
            foreach (Transform child in category.parent)
                Destroy(child.gameObject);

            int playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);

            // Tách item đã mở và chưa mở
            var sortedItems = category.data.items
                .OrderBy(item =>
                {
                    // Ưu tiên: item đã đủ level sẽ có giá trị 0 -> lên đầu
                    // item chưa đủ level sẽ có giá trị 1 -> xuống dưới
                    bool unlocked = playerLevel >= item.requiredLevel;
                    return unlocked ? 0 : 1;
                })
                .ThenBy(item =>
                {
                    // Thêm sắp xếp theo giá nếu cùng trạng thái
                    if (int.TryParse(item.priceText, out int price))
                        return price;
                    return int.MaxValue;
                });

            // Tạo UI
            foreach (var item in sortedItems)
            {
                GameObject go = Instantiate(category.prefab, category.parent);
                go.GetComponent<ShopItemUI>().Setup(item);
            }
        }


    }
}
