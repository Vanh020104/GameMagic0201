using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonController : MonoBehaviour
{
    [SerializeField] private string homeScene = "Scenes_Home_Game";
    [SerializeField] private string battleScene = "LayoutBattle";
    public void OnClickPlay()
    {
        SceneManager.LoadScene(battleScene, LoadSceneMode.Single);
    }

    public void BackHome()
    {
        SceneManager.LoadScene(homeScene, LoadSceneMode.Single);
    }

}

