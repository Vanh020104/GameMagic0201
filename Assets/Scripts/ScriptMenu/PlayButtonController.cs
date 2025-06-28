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
        StartCoroutine(ShowLoadingThenBackHome());
    }

    private IEnumerator ShowLoadingThenBackHome()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        yield return new WaitForSeconds(delayBeforeLoad);

        // Đọc số lần đã về Home
        int backHomeCount = PlayerPrefs.GetInt("BackHomeCount", 0);
        backHomeCount++;

        if (backHomeCount >= 3)
        {
            PlayerPrefs.SetInt("BackHomeCount", 0); // Reset về 0
            if (adManager != null && adManager.HasInterstitialReady())
            {
                adManager.ShowInterstitialAd(() =>
                {
                    SceneManager.LoadScene(homeScene, LoadSceneMode.Single);
                });
                yield break;
            }
        }
        else
        {
            PlayerPrefs.SetInt("BackHomeCount", backHomeCount); // Lưu lại
        }

        SceneManager.LoadScene(homeScene, LoadSceneMode.Single);
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
