using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using FMODUnity;
using Febucci.UI.Core;
using Febucci.UI;
using DG.Tweening;
using NaughtyAttributes;

public class DialogueManager : MonoBehaviour
{
	#region Variables
	public static DialogueManager instance;
	[HideInInspector] public DialogueTrigger narrationMessageTrigger;

	[Header("Dialogue Panels")]
	public bool useOldPanels = true;
	[SerializeField] DialoguePanelManager legacyDialoguePanels;
	[SerializeField] DialoguePanelManager UpdatedDialoguePanels;
	[SerializeField] Image bottomBorderIMG;
	[SerializeField] GameObject dialogueReplyPrefab;
	Tween bottomBorderIMGTween;

	DialoguePanelManager activeDialoguePanelManager;
	DialoguePanel activeDialoguePanel = null;

	//Set These Based on The Active Panel
	TMP_Text dialogueText;
	GameObject normalPrompt, parentButtonGO, parentReplyButtonGO, currentPanelGO;
	Button leftButton, rightButton, firstReplyButton;
	public Button LeftButton => leftButton;
	public Button FirstReplyButton => firstReplyButton;
	TypewriterCore typewriterCore;
	TypewriterByCharacter typewriter;

	//Current Character References
	GameObject currentSpeakerIMG;
	Dictionary<DialogueProfile_SO, Animator> participantAnimators = new Dictionary<DialogueProfile_SO, Animator>();
	Dictionary<DialogueProfile_SO, GameObject> participantSpriteGOs = new Dictionary<DialogueProfile_SO, GameObject>();
	public Dictionary<DialogueProfile_SO, Animator> ParticipantAnimators => participantAnimators;

	[Header("Dialogue Settings")]
	[SerializeField] float spriteSwapDelay = 0.03f;
	public bool usePluginSystem = true;
	[ShowIf("usePluginSystem")] public float defaultTypewriterDialogueSpeed = 0.02f;
	[HideIf("usePluginSystem")] public float dialogueSpeed = 0.3f;

	[Space]
	public bool useCustomSFXFrequency = false;
	[Tooltip("If 2, SFX will play on every second letter etc")]
	public int SFXFrequency = 2;
	int defaultSFXFrequency = 5;
	public bool usePitchVariation = true;
	public Vector2 volumeRandomisationRange = new Vector2(0.3f, 0.4f);
	[Range(0.05f, 0.5f)]
	public float pitchChangeMultiplier = 0.1f;

	[Space]
	public float sfxDelay = 0.015f;
	public float specialCharacterDelay = 0.015f;
	public float highlightedWordDelay = 0.05f;
	public float interruptDelay = 0.015f;
	bool wordDelayActive = false;

	[Header("Extra Settings For Cutscene Compatibility")]
	public bool openingCutscene = false;
	public bool useCharacterDialogueBoxes = false;
	public bool useCustomAnimator = false;
	public bool useTriggerAudioOnly = false;
	public TMP_Text customCutsceneTextBox;
	[SerializeField] bool useDialogueSound = true;
	public float onScreenTextDelay = 3f;
	bool cutscene = false;
	float durationOfText;
	bool startTiming = false;
	public bool UseDialogueSound
    {
		get { return useDialogueSound; }
		set { useDialogueSound = value; }
	}

	[Header("Testing/Hacks: ")]
	public bool testUsingSFXDelay;
	public bool canSkip = true, preloadScene = false;
	public GameObject sceneTriggerHack;
	public bool battleSequence;
	int dialogueIndexCheck;
	public GameObject tutorialPrompt;
	public GameObject npcNormalPrompt;

	[Header("References: ")]
	[SerializeField, ReadOnly] bool dialogueRestricted = false;
	[SerializeField, ReadOnly] bool dialogueInProgress = false, dialogueActive = false, dialogueButtonsActive = false, dialogueReplyActive = false, interruptDialogue = false;
	[SerializeField, ReadOnly] List<Animator> participantAnimatorsRef = new List<Animator>();
	[SerializeField, ReadOnly] Animator currentSpeakerAnimator;
	[SerializeField, ReadOnly] DialogueSpeaker currentSpeaker;
	[HideInInspector] public bool checkForPausedAnimators = false;
	public DialogueSpeaker CurrentSpeaker => currentSpeaker;
	public Animator CurrentSpeakerAnimator => currentSpeakerAnimator;
	public bool DialogueInProgress => dialogueInProgress;
	public bool DialogueActive => dialogueActive;
	public bool DialogueButtonsActive => dialogueButtonsActive;
	public bool DialogueReplyActive => dialogueReplyActive;
	public bool RestrictDialgoue
	{
		get { return dialogueRestricted; }
		set { dialogueRestricted = value; }
	}
	public bool InterruptDialogue
    {
		get { return interruptDialogue; }
		set { interruptDialogue = value; }
    }

	bool _continueOnDialogueEnd = false;
	GameObject _continuationDialogueGO = null;
	bool showAllCharactersOnDialogueStart = false;
	bool narrationDialogue = false;
	[HideInInspector] public bool initPanel = true;
	bool panelTweenActive = true;
	int index;
	public int Index => index;
	public bool PanelTweenActive
	{
		get { return panelTweenActive; }
		set { panelTweenActive = value; }
	}

	//These are updated with dialogue triggers
	List<DialogueEntry> currentDialogueEntries;
	List<DialogueParticipant> currentDialogueParticipants;
	UnityAction currentLocalFinishEvent;
	UnityEvent currentLocalFinishUnityEvent;
	//[HideInInspector] public EventReference currentDialogueSoundEventRef;
	[HideInInspector] public AudioClip currentDialogueClipRef;
	public List<DialogueEntry> CurrentDialogueEntries => currentDialogueEntries;
	public List<DialogueParticipant> CurrentDialogueParticipants => currentDialogueParticipants;

