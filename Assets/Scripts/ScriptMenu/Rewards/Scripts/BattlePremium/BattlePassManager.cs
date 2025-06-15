using UnityEngine;

public static class BattlePassManager
{
    private const string PurchaseKey = "BattlePassActivated";

    public static bool IsActivated()
    {
        return PlayerPrefs.GetInt(PurchaseKey, 0) == 1;
    }

    public static void Activate()
    {
        PlayerPrefs.SetInt(PurchaseKey, 1);
        PlayerPrefs.Save();
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(PurchaseKey);
        PlayerPrefs.Save();
    }
}
