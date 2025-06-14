using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float smoothingSpeed = 0.1f;

    private InputManager inputHandler;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private float currentDirectionX;
    private float currentDirectionY;
    private bool wasInJumpStartOrLanding;

    void Start()
    {
        inputHandler = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        wasInJumpStartOrLanding = false;
    }

    void Update()
    {
        if (animator != null)
            UpdateAnimationParameters();
    }

    public void TriggerJump()
    {
        if (animator != null)
            animator.SetTrigger("Jump");
    }

    public void SetIsGrounded(bool value)
    {
        if (animator != null)
            animator.SetBool("IsGrounded", value);
    }

    private void UpdateAnimationParameters()
    {
        if (animator == null || playerMovement == null) return;

        Vector2 input = inputHandler.MoveInput;
        currentDirectionX = Mathf.Lerp(currentDirectionX, input.x, Time.deltaTime / smoothingSpeed);
        currentDirectionY = Mathf.Lerp(currentDirectionY, input.y, Time.deltaTime / smoothingSpeed);
        animator.SetFloat("DirectionX", currentDirectionX);
        animator.SetFloat("DirectionY", currentDirectionY);
        animator.SetFloat("Speed", input.magnitude > 0.1f ? 1f : 0f);
        animator.SetBool("IsCrouching", playerMovement.IsCrouching());
        animator.SetBool("IsAiming", playerCombat.IsAiming());
        animator.SetBool("IsRunning", inputHandler.RunPressed && input.magnitude > 0.1f);

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isCurrentlyInJumpStartOrLanding = stateInfo.IsName("JumpStart") || stateInfo.IsName("Landing");
        if (isCurrentlyInJumpStartOrLanding != wasInJumpStartOrLanding)
        {
            playerMovement.SetInJumpStartOrLanding(isCurrentlyInJumpStartOrLanding);
            wasInJumpStartOrLanding = isCurrentlyInJumpStartOrLanding;
        }
        animator.speed = 1f;
    }
}