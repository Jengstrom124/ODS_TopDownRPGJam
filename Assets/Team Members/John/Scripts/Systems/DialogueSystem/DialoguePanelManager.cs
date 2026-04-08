using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class DialoguePanel
{
	public GameObject panelGO;
	public Sprite panelSprite;
	public GameObject characterIMG;
	public GameObject flippedCharacterIMG;
	public GameObject dialogueFinishPromptGO;
	public GameObject replyButtonHolderGO;

	[Space]
	[FormerlySerializedAs("panelText")] public TMPro.TMP_Text bodyText;
	public TMPro.TMP_Text nameText;

	[Space]
	public Button leftButton;
	public Button rightButton;
	public Button smallLeftButton, smallRightButton;
}
public class DialoguePanelManager : MonoBehaviour
{
	public DialoguePanel narrationPanel;
	public DialoguePanel communicatorPanel, middleShort, middleLong;
	public DialoguePanel leftShort, leftLong, leftXLong, innerLeftShort, innerleftLong, innerLeftXLong;
	public DialoguePanel rightShort, rightLong, innerRightShort, innerRightLong;

	[Space]
	[SerializeField] GameObject[] dialogueSpriteGOs;
	[SerializeField] GameObject[] panelTextBoxes;
	GameObject[] dialoguePanelGOs;

	[SerializeField] GameObject narrationBlurryTextHackGO;
	bool narrationHackComplete = false;

    private void Awake()
    {
		dialoguePanelGOs = new GameObject[14];
		dialoguePanelGOs[0] = narrationPanel.panelGO;
		dialoguePanelGOs[1] = communicatorPanel.panelGO;
		dialoguePanelGOs[2] = middleShort.panelGO;
		dialoguePanelGOs[3] = middleLong.panelGO;

		dialoguePanelGOs[4] = leftShort.panelGO;
		dialoguePanelGOs[5] = leftLong.panelGO;
		dialoguePanelGOs[6] = leftXLong.panelGO;
		dialoguePanelGOs[7] = innerLeftShort.panelGO;
		dialoguePanelGOs[8] = innerleftLong.panelGO;
		dialoguePanelGOs[9] = innerLeftXLong.panelGO;

		dialoguePanelGOs[10] = rightShort.panelGO;
		dialoguePanelGOs[11] = rightLong.panelGO;
		dialoguePanelGOs[12] = innerRightShort.panelGO;
		dialoguePanelGOs[13] = innerRightLong.panelGO;
	}

	//-- HACK -- Doing this to workaround plugin system initialising text with "New Text"
    IEnumerator Start()
    {
		if (panelTextBoxes.Length <= 0) yield break;

		HandleInitPanelTextBoxHack(true);
		yield return new WaitForSeconds(0.15f);
		HandleInitPanelTextBoxHack(false);
	}
	void HandleInitPanelTextBoxHack(bool setActive)
	{
		Vector2 startPos;
		RectTransform tempRectTransform;
		foreach (GameObject panelTextGO in panelTextBoxes)
		{
			tempRectTransform = panelTextGO.GetComponent<RectTransform>();
			startPos = tempRectTransform.anchoredPosition;

			if(setActive) tempRectTransform.anchoredPosition = new Vector2(startPos.x, -500f); //Move out of Cam -- Before turning on

			panelTextGO.SetActive(setActive);

			if(!setActive) tempRectTransform.anchoredPosition = new Vector2(startPos.x, 50f); //Reset Pos -- after turning off
		}
	}
	void UpdatePanelTextBoxes(bool setActive)
    {
		foreach (GameObject panelTextGO in panelTextBoxes)
		{
			panelTextGO.SetActive(setActive);
		}
	}
	//------------------------------

	public void DisableAllDialoguePanels()
    {
		foreach(GameObject panelGO in dialoguePanelGOs)
        {
			DialoguePanelTweenManager panelTweenManager = panelGO.GetComponent<DialoguePanelTweenManager>();
			if (panelTweenManager != null) panelTweenManager.HidePanel();
			else panelGO.SetActive(false);
		}
	}
	public void DisableAllOtherDialoguePanels(GameObject panelToKeep)
	{
		foreach (GameObject panelGO in dialoguePanelGOs)
		{
			if (panelGO == panelToKeep) continue;
			DialoguePanelTweenManager panelTweenManager = panelGO.GetComponent<DialoguePanelTweenManager>();
			if (panelTweenManager != null) panelTweenManager.HidePanel(false);
			else panelGO.SetActive(false);
		}
	}

	float _disableDelay = 0;
	public void DisableAllSprites()
    {
		_disableDelay = 0;

		foreach (GameObject spriteGO in dialogueSpriteGOs)
        {
			if(spriteGO.activeInHierarchy)
            {
				spriteGO.GetComponent<DialogueSpriteTweenManager>().DisableSprite(_disableDelay);
				_disableDelay += 0.12f;
			}
		}
	}

	public void HandleNarrationBlurryTextHack()
    {
		if (narrationBlurryTextHackGO == null) return;

		if (narrationHackComplete) narrationBlurryTextHackGO.SetActive(false);
		else
        {
			narrationBlurryTextHackGO.SetActive(true);
			narrationHackComplete = true;
		}
    }
}
