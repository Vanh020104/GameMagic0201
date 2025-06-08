using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam == null) return;
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
    }
}
