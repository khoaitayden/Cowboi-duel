using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float smoothingSpeed = 0.1f; // Adjust this for smoother transitions

    private InputManager inputHandler;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private float currentDirectionX;
    private float currentDirectionY;

    void Start()
    {
        inputHandler = GetComponent<InputManager>();
        playerMovement = GetComponent<PlayerMovement>();
        playerCombat = GetComponent<PlayerCombat>();
        if (animator == null) Debug.LogWarning("Animator component not assigned or found on " + gameObject.name);
    }

    void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        Vector2 input = inputHandler.MoveInput;

        // Smoothly interpolate direction parameters
        currentDirectionX = Mathf.Lerp(currentDirectionX, input.x, Time.deltaTime / smoothingSpeed);
        currentDirectionY = Mathf.Lerp(currentDirectionY, input.y, Time.deltaTime / smoothingSpeed);

        // Set direction for blend tree
        animator.SetFloat("DirectionX", currentDirectionX);
        animator.SetFloat("DirectionY", currentDirectionY);

        // Set speed based on input magnitude
        animator.SetFloat("Speed", input.magnitude > 0.1f ? 1.0f : 0.0f);

        // Set other states
        animator.SetBool("IsCrouching", playerMovement.IsCrouching());
        animator.SetBool("IsJumping", !playerMovement.IsGrounded());
        animator.SetBool("IsAiming", playerCombat.IsAiming());
        animator.SetBool("IsRunning", inputHandler.RunPressed && input.magnitude > 0.1f); // Allow running while crouching
        animator.SetBool("IsGrounded", playerMovement.IsGrounded());

        // Reset animation speed to default (no speedup)
        animator.speed = 1f;
    }
}