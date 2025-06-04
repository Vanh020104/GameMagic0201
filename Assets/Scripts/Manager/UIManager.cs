using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public BattleLayout battleLayoutPrefab;
    private BattleLayout currentLayout;
    private void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmCombat);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentLayout != null)
        {
            Destroy(currentLayout.gameObject);
        }

        if (scene.name == "LayoutBattle")
        {
            currentLayout = Instantiate(battleLayoutPrefab);
            string playerName = PlayerPrefs.GetString("PlayerName", "Player");
            currentLayout.playerNameText.text = playerName;
            AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmCombat);
        }
        else if (scene.name == "Scenes_Home_Game")
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmHome);
        }
    }

    public BattleLayout GetLayout()
    {
        return currentLayout;
    }
}
