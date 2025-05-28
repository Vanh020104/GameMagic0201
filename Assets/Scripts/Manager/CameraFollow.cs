// using UnityEngine;

// public class CameraFollow : MonoBehaviour
// {
//     public Transform target;
//     public Vector3 offset = new Vector3(0, 45, -35);
//     public float smoothSpeed = 10f;

//     public bool enableSmoothRotation = false;
//     public float rotationSmoothSpeed = 10f;

//     void LateUpdate()
//     {
//         if (!target) return;

//         // Di chuyển mượt
//         Vector3 desiredPosition = target.position + offset;
//         Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
//         transform.position = smoothedPosition;

//         // Xoay mượt (tùy chọn)
//         if (enableSmoothRotation)
//         {
//             Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
//             transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
//         }
//     }
// }

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 55, -45);
    public float smoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;

    public bool enableSmoothRotation = false;
    public float rotationSmoothSpeed = 10f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        if (enableSmoothRotation)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }
}
