using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BotWorldUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Slider hpSlider;
    public Slider manaSlider;
    public Transform followTarget;
    private BotStats stats;

    public void Init(BotStats statData, Transform follow)
    {
        stats = statData;
        followTarget = follow;

        nameText.text = stats.botName;
        hpSlider.maxValue = stats.maxHP;
        manaSlider.maxValue = stats.maxMana;
        UpdateUI();
    }

    void LateUpdate()
    {
        if (followTarget != null)
        {
            Vector3 targetPos = followTarget.position;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f); 
            transform.forward = Camera.main.transform.forward;
        }

        if (stats != null)
        UpdateUI();
    }


    void UpdateUI()
    {
        hpSlider.value = stats.currentHP;
        manaSlider.value = stats.currentMana;
    }
}
