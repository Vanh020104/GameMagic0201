using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance;

    public Transform zoneVisual;
    public float zoneRadius = 150f;
    public float shrinkAmount = 20f;
    public float shrinkInterval = 10f;

    private float shrinkTimer = 0f;
    public Vector3 zoneCenter;

    void Awake()
    {
        Instance = this;
        zoneCenter = transform.position;
    }

    void Start()
    {
        UpdateVisual();
    }

    void Update()
    {
        shrinkTimer += Time.deltaTime;
        if (shrinkTimer >= shrinkInterval)
        {
            shrinkTimer = 0f;
            ShrinkZone();
        }
    }

    void ShrinkZone()
    {
        zoneRadius -= shrinkAmount;
        if (zoneRadius < 10f) zoneRadius = 10f;

        // Di chuyển bo nhẹ để tránh predict dễ
        zoneCenter += new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10));
        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (zoneVisual != null)
        {
            zoneVisual.position = zoneCenter;
            zoneVisual.localScale = new Vector3(zoneRadius * 2f, 0.1f, zoneRadius * 2f);
        }
    }

    public bool IsInsideZone(Vector3 pos)
    {
        return Vector3.Distance(pos, zoneCenter) <= zoneRadius;
    }

    public Vector3 GetSafePoint(Vector3 pos)
    {
        Vector3 dir = (pos - zoneCenter).normalized;
        return zoneCenter + dir * (zoneRadius - 1f);
    }
}
