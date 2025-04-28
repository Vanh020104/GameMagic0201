using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    private int level = 1;
    private int currentExp = 0;
    private int expToNext = 100;

    private float currentFill = 0f;
    private float targetFill = 0f;
    private float fillSpeed = 100f;
    public System.Action<int> OnLevelChanged;

    void Start()
    {
        expSlider.minValue = 0f;
        expSlider.maxValue = 100f;
        expSlider.value = 0f;
        levelText.text = $"Level {level}";
    }

    void Update()
    {
        if (currentFill < targetFill)
        {
            currentFill += fillSpeed * Time.deltaTime;
            if (currentFill > targetFill)
                currentFill = targetFill;

            expSlider.value = currentFill;
        }
    }

    public void AddExp(int amount)
    {
        currentExp += amount;

        bool leveledUp = false;

        while (currentExp >= expToNext)
        {
            currentExp -= expToNext;
            level++;
            leveledUp = true;
        }

        if (leveledUp)
        {
            levelText.text = $"Level {level}";
            currentFill = 0f;
            expSlider.value = 0f; 
            OnLevelChanged?.Invoke(level);
        }

        targetFill = (float)currentExp / expToNext * 100f;
    }
}
