using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePassLevelUI : MonoBehaviour
{
    public BattlePassDatabase database;

    [Header("UI Components")]
    public RectTransform levelNumberParent;
    public GameObject levelLabelPrefab;
    public Slider levelSlider;

    private void Start()
    {
        // Lấy level người chơi và hiển thị thanh tiến độ + label level
        int playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        int totalLevels = database.levels.Count;

        GenerateLevelLabels(playerLevel);

        HorizontalLayoutGroup layout = levelNumberParent.GetComponent<HorizontalLayoutGroup>();
        float spacing = layout.spacing;
        float padding = layout.padding.left + layout.padding.right;

        float elementWidth = levelLabelPrefab.GetComponent<LayoutElement>()?.preferredWidth
            ?? levelLabelPrefab.GetComponent<RectTransform>().rect.width;

        float fullWidth = padding + (elementWidth * totalLevels) + (spacing * (totalLevels - 1));
        float fullWidthScrollable = fullWidth - elementWidth;

        levelNumberParent.sizeDelta = new Vector2(fullWidth, levelNumberParent.sizeDelta.y);
        RectTransform levelSliderRect = levelSlider.GetComponent<RectTransform>();
        levelSliderRect.sizeDelta = new Vector2(fullWidth, levelSliderRect.sizeDelta.y);

        float positionX = layout.padding.left + (playerLevel - 1) * (elementWidth + spacing);
        levelSlider.minValue = 0;
        levelSlider.maxValue = 1;
        levelSlider.value = Mathf.Clamp01(positionX / fullWidthScrollable);
    }

    // Tạo các chấm số level và làm nổi bật chấm hiện tại
    private void GenerateLevelLabels(int playerLevel)
    {
        foreach (Transform child in levelNumberParent)
        {
            Destroy(child.gameObject);
        }

        int totalLevels = database.levels.Count;

        for (int i = 0; i < totalLevels; i++)
        {
            GameObject go = Instantiate(levelLabelPrefab, levelNumberParent);
            TMP_Text text = go.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = (i + 1).ToString();
                if (i + 1 == playerLevel)
                {
                    text.color = Color.yellow;
                    go.transform.localScale = Vector3.one * 1.2f;
                }
            }
        }
    }
}
