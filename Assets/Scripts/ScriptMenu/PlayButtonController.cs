// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class PlayButtonController : MonoBehaviour
// {
//     [SerializeField] private string homeScene = "Scenes_Home_Game";
//     [SerializeField] private string battleScene = "LayoutBattle";
//     public void OnClickPlay()
//     {
//         SceneManager.LoadScene(battleScene, LoadSceneMode.Single);
//     }

//     public void BackHome()
//     {
//         SceneManager.LoadScene(homeScene, LoadSceneMode.Single);
//     }

// }


using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayButtonController : MonoBehaviour
{
    [SerializeField] private string homeScene = "Scenes_Home_Game";
    [SerializeField] private string battleScene = "LayoutBattle";
    [SerializeField] private float delayBeforeLoad = 1.5f; // thời gian chờ sau khi show ads

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
        if (adManager != null && adManager.HasInterstitialReady())
        {
            adManager.ShowInterstitialAd(() => {
                SceneManager.LoadScene(homeScene, LoadSceneMode.Single); // chỉ chuyển khi ads xong
            });
        }
        else
        {
            SceneManager.LoadScene(homeScene, LoadSceneMode.Single);
        }
    }
    public void ExitBattle()
    {
        if (adManager != null && adManager.HasInterstitialReady())
        {
            adManager.ShowInterstitialAd(() => {
                TriggerForcedEnd(); // ← xử lý kết quả trước khi chuyển scene
            });
        }
        else
        {
            TriggerForcedEnd();
        }
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
