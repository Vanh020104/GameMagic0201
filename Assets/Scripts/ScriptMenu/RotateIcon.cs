using UnityEngine;

public class RotateIcon : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 180f; 
    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
