using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 35, -25);
    public float smoothSpeed = 5f;

    public bool enableSmoothRotation = false;
    public float rotationSmoothSpeed = 5f;

    void LateUpdate()
    {
        if (!target) return;

        // Di chuyển mượt
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Xoay mượt (tùy chọn)
        if (enableSmoothRotation)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }
}
