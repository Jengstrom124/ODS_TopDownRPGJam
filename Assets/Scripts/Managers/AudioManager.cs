using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using DG.Tweening;

//    -------------------------------------------
//    Handles Calling/Triggering all Game Audio
//    Use FMODEvents to source audio files to Trigger!
//    EG AudioManager.instance.PlayOneShot(FMODEvents.instance.yourSfx)
//    EG AudioManager.instance.PlayOneShot(FMODEvents.instance.playerBattleEvents.yourSfx)
//    -------------------------------------------
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    List<EventInstance> eventInstances;
    public EventReference testEvent;
    public EventInstance currentInstance;

    //TODO: UPDATE WITH FMOD
    public AudioSource titleScreenAudioSourceHack;
    AudioSource oneshotAudioSource;

    private void Awake()
    {
        instance = this;
        eventInstances = new List<EventInstance>();

        oneshotAudioSource = GetComponent<AudioSource>();
    }

    #region OneShot
    public void PlayOneShot(AudioClip sfx)
    {
        if(sfx == null) return;
        oneshotAudioSource.PlayOneShot(sfx);
    }
    public void PlayOneShot(EventReference sfx)
    {
        RuntimeManager.PlayOneShot(sfx);
    }
    public void PlayOneShot(EventReference sfx, float delay)
    {
        StartCoroutine(PlayOneShotCoroutine(sfx, delay));
    }
    IEnumerator PlayOneShotCoroutine(EventReference sfx, float delay)
    {
        yield return new WaitForSeconds(delay);

        RuntimeManager.PlayOneShot(sfx);
    }
    #endregion

    #region MODULAR SOUNDINSTANCE SETUP
    /// <summary>
    /// Play or Stop an EventInstance (looping sfx/track)
    /// </summary>
    /// <param name="soundInstance">The EventInstance you want to call</param>
    /// <param name="playSound">True = Play. False = Stop.</param>
    public void PlaySoundInstance(EventInstance soundInstance, bool playSound, bool allowFadeout = true)
    {
        PLAYBACK_STATE soundInstanceState;
        soundInstance.getPlaybackState(out soundInstanceState);

        if (playSound)
        {
            if (soundInstanceState == PLAYBACK_STATE.STOPPED) soundInstance.start();
        }
        else
        {
            if (soundInstanceState == PLAYBACK_STATE.PLAYING)
            {
                if(allowFadeout) soundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                else soundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }
    }

    /// <summary>
    /// Play an EventInstance for the given duration.
    /// </summary>
    /// <param name="soundInstance">The EventInstance you want to call</param>
    /// <param name="duration">Duration in seconds before EventInstance is stopped.</param>
    public void PlaySoundInstance(EventInstance soundInstance, float duration)
    {
        PLAYBACK_STATE soundInstanceState;
        soundInstance.getPlaybackState(out soundInstanceState);

        if (soundInstanceState == PLAYBACK_STATE.STOPPED) soundInstance.start();

        StartCoroutine(StopSoundInstanceCoroutine(soundInstance, duration));
    }
    IEnumerator StopSoundInstanceCoroutine(EventInstance soundInstance, float delay)
    {
        yield return new WaitForSeconds(delay);

        soundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    /// <summary>
    /// Play or Stop an EventInstance based on Delay.
    /// </summary>
    /// <param name="soundInstance">The EventInstance you want to call</param>
    /// <param name="playSound">True = Play. False = Stop.</param>
    /// <param name="delay">Delay in seconds before EventInstance is Started or Stopped.</param>
    public void PlaySoundInstance(EventInstance soundInstance, bool playSound, float delay)
    {
        StartCoroutine(StartSoundInstanceCoroutine(soundInstance, playSound, delay));
    }
    IEnumerator StartSoundInstanceCoroutine(EventInstance soundInstance, bool playSound, float delay)
    {
        yield return new WaitForSeconds(delay);

        PlaySoundInstance(soundInstance, playSound);
    }
    #endregion

    #region Utility
    public void SetInstanceParameter(EventInstance instance, string name, int value)
    {
        instance.setParameterByName(name, value);
    }

    public EventInstance CreateEventInstance(EventReference sound)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(sound);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }
    void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }
    private void OnDestroy()
    {
        CleanUp();
    }
    #endregion
}
