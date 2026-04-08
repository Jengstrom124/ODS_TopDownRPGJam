using UnityEngine;
using Febucci.UI.Core;
using Febucci.UI.Core.Parsing;
using Febucci.UI;
using TMPro;
using FMODUnity;
using NaughtyAttributes;

public class TypewriterEventsManager : MonoBehaviour
{
    [SerializeField] TypewriterCore typewriterCore;
	[SerializeField] TypewriterByCharacter typewriter;
	[SerializeField] TextAnimator_TMP textAnimator;
	[SerializeField] bool dialogueSystemTypewriter = false;
	[SerializeField, HideIf("dialogueSystemTypewriter")] AudioClip myDialogueClip;

	private void Awake()
	{
		if(dialogueSystemTypewriter)
        {
			if (DialogueManager.instance.usePluginSystem)
			{
				typewriterCore.onMessage.AddListener(OnMessage);
				typewriter.waitForNormalChars = DialogueManager.instance.defaultTypewriterDialogueSpeed;
			}
			else
			{
				Destroy(typewriterCore);
				Destroy(textAnimator);
			}
		}
		else
        {
			typewriterCore.onMessage.AddListener(OnMessage);
		}
	}
    private void OnDestroy()
    {
		typewriterCore.onMessage.RemoveListener(OnMessage);
	}

    void OnMessage(EventMarker eventData)
	{
		var eventName = eventData.name;

		if (eventName == "cont") //<?cont>
		{
			DialogueManager.instance.InterruptDialogue = true;
		}
		else if (eventName == "pause")
		{
			DialogueManager.instance.CurrentSpeakerAnimator.speed = 0;
			DialogueManager.instance.checkForPausedAnimators = true;
		}
		else if (eventName == "idle")
		{
			if (DialogueManager.instance.CurrentDialogueEntries.Count <= DialogueManager.instance.Index)
			{
				Debug.Log("Idle event called - index out of range");
				return;
			}

			if (!DialogueManager.instance.CurrentDialogueEntries[DialogueManager.instance.Index].speaker.additionalAnims) DialogueManager.instance.CurrentSpeakerAnimator.SetTrigger("Idle");
			else DialogueManager.instance.CurrentSpeakerAnimator.Play(DialogueManager.instance.CurrentSpeaker.idleAnimation.name);
		}
		else if (eventName == "talk")
		{
			if (DialogueManager.instance.CurrentDialogueEntries.Count <= DialogueManager.instance.Index)
			{
				Debug.Log("Idle event called - index out of range");
				return;
			}
			DialogueManager.instance.CurrentSpeakerAnimator.Play(DialogueManager.instance.CurrentSpeaker.talkAnimation.name);
		}
		else if (eventName == "resume")
		{
			DialogueManager.instance.CurrentSpeakerAnimator.speed = 1;
		}
		else if (eventName == "resume_hap")
		{
			//<?resume_hap=NAME>
			if (eventData.parameters[0] == "Takoyaki")
			{
				DialogueManager.instance.CurrentSpeakerAnimator.Play(DialogueAnimationLibrary.instance.happyTalk.name);
			}
			else if (eventData.parameters[0] == "Tempura")
			{
				DialogueManager.instance.CurrentSpeakerAnimator.Play(DialogueAnimationLibrary.instance.temHappyTalk.name);
			}
		}
		else if (eventName == "uniqueAnim")
        {
			DialogueManager.instance.CurrentSpeakerAnimator.Play(DialogueManager.instance.CurrentDialogueEntries[DialogueManager.instance.Index].speaker.uniqueAnimation.name);
		}
		else if (eventName == "blip")
		{
			AudioManager.instance.PlayOneShot(DialogueManager.instance.currentDialogueClipRef);
		}
		else if (eventName == "blipBark")
		{
			AudioManager.instance.PlayOneShot(myDialogueClip);
		}
		else if (eventName == "mute")
		{
			DialogueManager.instance.UseDialogueSound = false;
		}
		else if (eventName == "event")
		{
			DialogueManager.instance.CurrentDialogueEntries[DialogueManager.instance.Index].events.otherEventHolder?.Invoke();
		}
		else if (eventName == "oneshotSFX")
		{
			if (DialogueManager.instance.CurrentDialogueEntries[DialogueManager.instance.Index].customOptions == null) return;
			if (DialogueManager.instance.CurrentDialogueEntries[DialogueManager.instance.Index].customOptions.oneOffCustomSFX.IsNull) return;
			AudioManager.instance.PlayOneShot(DialogueManager.instance.CurrentDialogueEntries[DialogueManager.instance.Index].customOptions.oneOffCustomSFX);
		}
		else if (eventName == "color")
		{
			//<?color=red>
			//if (eventData.parameters[0] == "red") dialogueText.text.Insert(eventData.index, TextColours.red); //Not working yet :(
		}
		else if (eventName.Contains("spriteTween")) //This can be updated to parameter
		{
			//Full Event Example = <?spriteTween_Takoyaki_Jump> (State the Character & The Tween Type)

			GameObject spriteToTween = null;

			foreach (DialogueParticipant participant in DialogueManager.instance.CurrentDialogueParticipants)
			{
				if (eventName.Contains(participant.dialogueProfile.characterName))
				{
					spriteToTween = DialogueManager.instance.ParticipantAnimators[participant.dialogueProfile].gameObject;
				}
			}

			if (spriteToTween == null)
			{
				Debug.Log("spriteToTween NULL");
				return;
			}

			if (eventName.Contains("Jump")) spriteToTween.GetComponent<DialogueSpriteTweenManager>().DOJump();
			else if (eventName.Contains("Shake")) spriteToTween.GetComponent<DialogueSpriteTweenManager>().DOShake();
			else return;
		}
	}

	public void OnCurrentDialogueFinishedCallback()
    {
		if(GetComponent<TMP_Text>().text == "")
        {
			Debug.Log("Text Reset -- Ignore Complete");
			return;
        }

		DialogueManager.instance.OnCurrentDialogueFinished();
    }
	public void TriggerDialogueSFX(char c)
    {
		if (dialogueSystemTypewriter) DialogueManager.instance.TriggerDialogueSFX(c);
		else
        {
			//Set This Up -- possible also decouple ths whole thing from the dialogue manager?
			DialogueManager.instance.TriggerDialogueSFX(c, myDialogueClip);
		}
    }
}
