using UnityEngine;

public class ShoulderCamera : MonoBehaviour
{
    // Camera positioning settings
    [SerializeField] private Transform target;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float defaultDistance;
    [SerializeField] private float aimDistance;
    [SerializeField] private float defaultHeight;
    [SerializeField] private float crouchHeight;
    [SerializeField] private float runHeight;
    [SerializeField] private float shoulderOffset;

    // Smoothing settings
    [SerializeField] private float smoothSpeed;
    [SerializeField] private float heightTransitionSpeed;

    // Field of view settings
    [SerializeField] private float defaultFOV;
    [SerializeField] private float aimFOV;

    private Vector3 currentVelocity;
    private float currentDistance;
    private float currentFOV;
    private float currentHeight;
    private float targetHeight;
    private bool isRightShoulder = true;
    private InputManager inputHandler;
    private Camera cam;
    private PlayerMovement playerMovement;

    void Start()
    {
        // Initialize components with safety checks
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component not found on " + gameObject.name);
            return;
        }
        inputHandler = FindObjectOfType<InputManager>();
        if (inputHandler == null)
        {
            Debug.LogError("InputManager not found in the scene.");
        }
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement not found in the scene.");
        }
        currentDistance = defaultDistance;
        currentFOV = defaultFOV;
        currentHeight = defaultHeight;
        targetHeight = defaultHeight;
        cam.fieldOfView = currentFOV;
    }

    void Update()
    {
        HandleShoulderToggle();
    }

    void LateUpdate()
    {
        if (target == null || cameraPivot == null) return;

        targetHeight = GetTargetHeight();
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * heightTransitionSpeed);
        Vector3 desiredPosition = CalculateCameraPosition();
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 0.1f / smoothSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, cameraPivot.rotation, smoothSpeed * Time.deltaTime);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, currentFOV, smoothSpeed * Time.deltaTime);
    }

    private float GetTargetHeight()
    {
        if (playerMovement != null && playerMovement.IsCrouching() && (inputHandler == null || !inputHandler.RunPressed))
            return crouchHeight;
        if (inputHandler != null && inputHandler.RunPressed)
            return runHeight;
        return defaultHeight;
    }

    private Vector3 CalculateCameraPosition()
    {
        float offset = isRightShoulder ? shoulderOffset : -shoulderOffset;
        Vector3 shoulderOffsetVector = cameraPivot.right * offset;
        return cameraPivot.position - (cameraPivot.forward * currentDistance) + shoulderOffsetVector + (Vector3.up * currentHeight);
    }

    private void HandleShoulderToggle()
    {
        if (inputHandler != null && inputHandler.ShoulderTogglePressed)
        {
            isRightShoulder = !isRightShoulder;
        }
    }

    public void SetAiming(bool isAiming)
    {
        currentDistance = isAiming ? aimDistance : defaultDistance;
        currentFOV = isAiming ? aimFOV : defaultFOV;
    }
}