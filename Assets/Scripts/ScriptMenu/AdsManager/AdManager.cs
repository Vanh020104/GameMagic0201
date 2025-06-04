using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    private BannerView bannerView;
    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;
    private Action onInterstitialClosed;
    [Header("Ad Unit IDs")]
    [SerializeField] private string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";        // TEST banner
    [SerializeField] private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";      // TEST rewarded
    [SerializeField] private string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";  // TEST interstitial

    private Action onRewardedCallback;
    void Awake() => DontDestroyOnLoad(this.gameObject);

    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob Initialized");
            LoadBannerAd();
            LoadRewardedAd();
            LoadInterstitialAd();
        });
    }

    #region Banner
    private void LoadBannerAd()
    {
        if (bannerView != null)
            bannerView.Destroy();

        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        var adRequest = new AdRequest();
        bannerView.LoadAd(adRequest);
    }
    #endregion

    #region Rewarded
    private void LoadRewardedAd()
    {
        var adRequest = new AdRequest();

        RewardedAd.Load(rewardedAdUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Rewarded ad failed: " + error.GetMessage());
                return;
            }

            rewardedAd = ad;

            rewardedAd.OnAdFullScreenContentClosed += () => LoadRewardedAd();
        });
    }

    public void ShowRewardedAd(Action onRewarded)
    {
        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            onRewardedCallback = onRewarded;

            rewardedAd.Show((GoogleMobileAds.Api.Reward reward) =>
            {
                onRewardedCallback?.Invoke();
            });
        }
        else
        {
            Debug.Log("Rewarded ad not ready");
        }
    }
    #endregion

    #region Interstitial
    private void LoadInterstitialAd()
    {
        var adRequest = new AdRequest();

        InterstitialAd.Load(interstitialAdUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.LogError("Interstitial load failed: " + error?.GetMessage());
                return;
            }

            interstitialAd = ad;

            // 👇 CHỈ đăng ký callback MỘT LẦN
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial closed");
                onInterstitialClosed?.Invoke();    // gọi callback nếu có
                onInterstitialClosed = null;       // reset tránh gọi lại lần sau
                LoadInterstitialAd();              // Load lại
            };

            Debug.Log("Interstitial loaded");
        });
    }


    public void ShowInterstitialAd(Action onClosed = null)
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            onInterstitialClosed = onClosed;
            interstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial not ready");
            onClosed?.Invoke(); // fallback nếu không có quảng cáo
        }
    }

    #endregion
    public bool HasInterstitialReady()
    {
        return interstitialAd != null && interstitialAd.CanShowAd();
    }


}
