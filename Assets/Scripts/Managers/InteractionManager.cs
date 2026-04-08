using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInteractionTrigger
{
    public string name;
    public GameObject triggerGO;
    public Vector2 triggerDirection;
}
public class InteractionManager : MonoBehaviour
{
    [Header("Setup")] 
    [SerializeField] PlayerInteractionTrigger[] interactionTriggers;
    [SerializeField] PlayerModel player;

    //REF
    GameObject currentTrigger;

    public void HandleMovementTriggers()
    {
        foreach (PlayerInteractionTrigger interactTrigger in interactionTriggers)
        {
            if (player.Movement == interactTrigger.triggerDirection)
            {
                currentTrigger = interactTrigger.triggerGO;
                currentTrigger.SetActive(true);
            }
            else interactTrigger.triggerGO.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // else if (collision.GetComponent<ItemPickup>() != null)
        // {
        //     currentItem = collision.GetComponent<ItemPickup>();
        //     if (!currentItem.isInteractable) return;
        //     player.interactable = currentItem;
        //     itemInRadius = true;

        //     //Check which item we are and display the result
        //     if(currentItem.isItem) player.TriggerInteractableFoundEvent(Inventory.instance.CheckCollectedItems(currentItem.item), true);
        //     else if (currentItem.isWeapon) player.TriggerInteractableFoundEvent(Inventory.instance.CheckCollectedItems(currentItem.weapon), true);
        //     else player.TriggerInteractableFoundEvent(Inventory.instance.CheckCollectedItems(currentItem.specialItem), true);
        // }
        //else 
        if (collision.GetComponent<IInteractable>() != null)
        {
            //if (itemInRadius) return;
            //if (collision.GetComponent<SceneTrigger>() != null || !collision.isTrigger) return;
            if(!collision.isTrigger) return;
            //if (DialogueManager.instance.DialogueActive) return;
            // if (collision.GetComponent<DialogueTrigger>() != null)
            //     if (collision.GetComponent<DialogueTrigger>().useTriggerBox) return;
            // if (collision.GetComponent<DoorTrigger>() != null)
            //     if (collision.GetComponent<DoorTrigger>().useOnTriggerEnter) return;


            player.SetCurrentInteractable(collision.GetComponent<IInteractable>());
            Debug.Log("Interaction Found");
            //Show 'E'
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // if (collision.GetComponent<ItemPickup>() != null)
        // {
        //     itemInRadius = false;
        //     triggerExitCountWhileItemActive = 0;

        //     currentItem = null;
        //     player.interactable = null;
        //     player.TriggerInteractableFoundEvent(false, false);
        // }
        //else 
        if (collision.GetComponent<IInteractable>() != null)
        {
            player.SetCurrentInteractable(null);
            //Hide 'E'

            TurnOnOffTrigger();
        }
    }

    void TurnOnOffTrigger()
    {
        currentTrigger.SetActive(false);
        Invoke("ResetTrigger", 0.1f);
    }
    void ResetTrigger()
    {
        currentTrigger.SetActive(true);
    }
}
