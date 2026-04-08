using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [SerializeField] PlayerInput playerInput;

    //NOTE: Not best practice. Will need refactoring if game involves more players
    //or player reference cannot be set in the scene. Quick Temp solution for now.
    [SerializeField] PlayerModel player;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitControls();
    }

    public void InitControls()
    {
        playerInput.ActivateInput();
        
        playerInput.actions.FindAction("Interact").performed += OnInteractPerformed;

        playerInput.actions.FindAction("Movement").performed += OnMovementOnperformed;
        playerInput.actions.FindAction("Movement").canceled += OnMovementOnperformed;
        playerInput.actions.FindAction("Sprint").performed += OnSprintOnperformed;
        playerInput.actions.FindAction("Sprint").canceled += OnSprintOnperformed;

        playerInput.actions.FindAction("ProgressDialogue").performed += aContext => DialogueManager.instance.ContinueDialogue();
    }

    private void OnInteractPerformed(InputAction.CallbackContext aContext)
    {
        player.Interact();
    }

    private void OnMovementOnperformed(InputAction.CallbackContext aContext)
    {
        if (aContext.phase == InputActionPhase.Performed)
        {
            //if (lockControlls) return;

            player.Movement = aContext.ReadValue<Vector2>();
            player.IsMoving = true;
        }
        else if (aContext.phase == InputActionPhase.Canceled)
        {
            player.IsMoving = false;
        }
    }

    private void OnSprintOnperformed(InputAction.CallbackContext aContext)
    {
        if (aContext.phase == InputActionPhase.Performed)
        {
            player.OnSprint(true);
        }
        else if (aContext.phase == InputActionPhase.Canceled)
        {
            player.OnSprint(false);
        }
    }
}
