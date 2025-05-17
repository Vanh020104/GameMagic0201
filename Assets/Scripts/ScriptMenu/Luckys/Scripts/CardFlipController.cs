using UnityEngine;
using DG.Tweening;

public class CardFlipController : MonoBehaviour
{
    public GameObject backFace;
    public GameObject frontFace;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void FlipToFront()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOScaleX(0f, 0.2f).SetEase(Ease.InBack));
        seq.AppendCallback(() =>
        {
            backFace.SetActive(false);
            frontFace.SetActive(true);
        });
        seq.Append(rect.DOScaleX(1f, 0.2f).SetEase(Ease.OutBack));
    }

    public void FlipToBack()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOScaleX(0f, 0.2f).SetEase(Ease.InBack));
        seq.AppendCallback(() =>
        {
            backFace.SetActive(true);
            frontFace.SetActive(false);
        });
        seq.Append(rect.DOScaleX(1f, 0.2f).SetEase(Ease.OutBack));
    }
}
