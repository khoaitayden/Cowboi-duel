using UnityEngine;
using System.Collections;

public class PlayerLook : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] private float lookSensitivity;
    [SerializeField] private Transform cameraMountPoint;
    [SerializeField] private float maxFreeCameraRotation;

    private InputManager inputHandler;
    private float verticalCameraAngle;
    private float horizontalCameraAngle;
    private float bodyRotationAngle;
    private float initialBodyRotation;
    private bool isFreeCameraActive;
    private bool isReturningToNormal;
    private IEnumerator turnBackCoroutine;

    public float HorizontalCameraAngle => horizontalCameraAngle;
    public bool IsReturningToNormal => isReturningToNormal;

    void Start()
    {
        inputHandler = GetComponent<InputManager>();
        horizontalCameraAngle = transform.eulerAngles.y;
        bodyRotationAngle = horizontalCameraAngle;
        initialBodyRotation = horizontalCameraAngle;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (cameraMountPoint == null)
        {
            Debug.LogError("CameraMountPoint not assigned on " + gameObject.name);
        }
    }

    void Update()
    {
        HandleLookInput();
        if (cameraMountPoint != null)
        {
            cameraMountPoint.localPosition = Vector3.zero;
        }
    }

    private void HandleLookInput()
    {
        Vector2 look = inputHandler.LookInput * lookSensitivity;
        horizontalCameraAngle += look.x;
        verticalCameraAngle = Mathf.Clamp(verticalCameraAngle - look.y, -80f, 80f);

        if (inputHandler.FreeCameraPressed)
        {
            StartFreeCameraMode();
            UpdateFreeCameraRotation();
        }
        else
        {
            EndFreeCameraMode();
            if (!isReturningToNormal)
            {
                UpdateBodyAndCameraRotation();
            }
        }
    }

    private void StartFreeCameraMode()
    {
        if (!isFreeCameraActive)
        {
            initialBodyRotation = bodyRotationAngle;
            isFreeCameraActive = true;
            isReturningToNormal = false;
            if (turnBackCoroutine != null) StopCoroutine(turnBackCoroutine);
        }
    }

    private void UpdateFreeCameraRotation()
    {
        float minAngle = initialBodyRotation - maxFreeCameraRotation;
        float maxAngle = initialBodyRotation + maxFreeCameraRotation;
        horizontalCameraAngle = Mathf.Clamp(horizontalCameraAngle, minAngle, maxAngle);
        if (cameraMountPoint != null)
        {
            cameraMountPoint.rotation = Quaternion.Euler(verticalCameraAngle, horizontalCameraAngle, 0f);
        }
    }

    private void EndFreeCameraMode()
    {
        if (isFreeCameraActive && !isReturningToNormal)
        {
            turnBackCoroutine = SmoothReturnToBodyRotation();
            StartCoroutine(turnBackCoroutine);
            isFreeCameraActive = false;
        }
    }

    private void UpdateBodyAndCameraRotation()
    {
        bodyRotationAngle = horizontalCameraAngle;
        transform.rotation = Quaternion.Euler(0, bodyRotationAngle, 0);
        if (cameraMountPoint != null)
        {
            cameraMountPoint.rotation = Quaternion.Euler(verticalCameraAngle, bodyRotationAngle, 0f);
        }
    }

    private IEnumerator SmoothReturnToBodyRotation()
    {
        isReturningToNormal = true;
        float startYaw = bodyRotationAngle;
        float targetYaw = initialBodyRotation;
        float elapsedTime = 0f;
        float duration = 1f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            bodyRotationAngle = Mathf.LerpAngle(startYaw, targetYaw, t);
            transform.rotation = Quaternion.Euler(0, bodyRotationAngle, 0);
            if (cameraMountPoint != null)
            {
                cameraMountPoint.rotation = Quaternion.Euler(verticalCameraAngle, bodyRotationAngle, 0f);
            }
            yield return null;
        }

        bodyRotationAngle = targetYaw;
        horizontalCameraAngle = bodyRotationAngle;
        isReturningToNormal = false;
    }
}