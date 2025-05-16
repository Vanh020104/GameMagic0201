using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShopCategoryData", menuName = "Shop/Category")]
public class ShopCategoryData : ScriptableObject
{
    public List<ShopItemData> items;
}
