using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using NaughtyAttributes;
using Febucci.UI.Core;
using UnityEngine.UI;
using UnityEngine.Events;

#region Custom Classes
public enum DialogueBarkTween
{
    Fade,
    MoveY,
    MoveX
}
public enum DialogueBarkEffect
{
    Static,
    Shake,
}
public enum DialogueBarkSize
{
    XS,
    S,
    M,
    L,
    XL,
    LongSingle,
    LongDouble
}
[System.Serializable]
public class DialogueBarkUI
{
    public DialogueBarkSize barkSize;
    public Sprite barkSprite;
}
[System.Serializable]
public class BarkEvents
{
	[Tooltip("Auto called when this individual dialogue bark begins")] public UnityEvent onBarkShownEvent;
	[Tooltip("Auto called when this individual dialogue bark ends")] public UnityEvent onBarkCompleteEvent;
}
[System.Serializable]
public class DialogueBark
{
    public string barkText;
    public DialogueBarkSize barkSize = DialogueBarkSize.M;
    public DialogueBarkTween tweenMode;
    public DialogueBarkEffect effect = DialogueBarkEffect.Static;
    public TextAlignmentOptions textAlignment = TextAlignmentOptions.Center;
    [Tooltip("Default Size: 0.15")] public float fontSize = 0.15f;

    [Space]
    [AllowNesting, ShowIf("tweenMode", DialogueBarkTween.MoveY), Tooltip("0.25 = MoveDown to StartPos. StartPos is the EndPos")]
    public float yOffset = 0.25f;
    [AllowNesting, ShowIf("tweenMode", DialogueBarkTween.MoveX), Tooltip("0.25 = MoveLeft to StartPos. StartPos is the EndPos")]
    public float xOffset = 0.25f;

    [Space]
    public BarkEvents events;
}
#endregion

public class DialogueBarkTrigger : MonoBehaviour
{
    //---------------TODO: MAYBE THIS BARK SYSTEM IS REFACTORED-------------------------------
    //Use this as a 'BarkManager' and have 'BarkTriggers' that handles all the settings/text of barks
    //That get passed onto this to show the bark --- similiar to dialoguesystem?

    [Header("Setup")]
    [SerializeField] RectTransform barkTransform;
    [SerializeField] CanvasGroup barkUIGroup;
    [SerializeField] TMP_Text barkText;
    [SerializeField] Image barkImage;
    [SerializeField] DialogueBarkUI[] barkSprites;

    [Header("Config")]
    [SerializeField] float tweenDuration = 0.4f;
    [SerializeField] bool useTypewriter = false;
    [SerializeField, ShowIf("useTypewriter")] TypewriterCore typewriterCore;
    [SerializeField] bool showOnce = false;
    [SerializeField] DialogueBark[] myBarks;
    [SerializeField] bool autoHideBark = false;
    [SerializeField, ShowIf("autoHideBark")] float autoHideAfterDuration = 2f;
    float defaultAutoHideAfterDurationRef;

    //Refs
    Tween effectTween;
    DialogueBark currentBark;
    int index = 0;
    bool barkShown = false, barkHidden = false;
    Vector2 startPos;

    public event System.Action onBarkShownEvent, onBarkHiddenEvent;
    private void Awake()
    {
        startPos = barkTransform.anchoredPosition;
        barkUIGroup.alpha = 0;
        defaultAutoHideAfterDurationRef = autoHideAfterDuration;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Utility.IsPlayer(collision)) return;
        if (barkHidden) return;
        if (barkShown && showOnce) return;

