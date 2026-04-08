using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using NaughtyAttributes;

public enum DialogueSpriteEntryTween
{
    Default,
    Jump,
    SlowSlideIn,
    SlideUp,
    NONE
}
public enum DialogueSpriteExitTween
{
    Default,
    SlideDown,
    NONE
}
public class DialogueSpriteTweenManager : MonoBehaviour
{
    public RectTransform rectTransform;
    Image dialogueSprite;

    [Header("Default Entrance Tween")]
    [SerializeField] private float defaultEntranceDuration = 0.3f;
    [SerializeField, HideIf("myEntryTween", DialogueSpriteEntryTween.SlideUp)] private float XPositionDifference = -20f;
    [SerializeField, ShowIf("myEntryTween", DialogueSpriteEntryTween.SlideUp)] private float YPositionDifference = -20f;

    [Header("Ref:")]
    public DialogueSpriteEntryTween myEntryTween = DialogueSpriteEntryTween.Default;
    public DialogueSpriteExitTween myExitTween = DialogueSpriteExitTween.Default;
    [SerializeField] Vector2 originAnchoredPosition;
    Tween entryTween, effectTween;
    [HideInInspector] public float disableDuration = 0.275f;
    private void Awake()
    {
        dialogueSprite = GetComponent<Image>();
    }
    private void OnEnable()
    {
        originAnchoredPosition = rectTransform.anchoredPosition; //Find Origin OnEnable Due To Changing X Offset

        if (myEntryTween == DialogueSpriteEntryTween.NONE) FadeSprite(0);
        else if (myEntryTween == DialogueSpriteEntryTween.Default) DefaultEntryTween(defaultEntranceDuration);
        else if (myEntryTween == DialogueSpriteEntryTween.Jump) JumpEntryTween();
        else if (myEntryTween == DialogueSpriteEntryTween.SlowSlideIn) DefaultEntryTween(defaultEntranceDuration * 2);
        else if (myEntryTween == DialogueSpriteEntryTween.SlideUp) SlideUpTween(defaultEntranceDuration);
    }

    #region Entry Tweens
    void DefaultEntryTween(float duration)
    {
        //Offset pos for slide in
        rectTransform.anchoredPosition = new Vector2(originAnchoredPosition.x + XPositionDifference, originAnchoredPosition.y);
        FadeSprite(duration);

        //Slide Sprite into place
        entryTween = rectTransform.DOAnchorPosX(originAnchoredPosition.x, duration, false).SetDelay(0.025f);
        entryTween.Play();
    }
    void JumpEntryTween()
    {
        FadeSprite(0);
        entryTween = rectTransform.DOJumpAnchorPos(originAnchoredPosition, 25f, 1, 0.5f).SetEase(Ease.OutBack);
        entryTween.Play();
    }
    void SlideUpTween(float duration)
    {
        //Offset pos for slide in
        rectTransform.anchoredPosition = new Vector2(originAnchoredPosition.x, originAnchoredPosition.y + YPositionDifference);
        FadeSprite(duration);

        //Slide Sprite into place
        entryTween = rectTransform.DOAnchorPosY(originAnchoredPosition.y, duration, false).SetDelay(0.025f);
        entryTween.Play();
    }
    #endregion

    void FadeSprite(float duration)
    {
        dialogueSprite.DOFade(1, duration - (duration * 0.5f)).SetEase(Ease.Linear).SetDelay(0.025f);
    }

    #region Dialogue Tweens
    [ContextMenu("DOJump")]
    public void DOJump()
    {
        rectTransform.DOJumpAnchorPos(originAnchoredPosition, 15f, 1, 0.6f);
    }

    bool shakeActive = false;
    [ContextMenu("DOShake")]
    public void DOShake()
    {
        if (shakeActive) KillShake();
        else
        {
            effectTween = rectTransform.DOShakeAnchorPos(999f, 2f, 10, 45f, false, false);
            effectTween.Play();
            shakeActive = true;
        }
    }
    void KillShake()
    {
        CleanUpEffectTween();
        shakeActive = false;
    }
    #endregion

    public void DisableSprite(float delay = 0f)
    {
        if(originAnchoredPosition == Vector2.zero) originAnchoredPosition = rectTransform.anchoredPosition; //HACK: Make sure origin pos is init before changing its pos

        if (myExitTween == DialogueSpriteExitTween.Default) rectTransform.DOAnchorPosX(originAnchoredPosition.x + XPositionDifference, 0.275f, false).SetDelay(delay);
        else if (myExitTween == DialogueSpriteExitTween.SlideDown) rectTransform.DOAnchorPosY(originAnchoredPosition.y + YPositionDifference, 0.275f, false).SetDelay(delay);

        if (myExitTween == DialogueSpriteExitTween.NONE) dialogueSprite.DOFade(0, 0).OnComplete(delegate { gameObject.SetActive(false); });
        else dialogueSprite.DOFade(0, 0.275f).OnComplete(delegate { gameObject.SetActive(false); }).SetDelay(delay);

        disableDuration = 0.275f; //Make sure to update this if disable timings change in future
    }
    private void OnDisable()
    {
        if (entryTween != null) entryTween.Kill();
        if (effectTween != null) effectTween.Kill();
        rectTransform.anchoredPosition = originAnchoredPosition;
    }
    void CleanUpEffectTween()
    {
        if (effectTween != null) effectTween.Kill();
        rectTransform.anchoredPosition = originAnchoredPosition;
    }
}
