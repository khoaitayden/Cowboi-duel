using UnityEngine;

public class ShoulderCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform cameraPivot;    
    [SerializeField] private float normalDistance;     
    [SerializeField] private float aimDistance;        
    [SerializeField] private float height;        
    [SerializeField] private float shoulderOffset;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private float normalFOV;   
    [SerializeField] private float aimFOV;      
    private Vector3 currentVelocity;
    private float currentDistance; 
    private float currentFOV;
    private bool isRightShoulder = true;
    private InputManager inputHandler;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>(); 
        inputHandler = FindObjectOfType<InputManager>();
        currentDistance = normalDistance;
        currentFOV = normalFOV;
        cam.fieldOfView = currentFOV;
    }

    void Update()
    {
        HandleShoulderToggle();
    }

    void LateUpdate()
    {
        if (target == null || cameraPivot == null) return;

        float offset = isRightShoulder ? shoulderOffset : -shoulderOffset;
        Vector3 shoulderOffsetVector = cameraPivot.right * offset;

        Vector3 desiredPosition = cameraPivot.position
                                - (cameraPivot.forward * currentDistance)
                                + shoulderOffsetVector
                                + (Vector3.up * height);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

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
}