using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float jumpStrength = 5f;
    [SerializeField] private float jumpPreparationDelay = 0.2f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float runSpeedBoost = 1.5f;
    [SerializeField] private float crouchSpeedReduction = 0.5f;
    [SerializeField] private float backwardSpeedReduction = 0.7f;

    [Header("Dependencies")]
    [SerializeField] private Transform cameraMountPoint;
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private InputManager inputHandler;
    private bool isGrounded;
    private bool isCrouching;
    private bool isJumping;
    private bool isInJumpStartOrLanding;
    private bool isPreparingJump; // Kept for potential animation use
    private float jumpStartTime;
    private float jumpStartHeight;
    private float lastJumpTime = -100f;

    public bool IsInJumpStartOrLanding => isInJumpStartOrLanding;
    public float JumpHeight => jumpStartHeight - transform.position.y;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        inputHandler = GetComponent<InputManager>();
        playerLook = GetComponent<PlayerLook>();
        if (playerAnimation == null)
            playerAnimation = GetComponent<PlayerAnimation>();
    }

    void FixedUpdate()
    {
        isGrounded = CheckGrounded();
        HandlePlayerMovement();
        HandlePlayerJump();
        UpdatePlayerRotation();
        if (playerAnimation != null)
            playerAnimation.SetIsGrounded(isGrounded);

        if (isGrounded && !isInJumpStartOrLanding && isJumping)
            isJumping = false;
    }

    private void HandlePlayerMovement()
    {
        Vector2 input = Vector2.ClampMagnitude(inputHandler.MoveInput, 1f);
        Vector3 moveDirection = CalculateMoveDirection(input);
        float speed = CalculateMoveSpeed(input);

        if (isInJumpStartOrLanding) // Only stop movement during jump start or landing
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = 0f;
            velocity.z = 0f;
            rb.linearVelocity = velocity;
        }
        else
        {
            Vector3 targetVelocity = moveDirection * speed * input.magnitude;
            Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 10f * Time.fixedDeltaTime);
            newVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = newVelocity;
        }

        if (input.magnitude < 0.1f && !isInJumpStartOrLanding)
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    private Vector3 CalculateMoveDirection(Vector2 input)
    {
        if (inputHandler.FreeCameraPressed)
            return (transform.right * input.x + transform.forward * input.y).normalized;

        Vector3 cameraForward = cameraMountPoint.forward;
        Vector3 cameraRight = cameraMountPoint.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        return (cameraRight * input.x + cameraForward * input.y).normalized;
    }

    private float CalculateMoveSpeed(Vector2 input)
    {
        float speed = baseMoveSpeed;
        if (inputHandler.RunPressed) speed *= runSpeedBoost;
        else if (isCrouching) speed *= crouchSpeedReduction;
        if (input.y < -0.1f) speed *= backwardSpeedReduction;
        return speed;
    }

    private void HandlePlayerJump()
    {
        if (inputHandler.JumpPressed && isGrounded && !isJumping && !isInJumpStartOrLanding &&
            Time.time - lastJumpTime >= jumpCooldown && (!isCrouching || inputHandler.RunPressed))
        {
            isJumping = true;
            lastJumpTime = Time.time;
            if (playerAnimation != null)
                playerAnimation.TriggerJump();
            StartCoroutine(ApplyJumpForceAfterDelay());
        }
    }

    private IEnumerator ApplyJumpForceAfterDelay()
    {
        isPreparingJump = true; // Kept for potential animation use
        yield return new WaitForSeconds(jumpPreparationDelay);
        if (rb != null)
        {
            jumpStartTime = Time.time;
            jumpStartHeight = transform.position.y;
            rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            isGrounded = false;
            isPreparingJump = false;
        }
    }

    private void UpdatePlayerRotation()
    {
        if (playerLook != null && !inputHandler.FreeCameraPressed && !playerLook.IsReturningToNormal)
            transform.rotation = Quaternion.Euler(0, playerLook.HorizontalCameraAngle, 0);
    }

    private bool CheckGrounded()
    {
        Vector3[] rayOrigins = new Vector3[]
        {
            transform.position + Vector3.up * 0.1f,
            transform.position + Vector3.up * 0.1f + transform.right * 0.2f,
            transform.position + Vector3.up * 0.1f - transform.right * 0.2f,
            transform.position + Vector3.up * 0.1f + transform.forward * 0.2f,
            transform.position + Vector3.up * 0.1f - transform.forward * 0.2f
        };

        bool isGroundedNow = false;
        foreach (Vector3 origin in rayOrigins)
        {
            if (Physics.Raycast(origin, Vector3.down, 1.5f, groundLayer))
            {
                isGroundedNow = true;
                break;
            }
            Debug.DrawRay(origin, Vector3.down * 1.5f, Color.red);
        }

        if (!isGroundedNow && isGrounded)
        {
            jumpStartTime = Time.time;
            jumpStartHeight = transform.position.y;
            if (playerAnimation != null && !isJumping)
                playerAnimation.TriggerJump();
        }
        else if (isGroundedNow && !isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            Debug.Log("Forced Grounded at Position: " + transform.position.y);
        }

        Debug.Log("Ground Check: Hit=" + isGroundedNow + ", Position=" + transform.position.y + ", VelocityY=" + rb.linearVelocity.y);
        return isGroundedNow;
    }

    public void SetCrouching(bool crouching) => isCrouching = crouching;
    public bool IsGrounded() => isGrounded;
    public bool IsCrouching() => isCrouching;
    public void SetInJumpStartOrLanding(bool value) => isInJumpStartOrLanding = value;
}