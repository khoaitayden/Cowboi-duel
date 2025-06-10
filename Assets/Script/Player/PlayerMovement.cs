using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float jumpStrength = 5f;
    [SerializeField] private float runSpeedBoost = 1.5f;
    [SerializeField] private float crouchSpeedReduction = 0.5f;

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
        Vector3 direction = new Vector3(input.x, 0, input.y).normalized;

        if (direction.magnitude >= 0.1f)
        {
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
                cameraForward.Normalize();
                cameraRight.Normalize();
                moveDir = (cameraRight * input.x + cameraForward * input.y).normalized;
            }

            float speed = baseMoveSpeed;
            if (inputHandler.RunPressed && !isCrouching) speed *= runSpeedBoost;
            else if (isCrouching) speed *= crouchSpeedReduction;

            Vector3 targetVelocity = moveDir * speed;
            Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 10f * Time.fixedDeltaTime);
            newVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = newVelocity;
        }
        else
        {
            Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, 10f * Time.fixedDeltaTime);
            newVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = newVelocity;
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