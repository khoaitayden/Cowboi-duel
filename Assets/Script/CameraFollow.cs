using UnityEngine;

public class ShoulderCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform cameraPivot;    
    [SerializeField] private float distance;     
    [SerializeField] private float height;        
    [SerializeField] private float shoulderOffset;
    [SerializeField] private float smoothSpeed;
    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (target == null || cameraPivot == null) return;

        Vector3 shoulderOffsetVector = cameraPivot.right * shoulderOffset;

        Vector3 desiredPosition = cameraPivot.position
                                - (cameraPivot.forward * distance)
                                + shoulderOffsetVector
                                + (Vector3.up * height);

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / smoothSpeed);

        transform.rotation = Quaternion.Lerp(transform.rotation, cameraPivot.rotation, smoothSpeed * Time.deltaTime);
    }
}
