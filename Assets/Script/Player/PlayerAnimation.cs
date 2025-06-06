using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationSpeedMultiplier;

    private InputManager inputHandler;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private Rigidbody rb;
    private float currentMovementSpeed;
    private float horizontalDirection;
    private float forwardDirection;
    private float movementSmoothingTime = 0.1f;

    private enum MovementState { Idle, Walk, Run, Crouch }
    private MovementState currentMovementState = MovementState.Idle;

    void Start()
    {
        inputHandler = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        Vector2 input = inputHandler.MoveInput;
        float targetSpeed = input.magnitude * playerMovement.baseMoveSpeed;
        float targetDirectionX = input.x;
        float targetDirectionY = input.y;

        currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, targetSpeed, Time.deltaTime / movementSmoothingTime);
        horizontalDirection = Mathf.Lerp(horizontalDirection, targetDirectionX, Time.deltaTime / movementSmoothingTime);
        forwardDirection = Mathf.Lerp(forwardDirection, targetDirectionY, Time.deltaTime / movementSmoothingTime);

        float horizontalSpeed = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude;

        float baseAnimationSpeed = 1.0f;
        if (currentMovementState == MovementState.Run && !playerMovement.IsCrouching())
        {
            baseAnimationSpeed *= playerMovement.runSpeedBoost;
        }
        else if (playerMovement.IsCrouching())
        {
            baseAnimationSpeed *= playerMovement.crouchSpeedReduction;
        }

        float maxSpeed = playerMovement.baseMoveSpeed * Mathf.Max(1.0f, playerMovement.runSpeedBoost, playerMovement.crouchSpeedReduction);
        animator.speed = Mathf.Clamp01(horizontalSpeed / maxSpeed) * baseAnimationSpeed * animationSpeedMultiplier;

        animator.SetFloat("Speed", currentMovementSpeed);
        animator.SetFloat("DirectionX", horizontalDirection);
        animator.SetFloat("DirectionY", forwardDirection);
        animator.SetBool("IsCrouching", playerMovement.IsCrouching());
        animator.SetBool("IsAiming", playerCombat.IsAiming());
        animator.SetBool("IsGrounded", playerMovement.IsGrounded());
    }
}