using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] bool forceBad = false;
    [SerializeField] GameObject goodDialogue, badDialogue;
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

    public void Exterminate()
    {
        gameObject.SetActive(false);
    }
}
