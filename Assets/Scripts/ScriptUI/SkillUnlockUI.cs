using UnityEngine;
using UnityEngine.UI;

public class SkillUnlockUI : MonoBehaviour
{
    public int unlockLevel = 2;
    public GameObject lockGroup;    
    public GameObject unlockButton;
    public GameObject glowBorder;
    public Button skillButton;

    private bool isUnlocked = false;
    [SerializeField] private LevelUI levelUI;


    void Start()
    {
        if (levelUI != null)
            levelUI.OnLevelChanged += RefreshState;

        RefreshState(1); // ban đầu
    }


    public void RefreshState(int playerLevel)
    {
        if (isUnlocked)
        {
            lockGroup.SetActive(false);
            unlockButton.SetActive(false);
            glowBorder.SetActive(true);
            skillButton.interactable = true;
        }
        else if (playerLevel >= unlockLevel)
        {
            lockGroup.SetActive(false);
            unlockButton.SetActive(true);
            glowBorder.SetActive(false);
            skillButton.interactable = false;
        }
        else
        {
            lockGroup.SetActive(true);
            unlockButton.SetActive(false);
            glowBorder.SetActive(false);
            skillButton.interactable = false;
        }
    }

    public void UnlockSkill()
    {
        isUnlocked = true;
        RefreshState(unlockLevel); // cập nhật lại UI
        Debug.Log($"[SkillUnlock] Unlocked {gameObject.name}");
    }
}
