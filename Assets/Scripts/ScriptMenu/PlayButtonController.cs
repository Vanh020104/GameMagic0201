using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayButtonController : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "LayoutBattle";
    [SerializeField] private string sceneHome = "Scenes_Home_Game";

    public void OnClickPlay()
    {
        StartCoroutine(DelayLoadScene());
    }

    IEnumerator DelayLoadScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(sceneToLoad);
    }

    public void BackHome()
    {
        SceneManager.LoadScene(sceneHome);
    }
}
