using UnityEngine;

public class LogoShine : MonoBehaviour
{
    public RectTransform shineTransform;
    public float speed = 400f; // Pixel per second

    private float startX;
    private float endX;

    private void Start()
    {
        RectTransform parentRect = GetComponent<RectTransform>();
        startX = -parentRect.rect.width * 0.75f;
        endX = parentRect.rect.width * 0.75f;

        ResetPosition();
    }

    private void Update()
    {
        shineTransform.anchoredPosition += Vector2.right * speed * Time.deltaTime;

        if (shineTransform.anchoredPosition.x >= endX)
        {
            ResetPosition();
        }
    }

    private void ResetPosition()
    {
        shineTransform.anchoredPosition = new Vector2(startX, shineTransform.anchoredPosition.y);
    }
}
