using UnityEngine;
using DG.Tweening;

public class CinematicBars : MonoBehaviour
{
    public static CinematicBars instance;
    private void Awake()
    {
        instance = this;

        topBarStartPos = topBar.anchoredPosition.y;
        bottomBarStartPos = bottomBar.anchoredPosition.y;
    }

    public GameObject barHolder;
    public RectTransform topBar, bottomBar;
    [SerializeField] float transitionEnterDuration = 1.5f;
    [SerializeField] float transitionExitDuration = 2.25f;

    [Header("Ref Only: ")]
    [SerializeField] float topBarStartPos;
    [SerializeField] float bottomBarStartPos;
    public bool active = false;
    [SerializeField] bool manualBarsActive = false;

    public void ShowBars()
    {
        if (active) return;

        barHolder.SetActive(true);
        topBar.DOAnchorPosY(0, transitionEnterDuration);
        bottomBar.DOAnchorPosY(0, transitionEnterDuration);
        active = true;
    }
    public void ShowBarsManual(bool showBars)
    {
        manualBarsActive = showBars;
        if (showBars) ShowBars();
        else HideBars();
    }

    public void HideBars()
    {
        if (manualBarsActive || !active) return;

        active = false;
        topBar.DOAnchorPosY(topBarStartPos, transitionExitDuration);
        bottomBar.DOAnchorPosY(bottomBarStartPos, transitionExitDuration).OnComplete(delegate { barHolder.SetActive(false); });
    }

    private void OnDisable()
    {
        active = false;
    }
}
