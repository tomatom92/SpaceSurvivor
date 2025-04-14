using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSubscription : MonoBehaviour
{
    // variables

    public Vector2 MoveInput {  get; private set; } = Vector2.zero;

    public bool MenuInput { get; private set; } = false;
    public bool AbilityInput { get; private set; } = false;
    public bool FireInput { get; private set; } = false;
    



    PlayerControls _Input = null;

    private void OnEnable()
    {
        _Input = new PlayerControls();

        _Input.Player.Enable();

        _Input.Player.Move.performed += SetMovement;
        _Input.Player.Move.canceled += SetMovement;

        _Input.Player.Ability.started += SetAbility;
        _Input.Player.Ability.canceled += SetAbility;

    }
    private void OnDisable()
    {
        _Input.Player.Move.performed -= SetMovement;
        _Input.Player.Move.canceled -= SetMovement;
        _Input.Player.Ability.started -= SetAbility;
        _Input.Player.Ability.canceled -= SetAbility;
        _Input.Player.Disable();
    }

    private void Awake()
    {
        // Create a new instance of the input actions
        _Input = new PlayerControls();

        // Clear any potential persisted bindings
        _Input.asset.Disable();
        _Input.asset.Enable();
    }

    private void Update()
    {
        MenuInput = _Input.UI.Submit.WasPressedThisFrame();
        FireInput = _Input.Player.Fire.IsPressed(); // Change this to IsPressed() instead of WasPressedThisFrame()

        //Debug.Log($"Fire action value: {_Input.Player.Fire.ReadValue<float>()}");
    }


    void SetMovement(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
    }
    void SetAbility(InputAction.CallbackContext ctx)
    {
        AbilityInput = ctx.started;
    }




}