	//Events
	public event System.Action onDialogueFinishEvent, onDialogueStartedEvent;
	public event System.Action globalOnDialogueStartedEvent, globalOnDialogueFinishedEvent;
	bool triggerDialogueFinishedEvent;
    #endregion

    #region Init System
    private void Awake()
    {
		instance = this;
		narrationMessageTrigger = GetComponent<DialogueTrigger>();

		defaultSFXFrequency = SFXFrequency;
		if (useOldPanels) activeDialoguePanelManager = legacyDialoguePanels;
		else activeDialoguePanelManager = UpdatedDialoguePanels;
	}
    private void Start()
    {
		//Make sure all dialogue is cleared on fresh game start
		ResetAndClearDialogue();
		onDialoguePanelTweenCompleteEvent += OnDialoguePanelInitListener;
	}

	private void OnDestroy()
    {
		onDialoguePanelTweenCompleteEvent -= OnDialoguePanelInitListener;
	}
	#endregion

	#region Init Dialogue
	void InitGameState()
    {
		//When dialogue begins, update the game state
		if (GameManager.instance != null) GameManager.instance.UpdateDialogueGameState(true);
		if (!useOldPanels && bottomBorderIMG != null) //TODO: Add bool to make this optional ---------------------------------
		{
			bottomBorderIMG.gameObject.SetActive(true);

			if (bottomBorderIMGTween != null) bottomBorderIMGTween.Kill();
			bottomBorderIMGTween = bottomBorderIMG.DOFade(1, 0.5f).SetEase(Ease.Linear);
			bottomBorderIMGTween.Play();
		}
	}

	public void StartDialogue(List<DialogueParticipant> dialogueParticipants, List<DialogueEntry> dialogueEntriesRecieved, bool triggerDialogueFinished, bool triggerDialogueStarted, bool showAllCharactersOnStart, UnityAction localFinishEvent, UnityEvent localFinishUnityEvent, bool continueOnDialogueEnd, GameObject continuationDialogueGO, bool useCinematicBars = false)
	{
		if (dialogueActive || dialogueRestricted)
		{
			StartCoroutine(StartDialogueDelayHackCoroutine(dialogueParticipants, dialogueEntriesRecieved, triggerDialogueFinished, triggerDialogueStarted, showAllCharactersOnStart, localFinishEvent, localFinishUnityEvent, continueOnDialogueEnd, continuationDialogueGO, useCinematicBars));
			return;
		}

		InitGameState();
		InitDialogueEntry(dialogueParticipants, dialogueEntriesRecieved, showAllCharactersOnStart, continueOnDialogueEnd, continuationDialogueGO);
		HandleOnStartEvents(triggerDialogueFinished, triggerDialogueStarted, localFinishEvent, localFinishUnityEvent);

		if (_replyButtonsActive)
		{
			//Cleanup if a dialogue reply continuation
			CleanupDialogueReplyButtons();
			_replyButtonsActive = false;
			ContinueDialogue();
		}
		else
        {
			//Start Dialogue
			initPanel = true;
			StartCoroutine(DisplayNextDialogueCoroutine(currentDialogueEntries[index].dialogue));
			if (useCinematicBars) CinematicBars.instance.ShowBars();
		}
	}
	IEnumerator StartDialogueDelayHackCoroutine(List<DialogueParticipant> dialogueParticipants, List<DialogueEntry> dialogueEntriesRecieved, bool triggerDialogueFinished, bool triggerDialogueStarted, bool showAllCharactersOnStart, UnityAction localFinishEvent, UnityEvent localFinishUnityEvent, bool continueOnDialogueEnd, GameObject continuationDialogueGO, bool useCinematicBars = false)
    {
		yield return new WaitForSeconds(0.4f);
		StartDialogue(dialogueParticipants, dialogueEntriesRecieved, triggerDialogueFinished, triggerDialogueStarted, showAllCharactersOnStart, localFinishEvent, localFinishUnityEvent, continueOnDialogueEnd, continuationDialogueGO, useCinematicBars);
    }

	void InitDialogueEntry(List<DialogueParticipant> dialogueParticipants, List<DialogueEntry> dialogueEntriesRecieved, bool showAllCharactersOnStart, bool continueOnDialogueEnd, GameObject continuationDialogueGO)
    {
		cutscene = false;
		index = 0; //Dialogue Entry Index
		dialogueIndexCheck = 0; //Per Character Index -- used for SFX
		currentDialogueEntries = dialogueEntriesRecieved; //Incomming Dialogue sequence
		currentDialogueParticipants = dialogueParticipants;
		showAllCharactersOnDialogueStart = showAllCharactersOnStart;

		_continueOnDialogueEnd = continueOnDialogueEnd;
		_continuationDialogueGO = continuationDialogueGO;

		dialogueActive = true;
	}
	void HandleOnStartEvents(bool triggerDialogueFinished, bool triggerDialogueStarted, UnityAction localFinishEvent, UnityEvent localFinishUnityEvent)
    {
		globalOnDialogueStartedEvent?.Invoke();

		triggerDialogueFinishedEvent = triggerDialogueFinished;
		if (triggerDialogueStarted) onDialogueStartedEvent?.Invoke();
		currentLocalFinishEvent = localFinishEvent;
		currentLocalFinishUnityEvent = localFinishUnityEvent;
	}

