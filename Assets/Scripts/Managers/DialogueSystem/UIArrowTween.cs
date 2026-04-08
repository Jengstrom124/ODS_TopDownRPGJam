using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

public class UIArrowTween : MonoBehaviour
{
    public RectTransform rectTransform;

    private Vector2 originAnchoredPosition;
    [SerializeField] bool tweenXAxis = false;
    [SerializeField, HideIf("tweenXAxis")] private float YPositionDifference;
    [SerializeField, ShowIf("tweenXAxis")] private float XPositionDifference;
    [SerializeField] private float moveDuration;

    Tween tween;
    private void OnEnable()
    {
        originAnchoredPosition = rectTransform.anchoredPosition;

        TweenDown();
    }

    void TweenDown()
    {
        if (tweenXAxis) tween = rectTransform.DOAnchorPosX(originAnchoredPosition.x + XPositionDifference, moveDuration, false).SetEase(Ease.InOutSine).OnComplete(TweenUp);
        else tween = rectTransform.DOAnchorPosY(originAnchoredPosition.y + YPositionDifference, moveDuration, false).SetEase(Ease.InOutSine).OnComplete(TweenUp);
        tween.Play();
    }
    void TweenUp()
    {
        if (tweenXAxis) tween = rectTransform.DOAnchorPosX(originAnchoredPosition.x, moveDuration, false).SetEase(Ease.InOutSine).OnComplete(TweenDown);
        else tween = rectTransform.DOAnchorPosY(originAnchoredPosition.y, moveDuration, false).SetEase(Ease.InOutSine).OnComplete(TweenDown);
        tween.Play();
    }

    private void OnDisable()
    {
        if (tween != null) tween.Kill();
        rectTransform.anchoredPosition = originAnchoredPosition;
    }
}
