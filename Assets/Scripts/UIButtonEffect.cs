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


        Sequence uiSequence = DOTween.Sequence();

        uiSequence.Append(rectTransform.DOPunchRotation(new Vector3(0, 0, 5f), 1.5f, 10, 1f));
        //uiSequence.Join(rectTransform.DOShakeAnchorPos(
        //    duration: 2f,
        //    strength: new Vector2(3f, 3f),
        //    vibrato: 10,
        //    randomness: 90f,
        //    snapping: false,
        //    fadeOut: true
        //));
        uiSequence.Append(rectTransform.DOLocalRotate(new Vector3(0, 0, 30f), 0.3f).SetEase(Ease.InOutBack));

        //uiSequence.Append(rectTransform.DOAnchorPosY(originalPosition.y + 50f, 1f).SetEase(Ease.OutQuad));
        //uiSequence.Join(rectTransform.DOLocalRotate(new Vector3(0, 0, 20f), 0.5f).SetEase(Ease.OutQuad));

        //uiSequence.Append(rectTransform.DOLocalRotate(new Vector3(0, 0, 40f), 0.5f).SetEase(Ease.OutQuad));
        //uiSequence.Append(rectTransform.DOLocalRotate(new Vector3(0, 0, 20f), 0.2f).SetEase(Ease.OutQuad));

        //uiSequence.Append(rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.InOutQuad));
        //uiSequence.Join(rectTransform.DOLocalRotate(originalRotation, 0.3f).SetEase(Ease.InOutQuad));

    }
}