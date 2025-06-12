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
        // ✅ Instantiate trước
        currentLayout = Instantiate(battleLayoutPrefab);

        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        currentLayout.playerNameText.text = playerName;
        AudioManager.Instance.PlayMusic(AudioManager.Instance.bgmCombat);

        // ✅ Rồi mới gán icon kỹ năng
        if (GameData.SelectedHero != null && GameData.SelectedHero.skills != null)
        {
            SkillData[] skills = GameData.SelectedHero.skills;

            if (skills.Length > 0 && skills[0] != null)
                currentLayout.firstSkillIcon.sprite = skills[0].skillIcon;

            if (skills.Length > 1 && skills[1] != null)
                currentLayout.secondSkillIcon.sprite = skills[1].skillIcon;

            if (skills.Length > 2 && skills[2] != null)
                currentLayout.thirdSkillIcon.sprite = skills[2].skillIcon;
        }
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
