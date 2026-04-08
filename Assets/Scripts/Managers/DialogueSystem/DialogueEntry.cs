using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.Serialization;
using NaughtyAttributes;
using FMODUnity;

//For each dialogue entry
[System.Serializable]
public class DialogueEntry
{
	//The dialogue
	[TextArea(3, 10)]
	[ResizableTextArea]
	public string dialogue;

	//Speaker
	public DialogueSpeaker speaker;
	//Listeners
	public List<DialogueListener> listeners;

	[FormerlySerializedAs("customDialogueOptions")]
	public DialogueEntryEvents events;
	public CustomDialogueEntryOptions customOptions;
	public DialogueButtonOptions dialogueButtonOptions;
}

[System.Serializable]
public class DialogueEntryEvents
{
	[Tooltip("Auto called when this individual dialogue entry begins")] public UnityEvent onDialogueStartEvent;
	[Tooltip("Auto called when this individual dialogue entry ends")] public UnityEvent onDialogueFinishEvent;
	[Tooltip("ONLY triggered by calling '<?event>' event")] public UnityEvent otherEventHolder;
}
[System.Serializable]
public class CustomDialogueEntryOptions
{
	[Header("Game Settings")]
	[Tooltip("TRUE = Force End Dialogue Early")] public bool endDialogue = false;
	[AllowNesting, ShowIf("endDialogue"), Tooltip("TRUE = Trigger Local Events")] public bool triggerEvents = false;
	[Tooltip("Force Dialogue to type out in full. No Progressing To Skip Typing")] public bool nonSkippable = false;
	public bool hideSpeakerName = false;
	//public bool conditionalEntry = false;
	//public ConditionCheck entryCondition;
	[AllowNesting, ShowIf("conditionalEntry")] public bool requiredValue = true;

	[Header("Text Settings")]
	[Tooltip("Default Size = 50")] public float fontSize = 50f;
	[Tooltip("Default Speed = 0.02")] public float typeSpeed = 0.02f;
	public TextColour[] coloursToCheck;
	public bool useCentreAlignment = false;
	public bool avoidMultiplePunctuactionWait = false;

	[Header("Audio Settings")]
	[Tooltip("Triggered by calling '<?oneshotSFX>' event")] public EventReference oneOffCustomSFX;
}

[System.Serializable]
public class DialogueButtonOptions
{
	public bool showButtonsAfterDialogue;
	[AllowNesting, ShowIf("showButtonsAfterDialogue")] public bool useSmallButtons = false;
	[AllowNesting, ShowIf("showButtonsAfterDialogue")] public string leftButtonText;
	[AllowNesting, ShowIf("showButtonsAfterDialogue")] public string rightButtonText;
	public UnityEvent leftButtonEvent, rightButtonEvent;

	public bool showDialogueReplyButtons = false;
	[AllowNesting, ShowIf("showDialogueReplyButtons")] public DialogueReplyButton[] replyOptions;
}
[System.Serializable]
public class DialogueReplyButton
{
	public string replyText;
	[AllowNesting, DisableIf("startNewTrigger"), Tooltip("End Dialogue Sequence Upon Option Selected.")] public bool endDialogueOnSelect = true;
	[AllowNesting, DisableIf("endDialogueOnSelect"), Tooltip("Begin a New Dialogue Branch Upon Option Selected -- Use OnSelectedEvent to Manually Assign Desired DialogueTriggerGO")] public bool startNewTrigger = false;
	public UnityEvent onSelectedEvent;

	
	[Tooltip("TRUE = Reply will not show again if selected.")] public bool hideOnSelect = false;
	[AllowNesting, ShowIf("startNewTrigger"), Tooltip("TRUE = Trigger DialgoueTrigger OnLocalEndEvents")] public bool triggerLocalEndEvent = false;
	[HideInInspector] public bool replySelected = false;
}

// As with UnityEvents, custom callbacks must have a non-generic wrapper class marked as [Serializable] in order to be serialized by Unity
//[System.Serializable]
//public class ConditionCheck : SerializableCallback<bool> { }
