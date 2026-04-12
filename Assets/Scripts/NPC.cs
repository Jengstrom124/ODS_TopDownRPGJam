using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using UnityEngine;
using DG.Tweening;

public class NPC : MonoBehaviour
{
    [Header("Main Setup")]
    [SerializeField] bool forceBad = false;
    [SerializeField] GameObject goodDialogue, badDialogue;
    [SerializeField] AudioClip startSFX, deathSFX;

    [Header("Death Tween")]
    [SerializeField] float spinDuration = 1f;
    [SerializeField] Vector3 deathJumpPos = new Vector3(100, 100, 0);
    [SerializeField] float jumpPower = 5f, jumpDuration = 3f;
    //REF
    [SerializeField] bool isGood = true;

    void Start()
    {
        if(forceBad)
        {
            badDialogue.SetActive(true);
            return;
        }


        int i = Random.Range(0, 2);
        if(i == 1) isGood = false;
        else isGood = true;

        if(isGood) goodDialogue.SetActive(true);
        else badDialogue.SetActive(true);
    }

    public void OnConversationStart()
    {
        if(startSFX == null) return;       
        AudioManager.instance.PlayOneShot(startSFX);
    }

    [ContextMenu("Debug/Exterminate")]
    public void Exterminate()
    {
        AudioManager.instance.PlayOneShot(deathSFX);
        transform.DORotate(new Vector3(0, 0, 1080f), spinDuration, RotateMode.FastBeyond360);
        transform.DOJump(transform.position + deathJumpPos, jumpPower, 1, jumpDuration).SetEase(Ease.Linear).OnComplete(DisableGO);
    }
    void DisableGO()
    {
        gameObject.SetActive(false);
    }
}
