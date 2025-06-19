using UnityEngine;
using UnityEngine.AI;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance;

    [Header("Bo Settings")]
    public float startRadius = 550f;
    public float shrinkRate = 30f; // mỗi lần thu giảm 20 đơn vị
    public float shrinkInterval = 10f;
    public float damagePerSecond = 10f;

    private float currentRadius;
    private Transform zoneVisual;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentRadius = startRadius;
        zoneVisual = transform;
        SetVisualScale();

        InvokeRepeating(nameof(ShrinkZone), shrinkInterval, shrinkInterval);
    }
public Vector3 GetSafePoint(Vector3 currentPosition)
{
    Vector3 dirToCenter = (transform.position - currentPosition).normalized;
    float safeDistance = currentRadius - 2f; // 2f là khoảng lệch an toàn
    Vector3 safePoint = transform.position + dirToCenter * safeDistance;

    NavMeshHit hit;
    if (NavMesh.SamplePosition(safePoint, out hit, 20f, NavMesh.AllAreas))
        return hit.position;

    return currentPosition; // fallback nếu không tìm thấy trên navmesh
}

    void Update()
    {
        foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (IsOutsideZone(obj.transform.position))
            {
                var info = obj.GetComponent<PlayerInfo>();
                if (info != null && !info.hasDied)
                {
                    info._hp -= (int)(damagePerSecond * Time.deltaTime);
                }
            }
        }

        foreach (var bot in GameObject.FindGameObjectsWithTag("Bot"))
        {
            var stats = bot.GetComponent<BotStats>();
            if (stats != null && stats.currentHP > 0 && IsOutsideZone(bot.transform.position))
            {
                stats.currentHP -= damagePerSecond * Time.deltaTime;
            }
        }
    }

    void ShrinkZone()
    {
        currentRadius -= shrinkRate;
        if (currentRadius < 10f) currentRadius = 10f;
        SetVisualScale();
    }

    void SetVisualScale()
    {
        if (zoneVisual != null)
        {
            float diameter = currentRadius * 2f;
            zoneVisual.localScale = new Vector3(diameter, 0.1f, diameter); // scale XZ
        }
    }

    public bool IsOutsideZone(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) > currentRadius;
    }

    public bool IsInsideZone(Vector3 position)
    {
        return !IsOutsideZone(position);
    }
}
