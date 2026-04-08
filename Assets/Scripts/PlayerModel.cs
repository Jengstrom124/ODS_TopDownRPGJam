using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] InteractionManager interactionManager;
    [SerializeField] float moveSpeed = 2f;

    Rigidbody2D rb;

    //refs
    Vector2 movement;
    bool isMoving = false;
    IInteractable currentInteractable = null;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isMoving) rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    #region Getters/Setters/Properties
    public Vector2 Movement
    {
        get { return movement; }
        set
        {
            movement = value;
            interactionManager.HandleMovementTriggers();
        }
    }
    public bool IsMoving
    {
        get { return isMoving; }
        set { isMoving = value; }
    }
    public void SetCurrentInteractable(IInteractable _newInteractable)
    {
        currentInteractable = _newInteractable;
    }
    #endregion



    //Inputs
    public void Interact()
    {
        if(currentInteractable != null) currentInteractable.Interact();
    } 

    public void OnSprint(bool isSprinting)
    {
        if(isSprinting)
        {
            Debug.Log("Start Sprinting");
        }
        else
        {
            Debug.Log("Stop Sprinting");
        }
    }
}
