using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float runSpeedMultiplier;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform cameraPivot;

    [Header("Gun")]
    [SerializeField] private Gun gun;

    private Rigidbody rb;
    private PlayerInputHandler inputHandler;
    private bool isGrounded;
    private float cameraPitch = 0f;
    private bool isAiming = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    void Update()
    {
        HandleLook();
        HandleFire();
        HandleReload();
        HandleAim();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleLook()
    {
        Vector2 look = inputHandler.LookInput * mouseSensitivity;

        transform.Rotate(Vector3.up * look.x);

        cameraPitch -= look.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        if (cameraPivot != null)
            cameraPivot.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
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
        if (inputHandler.AimPressed && !isAiming && isGrounded && !inputHandler.RunPressed)
        {
            isAiming = true;
            ShoulderCamera shoulderCam = cameraPivot.GetComponentInChildren<ShoulderCamera>();
            if (shoulderCam != null)
                shoulderCam.SetAiming(true);
            gun.SetAiming(true);
        }
        else if ((!inputHandler.AimPressed || !isGrounded || inputHandler.RunPressed) && isAiming)
        {
            isAiming = false;
            ShoulderCamera shoulderCam = cameraPivot.GetComponentInChildren<ShoulderCamera>();
            if (shoulderCam != null)
                shoulderCam.SetAiming(false);
            gun.SetAiming(false);
        }
    }

    private void HandleMovement()
    {
        Vector2 input = inputHandler.MoveInput;
        Vector3 direction = new Vector3(input.x, 0, input.y).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDir = Quaternion.Euler(0, transform.eulerAngles.y, 0) * direction;

            float currentSpeed = moveSpeed;
            if (inputHandler.RunPressed)
            {
                currentSpeed *= runSpeedMultiplier;
            }

            rb.linearVelocity = new Vector3(moveDir.x * currentSpeed, rb.linearVelocity.y, moveDir.z * currentSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void HandleJump()
    {
        if (inputHandler.JumpPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false; 
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false; 
    }
}