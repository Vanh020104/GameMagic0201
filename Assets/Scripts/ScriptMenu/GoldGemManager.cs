using TMPro;
using UnityEngine;

public class GoldGemManager : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemText;

    private int goldAmount;
    private int gemAmount;

    private void Start()
    {
        LoadCurrencies();
        UpdateUI();
    }

    private void LoadCurrencies()
    {
        goldAmount = PlayerPrefs.GetInt("Gold", 0);
        gemAmount = PlayerPrefs.GetInt("Gem", 0);
    }

    private void SaveCurrencies()
    {
        PlayerPrefs.SetInt("Gold", goldAmount);
        PlayerPrefs.SetInt("Gem", gemAmount);
    }

    private void UpdateUI()
    {
        goldText.text = goldAmount.ToString("N0");
        gemText.text = gemAmount.ToString("N0");
    }


    public void AddGold(int amount)
    {
        goldAmount += amount;
        UpdateUI();
        SaveCurrencies();
    }

    public void AddGem(int amount)
    {
        gemAmount += amount;
        UpdateUI();
        SaveCurrencies();
    }
}
