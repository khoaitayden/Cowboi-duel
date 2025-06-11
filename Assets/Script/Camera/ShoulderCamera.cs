using UnityEngine;

public class ShoulderCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform cameraPivot;    
    [SerializeField] private float normalDistance = 2f;     
    [SerializeField] private float aimDistance = 1f;        
    [SerializeField] private float normalHeight = 1.5f;     // Height when standing
    [SerializeField] private float crouchHeight = 0.8f;     // Reduced height when crouching
    [SerializeField] private float runHeight = 1.8f;        // Height when running
    [SerializeField] private float shoulderOffset = 0.5f;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float heightTransitionSpeed = 5f; // Speed of height transition
    [SerializeField] private float normalFOV = 60f;   
    [SerializeField] private float aimFOV = 40f;      
    private Vector3 currentVelocity;
    private float currentDistance; 
    private float currentFOV;
    private float currentHeight;  
    private float targetHeight;   // Target height to interpolate toward
    private bool isRightShoulder = true;
    private InputManager inputHandler;
    private Camera cam;
    private PlayerMovement playerMovement;

    void Start()
    {
        cam = GetComponent<Camera>(); 
        inputHandler = FindObjectOfType<InputManager>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        currentDistance = normalDistance;
        currentFOV = normalFOV;
        currentHeight = normalHeight;
        targetHeight = normalHeight;
        cam.fieldOfView = currentFOV;
        if (inputHandler == null) Debug.LogWarning("InputManager not found on " + gameObject.name);
        if (playerMovement == null) Debug.LogWarning("PlayerMovement not found on " + gameObject.name);
    }

    void LateUpdate()
    {
        if (target == null || cameraPivot == null) return;

        // Determine target height based on state
        if (playerMovement.IsCrouching() && (inputHandler == null || !inputHandler.RunPressed))
        {
            targetHeight = crouchHeight;
        }
        else if (inputHandler != null && inputHandler.RunPressed)
        {
            targetHeight = runHeight;
        }
        else
        {
            targetHeight = normalHeight;
        }

        // Smoothly interpolate currentHeight toward targetHeight
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * heightTransitionSpeed);

        float offset = isRightShoulder ? shoulderOffset : -shoulderOffset;
        Vector3 shoulderOffsetVector = cameraPivot.right * offset;

        // Simple position calculation
        Vector3 desiredPosition = cameraPivot.position - (cameraPivot.forward * currentDistance) + shoulderOffsetVector + (Vector3.up * currentHeight);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 0.1f / smoothSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, cameraPivot.rotation, smoothSpeed * Time.deltaTime);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, currentFOV, smoothSpeed * Time.deltaTime);
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
        currentDistance = isAiming ? aimDistance : normalDistance;
        currentFOV = isAiming ? aimFOV : normalFOV;
    }

    void Update()
    {
        HandleShoulderToggle();
    }
}