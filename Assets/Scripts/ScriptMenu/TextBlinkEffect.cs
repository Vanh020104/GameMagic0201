using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBlinkEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private float fadeDuration = 1f;

    private float timer = 0f;
    private bool fadingOut = true;

    private void Update()
    {
        timer += Time.deltaTime;
        float alpha = fadingOut
            ? Mathf.Lerp(1f, 0.2f, timer / fadeDuration)
            : Mathf.Lerp(0.2f, 1f, timer / fadeDuration);

        Color color = targetText.color;
        color.a = alpha;
        targetText.color = color;

        if (timer >= fadeDuration)
        {
            timer = 0f;
            fadingOut = !fadingOut;
        }
    }
}
