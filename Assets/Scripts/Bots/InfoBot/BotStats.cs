using UnityEngine;

public class BotStats : MonoBehaviour
{
    public string botName;
    public float maxHP = 700f;
    public float currentHP = 700f;
    public float maxMana = 100f;
    public float currentMana = 100f;
    public int baseDamage = 50;
    public GameObject floatingTextPrefab;
    public Transform popupPoint;
    public bool isDead = false;
    public GameObject healHeartPrefab; 
    public float lastHealTime = -999f;
    private static readonly string[] RandomNames =
    {
        "DarkWolf", "Shadow", "FlameZ", "NoobMaster", "Pro999", "BotDemon", "MagicBoy", "QueenAI", "Zetta", "Raze","Peter",
        "LucShaw", "Mount", "Amad", "Heaven", "Fernamdes", "Cash", "Konsa", "Onana", "Dalot Dialo", "Harry Kante","Vanh 02"
    };

    void Awake()
    {
        botName = RandomNames[Random.Range(0, RandomNames.Length)];
        currentHP = maxHP;
        currentMana = maxMana;
    }
}



