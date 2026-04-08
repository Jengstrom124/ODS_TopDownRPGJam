using UnityEngine;
using UnityEngine.UI;

public class DialogueButtonsHack : MonoBehaviour
{
    public static DialogueButtonsHack instance;

    public Button topButton;
    public Button closeButton;
    public bool dialogueButtonsActive = false;

    private void Awake()
    {
        instance = this;
    }
}
