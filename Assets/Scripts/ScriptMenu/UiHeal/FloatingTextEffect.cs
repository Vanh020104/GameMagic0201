using TMPro;
using UnityEngine;

public class FloatingTextEffect : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float fadeDuration = 1.5f;

    private float timer;
    private TextMeshPro tmp;

    void Start()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        if (tmp != null)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alpha);
        }
    }
}
