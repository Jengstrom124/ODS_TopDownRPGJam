using UnityEngine;
using NaughtyAttributes;

public class ButtonSFXManager : MonoBehaviour
{
    [SerializeField, ReadOnly] bool ignoreHoverSound = false;

    public void SelectSFX()
    {
        //AudioManager.instance.PlayOneShot(FMODEvents.instance.uiEvents.uiSelect);
    }

    public void HoverSFX()
    {
        if (ignoreHoverSound)
        {
            ignoreHoverSound = false;
            return;
        }

        //AudioManager.instance.PlayOneShot(FMODEvents.instance.uiEvents.uiHover);
    }

    public void BackSFX()
    {
        //AudioManager.instance.PlayOneShot(FMODEvents.instance.uiEvents.uiBack);
    }

    public void IgnoreHoverSound(bool ignore)
    {
        ignoreHoverSound = ignore;
    }
}
