using UnityEngine;
using DG.Tweening;
using Febucci.UI.Core;
using UnityEngine.Events;
using NaughtyAttributes;

[RequireComponent(typeof(CanvasGroup))]
public class IntroDialogueBox : MonoBehaviour
{
    public bool canProgress = true;
    [SerializeField] bool fadeOnHide = false;
    [SerializeField, ShowIf("fadeOnHide")] float fadeOutDuration = 1f;

    [Space]
    public UnityEvent triggerOnAwake;
    public float initDelay = 0.5f;
    RectTransform rectTransform;
    CanvasGroup canvasGroup;
    TypewriterCore typewriterCore;

    private Vector2 originAnchoredPosition;
    [SerializeField] private float YTweenDistance;
    [SerializeField] private float fadeDuration, moveDuration;

    private void Awake()
    {
        triggerOnAwake?.Invoke();
        //if (disableOnAwake != null) disableOnAwake.SetActive(false);
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        originAnchoredPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0;
    }

    Tween tween;
    private void OnEnable()
    {
        Invoke("BeginDialogue", initDelay);
    }
    void BeginDialogue()
    {
        rectTransform.DOAnchorPosY(originAnchoredPosition.y - YTweenDistance, 0f);

        typewriterCore = GetComponentInChildren<TypewriterCore>();
        canvasGroup.DOFade(1, fadeDuration).SetEase(Ease.Linear).OnComplete(delegate { typewriterCore.StartShowingText(); });
        TweenUp();

        Invoke("TextCheck", fadeDuration + 0.4f);
    }
    void TextCheck()
    {
        if (!gameObject.activeSelf)
        {
            Debug.Log("GameObject Inactive");
            return;
        }

        if (!typewriterCore.isShowingText) typewriterCore.StartShowingText();
    }

    void TweenUp()
    {
        tween = rectTransform.DOAnchorPosY(originAnchoredPosition.y, moveDuration, false).SetEase(Ease.InOutSine).SetDelay(0.05f);
        tween.Play();
    }

    public void HideDialogueBox()
    {
        if (fadeOnHide) canvasGroup.DOFade(0, fadeOutDuration).SetEase(Ease.Linear).OnComplete(delegate { gameObject.SetActive(false); });
        else gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (tween != null) tween.Kill();

        canvasGroup.alpha = 0;
        rectTransform.anchoredPosition = originAnchoredPosition;
    }
}
