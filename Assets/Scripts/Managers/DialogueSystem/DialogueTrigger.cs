using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using UnityEngine.Serialization;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Configurations")]
    public List<DialogueParticipant> participants; //List Of Dialogue Profiles/Participants Involved
    public bool triggerDialogueFinisedEvent = false;
    public bool triggerDialogueStartedEvent = false;

    [Space]
    public bool multipleDialogueEntries = false;
    [ShowIf("multipleDialogueEntries")] public bool repeatDialogueEntries = false;
    public bool stopTriggerAfterDialogue = false;

    [Header("Cutscene Setup")]
    public bool isCutscene = false;
    public bool triggerOnAwake = false;
    [ShowIf("triggerOnAwake")] public float awakeDelay = 0f;
    public bool triggerOnEnableUsingTimeline = false;
    public bool showCinematicBars = false;

    [Header("Other: ")]
    [SerializeField] bool isInteractable = true;
    public bool useTriggerBox = false;
    [SerializeField, ShowIf("useTriggerBox")] bool allowContinueousTriggers = false;
    [Tooltip("Starts Dialogue With ALL Characters On Screen"), FormerlySerializedAs("continuationDialogue")] public bool showAllCharactersOnStart = false;
    [Tooltip("Begin a New DialogueTrigger Branch without 'Ending' The Conversation")] public bool continueOnDialogueEnd = false;
    [ShowIf("continueOnDialogueEnd"), Tooltip("Dialogue To Activate")] public GameObject myContinuationDialogue = null;
    public bool forceQuestionMarkIcon = false;
    public bool turnOffInteract = false;
    public bool destroyAfterTrigger = false;

    [Space]
    public UnityEvent localOnFinishUnityEvent;
    public UnityEvent localOnStartUnityEvent;
    public UnityAction localOnFinishEvent; //Old - Need to clean this up?

    [Header("Debugs")]
    [SerializeField, ReadOnly] bool dialogueLaunched;
    [ReadOnly, ShowIf("multipleDialogueEntries")] public int index = 0;

    [Space]
    [HideIf("multipleDialogueEntries")] public List<DialogueEntry> diagloueEntries;
    [ShowIf("multipleDialogueEntries")] public List<DialogueEntryHolder> myDialogueEntries; //If we want to create a library of dialogue entries we can use this

    IEnumerator Start()
    {
        if (!triggerOnAwake) yield break; //Don't Trigger
        if (!isCutscene) yield return new WaitForSeconds(awakeDelay); //Use custom delay time if not bound by cutscene restrictions
        Interact();
    }
    private void OnEnable()
    {
        if (triggerOnAwake && dialogueLaunched) Invoke("Interact", awakeDelay); //Getting called in start already so only call here after initial call
    }

    public void Interact()
    {
        if (!isInteractable) return;
        if (stopTriggerAfterDialogue && dialogueLaunched) return;
        dialogueLaunched = true;
        
        StartDialogue();

        //if (turnOffInteract) InteractionManager.instance.TurnOffInteract();
        if (destroyAfterTrigger) Destroy(gameObject);
    }
    public void Interact(float delay)
    {
        Invoke("Interact", delay);
    }

    void StartDialogue()
    {
        localOnStartUnityEvent?.Invoke();

        if (!multipleDialogueEntries) TriggerDialogue();
        else MultipleDialogueTrigger();
    }

    void MultipleDialogueTrigger()
    {
        //Send the dialogue of the current index then increment the index
        if (index < myDialogueEntries.Count)
        {
            TriggerDialogue(myDialogueEntries[index].dialogueEntries);
            index++;

            if (index >= myDialogueEntries.Count) index -= 1;
            return;
        }

        if (repeatDialogueEntries) //Reset Index and Start Dialogue Over
        {
            index = 0;
            TriggerDialogue(myDialogueEntries[index].dialogueEntries);
            index++;
            return;
        }

        //send the last entry in the list
        TriggerDialogue(myDialogueEntries[index - 1].dialogueEntries);
    }

    public void TriggerDialogue() //Used for calling non multiple entries dialogue (sends default entry list)
    {
        TriggerDialogue(diagloueEntries);
    }
    public void TriggerDialogue(List<DialogueEntry> dialogueEntries_) //Compatible with multiple entry dialogue callbacks
    {
        if (dialogueEntries_.Count <= 0)
        {
            Debug.Log("No Dialogue Left/Found");
            return;
        }

        DialogueManager.instance.StartDialogue(participants, dialogueEntries_, triggerDialogueFinisedEvent, triggerDialogueStartedEvent, showAllCharactersOnStart, localOnFinishEvent, localOnFinishUnityEvent, continueOnDialogueEnd, myContinuationDialogue, showCinematicBars);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!useTriggerBox) return;
        if (collision.isTrigger) return; //We only want the physical player collision
        if (collision.GetComponent<PlayerModel>() == null) return;

        StartDialogue();

        //Cleanup
        if (destroyAfterTrigger) Destroy(gameObject);
        if(!multipleDialogueEntries && !allowContinueousTriggers) useTriggerBox = false;
    }

    #region Utility
    public void ReduceDialogueIndex()
    {
        index--;
    }
    public bool IsItem()
    {
        return false;
    }

    public bool ForceQuestionMarkIcon()
    {
        return forceQuestionMarkIcon;
    }
    public void ShakeCam(float delay)
    {
        //CameraManager.instance.ShakeCam(0.35f, 0.075f, 4, 180, delay);
    }
    [ContextMenu("Debug/ShakeCam")]
    void ShakeCam()
    {
        ShakeCam(0);
    }

    public void SetContinuationDialogue(GameObject dialogueGO)
    {
        continueOnDialogueEnd = true;
        myContinuationDialogue = dialogueGO;
    }

    public void SetInteractableStatus(bool isInteractable_)
    {
        isInteractable = isInteractable_;
        foreach (Collider2D collider2D in GetComponents<Collider2D>())
        {
            if (collider2D.isTrigger) collider2D.enabled = isInteractable_;
        }
    }
    public void ResetTriggerToDefault()
    {
        triggerDialogueFinisedEvent = false;
        triggerDialogueStartedEvent = false;
        multipleDialogueEntries = false;
    }
    public void SetToMultipleDialogueEntries(bool isMultipleEntries, bool repeatDialogueEntries_ = false)
    {
        multipleDialogueEntries = isMultipleEntries;
        repeatDialogueEntries = repeatDialogueEntries_;
    }
    public void ClearMultipleDialogueEntries()
    {
        myDialogueEntries.Clear();
    }

    #region Updating Dialogue Entries

    #region Single Entries
    public void UpdateSingleEntryDialogue(string dialogue, List<DialogueEntry> dialogueEntriesRef_ = null)
    {
        if (dialogueEntriesRef_ == null) dialogueEntriesRef_ = diagloueEntries;
        dialogueEntriesRef_[0].dialogue = dialogue;
    }
    public void UpdateSingleEntryDialogue(string dialogue, DialogueProfile_SO speakerProfile, List<DialogueEntry> dialogueEntriesRef_ = null)
    {
        if (dialogueEntriesRef_ == null) dialogueEntriesRef_ = diagloueEntries;

        UpdateSingleEntryDialogue(dialogue, dialogueEntriesRef_);
        dialogueEntriesRef_[0].speaker.dialogueProfile = speakerProfile;
    }
    void UpdateSingleEntryAnims(AnimationClip talkAnim, AnimationClip idleAnim = null, List<DialogueEntry> dialogueEntriesRef_ = null)
    {
        if (dialogueEntriesRef_ == null) dialogueEntriesRef_ = diagloueEntries;

        dialogueEntriesRef_[0].speaker.talkAnimation = talkAnim;
        if (idleAnim == null) dialogueEntriesRef_[0].speaker.additionalAnims = false;
        else
        {
            dialogueEntriesRef_[0].speaker.additionalAnims = true;
            dialogueEntriesRef_[0].speaker.idleAnimation = idleAnim;
        }
    }
    public void UpdateSingleEntryDialogue(string dialogue, AnimationClip talkAnim, AnimationClip idleAnim = null)
    {
        UpdateSingleEntryDialogue(dialogue);
        UpdateSingleEntryAnims(talkAnim, idleAnim);
    }
    public void UpdateSingleEntryDialogue(string dialogue, DialogueProfile_SO speakerProfile, AnimationClip talkAnim, AnimationClip idleAnim = null)
    {
        UpdateSingleEntryDialogue(dialogue, speakerProfile);
        UpdateSingleEntryAnims(talkAnim, idleAnim);
    }

    public void UpdateEntries(int dialogueIndex, string newDialogue, DialogueProfile_SO speakerProfile, AnimationClip talkAnim, AnimationClip idleAnim, DialogueProfile_SO listenerProfile, AnimationClip listenIdleAnim, AnimationClip listenReactAnim, List<DialogueEntry> dialogueEntriesRef_ = null)
    {
        if (dialogueEntriesRef_ == null) dialogueEntriesRef_ = diagloueEntries;
        if (dialogueEntriesRef_.Count < dialogueIndex + 1) dialogueEntriesRef_.Add(new DialogueEntry());

        dialogueEntriesRef_[dialogueIndex].dialogue = newDialogue;
        dialogueEntriesRef_[dialogueIndex].speaker = new DialogueSpeaker();
        dialogueEntriesRef_[dialogueIndex].speaker.dialogueProfile = speakerProfile;
        dialogueEntriesRef_[dialogueIndex].speaker.talkAnimation = talkAnim;
        dialogueEntriesRef_[dialogueIndex].speaker.additionalAnims = true;
        dialogueEntriesRef_[dialogueIndex].speaker.idleAnimation = idleAnim;

        if (dialogueEntriesRef_[dialogueIndex].listeners == null)
        {
            dialogueEntriesRef_[dialogueIndex].listeners = new List<DialogueListener>();
            dialogueEntriesRef_[dialogueIndex].listeners.Add(new DialogueListener());
        }
        if (dialogueEntriesRef_[dialogueIndex].listeners.Count == 0) dialogueEntriesRef_[dialogueIndex].listeners.Add(new DialogueListener());
        dialogueEntriesRef_[dialogueIndex].listeners[0].dialogueProfile = listenerProfile;
        dialogueEntriesRef_[dialogueIndex].listeners[0].idleAnimation = listenIdleAnim;
        dialogueEntriesRef_[dialogueIndex].listeners[0].additionalAnims = true;
        dialogueEntriesRef_[dialogueIndex].listeners[0].reactAnimation = listenReactAnim;

        dialogueEntriesRef_[dialogueIndex].dialogueButtonOptions = new DialogueButtonOptions();
    }
    #endregion

    #region Multiple Entries
    public void UpdateMultipleDialogueEntries(int interactionIndex, string newDialogue, AnimationClip talkAnim = null, AnimationClip idleAnim = null, DialogueProfile_SO speaker = null, bool isLastInteraction = false)
    {
        if (myDialogueEntries.Count < interactionIndex + 1)
        {
            myDialogueEntries.Add(new DialogueEntryHolder());
            myDialogueEntries[interactionIndex].dialogueEntries = new List<DialogueEntry>();
            myDialogueEntries[interactionIndex].dialogueEntries.Add(new DialogueEntry());

            myDialogueEntries[interactionIndex].dialogueEntries[0].speaker = new DialogueSpeaker();
            myDialogueEntries[interactionIndex].dialogueEntries[0].listeners = new List<DialogueListener>();
            myDialogueEntries[interactionIndex].dialogueEntries[0].dialogueButtonOptions = new DialogueButtonOptions();
        }

        //UpdateSingleEntryDialogue(newDialogue, myDialogueEntries[interactionIndex].dialogueEntries);
        myDialogueEntries[interactionIndex].dialogueEntries[0].dialogue = newDialogue;
        if (talkAnim != null) myDialogueEntries[interactionIndex].dialogueEntries[0].speaker.talkAnimation = talkAnim;
        if (idleAnim != null) myDialogueEntries[interactionIndex].dialogueEntries[0].speaker.idleAnimation = idleAnim;
        if (speaker != null) myDialogueEntries[interactionIndex].dialogueEntries[0].speaker.dialogueProfile = speaker;

        if (isLastInteraction) RemoveAllDialgoueEntriesBeyondIndex(interactionIndex);
    }
    public void UpdateMultipleDialogueEntries(int interactionIndex, int entryIndex, string newDialogue, DialogueProfile_SO speaker, AnimationClip talkAnim, AnimationClip idleAnim, DialogueProfile_SO listenerProfile, AnimationClip listenIdleAnim, AnimationClip listenReactAnim)
    {
        if (myDialogueEntries.Count < interactionIndex + 1) myDialogueEntries.Add(new DialogueEntryHolder());

        UpdateEntries(entryIndex, newDialogue, speaker, talkAnim, idleAnim, listenerProfile, listenIdleAnim, listenReactAnim, myDialogueEntries[interactionIndex].dialogueEntries);
    }

    void RemoveAllDialgoueEntriesBeyondIndex(int interactionIndex)
    {
        if (myDialogueEntries.Count <= interactionIndex + 1) return;

        List<DialogueEntryHolder> entriesToRemove = new List<DialogueEntryHolder>();
        int index = 0;
        foreach (DialogueEntryHolder dialogueEntryHolder in myDialogueEntries)
        {
            if (index > interactionIndex) entriesToRemove.Add(dialogueEntryHolder);
            index++;
        }
        foreach (DialogueEntryHolder entryToRemove in entriesToRemove)
        {
            myDialogueEntries.Remove(entryToRemove);
        }
    }

    [ContextMenu("Debug/UpdateFirstMultipleDialogueEntriesDebug")]
    void UpdateFirstMultipleDialogueEntriesDebug()
    {
        SetToMultipleDialogueEntries(true);

        DialogueAnimationLibrary animLibrary = DialogueAnimationLibrary.instance;
        UpdateMultipleDialogueEntries(0, "Test 1", animLibrary.temNeutralTalk, animLibrary.temNeutralClosed, animLibrary.temProfile_Right);
    }
    [ContextMenu("Debug/UpdateSecondMultipleDialogueEntriesDebug")]
    void UpdateSecondMultipleDialogueEntriesDebug()
    {
        SetToMultipleDialogueEntries(true);

        DialogueAnimationLibrary animLibrary = DialogueAnimationLibrary.instance;
        UpdateMultipleDialogueEntries(1, "Test 2", animLibrary.temNeutralTalk, animLibrary.temNeutralClosed, animLibrary.temProfile_Right);
    }
    [ContextMenu("Debug/ChangeSecondMultipleDialogueEntriesDebug")]
    void ChangeSecondMultipleDialogueEntriesDebug()
    {
        SetToMultipleDialogueEntries(true);

        DialogueAnimationLibrary animLibrary = DialogueAnimationLibrary.instance;
        UpdateMultipleDialogueEntries(1, 0, "Updated Test 2 Dialogue", animLibrary.temProfile_Right, animLibrary.temNeutralTalk, animLibrary.temNeutralClosed, animLibrary.takoProfile, animLibrary.neutralClosed, animLibrary.neutralClosed);
    }

    [ContextMenu("Debug/UpdateFirstMultipleDialogueEntriesToOnlyEntryDebug")]
    void UpdateFirstMultipleDialogueEntriesToOnlyEntryDebug()
    {
        SetToMultipleDialogueEntries(true);

        DialogueAnimationLibrary animLibrary = DialogueAnimationLibrary.instance;
        UpdateMultipleDialogueEntries(0, "Test 1 Final Interaction", animLibrary.temNeutralTalk, animLibrary.temNeutralClosed, animLibrary.temProfile_Right, true);
    }
    #endregion

    #endregion

    #endregion
}
