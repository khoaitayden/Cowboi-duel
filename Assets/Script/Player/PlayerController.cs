using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float runSpeedMultiplier;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private Transform cameraPivot;

    [Header("Gun")]
    [SerializeField] private Gun gun;

    [Header("Body Rotation")]
    [SerializeField] private float bodyRotationThreshold; 
    [SerializeField] private float bodyRotationSpeed;

    private Rigidbody rb;
    private PlayerInputHandler inputHandler;
    private bool isGrounded;
    private float cameraPitch = 0f;
    private float cameraYaw = 0f;
    private bool isAiming = false;
    private bool shouldRotateBody = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        inputHandler = GetComponent<PlayerInputHandler>();
        
        cameraYaw = transform.eulerAngles.y;
    }

    void Update()
    {
        HandleLook();
        HandleBodyRotation();
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

        cameraYaw += look.x;

        cameraPitch -= look.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        if (cameraPivot != null)
            cameraPivot.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
    }

    private void HandleBodyRotation()
    {
        float angleDiff = Mathf.DeltaAngle(transform.eulerAngles.y, cameraYaw);

        if (Mathf.Abs(angleDiff) > bodyRotationThreshold)
        {
            shouldRotateBody = true;
        }
        else if (Mathf.Abs(angleDiff) < bodyRotationThreshold - 10f)
        {
            shouldRotateBody = false;
        }

        if (shouldRotateBody)
        {
            float targetYaw = cameraYaw;
            float currentYaw = transform.eulerAngles.y;
            float newYaw = Mathf.LerpAngle(currentYaw, targetYaw, bodyRotationSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.Euler(0, newYaw, 0);
        }
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
            Vector3 cameraForward = new Vector3(cameraPivot.forward.x, 0, cameraPivot.forward.z).normalized;
            Vector3 cameraRight = new Vector3(cameraPivot.right.x, 0, cameraPivot.right.z).normalized;
            
            Vector3 moveDir = (cameraRight * input.x + cameraForward * input.y).normalized;

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
    //public bool IsAiming() => isAiming;
}