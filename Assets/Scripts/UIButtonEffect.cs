using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIButtonEffect : MonoBehaviour
{
    public Image image;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector3 originalRotation;
    private Vector2 originalPivot;
    private Vector2 originalSizeDelta;

    void Start()
    {
        rectTransform = image.GetComponent<RectTransform>();

        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localEulerAngles;
        originalPivot = rectTransform.pivot;
        PlayComplexAnimation();
    }

    public void PlayComplexAnimation()
    {
        //DOTween.Kill(rectTransform);

        //rectTransform.anchoredPosition = originalPosition;
        //rectTransform.localEulerAngles = originalRotation;

        Sequence uiSequence = DOTween.Sequence();
        
        uiSequence.Append(rectTransform.DOAnchorPosY(originalPosition.y + 50f, 1f).SetEase(Ease.OutQuad));
        uiSequence.Join(rectTransform.DOLocalRotate(new Vector3(0, 0, 20f), 0.5f).SetEase(Ease.OutQuad));
        //uiSequence.Join(rectTransform.DOSizeDelta(originalSizeDelta * 1.5f, 0.5f).SetEase(Ease.OutQuad));

        //uiSequence.Append(rectTransform.DOPivot(new Vector2(0, 0), 0.1f).SetEase(Ease.OutQuad));
        uiSequence.Append(rectTransform.DOLocalRotate(new Vector3(0, 0, 40f), 0.5f).SetEase(Ease.OutQuad));
        uiSequence.Append(rectTransform.DOLocalRotate(new Vector3(0, 0, 20f), 0.2f).SetEase(Ease.OutQuad));
        //uiSequence.Append(rectTransform.DOPivot(originalPivot, 0.1f).SetEase(Ease.OutQuad));

        uiSequence.Append(rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.InOutQuad));
        uiSequence.Join(rectTransform.DOLocalRotate(originalRotation, 0.3f).SetEase(Ease.InOutQuad));
        
    }
}