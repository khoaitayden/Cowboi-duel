using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerInput inputActions;
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool RunPressed { get; private set; }
    public bool FirePressed { get; private set; }
    public bool ReloadPressed { get; private set; }
    public bool AimPressed { get; private set; }
    public bool FreeCameraPressed { get; private set; }
    public bool ShoulderTogglePressed { get; private set; }

    private void Awake()
    {
        inputActions = new PlayerInput();

        inputActions.Player.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => MoveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => LookInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => JumpPressed = true;

        inputActions.Player.Run.performed += ctx => RunPressed = true;
        inputActions.Player.Run.canceled += ctx => RunPressed = false;

        inputActions.Player.Fire.performed += ctx => FirePressed = true;

        inputActions.Player.Reload.performed += ctx => ReloadPressed = true;

        inputActions.Player.Aim.performed += ctx => AimPressed = true;
        inputActions.Player.Aim.canceled += ctx => AimPressed = false;

        inputActions.Player.FreeCamera.performed += ctx => FreeCameraPressed = true;
        inputActions.Player.FreeCamera.canceled += ctx => FreeCameraPressed = false;

        inputActions.Player.ToggleLeftOrRight.performed += ctx => ShoulderTogglePressed = true;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void FixedUpdate()
    {
        JumpPressed = false;
        FirePressed = false;
        ReloadPressed = false;
        ShoulderTogglePressed = false;
    }
}