using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

public class CustomClasses : MonoBehaviour
{

}

#region Dialogue System Classes
[System.Serializable]
public class DialogueEntryHolder
{
    public List<DialogueEntry> dialogueEntries;
}

[System.Serializable]
public class DialogueParticipantGeneric
{
    //public DialogueProfile_SO myDialogueProfile;
}
[System.Serializable]
public class DialogueParticipant : DialogueParticipantGeneric
{
    public DialogueProfile_SO dialogueProfile;
    public DialogueBoxType dialogueBox;
    public DialogueSpriteEntryTween spriteEntryTween = DialogueSpriteEntryTween.Default;
    public DialogueSpriteExitTween spriteExitTween = DialogueSpriteExitTween.Default;
    public bool overrideOffset = false;
    [AllowNesting, ShowIf("overrideOffset")] public float customOffset = 0;
}
[System.Serializable]
public class DialogueSpeaker : DialogueParticipantGeneric
{
    public DialogueProfile_SO dialogueProfile;
    public AnimationClip talkAnimation;
    public bool additionalAnims = false;
    [AllowNesting, ShowIf("additionalAnims"), Tooltip("Triggered On Dialogue Finished. NULL = Plays Paired Anim By Default. EG HappyTalk will play HappyIdle.")]
    public AnimationClip idleAnimation = null;
    [AllowNesting, ShowIf("additionalAnims"), Tooltip("Only Triggered by Event Use <?uniqueAnim>")]
    public AnimationClip uniqueAnimation = null;

    [Space]
    public bool overrideSortLayer = false;
    public bool overrideDialogueBoxType = false;
    [AllowNesting, ShowIf("overrideDialogueBoxType")] public DialogueBoxType dialogueBox;
    [Tooltip("TRUE = Swap Places With An Active Participant")] public bool isNewSpeaker = false;
}
[System.Serializable]
public class DialogueListener
{
    public DialogueProfile_SO dialogueProfile;
    public AnimationClip idleAnimation;
    public bool additionalAnims = false;
    [AllowNesting, ShowIf("additionalAnims"), Tooltip("Triggered On Dialogue Finished. NULL = Plays IdleAnim By Default")]
    public AnimationClip reactAnimation = null;
    [AllowNesting, ShowIf("additionalAnims"), Tooltip("Only Triggered by Event Use")]
    public AnimationClip uniqueAnimation = null;

    public bool overrideOffset = false;
    [AllowNesting, ShowIf("overrideOffset")]
    public float newXOffset, tweenDuration = 0.5f;
    [HideInInspector] public GameObject dialogueIMGRef;
}

[System.Serializable]
public class BarkSequence
{
    public DialogueBarkTrigger barkTrigger;
    public float barkDelay = 0;
    public UnityEvent onBarkEvent;
}
#endregion

[System.Serializable]
public class Objective
{
    public string name;
    public string description;
    public int iD;

    public DialogueEntry weCanUseThisToUpdateDialogue;
    public DialogueEntry weCanUseThisToUpdateDialoguePostObjective;
}


public enum ItemType
{
    Weapon,
    Item,
    SpecialItem
}
[System.Serializable]
public class ItemBase //Base class for all item types to allow them to be generically referenced
{
    public string name;
    [Tooltip("EG: Healing/Antidote // Gun/Melee")]
    public string type_;
    [Tooltip("EG: REGEN: // DAMAGE: ")]
    public string valueType_ = "REGEN:";
    public int valueAmount_;
    public int count_; //Amount we are picking up

    [Space]
    public Sprite sprite_;
    [TextArea(3, 10)]
    public string description_;
    [Tooltip("For Mouse Hover Equipped UI")]
    public bool configureSecondaryUI = false;
    [AllowNesting, ShowIf("configureSecondaryUI")] public Sprite secondarySprite_;
    [AllowNesting, ShowIf("configureSecondaryUI"), ResizableTextArea] public string secondaryDescription_;

    [Space]
    public bool canGive = false;
    public int buyPrice = 10;
    [HideInInspector] public int inventoryCount; //inventory amount
}

[System.Serializable]
public class ItemUI
{
    public string itemName;
    public GameObject uiRef;
    public GameObject blankUI;
    public GameObject infoText;
    public TMPro.TMP_Text countText;
}
