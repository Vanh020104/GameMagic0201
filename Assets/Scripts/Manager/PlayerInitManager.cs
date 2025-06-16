using UnityEngine;

public class PlayerInitManager : MonoBehaviour
{
    public EquipDatabaseSO equipDatabase;


    void Start()
    {
        // PlayerPrefs.DeleteAll();
        // PlayerPrefs.Save();
        // PlayerPrefs.DeleteKey("IsFirstLogin");
        // PlayerPrefs.Save();

    }
    private void Awake()
    {   
        if (!PlayerPrefs.HasKey("IsFirstLogin"))
        {
            InitPlayer();
        }
    }

    private void InitPlayer()
    {
        // Name
        int randomNum = Random.Range(1000, 9999);
        PlayerPrefs.SetString("PlayerName", "Player" + randomNum);

        // Level & EXP
        int level = 1;
        int exp = Mathf.FloorToInt(GetRequiredExp(level) * 0.3f);
        PlayerPrefs.SetInt("PlayerLevel", level);
        PlayerPrefs.SetInt("PlayerExp", exp);
        
        // Currency
        PlayerPrefs.SetInt("Gold", 500);
        PlayerPrefs.SetInt("Gem", 20);
        PlayerPrefs.SetInt("LuckyKey", 3);
        // Rank
        PlayerPrefs.SetInt("PlayerRankIndex", 0);
        PlayerPrefs.SetInt("PlayerRankExp", 50);

        // Equip mặc định (mỗi loại lấy item đầu tiên)
        foreach (EquipType type in System.Enum.GetValues(typeof(EquipType)))
        {
            var list = equipDatabase.GetByType(type);
            if (list.Length > 0)
            {
                var item = list[0];
                string id = item.itemId;
                PlayerPrefs.SetInt($"Equip_{id}_Unlocked", 1);
                PlayerPrefs.SetInt($"Equip_{id}_Level", item.baseLevel);
                PlayerPrefs.SetInt($"Equip_{id}_Damage", item.baseDamage);

                // Ghi nhớ là item đang được gắn
                PlayerPrefs.SetString($"Equipped_{type}", id);
            }
        }

        PlayerPrefs.SetInt("IsFirstLogin", 1);
        PlayerPrefs.Save();
    }

    private int GetRequiredExp(int level)
    {
        return Mathf.FloorToInt(100 * Mathf.Pow(level, 1.5f));
    }
}
