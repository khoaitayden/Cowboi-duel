using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform target;
    [SerializeField] private Transform cameraPivot;

    [Header("Settings")]
    [SerializeField] private float smoothSpeed;

    void Update()
    {
        UpdateTarget();
    }

    private void UpdateTarget()
    {
        if (crosshair == null || playerCamera == null || cameraPivot == null) return;

        Ray ray = new Ray(cameraPivot.position, cameraPivot.forward);
        Vector3 crosshairWorldPos = ray.origin + ray.direction * 10f;

        if (target != null)
        {
            target.position = Vector3.Lerp(target.position, crosshairWorldPos, smoothSpeed * Time.deltaTime);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            if (target != null) Gizmos.DrawSphere(target.position, 0.15f);

            if (cameraPivot != null)
            {
                Gizmos.color = Color.yellow;
            }
        }
    }
}