        ForceShowBark();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!Utility.IsPlayer(collision)) return;
        if (barkHidden) return;
        if (barkShown && showOnce) return;

        ForceHideBark();
    }

    #region Utility
    bool customBarkActive_ = false, autoHideBarkValue;
    public void ShowCustomBark(DialogueBark customBark_, float barkDuration)
    {
        //Setup bark
        customBarkActive_ = true;
        autoHideBarkValue = autoHideBark;
        autoHideBark = true;
        autoHideAfterDuration = barkDuration;

        //Show
        currentBark = customBark_;
        ForceShowBark();
    }
    public void ShowCustomBark(string barkText_, DialogueBarkSize barkSize_, float barkDuration = 2f, DialogueBarkTween barkTween_ = DialogueBarkTween.Fade, DialogueBarkEffect barkEffect_ = DialogueBarkEffect.Static, TextAlignmentOptions textAlignment_ = TextAlignmentOptions.Center, float textSize_ = 0)
    {
        //Setup bark
        customBarkActive_ = true;
        autoHideBarkValue = autoHideBark;
        autoHideBark = true;
        autoHideAfterDuration = barkDuration;

        //Create Bark
        DialogueBark customBark = new DialogueBark();
        customBark.barkText = barkText_;
        customBark.barkSize = barkSize_;
        customBark.tweenMode = barkTween_;
        customBark.effect = barkEffect_;
        customBark.textAlignment = textAlignment_;
        customBark.fontSize = textSize_;

        //Show
        currentBark = customBark;
        ForceShowBark();
    }
    public void ForceShowBarkDelay(float delay)
    {
        Invoke("ForceShowBark", delay);
    }
    [ContextMenu("Debug/ForceShowBark")]
    public void ForceShowBark()
    {
        if(myBarks.Length <= 0 && !customBarkActive_) return;
        HandleSettingBarkText(customBarkActive_);
        HandleDisplayingBarkText(true);
        onBarkShownEvent?.Invoke();

        if(currentBark.events != null) currentBark.events.onBarkShownEvent?.Invoke();
    }
    public void OnBarkShownTypewriter()
    {
        if (!autoHideBark || !useTypewriter) return;
        Invoke("ForceHideBark", autoHideAfterDuration);
    }
    public void OnBarkShown()
    {
        if (!autoHideBark || useTypewriter) return;
        Invoke("ForceHideBark", autoHideAfterDuration);
    }
    void ForceHideBark()
    {
        if(myBarks.Length <= 0 && !customBarkActive_) return;

        //Hide Bark
        barkShown = true;
        HandleDisplayingBarkText(false);
        onBarkHiddenEvent?.Invoke();

        if(customBarkActive_) CleanupCustomBarkSettings();
        if(currentBark.events != null) currentBark.events.onBarkCompleteEvent?.Invoke();
    }
    void CleanupCustomBarkSettings()
    {
        //Restore Bark Defaults
        if (!customBarkActive_) return;

        customBarkActive_ = false;
        autoHideBark = autoHideBarkValue;
        autoHideAfterDuration = defaultAutoHideAfterDurationRef;
    }

    void HandleBarkSprite()
    {
        foreach (DialogueBarkUI barkSprite in barkSprites)
        {
            if (barkSprite.barkSize == currentBark.barkSize) SetBarkSprite(barkSprite.barkSprite);
        }
    }
    void SetBarkSprite(Sprite newSprite)
    {
        barkImage.sprite = newSprite;
        barkImage.SetNativeSize();
    }

    void HandleSettingBarkText(bool customBarkActive = false)
    {
        //Get Bark
        if(!customBarkActive)
        {
            currentBark = myBarks[index];

            //Update Index
            index++;
            if (index >= myBarks.Length) index = 0;
        }

        HandleBarkSprite();
        SetBarkText(currentBark.barkText, currentBark.textAlignment);
    }
    void SetBarkText(string barkDialogue, TextAlignmentOptions barkAlignment)
    {
        barkText.alignment = barkAlignment;
        if (currentBark.fontSize == 0) barkText.fontSize = 0.15f;
        else barkText.fontSize = currentBark.fontSize;

        if (useTypewriter)
        {
            typewriterCore.ShowText(barkDialogue);
            typewriterCore.StartShowingText();
        }
        else barkText.text = barkDialogue;
    }

    float endValueFromBool;
    void HandleDisplayingBarkText(bool showBark)
    {
        if (showBark) endValueFromBool = 1;
        else endValueFromBool = 0;

        if (currentBark.tweenMode == DialogueBarkTween.Fade) FadeBarkUI(endValueFromBool);
        else if (currentBark.tweenMode == DialogueBarkTween.MoveY) MoveYBark(showBark);
        else if (currentBark.tweenMode == DialogueBarkTween.MoveX) MoveXBark(showBark);

        HandleBarkEffects(showBark);
    }

    void HandleBarkEffects(bool showBark)
    {
        if(!showBark)
        {
            Utility.KillTween(effectTween);
            return;
        }

        if (currentBark.effect == DialogueBarkEffect.Shake) effectTween = barkTransform.DOShakeAnchorPos(999f, 0.015f, 15, 90, false, false).SetDelay(tweenDuration);
        effectTween.Play();
    }
    #endregion

    #region Bark Tweens
    Tween fadeTween, transformTween, scaleTween;
    public void HideBark() //Disables Bark
    {
        if (fadeTween != null) fadeTween.Kill();
        fadeTween = barkUIGroup.DOFade(0, tweenDuration / 2).OnComplete(delegate { gameObject.SetActive(false); });
        fadeTween.Play();
    }

    void FadeBarkUI(float endValue)
    {
        if (fadeTween != null) fadeTween.Kill();
        fadeTween = barkUIGroup.DOFade(endValue, tweenDuration).OnComplete(OnBarkShown);
        fadeTween.Play();
    }
    void ScaleBark(bool showUI)
    {
        if (scaleTween != null) scaleTween.Kill();
        if(showUI)
        {
            barkTransform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
            scaleTween = barkTransform.DOScale(1, tweenDuration);
        }
        else scaleTween = barkTransform.DOScale(0.85f, tweenDuration);

        scaleTween.Play();
    }
    void MoveYBark(bool showUI) //Move Up/Down
    {
        Utility.KillTween(transformTween);
        Vector2 offset = new Vector2(startPos.x, startPos.y + currentBark.yOffset);

        if (showUI)
        {
            barkTransform.anchoredPosition = offset;

            transformTween = barkTransform.DOAnchorPosY(startPos.y, tweenDuration);
            FadeBarkUI(1);
        }
        else
        {
            transformTween = barkTransform.DOAnchorPosY(offset.y, tweenDuration);
            FadeBarkUI(0);
        }

        transformTween.Play();
        ScaleBark(showUI);
    }
    void MoveXBark(bool showUI) //Move Left/Right
    {
        Utility.KillTween(transformTween);
        Vector2 offset = new Vector2(startPos.x + currentBark.xOffset, startPos.y);

        if (showUI)
        {
            barkTransform.anchoredPosition = offset;

            transformTween = barkTransform.DOAnchorPosX(startPos.x, tweenDuration);
            FadeBarkUI(1);
        }
        else
        {
            transformTween = barkTransform.DOAnchorPosX(offset.x, tweenDuration);
            FadeBarkUI(0);
        }

        transformTween.Play();
        ScaleBark(showUI);
    }
    #endregion

    //Test
    [ContextMenu("Debug/TestCustomBark")]
    void TestCustomBark()
    {
        ShowCustomBark("Custom bark test!", DialogueBarkSize.M);
    }
}
