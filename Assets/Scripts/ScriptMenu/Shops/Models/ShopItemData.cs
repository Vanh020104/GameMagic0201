using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemData", menuName = "Shop/Item")]
public class ShopItemData : ScriptableObject
{
    [Header("Identification")]
    public string id;

    [Header("Display Info")]
    public string itemName;
    public Sprite icon;
    public int amount;
    public string priceText;
    public int requiredLevel;
    public bool isRewardedAd;
    public bool isKeyPurchase;
}
