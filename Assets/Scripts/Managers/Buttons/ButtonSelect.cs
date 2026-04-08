using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class ButtonSelect : MonoBehaviour
{
    public Selectable primarySelectable;
    [ShowIf("useSecondarySelectable")]
    public Selectable secondarySelectable;
    public Selectable lastSelectable;
    public bool selectLastSelectableOnDisableOnly = false;
    public bool ignoreSelectable = false, useSecondarySelectable = false;
    public void OnEnable()
    {
        if (lastSelectable != null && !selectLastSelectableOnDisableOnly)
        {
            lastSelectable.Select();
            lastSelectable = null;
        }
        else
        {
            if (!primarySelectable.interactable && !useSecondarySelectable) return;

            if(!primarySelectable.interactable && useSecondarySelectable)
            {
                secondarySelectable.Select();
                return;
            }

            primarySelectable.Select();
        }
    }

    void OnDisable()
    {
        if (ignoreSelectable)
        {
            ignoreSelectable = false;
            return;
        }

        if (selectLastSelectableOnDisableOnly && lastSelectable != null) lastSelectable.Select();
    }

    public void SetLastSelectable(Selectable selectable)
    {
        lastSelectable = selectable;
    }

    public void IgnoreLastSelectable()
    {
        ignoreSelectable = true;
    }
}
