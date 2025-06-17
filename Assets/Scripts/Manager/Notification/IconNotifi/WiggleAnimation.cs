using UnityEngine;

public class WigglePulse : MonoBehaviour
{
    public float wiggleDuration = 1.5f;
    public float pauseDuration = 2f;
    public float angle = 15f;
    public float speed = 20f;

    private float timer;
    private bool isWiggling = true;
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isWiggling)
        {
            float z = Mathf.Sin(Time.time * speed) * angle;
            transform.rotation = initialRotation * Quaternion.Euler(0f, 0f, z);

            if (timer >= wiggleDuration)
            {
                timer = 0f;
                isWiggling = false;
                transform.rotation = initialRotation;
            }
        }
        else
        {
            if (timer >= pauseDuration)
            {
                timer = 0f;
                isWiggling = true;
            }
        }
    }
}
