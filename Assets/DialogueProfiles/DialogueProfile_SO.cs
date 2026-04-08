using UnityEngine;
using FMODUnity;
using NaughtyAttributes;

public enum DialogueBoxType
{
    NoName,
    Communicator,
    MiddleShort,
    MiddleLong,
    LeftShort,
    LeftLong,
    InnerLeftShort,
    InnerLeftLong,
    RightShort,
    RightLong,
    InnerRightShort,
    InnerRightLong,
    LeftXLong,
    InnerLeftXLong
}

[CreateAssetMenu(fileName = "DialogueProfile_NAME", menuName = "Dialogue System/Create Dialogue Profile")]
public class DialogueProfile_SO : ScriptableObject
{
    public string characterName;
    public Sprite dialogueSprite;
    public RuntimeAnimatorController dialogueAnimator;
    //public EventReference dialogueVoice;
    public AudioClip voiceClip;
    public bool flipDialogueSprite = false;

    [Space]
    public float offset = 0;
    public float innerOffset = 0;

    [Space]
    public bool overrideSFXFrequency = false;
    [ShowIf("overrideSFXFrequency")] public int SFXFrequency = 5;

    [Space]
    public string animRefShortcut = "NameTalk_";
}
