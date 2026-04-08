using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class DialoguePanelTweenManager : MonoBehaviour
{
    [SerializeField] float startScale = 0.8f;
    [SerializeField] float panelTweenDuration = 0.12f;
    [SerializeField] bool isNegativeScale = false;
    float defaultXScale, defaultXStartScale;

    [Space]
    [SerializeField] float panelTweenOutDuration = 0.2f;

    CanvasGroup canvasGroup;
    RectTransform rectTransform;
    Vector3 startpos;

    private void Awake()
    {
        defaultXScale = transform.localScale.x;
        if (isNegativeScale) defaultXStartScale = -startScale;
        else defaultXStartScale = startScale;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        startpos = rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        if(DialogueManager.instance.initPanel)
        {
            //Tween Panel IN
            DialogueManager.instance.PanelTweenActive = true;
            DialogueManager.instance.panelTweenDuration = panelTweenDuration;
            rectTransform.anchoredPosition = new Vector3(startpos.x, startpos.y, startpos.z);
            transform.localScale = new Vector3(defaultXStartScale, startScale, startScale);

            canvasGroup.DOFade(1, panelTweenDuration).SetEase(Ease.Linear);
            transform.DOScaleX(defaultXScale, panelTweenDuration).SetDelay(0.025f).SetEase(Ease.Linear);
            transform.DOScaleY(1f, panelTweenDuration).SetDelay(0.025f).SetEase(Ease.Linear).OnComplete(DialogueManager.instance.OnDialoguePanelInit);
        }
        else if(!DialogueManager.instance.useOldPanels)
        {
            //Tween Up
            DialogueManager.instance.PanelTweenActive = true;
            DialogueManager.instance.panelTweenDuration = panelTweenDuration;
            transform.localScale = Vector3.one;
            rectTransform.anchoredPosition = new Vector3(startpos.x, 0, startpos.z);

            canvasGroup.DOFade(1, panelTweenDuration).SetEase(Ease.Linear).SetDelay(panelTweenOutDuration / 2);
            rectTransform.DOAnchorPosY(startpos.y, panelTweenDuration).OnComplete(DialogueManager.instance.OnDialoguePanelInit).SetDelay(panelTweenOutDuration / 2);
        }
        else
        {
            transform.localScale = new Vector3(defaultXScale, 1, 1);
            canvasGroup.alpha = 1;
            DialogueManager.instance.OnDialoguePanelInit();
        }
    }

    public void HidePanel(bool dialogueComplete = true)
    {
        if (!gameObject.activeInHierarchy) return;

        if(DialogueManager.instance.useOldPanels)
        {
            gameObject.SetActive(false);
            return;
        }

        if(dialogueComplete)
        {
            //Scale Panel OUT
            transform.DOScaleX(startScale, panelTweenDuration).SetEase(Ease.Linear);
            transform.DOScaleY(startScale, panelTweenDuration).SetEase(Ease.Linear);

            //Fade Panel out
            canvasGroup.DOFade(0, panelTweenDuration).SetEase(Ease.Linear).OnComplete(delegate { gameObject.SetActive(false); });
        }
        else
        {
            //Tween Up
            rectTransform.DOAnchorPosY(0, panelTweenOutDuration);

            //Fade Panel out
            canvasGroup.DOFade(0, panelTweenOutDuration).SetEase(Ease.Linear).OnComplete(delegate { gameObject.SetActive(false); });
        }
    }
}
