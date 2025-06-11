using UnityEngine;
using System.Collections;

public class PlayerLook : MonoBehaviour
{
    [Header("Mouse Look")]
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

    public float HorizontalCameraAngle => horizontalCameraAngle;
    public bool IsReturningToNormal => isReturningToNormal;

    private IEnumerator turnBackCoroutine;

    void Start()
    {
        inputHandler = GetComponent<InputManager>();
        horizontalCameraAngle = transform.eulerAngles.y;
        bodyRotationAngle = horizontalCameraAngle;
        initialBodyRotation = horizontalCameraAngle;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (cameraMountPoint == null) Debug.LogWarning("cameraMountPoint not assigned on " + gameObject.name);
    }

    void Update()
    {
        HandleLook();
        if (cameraMountPoint != null)
            cameraMountPoint.localPosition = Vector3.zero;
    }

    private void HandleLook()
    {
        Vector2 look = inputHandler.LookInput * lookSensitivity;
        horizontalCameraAngle += look.x;
        verticalCameraAngle -= look.y;
        verticalCameraAngle = Mathf.Clamp(verticalCameraAngle, -80f, 80f);

        if (inputHandler.FreeCameraPressed)
        {
            if (!isFreeCameraActive)
            {
                initialBodyRotation = bodyRotationAngle;
                isFreeCameraActive = true;
                isReturningToNormal = false;
                if (turnBackCoroutine != null) StopCoroutine(turnBackCoroutine);
            }

            float minAngle = initialBodyRotation - maxFreeCameraRotation;
            float maxAngle = initialBodyRotation + maxFreeCameraRotation;
            horizontalCameraAngle = Mathf.Clamp(horizontalCameraAngle, minAngle, maxAngle);
            if (cameraMountPoint != null)
            {
                cameraMountPoint.rotation = Quaternion.Euler(verticalCameraAngle, horizontalCameraAngle, 0f);
            }
        }
        else
        {
            if (isFreeCameraActive && !isReturningToNormal)
            {
                turnBackCoroutine = TurnBackToOriginalYaw();
                StartCoroutine(turnBackCoroutine);
                isFreeCameraActive = false;
            }

            if (!isReturningToNormal)
            {
                bodyRotationAngle = horizontalCameraAngle;
                transform.rotation = Quaternion.Euler(0, bodyRotationAngle, 0);
                if (cameraMountPoint != null)
                {
                    cameraMountPoint.rotation = Quaternion.Euler(verticalCameraAngle, bodyRotationAngle, 0f);
                }
            }
        }
    }

    private IEnumerator TurnBackToOriginalYaw()
    {
        isReturningToNormal = true;
        float startYaw = bodyRotationAngle;
        float targetYaw = initialBodyRotation;
        float elapsedTime = 0f;
        float duration = 1f;

        float totalAngle = Mathf.Abs(Mathf.DeltaAngle(startYaw, targetYaw));
        if (totalAngle < 0.01f)
        {
            bodyRotationAngle = targetYaw;
            transform.rotation = Quaternion.Euler(0, bodyRotationAngle, 0);
            if (cameraMountPoint != null)
            {
                cameraMountPoint.rotation = Quaternion.Euler(verticalCameraAngle, bodyRotationAngle, 0f);
            }
            horizontalCameraAngle = bodyRotationAngle;
            isReturningToNormal = false;
            yield break;
        }

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
        transform.rotation = Quaternion.Euler(0, bodyRotationAngle, 0);
        if (cameraMountPoint != null)
        {
            cameraMountPoint.rotation = Quaternion.Euler(verticalCameraAngle, bodyRotationAngle, 0f);
        }
        horizontalCameraAngle = bodyRotationAngle;
        isReturningToNormal = false;
    }
}