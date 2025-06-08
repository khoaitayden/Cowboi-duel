using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float baseMoveSpeed;
    [SerializeField] private float jumpStrength;
    public float runSpeedBoost;
    public float crouchSpeedReduction;

    [Header("Dependencies")]
    [SerializeField] private Transform cameraMountPoint;

    private Rigidbody rb;
    private InputManager inputHandler;
    private bool isGrounded;
    private bool isCrouching;
    private float lastJumpTime;
    private const float JUMP_COOLDOWN = 1f; 

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        inputHandler = GetComponent<InputManager>();
    }

    private void FixedUpdate()
    {
        isGrounded = CheckGrounded();
        HandleMovement();
        HandleJump();
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
                Vector3 bodyForward = transform.forward.normalized;
                Vector3 bodyRight = transform.right.normalized;
                moveDir = (bodyRight * input.x + bodyForward * input.y).normalized;
            }
            else
            {
                Vector3 cameraForward = new Vector3(cameraMountPoint.forward.x, 0, cameraMountPoint.forward.z).normalized;
                Vector3 cameraRight = new Vector3(cameraMountPoint.right.x, 0, cameraMountPoint.right.z).normalized;
                moveDir = (cameraRight * input.x + cameraForward * input.y).normalized;
            }

            float currentSpeed = baseMoveSpeed;
            if (inputHandler.RunPressed && !isCrouching)
            {
                currentSpeed *= runSpeedBoost;
                Debug.Log($"Running: Current Speed = {currentSpeed}");
            }
            else if (isCrouching)
            {
                currentSpeed *= crouchSpeedReduction;
                Debug.Log($"Crouching: Current Speed = {currentSpeed}");
            }
            else
            {
                Debug.Log($"Walking: Current Speed = {currentSpeed}");
            }

            Vector3 targetVelocity = moveDir * currentSpeed * input.magnitude;
            Vector3 currentVelocity = rb.linearVelocity;
            Vector3 newVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 10f * Time.fixedDeltaTime);
            newVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = newVelocity;
        }
        else
        {
            Vector3 currentVelocity = rb.linearVelocity;
            Vector3 newVelocity = Vector3.Lerp(currentVelocity, new Vector3(0, currentVelocity.y, 0), 10f * Time.fixedDeltaTime);
            rb.linearVelocity = newVelocity;
        }
    }

    private void HandleJump()
    {
        if (inputHandler.JumpPressed && isGrounded && !isCrouching && Time.time >= lastJumpTime + JUMP_COOLDOWN)
        {
            rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            isGrounded = false;
            lastJumpTime = Time.time;
        }
    }

    private bool CheckGrounded()
    {
        float rayLength = 0.6f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        bool grounded = Physics.Raycast(rayOrigin, Vector3.down, rayLength);
        return grounded;
    }

    public void SetCrouching(bool crouching)
    {
        isCrouching = crouching;
    }

    public bool IsGrounded() => isGrounded;
    public bool IsCrouching() => isCrouching;
}