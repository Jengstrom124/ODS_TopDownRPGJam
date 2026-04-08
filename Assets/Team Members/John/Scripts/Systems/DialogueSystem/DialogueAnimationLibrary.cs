using UnityEngine;

public class DialogueAnimationLibrary : MonoBehaviour
{
    public static DialogueAnimationLibrary instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Dialogue Profiles: ")]
    public DialogueProfile_SO takoProfile;
    public DialogueProfile_SO temProfile_Right;
    public DialogueProfile_SO temProfile_Left;

    [Header("Tako Dialogue Anims: ")]
    public AnimationClip angryClosed;
    public AnimationClip angryTalk, cheerfulClosed, cheerfultalk, cryHappyClosed, cryHappyTalk, crySadClosed, crySadTalk, crySuprisedClosed, determinedClosed, determinedTalk, discomfortClosed, discomfortTalk;
    public AnimationClip happyTalk, happyClosed, heartfeltClosed, heartfeltTalk, inquisitiveClosed, inquisitiveTalk, nervousClosed, nervousTalk, neutralTalk, neutralClosed, seriousTalk, seriousClosed, shockTalk, shockClosed, teaseTalk, teaseClosed;

    [Header("Tem Dialogue Anims: ")]
    public AnimationClip temAngryClosed;
    public AnimationClip temAngryTalk, temAwkwardClosed, temAwkwardTalk, temCaringClose, temCaringTalk, temComplainClosed, temComplainTalk, temConcernedClosed, temConcernedTalk, temFlusteredClosed, temFlusteredTalk;
    public AnimationClip temHappyTalk, temHappyClosed, temIntenseClosed, temIntenseTalk, temNeutralTalk, temNeutralClosed, temPleasantClosed, temPleasantTalk, temProtectClosed, temProtectTalk, temSeriousTalk, temSeriousClosed, temShockTalk, temShockClosed, temSmugClosed, temSmugTalk, temSuprisedClosed, temSuprisedTalk, temTiredClosed, temTiredTalk, temWorryClosed, temWorryTalk;

}
