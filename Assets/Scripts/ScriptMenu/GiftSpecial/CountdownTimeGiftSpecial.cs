using System;
using TMPro;
using UnityEngine;

public class CountdownTimeGiftSpecial : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI gemText;
    public TextMeshProUGUI originalPriceText;
    public TextMeshProUGUI offerPriceText;

    [Header("Random Countdown Time (seconds)")]
    public float minSeconds = 3600f;    // 1h
    public float maxSeconds = 18000f;   // 5h

    private DateTime endTime;

    private int currentGold;
    private int currentGem;
    private int originalPrice;
    private int offerPrice;

    void Start()
    {
        if (PlayerPrefs.HasKey("CountdownEndTime"))
        {
            long binaryTime = Convert.ToInt64(PlayerPrefs.GetString("CountdownEndTime"));
            endTime = DateTime.FromBinary(binaryTime);

            currentGold = PlayerPrefs.GetInt("GiftGold", 0);
            currentGem = PlayerPrefs.GetInt("GiftGem", 0);
            originalPrice = PlayerPrefs.GetInt("GiftOriginalPrice", 0);
            offerPrice = PlayerPrefs.GetInt("GiftOfferPrice", 0);
        }
        else
        {
            GenerateNewRewardCycle();
        }

        UpdateRewardUI();
    }


    void Update()
    {
        TimeSpan remaining = endTime - DateTime.Now;

        if (remaining.TotalSeconds <= 0)
        {
            GenerateNewRewardCycle();
            remaining = endTime - DateTime.Now;
        }

        infoText.text = string.Format("<i>{0}d {1}h{2:D2}'{3:D2}\" remaining</i>",
            remaining.Days, remaining.Hours, remaining.Minutes, remaining.Seconds);
    }

    void GenerateNewRewardCycle()
    {
        // Countdown
        float randomTime = UnityEngine.Random.Range(minSeconds, maxSeconds);
        endTime = DateTime.Now.AddSeconds(randomTime);
        PlayerPrefs.SetString("CountdownEndTime", endTime.ToBinary().ToString());

        // Rewards
        currentGold = UnityEngine.Random.Range(3000, 6001);
        currentGem = UnityEngine.Random.Range(30, 61);  
        originalPrice = UnityEngine.Random.Range(40, 61);    
        offerPrice = Mathf.RoundToInt(originalPrice / 2f);   

        // Save
        PlayerPrefs.SetInt("GiftGold", currentGold);
        PlayerPrefs.SetInt("GiftGem", currentGem);
        PlayerPrefs.SetInt("GiftOriginalPrice", originalPrice);
        PlayerPrefs.SetInt("GiftOfferPrice", offerPrice);
        PlayerPrefs.Save();

        UpdateRewardUI();
    }

    void UpdateRewardUI()
    {
        goldText.text = $"<color=red>{currentGold}</color>";
        gemText.text = $"<color=red>{currentGem}</color>";
        originalPriceText.text = $"<s>Original Price: {originalPrice}$</s>";
        offerPriceText.text = $"<color=red><b>Opffer Price: {offerPrice}$</b></color>";
    }
}
