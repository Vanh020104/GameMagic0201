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

        foreach (var category in categories)
        {
            foreach (Transform child in category.parent)
                Destroy(child.gameObject);

            int playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);

            // Tách item đã mở và chưa mở
            var sortedItems = category.data.items
            .OrderBy(item =>
            {
                // Ưu tiên item đã mở level
                bool unlocked = playerLevel >= item.requiredLevel;
                return unlocked ? 0 : 1;
            })
            .ThenBy(item =>
            {
                // Ưu tiên: Ads < Key < Mua thường
                if (item.isRewardedAd) return 0;
                if (item.isKeyPurchase) return 1;
                return 2;
            })
            .ThenBy(item =>
            {
                // Nếu cùng loại thì sắp xếp theo giá
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
