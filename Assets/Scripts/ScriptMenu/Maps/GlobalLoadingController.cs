using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalLoadingController : MonoBehaviour
{
    public static GlobalLoadingController Instance;

    [SerializeField] private GameObject loadingPanel;

    private void Awake()
    {
        Instance = this;
        loadingPanel.SetActive(false);
    }

    public void LoadSceneWithDelay(string sceneName, float delay = 2f)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, delay));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, float delay)
    {
        loadingPanel.SetActive(true);
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
