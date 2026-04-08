using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public PlayerInput playerInput;


    [Header("References/Debugs: ")]
    public bool timePaused = false;
    public string currentActionMap;
    public string prevActionMap;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        currentActionMap = "InGame";
    }

    public void SwitchCurrentActionMap(string mapName)
    {
        prevActionMap = playerInput.currentActionMap.name;
        playerInput.SwitchCurrentActionMap(mapName);
        currentActionMap = mapName;
    }
    public void UpdateDialogueGameState(bool inDialogue)
    {
        if (inDialogue) SwitchCurrentActionMap("InDialogue");
        else SwitchCurrentActionMap("InGame");
    }

    #region Pause/Resume Time
    public void PauseTime(bool pause)
    {
        //onTimePausedEvent?.Invoke(pause);

        if (pause) Time.timeScale = 0;
        else Time.timeScale = 1;

        timePaused = pause;
    }
    public void PauseTime(bool pause, float newTimeScale)
    {
        //onTimePausedEvent?.Invoke(pause);
        Time.timeScale = newTimeScale;
        timePaused = pause;
    }
    #endregion
}
