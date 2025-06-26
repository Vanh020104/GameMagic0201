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
    [SerializeField] private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917";      // TEST rewarded
    [SerializeField] private string interstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712";  // TEST interstitial

    private Action onRewardedCallback;
    public static AdManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(this.gameObject);
    }


    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob Initialized");
            // LoadBannerAd();
            LoadRewardedAd();
            LoadInterstitialAd();
        });
        InvokeRepeating(nameof(EnsureRewardedAdReady), 5f, 10f);
    }

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

    // public void ShowRewardedAd(Action onRewarded)
    // {
    //     if (rewardedAd != null && rewardedAd.CanShowAd())
    //     {
    //         onRewardedCallback = onRewarded;

    //         rewardedAd.Show((GoogleMobileAds.Api.Reward reward) =>
    //         {
    //             onRewardedCallback?.Invoke();
    //         });
    //     }
    //     else
    //     {
    //         NotificationPopupUI.Instance.Show("Check your internet!", false);
    //         Debug.Log("Rewarded ad not ready");
    //     }
    // }
    public void ShowRewardedAd(Action onRewarded)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("❌ Không có kết nối mạng — không thể hiển thị quảng cáo.");
            // Tùy hệ thống popup của mày, có thể gọi PopupManager.Instance.Show(...)
            ShowNoInternetPopup();
            return;
        }

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
            // Cũng có thể hiện popup "Không có quảng cáo sẵn sàng"
            ShowAdNotReadyPopup();
        }
    }
    private void ShowNoInternetPopup()
    {
        NotificationPopupUI.Instance.Show("No network connection!", false);
    }

    private void ShowAdNotReadyPopup()
    {
        NotificationPopupUI.Instance.Show("Ad not ready!", false);
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

    // Gọi trong Start() hoặc sau khi người chơi xem xong:

    private void EnsureRewardedAdReady()
    {
        if (rewardedAd == null || !rewardedAd.CanShowAd())
        {
            Debug.Log("🔁 Re-loading rewarded ad due to not ready");
            LoadRewardedAd();
        }
    }
    public bool HasRewardedAdReady()
    {
        return rewardedAd != null && rewardedAd.CanShowAd();
    }

}
