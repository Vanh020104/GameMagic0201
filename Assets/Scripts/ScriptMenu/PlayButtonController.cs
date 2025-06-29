using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayButtonController : MonoBehaviour
{
    [SerializeField] private string homeScene = "Scenes_Home_Game";
    [SerializeField] private string battleScene = "LayoutBattle";
    [SerializeField] private float delayBeforeLoad = 1.5f; // thời gian chờ sau khi show ads
    public GameObject loadingPanel;
    private AdManager adManager;
    private bool isReturningHome = false;

    private void Start()
    {
        adManager = FindObjectOfType<AdManager>();
    }


    public void OnClickPlay()
    {
        SceneManager.LoadScene(battleScene, LoadSceneMode.Single);
    }

    public void BackHome()
    {
        if (isReturningHome) return; 
        isReturningHome = true;

        StartCoroutine(ShowLoadingThenBackHome());
    }

    private IEnumerator ShowLoadingThenBackHome()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        yield return new WaitForSeconds(delayBeforeLoad);

        // Đếm số lần về Home
        int backHomeCount = PlayerPrefs.GetInt("BackHomeCount", 0);
        backHomeCount++;

        bool shouldShowAd = backHomeCount >= 3;
        PlayerPrefs.SetInt("BackHomeCount", shouldShowAd ? 0 : backHomeCount); // Reset nếu tới ngưỡng

        // Nếu cần hiện quảng cáo
        if (shouldShowAd)
        {
            // Nếu có mạng và quảng cáo sẵn sàng
            if (adManager != null 
                && adManager.HasInterstitialReady()
                && Application.internetReachability != NetworkReachability.NotReachable)
            {
                Debug.Log("📺 Showing interstitial ad before returning home...");

                adManager.ShowInterstitialAd(() =>
                {
                    SceneManager.LoadScene(homeScene, LoadSceneMode.Single);
                    isReturningHome = false;
                });

                yield break;
            }
            else
            {
                Debug.Log("📴 No internet or ad not ready → skipping ad.");
            }
        }

        // Không cần quảng cáo hoặc không có mạng
        SceneManager.LoadScene(homeScene, LoadSceneMode.Single);
        isReturningHome = false;
    }


    public void ExitBattle()
    {
        TriggerForcedEnd();

    }

    private void TriggerForcedEnd()
    {
        BattleEndManager endManager = FindObjectOfType<BattleEndManager>();
        if (endManager != null)
        {
            endManager.ForceEndMatchByQuit();
        }
        else
        {
            Debug.LogWarning("⚠️ Không tìm thấy BattleEndManager. Về thẳng Home.");
            SceneManager.LoadScene(homeScene); // fallback
        }
    }



    private IEnumerator DelayLoadHomeScene()
    {
        yield return new WaitForSeconds(delayBeforeLoad);
        SceneManager.LoadScene(homeScene, LoadSceneMode.Single);
    }
}
