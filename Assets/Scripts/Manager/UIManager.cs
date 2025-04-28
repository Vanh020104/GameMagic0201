using UnityEngine;

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

            currentLayout = Instantiate(battleLayoutPrefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public BattleLayout GetLayout()
    {
        return currentLayout;
    }
}
