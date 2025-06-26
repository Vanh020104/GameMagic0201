using TMPro;
using UnityEngine;

public class GoldGemManager : MonoBehaviour
{
    public static GoldGemManager Instance;

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemText;
    public TextMeshProUGUI keyText;

    private int goldAmount;
    private int gemAmount;
    private int keyAmount;
    public static event System.Action OnCurrencyChanged;

    private void NotifyCurrencyChange()
    {
        OnCurrencyChanged?.Invoke();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        LoadCurrencies();
        UpdateUI();
    }

    private void LoadCurrencies()
    {
        goldAmount = PlayerPrefs.GetInt("Gold", 0);
        gemAmount = PlayerPrefs.GetInt("Gem", 0);
        keyAmount = PlayerPrefs.GetInt("LuckyKey", 0);
    }

    private void SaveCurrencies()
    {
        PlayerPrefs.SetInt("Gold", goldAmount);
        PlayerPrefs.SetInt("Gem", gemAmount);
        PlayerPrefs.SetInt("LuckyKey", keyAmount);
    }

    private void UpdateUI()
    {
        goldText.text = goldAmount.ToString("N0");
        gemText.text = gemAmount.ToString("N0");
        keyText.text = keyAmount.ToString("N0");
    }

    public void AddGold(int amount)
    {
        goldAmount += amount;
        if (AudioManager.Instance != null && AudioManager.Instance.sfxGoldGain != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxGoldGain);
        }

        Debug.Log($" AddGold called: +{amount} → {goldAmount}");
        UpdateUI();
        SaveCurrencies();
        NotifyCurrencyChange();
    }

    public void AddGem(int amount)
    {
        gemAmount += amount;
        if (AudioManager.Instance != null && AudioManager.Instance.sfxGoldGain != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxGoldGain);
        }

        Debug.Log($"💎 AddGem called: +{amount} → {gemAmount}");
        UpdateUI();
        SaveCurrencies();
        NotifyCurrencyChange();

    }

    public bool SpendGold(int amount)
    {
        if (goldAmount >= amount)
        {
            goldAmount -= amount;
            UpdateUI();
            SaveCurrencies();
            return true;
        }
        return false;
    }
    public bool SpendGem(int amount)
    {
        if (gemAmount >= amount)
        {
            gemAmount -= amount;
            UpdateUI();
            SaveCurrencies();
            return true;
        }
        return false;
    }
    public void AddKey(int amount)
    {
        keyAmount += amount;
        Debug.Log($"🔑 AddKey called: +{amount} → {keyAmount}");
        UpdateUI();
        SaveCurrencies();
        NotifyCurrencyChange();
    }

    public bool SpendKey(int amount)
    {
        if (keyAmount >= amount)
        {
            keyAmount -= amount;
            UpdateUI();
            SaveCurrencies();
            NotifyCurrencyChange();
            return true;
        }
        return false;
    }

    public int GetKey() => keyAmount;


    public int GetGold() => goldAmount;
    public int GetGem() => gemAmount;

}