	public void StartContinuationDialogueFromReply(List<DialogueParticipant> dialogueParticipants, List<DialogueEntry> dialogueEntriesRecieved, bool triggerDialogueFinished, bool triggerDialogueStarted, bool showAllCharactersOnStart, UnityAction localFinishEvent, UnityEvent localFinishUnityEvent, bool continueOnDialogueEnd, GameObject continuationDialogueGO)
    {
		CleanupDialogueReplyButtons();
		_replyButtonsActive = false;

		InitDialogueEntry(dialogueParticipants, dialogueEntriesRecieved, showAllCharactersOnStart, continueOnDialogueEnd, continuationDialogueGO);
		HandleOnStartEvents(triggerDialogueFinished, triggerDialogueStarted, localFinishEvent, localFinishUnityEvent);

		ContinueDialogue();
	}
    #endregion

    private void Update()
    {
        if(startTiming) durationOfText += Time.deltaTime;
        
		//Hack for TakoIntro as controller doesn't exist yet
		if (canSkip && dialogueActive)
		{
			if (openingCutscene) return;
			if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				ContinueDialogue();
			}
		}
    }

	#region Main Dialogue System
	void ContinueDialogueDelay()
    {
		ContinueDialogue();
    }
	public void ForceContinueToNextLine()
	{
		if (dialogueInProgress) index++;
		
		//Turn Off Continue dialogue prompt
		TriggerDialoguePrompt(false);

		//Play Interact Sound
		//AudioManager.instance.PlayOneShot(FMODEvents.instance.uiEvents.eToSkip);

		//Start next dialogue or end dialogue if no more dialogue left
		if (index >= currentDialogueEntries.Count) EndDialogue();
		else StartCoroutine(DisplayNextDialogueCoroutine(currentDialogueEntries[index].dialogue));
	}
	public void ContinueDialogue(bool conditionMet = true)
	{
		if (dialogueButtonsActive || !dialogueActive || _replyButtonsActive) return;

		if (DialogueInProgress)
		{
			if (currentDialogueEntries[index].customOptions != null)
			{
				if (currentDialogueEntries[index].customOptions.nonSkippable) return;
			}
		}

		//if dialogue in progress - skip to end
		if (dialogueInProgress && !cutscene)
		{
			//Stop Dialogue & Update it to the correct dialogue UI & Text
			if (usePluginSystem) typewriterCore.SkipTypewriter();
			else
			{
				StopAllCoroutines();
				GetDialogueBox();

				dialogueText.text = currentDialogueEntries[index].dialogue;
				OnCurrentDialogueFinished();
			}
			return;
		}

		//Turn Off Continue dialogue prompt
		TriggerDialoguePrompt(false);

		//Play Interact Sound
		//AudioManager.instance.PlayOneShot(FMODEvents.instance.uiEvents.eToSkip);

		//Start next dialogue or end dialogue if no more dialogue left
		if (index >= currentDialogueEntries.Count) EndDialogue();
		else
        {
			if (conditionMet)
				if (CheckIfEndEarly()) return;

			StartCoroutine(DisplayNextDialogueCoroutine(currentDialogueEntries[index].dialogue));
		}
	}

	bool CheckIfEndEarly()
	{
		int indexCheck = index - 1;
		if (index == 0) indexCheck = 0;

		if (currentDialogueEntries[indexCheck].customOptions == null) return false;
		if (!DialogueInProgress && currentDialogueEntries[indexCheck].customOptions.endDialogue)
		{
			if (!currentDialogueEntries[indexCheck].customOptions.triggerEvents) ClearDialgoueEvents();
			ForceEndDialogue();
			return true;
		}

		return false;
	}

	bool dialogueSFXWasTempDisabled = false;
	IEnumerator DisplayNextDialogueCoroutine(string currentDialogue)
    {
		dialogueInProgress = true;
		dialogueSFXWasTempDisabled = false;
		dialogueReplyActive = false;

		yield return new WaitForSeconds(0.15f);

		if (dialogueText != null) dialogueText.text = "";

		if (!cutscene || cutscene && useCharacterDialogueBoxes)
        {
			GetDialogueBox();

			while (panelTweenActive) yield return new WaitForSeconds(0.1f); //Wait for all the UI to Init before starting animations
			if (currentSpeaker.isNewSpeaker && speakerIMGWasActive)
            {
				InitSprite(currentSpeakerIMG, currentSpeaker.dialogueProfile);
				yield return new WaitForSeconds(spriteSwapDuration);
			}

			GetDialogueAnimations(true);

			//Trigger This Entries DialogueStart Event
			if (currentDialogueEntries[index].events != null) currentDialogueEntries[index].events.onDialogueStartEvent?.Invoke();

			#region Reply Buttons
			if (currentDialogueEntries[index].dialogueButtonOptions != null)
			{
				if (currentDialogueEntries[index].dialogueButtonOptions.showDialogueReplyButtons)
				{
					dialogueReplyActive = true;
					int _index = 0;
					Button firstButton = null;
					parentReplyButtonGO = activeDialoguePanel.replyButtonHolderGO;

					while (panelTweenActive) yield return new WaitForSeconds(0.1f);

					foreach (DialogueReplyButton dialogueReply in currentDialogueEntries[index].dialogueButtonOptions.replyOptions)
					{
						if (dialogueReply.hideOnSelect && dialogueReply.replySelected)
						{
							_index++;
							continue;
						}

						//Spawn & Set Buttons
						Button replyButton = Instantiate(dialogueReplyPrefab, parentReplyButtonGO.transform).GetComponent<Button>();
						replyButton.GetComponentInChildren<TMP_Text>().text = "> " + dialogueReply.replyText;
						replyButton.onClick.AddListener(dialogueReply.onSelectedEvent.Invoke);
						replyButton.onClick.AddListener(delegate { dialogueReply.replySelected = true; });

						_replyButtonsActive = true;
						if (dialogueReply.endDialogueOnSelect) replyButton.onClick.AddListener(ForceEndDialogue);
						else if (dialogueReply.startNewTrigger)
						{
							dialogueActive = false;
							if (dialogueReply.triggerLocalEndEvent) replyButton.onClick.AddListener(TriggerLocalDialogueEndEvents);
						}
						else replyButton.onClick.AddListener(ContinueAndCleanDialogueReplyButtons);

						if (_index == 0) firstButton = replyButton;
						_index++;
					}

					firstReplyButton = firstButton;
					yield return new WaitForSeconds(0.15f);

					//Update Dialogue System
					dialogueText.transform.parent.parent.gameObject.SetActive(true); //HACK to make sure the ButtonParentGO is on
					dialogueInProgress = false;
					index++;

					firstButton.Select();
					yield break;
				}
			}
            #endregion
        }

		//Handle Custom Options
		HandleDialogueEntryOptions(currentDialogueEntries[index]);

		if (usePluginSystem)
        {
			//Check For Text Colours & Update Text Accordingly
			if (currentDialogueEntries[index].customOptions != null)
			{
				if (currentDialogueEntries[index].customOptions.coloursToCheck != null)
					if (currentDialogueEntries[index].customOptions.coloursToCheck.Length > 0) currentDialogue = CheckForColouredText(currentDialogue, currentDialogueEntries[index].customOptions.coloursToCheck);
			}

			//Show Text
			dialogueText.gameObject.SetActive(true);
			dialogueText.transform.parent.parent.gameObject.SetActive(true); //HACK

			if(typewriter.gameObject.activeInHierarchy) typewriter.ShowText(currentDialogue);
		}
    }

	bool _dialogueSkippable = true, _hideSpeakerName = false;
	float _fontSize, _textSpeed;
	void HandleDialogueEntryOptions(DialogueEntry currentDialogueEntry)
    {
		if (currentDialogueEntry.customOptions == null) return;

		//Text Alignment -- CHANGE: This to auto set the alignment & have the alignment options displayed
		if (currentDialogueEntry.customOptions.useCentreAlignment)
		{
			//EG: dialogueText.alignment = currentDialogueEntry.customOptions.TextAlignment;
			dialogueText.alignment = TextAlignmentOptions.Center;
			dialogueText.verticalAlignment = VerticalAlignmentOptions.Middle;
		}
		else
		{
			dialogueText.alignment = TextAlignmentOptions.Left;
			dialogueText.verticalAlignment = VerticalAlignmentOptions.Top;
		}

		//Font Size
		_fontSize = currentDialogueEntry.customOptions.fontSize;

		//HACK: Only make font size = defined size if size is not 0. Unity seems to serialise it to 0 by default sometimes.
		if (_fontSize == 0) _fontSize = 50f; //DEFAULT SIZE
		if (!useOldPanels) _fontSize -= 10f; //Make it smaller for new panels -- make this more robust

		dialogueText.fontSize = _fontSize;

		//Set Speeds
		if (usePluginSystem)
		{
			if (narrationDialogue)
            {
				typewriter.waitForNormalChars = 0.0175f;
				activeDialoguePanelManager.HandleNarrationBlurryTextHack();
			}
			else
            {
				//HACK: Only make SPEED = defined speed if not 0. Unity seems to serialise it to 0 by default sometimes.
				_textSpeed = currentDialogueEntry.customOptions.typeSpeed;
				if (_textSpeed == 0) _textSpeed = 0.02f; //DEFAULT SPEED
				typewriter.waitForNormalChars = _textSpeed;
			}

			//Plugin Specifc
			typewriter.avoidMultiplePunctuationWait = currentDialogueEntry.customOptions.avoidMultiplePunctuactionWait;
		}
		else
		{
			if (narrationDialogue) dialogueSpeed = 0.02f;
			else dialogueSpeed = currentDialogueEntry.customOptions.typeSpeed;
		}
	}

	public void OnCurrentDialogueFinished()
	{
		if (!dialogueActive) return; //HACK: For some reason text animator calls OnTextShowed() at beginning & end?

		dialogueInProgress = false;
		useDialogueSound = true;

		if(!cutscene || cutscene && useCharacterDialogueBoxes)
        {
			GetDialogueAnimations(false);
			if (narrationDialogue) activeDialoguePanelManager.HandleNarrationBlurryTextHack();
		}

		//Show Dialogue Buttons / Continue dialogue prompt
		if (!cutscene)
        {
			if (currentDialogueEntries[index].dialogueButtonOptions.showButtonsAfterDialogue)
			{
				dialogueButtonsActive = true;
				if(currentDialogueEntries[index].dialogueButtonOptions.useSmallButtons)
                {
					leftButton = activeDialoguePanel.smallLeftButton;
					rightButton = activeDialoguePanel.smallRightButton;
					parentButtonGO = leftButton.transform.parent.gameObject;
				}
				else
				{
					leftButton = activeDialoguePanel.leftButton;
					rightButton = activeDialoguePanel.rightButton;
					parentButtonGO = leftButton.transform.parent.gameObject;
				}

				leftButton.GetComponentInChildren<TMP_Text>().text = currentDialogueEntries[index].dialogueButtonOptions.leftButtonText;
				rightButton.GetComponentInChildren<TMP_Text>().text = currentDialogueEntries[index].dialogueButtonOptions.rightButtonText;
				leftButton.onClick.AddListener(currentDialogueEntries[index].dialogueButtonOptions.leftButtonEvent.Invoke);
				rightButton.onClick.AddListener(currentDialogueEntries[index].dialogueButtonOptions.rightButtonEvent.Invoke);
				Invoke("TurnOnButtonParentGOHack", 0.1f);
				return;
			}
			else TriggerDialoguePrompt(true);
		}

		//Trigger This Entries DialogueFinish Event
		if (currentDialogueEntries[index].events != null) currentDialogueEntries[index].events.onDialogueFinishEvent?.Invoke();

		//Progress Dialogue Position
		index++;
		//Auto Start Next Dialogue
		if (cutscene || interruptDialogue)
		{
			interruptDialogue = false;
			if (cutscene && !openingCutscene) Invoke("ContinueDialogueDelay", onScreenTextDelay);
			else ContinueDialogue();
		}
	}

	void ForceEndDialogue()
    {
		dialogueActive = true;
		_continueOnDialogueEnd = false; //TODO: We may not always want this to be skipped?
		EndDialogue();
	}
	public void EndDialogue()
    {
		if (!dialogueActive) return;

		dialogueReplyActive = false;
		dialogueActive = false;
		if (_continueOnDialogueEnd)
        {
			_continuationDialogueGO.SetActive(true);
			return;
        }

		if (cutscene) Debug.Log("DONE! Duration: " + durationOfText);

		startTiming = false;
		dialogueButtonsActive = false;
		listenersActive = false;

		if (!cutscene || cutscene && useCharacterDialogueBoxes)
        {
			//When dialogue ends, update the game state
			if(GameManager.instance != null) GameManager.instance.UpdateDialogueGameState(false);
			ResetAndClearDialogue();
		}

		//Trigger Dialogue Finished Event
		globalOnDialogueFinishedEvent?.Invoke();
		if (triggerDialogueFinishedEvent) onDialogueFinishEvent?.Invoke();
		if (currentLocalFinishEvent != null) currentLocalFinishEvent.Invoke();
		TriggerLocalDialogueEndEvents();
		ClearDialgoueEvents();

		if (CinematicBars.instance != null) CinematicBars.instance.HideBars();
	}
	void TriggerLocalDialogueEndEvents()
	{
		currentLocalFinishUnityEvent?.Invoke();
		ClearDialgoueEvents();
	}
	void ClearDialgoueEvents()
    {
		currentLocalFinishEvent = null;
		currentLocalFinishUnityEvent = null;
	}
	#endregion

	#region Utility Functions
	void TurnOnButtonParentGOHack()
	{
		parentButtonGO.SetActive(true);
	}

	void TriggerDialoguePrompt(bool setActive)
	{
		//Turn On/Off Continue dialogue prompt
		if (index <= 1 && tutorialPrompt != null)
		{
			tutorialPrompt.SetActive(setActive);

			//Hack to stop this from turning on again
			if (!setActive) tutorialPrompt = null;
		}
		else
		{
			if (normalPrompt != null) normalPrompt.SetActive(setActive);
		}
	}
	void ResetAndClearDialogue()
	{
		activeDialoguePanelManager.DisableAllSprites();
		activeDialoguePanelManager.DisableAllDialoguePanels();
		participantAnimators.Clear();
		participantAnimatorsRef.Clear();

		//Turn Off Continue dialogue prompt
		TriggerDialoguePrompt(false);

		foreach (Animator animator in participantAnimators.Values)
		{
			animator.speed = 1;
		}

		if (dialogueText != null)
		{
			dialogueText.text = "";
			dialogueText.color = Color.white;
		}
		if (parentButtonGO != null)
        {
			parentButtonGO.SetActive(false);
			leftButton.onClick.RemoveAllListeners();
			rightButton.onClick.RemoveAllListeners();
		}

		CleanupDialogueReplyButtons();
		_replyButtonsActive = false;

		if (!useOldPanels)
		{
			if (bottomBorderIMG == null) return;
			if (bottomBorderIMGTween != null) bottomBorderIMGTween.Kill();
			bottomBorderIMGTween = bottomBorderIMG.DOFade(0, 0.5f).SetEase(Ease.Linear).OnComplete(delegate { bottomBorderIMG.gameObject.SetActive(false); });
			bottomBorderIMGTween.Play();
		}
	}

	bool _replyButtonsActive = false;
	void ContinueAndCleanDialogueReplyButtons()
    {
		CleanupDialogueReplyButtons();
		Invoke("ContinueDialogueFromReply", 0.1f);
	}
	void ContinueDialogueFromReply()
    {
		_replyButtonsActive = false;
		ContinueDialogue();
    }
	void CleanupDialogueReplyButtons()
    {
		if (parentReplyButtonGO != null)
		{
			foreach (Transform child in parentReplyButtonGO.transform)
			{
				Destroy(child.gameObject);
			}
			parentReplyButtonGO = null;
		}
	}

	void InitSprite(GameObject dialogueIMG, DialogueProfile_SO dialogueProfie)
	{
		if (dialogueProfie.dialogueSprite == null) return;
		if (dialogueIMG == null) return;
		if (participantAnimators.ContainsKey(dialogueProfie) && dialogueIMG.activeSelf)
        {
			if (currentSpeaker.dialogueProfile == dialogueProfie && currentSpeaker.isNewSpeaker) Debug.Log("Swap Speaker");
			else
            {
				Debug.Log("Early Exit from InitSprite");
				return;
            }
        }

		if (HandleSpriteSwap(dialogueIMG, dialogueProfie)) return;

		//Setup Sprite
		dialogueIMG.GetComponent<Image>().sprite = dialogueProfie.dialogueSprite;
		dialogueIMG.GetComponent<Image>().SetNativeSize();

		//Get Animator
		Animator currentAnimator = dialogueIMG.GetComponent<Animator>();
		currentAnimator.runtimeAnimatorController = dialogueProfie.dialogueAnimator;
		currentAnimator.speed = 1;

		if (participantAnimators.ContainsKey(dialogueProfie)) participantAnimators.Remove(dialogueProfie);
		participantAnimators[dialogueProfie] = currentAnimator;
		participantAnimatorsRef.Add(currentAnimator);

		if (participantSpriteGOs.ContainsKey(dialogueProfie)) participantSpriteGOs.Remove(dialogueProfie);
		participantSpriteGOs[dialogueProfie] = dialogueIMG;

		//Handle Entrance/Exit Tweens
		DialogueSpriteTweenManager dialogueSpriteTweenManager = dialogueIMG.GetComponent<DialogueSpriteTweenManager>();
		if (dialogueSpriteTweenManager != null)
		{
			DialogueParticipant participant_ = GetParticipantFromCurrentParticipants(dialogueProfie);
			dialogueSpriteTweenManager.myEntryTween = participant_.spriteEntryTween;
			dialogueSpriteTweenManager.myExitTween = participant_.spriteExitTween;
		}

		dialogueIMG.SetActive(true);
	}

	float spriteSwapDuration;
	bool HandleSpriteSwap(GameObject dialogueIMG, DialogueProfile_SO dialogueProfie)
    {
		if (!dialogueIMG.activeInHierarchy) return false;

		DialogueSpriteTweenManager dialogueSpriteTweenManager = dialogueIMG.GetComponent<DialogueSpriteTweenManager>();
		dialogueSpriteTweenManager.DisableSprite();
		spriteSwapDuration = dialogueSpriteTweenManager.disableDuration + spriteSwapDelay;
		StartCoroutine(AppendNewDialogueSprite(dialogueIMG, dialogueProfie));
		return true;
    }
	IEnumerator AppendNewDialogueSprite(GameObject dialogueIMG, DialogueProfile_SO dialogueProfie)
    {
		yield return new WaitForSeconds(spriteSwapDuration - spriteSwapDelay);
		SetDialogueImageOffset(currentSpeakerIMG, currentSpeaker.dialogueProfile);
		yield return new WaitForSeconds(spriteSwapDelay / 2);
		InitSprite(dialogueIMG, dialogueProfie);
	}

	DialogueParticipant GetParticipantFromCurrentParticipants(DialogueProfile_SO profile)
    {
        foreach (DialogueParticipant dialogueParticipant in currentDialogueParticipants)
        {
            if (dialogueParticipant.dialogueProfile == profile)
            {
                return dialogueParticipant;
            }
        }

		return null;
    }

	DialogueParticipantGeneric participantGeneric;
	bool speakerIMGWasActive = false; //This bool is used to make sure we ONLY use the 'NewSpeaker' swap when it truly is a new speaker (aka speakerIMGActive) -- otherwise it can cause buggy behaviour with tween movements
	void GetDialogueBox()
    {
		//Init Refs
		narrationDialogue = false;
		currentSpeaker = currentDialogueEntries[index].speaker;
		if (currentSpeaker.dialogueProfile.characterName == "Narrator") narrationDialogue = true;

		//Get Dialogue Box
		if (currentSpeaker.overrideDialogueBoxType) ShowDialoguePanel(FindDialogueBox(currentSpeaker.dialogueBox));
		else ShowDialoguePanel(FindDialogueBox(GetParticipantFromCurrentParticipants(currentSpeaker.dialogueProfile).dialogueBox));

		//Setup Speaker IMG & SFX
		if (currentSpeaker.isNewSpeaker && speakerIMGWasActive) dialogueSpriteToShow = null;
		else dialogueSpriteToShow = delegate { InitSprite(currentSpeakerIMG, currentSpeaker.dialogueProfile); };
		currentDialogueClipRef = currentSpeaker.dialogueProfile.voiceClip;
		dialogueText.text = "";
		if (currentSpeaker.dialogueProfile.overrideSFXFrequency) SFXFrequency = currentSpeaker.dialogueProfile.SFXFrequency;
		else SFXFrequency = defaultSFXFrequency;
	}

	bool innerPanel_ = false;
	DialoguePanel GetDialoguePanel(DialogueBoxType dialogueBoxType)
    {
		if (dialogueBoxType == DialogueBoxType.InnerLeftShort || dialogueBoxType == DialogueBoxType.InnerLeftLong || dialogueBoxType == DialogueBoxType.InnerRightShort || dialogueBoxType == DialogueBoxType.InnerRightLong) innerPanel_ = true;
		else innerPanel_ = false;

		if (dialogueBoxType == DialogueBoxType.NoName) return activeDialoguePanelManager.narrationPanel;
		else if (dialogueBoxType == DialogueBoxType.Communicator) return activeDialoguePanelManager.communicatorPanel;
		else if (dialogueBoxType == DialogueBoxType.MiddleShort) return activeDialoguePanelManager.middleShort;
		else if (dialogueBoxType == DialogueBoxType.MiddleLong) return activeDialoguePanelManager.middleLong;
		else if (dialogueBoxType == DialogueBoxType.LeftShort) return activeDialoguePanelManager.leftShort;
		else if (dialogueBoxType == DialogueBoxType.LeftLong) return activeDialoguePanelManager.leftLong;
		else if (dialogueBoxType == DialogueBoxType.LeftXLong) return activeDialoguePanelManager.leftXLong;
		else if (dialogueBoxType == DialogueBoxType.InnerLeftShort) return activeDialoguePanelManager.innerLeftShort;
		else if (dialogueBoxType == DialogueBoxType.InnerLeftLong) return activeDialoguePanelManager.innerleftLong;
		else if (dialogueBoxType == DialogueBoxType.InnerLeftXLong) return activeDialoguePanelManager.innerLeftXLong;
		else if (dialogueBoxType == DialogueBoxType.RightShort) return activeDialoguePanelManager.rightShort;
		else if (dialogueBoxType == DialogueBoxType.RightLong) return activeDialoguePanelManager.rightLong;
		else if (dialogueBoxType == DialogueBoxType.InnerRightShort) return activeDialoguePanelManager.innerRightShort;
		else if (dialogueBoxType == DialogueBoxType.InnerRightLong) return activeDialoguePanelManager.innerRightLong;
		else return activeDialoguePanelManager.narrationPanel;
	}

	GameObject FindDialogueBox(DialogueBoxType dialogueBoxType)
    {
		activeDialoguePanel = GetDialoguePanel(dialogueBoxType);

		//Set Relevant Panel Refs-----

		//Set Body Text
		if (dialogueText != null) dialogueText.text = ""; //Clean prev text
		dialogueText = activeDialoguePanel.bodyText;
		dialogueText.text = "";
		typewriter = dialogueText.GetComponent<TypewriterByCharacter>();
		typewriterCore = dialogueText.GetComponent<TypewriterCore>();

		//Set Prompt
		normalPrompt = activeDialoguePanel.dialogueFinishPromptGO;

		//Set Name Tag
		if (activeDialoguePanel.nameText != null)
        {
			if (currentDialogueEntries[index].customOptions == null) activeDialoguePanel.nameText.text = currentSpeaker.dialogueProfile.characterName; //Set Name By Default
			else
			{
				//Check if name should be hidden, otherwise set name as per usual
				if (currentDialogueEntries[index].customOptions.hideSpeakerName) activeDialoguePanel.nameText.text = "???";
				else activeDialoguePanel.nameText.text = currentSpeaker.dialogueProfile.characterName;
			}
		}

		//Set Panel Sprite
		if (activeDialoguePanel.panelSprite != null)
        {
			activeDialoguePanel.panelGO.GetComponent<Image>().sprite = activeDialoguePanel.panelSprite;
			activeDialoguePanel.panelGO.GetComponent<Image>().SetNativeSize();
		}

		//Set Speaker IMG
		bool useFlippedIMG_ = false;
		if (currentSpeaker.dialogueProfile != null) useFlippedIMG_ = currentSpeaker.dialogueProfile.flipDialogueSprite;
		if (useFlippedIMG_) currentSpeakerIMG = activeDialoguePanel.flippedCharacterIMG;
		else currentSpeakerIMG = activeDialoguePanel.characterIMG;

		if (currentSpeakerIMG != null) speakerIMGWasActive = currentSpeakerIMG.activeSelf;
		if (!currentSpeaker.isNewSpeaker || !speakerIMGWasActive) SetDialogueImageOffset(currentSpeakerIMG, currentSpeaker.dialogueProfile);

		return activeDialoguePanel.panelGO;
	}
	void SetDialogueImageOffset(GameObject imageGO, DialogueProfile_SO dialogueProfile)
	{
		if (imageGO == null) return;

		DialogueParticipant participant = GetParticipantFromCurrentParticipants(dialogueProfile);
		float offset;
		if (participant.overrideOffset) offset = participant.customOffset;
		else if (innerPanel_) offset = dialogueProfile.innerOffset;
		else offset = dialogueProfile.offset;

		//Set Offset
		RectTransform currentIMGTransform = imageGO.GetComponent<RectTransform>(); ;
		currentIMGTransform.anchoredPosition = new Vector2(offset, currentIMGTransform.position.y);
	}
	public event System.Action onDialoguePanelTweenCompleteEvent;
	[HideInInspector] public float panelTweenDuration = 0.12f;
	UnityAction dialogueSpriteToShow;
	void ShowDialoguePanel(GameObject panelToShow)
    {
		activeDialoguePanelManager.DisableAllOtherDialoguePanels(panelToShow);
		panelToShow.SetActive(true);
		initPanel = false;
		currentPanelGO = panelToShow;
	}
	public void OnDialoguePanelInit()
    {
		dialogueSpriteToShow?.Invoke();
		onDialoguePanelTweenCompleteEvent?.Invoke();
		panelTweenActive = false;
    }

	bool listenersActive = false;
	void OnDialoguePanelInitListener()
    {
		if (!showAllCharactersOnDialogueStart || listenersActive) return;

		foreach (DialogueParticipant participant in currentDialogueParticipants)
		{
			DialoguePanel particpantPanel = GetDialoguePanel(participant.dialogueBox);
			GameObject listenerIMG;

			//Get IMG
			bool useFlippedIMG_ = false;
			if (participant.dialogueProfile != null) useFlippedIMG_ = participant.dialogueProfile.flipDialogueSprite;
			if (useFlippedIMG_) listenerIMG = particpantPanel.flippedCharacterIMG;
			else listenerIMG = particpantPanel.characterIMG;
			SetDialogueImageOffset(listenerIMG, participant.dialogueProfile);

			InitSprite(listenerIMG, participant.dialogueProfile);
		}

		listenersActive = true;
	}

	List<DialogueListener> listeners_ = new List<DialogueListener>();
	void GetDialogueAnimations(bool talking)
    {
		if(checkForPausedAnimators)
        {
			checkForPausedAnimators = false;
			foreach (Animator animator in participantAnimators.Values)
			{
				animator.speed = 1;
			}
		}

		if (currentSpeaker.dialogueProfile.dialogueSprite == null || currentSpeaker.dialogueProfile.dialogueAnimator == null || currentSpeaker.talkAnimation == null) currentSpeakerAnimator = null;
		else
        {
			//if (!participantAnimators.ContainsKey(currentSpeaker.dialogueProfile)) InitSprite(currentSpeakerIMG, currentSpeaker.dialogueProfile);
			if (!participantAnimators.ContainsKey(currentSpeaker.dialogueProfile))
			{
				Debug.LogError("Dialogue Profile: " + currentSpeaker.dialogueProfile + " Not Found in Participants");
				return;
			}
			currentSpeakerAnimator = participantAnimators[currentSpeaker.dialogueProfile];
			currentSpeakerAnimator.ResetTrigger("Idle"); //Need to reset here as the trigger can stay active if a transition doesn't occur
			currentSpeakerAnimator.speed = 1;
		}

		listeners_ = currentDialogueEntries[index].listeners;

		if (talking) //Play Talk Animations
        {
			if (currentSpeakerAnimator != null)
            {
				currentSpeakerAnimator.Play(currentSpeaker.talkAnimation.name);
				if(currentSpeaker.overrideSortLayer)
                {
					Canvas canvas = currentSpeakerAnimator.GetComponent<Canvas>();
					if (canvas == null)
					{
						Canvas newCanvas = currentSpeakerAnimator.gameObject.AddComponent<Canvas>();
						newCanvas.overrideSorting = true;
						newCanvas.sortingLayerName = "UI";
						newCanvas.sortingOrder = 1001;
					}
					else canvas.sortingOrder = 1001;
				}
			}

			foreach (DialogueListener listener in listeners_)
            {
				if(participantAnimators.ContainsKey(listener.dialogueProfile))
                {
					if (listener.idleAnimation != null && listener.idleAnimation.name != "") participantAnimators[listener.dialogueProfile].Play(listener.idleAnimation.name);
					Canvas canvas = participantAnimators[listener.dialogueProfile].GetComponent<Canvas>();
					if (canvas != null && canvas.overrideSorting) canvas.sortingOrder = 1000;
				}

				if (listener.overrideOffset)
                {
					participantSpriteGOs[listener.dialogueProfile].GetComponent<RectTransform>().DOAnchorPosX(listener.newXOffset, listener.tweenDuration); //Adjust Offset

					//Store the offset ref
					DialogueParticipant participant = GetParticipantFromCurrentParticipants(listener.dialogueProfile);
					participant.overrideOffset = true;
					participant.customOffset = listener.newXOffset;
				}
			}
		}
		else //Play Idle Animations
        {
			//SPEAKER: 
			if (currentSpeakerAnimator != null)
			{
				//Play Paired Idle Animation if Idle Anim Not Specified (EG: Happy Talk Anim will trigger Happy Idle Anim By Default)
				if (currentSpeaker.additionalAnims && currentSpeaker.idleAnimation != null) currentSpeakerAnimator.Play(currentSpeaker.idleAnimation.name);
				else currentSpeakerAnimator.SetTrigger("Idle");
			}

			//LISTENERS:
			foreach (DialogueListener listener in listeners_)
			{
				if (participantAnimators.ContainsKey(listener.dialogueProfile))
				{
					//Play Idle Anim By Default if React Anim Not Specified
					if (listener.additionalAnims && listener.reactAnimation != null) participantAnimators[listener.dialogueProfile].Play(listener.reactAnimation.name);
					else if (listener.idleAnimation != null) participantAnimators[listener.dialogueProfile].Play(listener.idleAnimation.name);
				}
			}
		}
	}

	public static string CheckForColouredText(string currentDialogue, TextColour[] textColour_)
	{
		Debug.Log("Checking For Coloured Text");
		string newColouredDialogue_ = currentDialogue;

		foreach (TextColour colour_ in textColour_)
		{
			newColouredDialogue_ = newColouredDialogue_.Replace(GetStringColourRef(colour_), TextColours.GetColour(colour_));
		}

		return newColouredDialogue_;
	}
	public static string GetStringColourRef(TextColour textColour_)
    {
		if (textColour_ == TextColour.Green) return "<green>";
		else if (textColour_ == TextColour.Yellow) return "<yellow>";
		else if (textColour_ == TextColour.Red) return "<red>";
		else if (textColour_ == TextColour.Pink) return "<pink>";
		else if (textColour_ == TextColour.Aqua) return "<aqua>";
		else return "<yellow>";
	}

	public void TriggerDialogueSFX(char c)
    {
		TriggerDialogueSFX(c, currentDialogueClipRef);
	}
	public void TriggerDialogueSFX(char c, AudioClip dialogueClip)
	{
		if(dialogueClip == null) return;

		//Handle Dialogue SFX------------------------------
		if (useDialogueSound)
		{
			if (c == '.') return;

			//Play Dialogue SFX - might need to seperate this from the main coroutine
			dialogueIndexCheck += 1;

			if (useCustomSFXFrequency)
			{
				if (dialogueIndexCheck % SFXFrequency == 0)
				{
					AudioManager.instance.PlayOneShot(dialogueClip);
				}
			}
			else AudioManager.instance.PlayOneShot(dialogueClip);
		}
		//---------------------------------------------------
	}

	public bool DialogueButtonsActiveCheck()
    {
		if (DialogueButtonsActive)
		{
			LeftButton.Select();
			return true;
		}
		if (DialogueReplyActive)
        {
			firstReplyButton.Select();
			return true;
        }

		return false;
	}

	#endregion

	#region Narration Message

	public void ShowNarrationMessage(string newMessage_)
	{
		if(narrationMessageTrigger == null)
		{
			Debug.LogError("Narration Dialogue Trigger Missing");
			return;
		}

		narrationMessageTrigger.UpdateSingleEntryDialogue(newMessage_);
		narrationMessageTrigger.TriggerDialogue();
	}

	#endregion
}
