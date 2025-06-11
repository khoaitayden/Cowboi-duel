using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float jumpStrength = 5f;
    [SerializeField] private float runSpeedBoost = 1.5f;
    [SerializeField] private float crouchSpeedReduction = 0.5f;
    [SerializeField] private float backwardSpeedReduction = 0.7f; // New variable to adjust backward speed (0 to 1)

    [Header("Dependencies")]
    [SerializeField] private Transform cameraMountPoint;
    [SerializeField] private PlayerLook playerLook;

    private Rigidbody rb;
    private InputManager inputHandler;
    private bool isGrounded;
    private bool isCrouching;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        inputHandler = GetComponent<InputManager>();
        playerLook = GetComponent<PlayerLook>();
    }

    void FixedUpdate()
    {
        isGrounded = CheckGrounded();
        HandleMovement();
        HandleJump();
        if (playerLook != null && !inputHandler.FreeCameraPressed && !playerLook.IsReturningToNormal)
        {
            transform.rotation = Quaternion.Euler(0, playerLook.HorizontalCameraAngle, 0);
        }
    }

    private void HandleMovement()
    {
        Vector2 input = inputHandler.MoveInput;
        input = Vector2.ClampMagnitude(input, 1f); // Prevent faster diagonal movement

        Vector3 moveDir;
        if (inputHandler.FreeCameraPressed)
        {
            moveDir = (transform.right * input.x + transform.forward * input.y).normalized;
        }
        else
        {
            Vector3 cameraForward = cameraMountPoint.forward;
            Vector3 cameraRight = cameraMountPoint.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            moveDir = (cameraRight * input.x + cameraForward * input.y).normalized;
        }

        float speed = baseMoveSpeed;
        if (inputHandler.RunPressed) speed *= runSpeedBoost; // Run takes priority, even when crouching
        else if (isCrouching) speed *= crouchSpeedReduction; // Apply crouch reduction only if not running

        // Reduce speed when moving backward (negative Y input in camera space)
        if (input.y < -0.1f) speed *= backwardSpeedReduction;

        Vector3 targetVelocity = moveDir * speed * input.magnitude;
        Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 10f * Time.fixedDeltaTime);
        newVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = newVelocity;

        // Update crouching state based on run input
        if (isCrouching && inputHandler.RunPressed)
        {
            // Do nothing while running from crouched state
        }
        else if (!inputHandler.RunPressed && isCrouching)
        {
            // Stay crouched when run is released
        }
        else if (!inputHandler.RunPressed && !isCrouching)
        {
            // Allow crouching toggle via other means if implemented elsewhere
        }

        // Stop horizontal movement if input is very small to prevent jittering
        if (input.magnitude < 0.1f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void HandleJump()
    {
        if (inputHandler.JumpPressed && isGrounded && !isCrouching)
        {
            rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private bool CheckGrounded()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 0.6f);
    }

    public void SetCrouching(bool crouching) => isCrouching = crouching;
    public bool IsGrounded() => isGrounded;
    public bool IsCrouching() => isCrouching;
}