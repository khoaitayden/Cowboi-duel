using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Gun")]
    [SerializeField] private Gun gun;

    private InputManager inputHandler;
    private PlayerMovement playerMovement;
    private bool isAiming;

    void Start()
    {
        inputHandler = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        HandleFire();
        HandleReload();
        HandleAim();
        HandleCrouch();
    }

    private void HandleFire()
    {
        if (inputHandler.FirePressed)
            gun.TryFire();
    }

    private void HandleReload()
    {
        if (inputHandler.ReloadPressed)
            gun.StartReload();
    }

    private void HandleAim()
    {
        if (inputHandler.AimPressed && !isAiming && playerMovement.IsGrounded() && !inputHandler.RunPressed)
        {
            isAiming = true;
            ShoulderCamera shoulderCam = GetComponentInChildren<ShoulderCamera>();
            if (shoulderCam != null)
                shoulderCam.SetAiming(true);
            gun.SetAiming(true);
        }
        else if ((!inputHandler.AimPressed || !playerMovement.IsGrounded() || inputHandler.RunPressed) && isAiming)
        {
            isAiming = false;
            ShoulderCamera shoulderCam = GetComponentInChildren<ShoulderCamera>();
            if (shoulderCam != null)
                shoulderCam.SetAiming(false);
            gun.SetAiming(false);
        }
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            PlayerMovement movement = GetComponent<PlayerMovement>();
            movement.SetCrouching(!movement.IsCrouching());
        }
    }

    public bool IsAiming() => isAiming;
}