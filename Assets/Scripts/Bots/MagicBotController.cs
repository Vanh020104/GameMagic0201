using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MagicBotController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float detectionRadius = 35f;
    public float attackRange = 25f;

    public Transform firePoint;
    public GameObject projectilePrefab;
    [SerializeField] private LayerMask targetLayerMask = ~0;

    [SerializeField]  private Rigidbody rb;
    private Vector3 moveDirection;
    private Transform target;
    private Animator anim;

    private float fireCooldown = 3f;
    private float lastFireTime;
    private bool canAttack = true;

    private enum BotState { Patrol, Chase, Attack, Retreat, Evade }
    private BotState currentState = BotState.Patrol;

    private Vector3 retreatDirection;
    private float mapRadius = 100f; // Giới hạn map
    private float retreatTimer = 0f;
    private float retreatDuration = 1f; // retreat tối đa 2s
    private float stuckTimer = 0f;
    private float stuckDuration = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        PickRandomDirection();
        InvokeRepeating(nameof(SearchTarget), 0f, 0.5f);
    }

    void Update()
    {
        HandleState();
        UpdateAnimation();
    }

    void HandleState()
    {
        if (target == null && currentState != BotState.Patrol)
        {
            currentState = BotState.Patrol;
            PickRandomDirection(); // Reset hướng nếu mất Player
        }

        switch (currentState)
        {
            case BotState.Patrol: Patrol(); break;
            case BotState.Chase: Chase(); break;
            case BotState.Attack: Attack(); break;
            case BotState.Retreat: Retreat(); break;
            case BotState.Evade: Evade(); break;
        }
    }


    void Patrol()
    {
        if (moveDirection == Vector3.zero)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > stuckDuration)
            {
                PickRandomDirection();
                stuckTimer = 0f;
            }
            return;
        }

        stuckTimer = 0f;
        var botInfo = gameObject.GetComponent<BotInfomation>();
        if (botInfo != null && !botInfo.hasdie)
        {
            MoveBot();
            LookAt(moveDirection);
        }
    }

    void Chase()
    {
        if (target == null || !target.gameObject.activeSelf || target.GetComponent<PlayerInfo>()?.hasDied == true)
        {
            target = null;
            currentState = BotState.Patrol;
            PickRandomDirection();
            return;
        }



        Vector3 dir = (target.position - transform.position).normalized;
        moveDirection = dir;
        var botInfo = gameObject.GetComponent<BotInfomation>();
        if (botInfo != null && !botInfo.hasdie)
        {
            MoveBot();
            LookAt(moveDirection);
        }


        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attackRange)
        {
            currentState = BotState.Attack;
        }
    }

    void Attack()
    {
        if (target == null || !target.gameObject.activeSelf || target.GetComponent<PlayerInfo>()?.hasDied == true)
        {
            target = null;
            currentState = BotState.Patrol;
            PickRandomDirection();
            return;
        }


        Vector3 dir = (target.position - transform.position).normalized;
        moveDirection = dir;
        var botInfo = gameObject.GetComponent<BotInfomation>();
        if (botInfo != null && !botInfo.hasdie)
        {
            MoveBot();
        }

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRange)
        {
            currentState = BotState.Chase;
            return;
        }

        if (canAttack && !botInfo.hasdie)
        {
            // transform.rotation = Quaternion.LookRotation(dir);
            // firePoint.rotation = transform.rotation;

            // Shoot();

            // canAttack = false;
            // Invoke(nameof(ResetAttack), fireCooldown);

            // // → KHÔNG gọi MoveBot() sau khi bắn
            // // Chuyển sang trạng thái né (retreat)
            // Vector3 away = (transform.position - target.position).normalized;
            // Vector3 side = Vector3.Cross(Vector3.up, away).normalized;
            // float sideDir = Random.value < 0.5f ? -1f : 1f;

            // retreatDirection = (away + side * sideDir * 0.5f).normalized;

            // currentState = BotState.Retreat;
            // return;
            Shoot();
            canAttack = false;
            Invoke(nameof(ResetAttack), fireCooldown);

            // Chuyển sang né tránh, không lập tức quay lại Chase
            currentState = BotState.Evade;
            PickEvadeDirection(); // giống Retreat
            return;
        }

        // Nếu chưa bắn được thì tránh bám đuôi player → Chuyển sang Patrol hoặc giữ khoảng cách
        if (!canAttack)
        {
            currentState = BotState.Retreat;
            return;
        }
    }

    void Evade()
    {
        if (canAttack)
        {
            currentState = BotState.Chase;
            return;
        }

        if (target == null || !target.gameObject.activeSelf || target.GetComponent<PlayerInfo>()?.hasDied == true)
        {
            target = null;
            currentState = BotState.Patrol;
            PickRandomDirection();
            return;
        }


        // Di chuyển random lệch khỏi player
        if (moveDirection == Vector3.zero || Random.value < 0.05f)
        {
            PickEvadeDirection();
        }

        var botInfo = gameObject.GetComponent<BotInfomation>();
        if (botInfo != null && !botInfo.hasdie)
        {
            MoveBot();
            LookAt(moveDirection);
        }
    }

    void PickEvadeDirection()
    {
        if (target == null) return;

        Vector3 away = (transform.position - target.position).normalized;
        Vector3 side = Vector3.Cross(Vector3.up, away).normalized;
        float sideDir = Random.value < 0.5f ? -1f : 1f;

        Vector3 evade = (away + side * sideDir * Random.Range(0.2f, 0.8f)).normalized;
        moveDirection = evade;
    }
    void Retreat()
    {
        retreatTimer += Time.deltaTime;

        if (target == null || retreatTimer > retreatDuration || canAttack)
        {
            retreatTimer = 0f;
            currentState = BotState.Chase;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > detectionRadius * 0.9f || canAttack)
        {
            currentState = BotState.Chase;
            return;
        }

        if (moveDirection == Vector3.zero || Random.value < 0.02f)
        {
            Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-1.5f, 1.5f));
            retreatDirection = (retreatDirection + offset).normalized;
        }

        // Đảm bảo retreatDirection không tiến gần player
        float currentDist = Vector3.Distance(transform.position, target.position);
        float futureDist = Vector3.Distance(transform.position + retreatDirection, target.position);
        if (futureDist < currentDist)
        {
            retreatDirection = (transform.position - target.position).normalized;
        }

        moveDirection = retreatDirection;
        var botInfo = gameObject.GetComponent<BotInfomation>();
        if (botInfo != null && !botInfo.hasdie)
        {
            MoveBot();
            LookAt(moveDirection);
        }
    }

    void MoveBot()
    {
        if (!IsPathClear(moveDirection))
        {
            Vector3 left = Quaternion.Euler(0, -45, 0) * moveDirection;
            Vector3 right = Quaternion.Euler(0, 45, 0) * moveDirection;

            if (IsPathClear(left))
                moveDirection = left.normalized;
            else if (IsPathClear(right))
                moveDirection = right.normalized;
            else
                moveDirection = Vector3.zero;
        }

        Vector3 newPos = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        newPos = ClampToMap(newPos);
        rb.MovePosition(newPos);
    }

    bool IsPathClear(Vector3 direction)
    {
        Vector3 start = transform.position;
        Vector3 end = start + direction.normalized * 2f;

        NavMeshHit hit;
        return !NavMesh.Raycast(start, end, out hit, NavMesh.AllAreas);
    }

    Vector3 ClampToMap(Vector3 pos)
    {
        Vector3 center = Vector3.zero;
        Vector3 offset = pos - center;
        if (offset.magnitude > mapRadius)
        {
            pos = center + offset.normalized * mapRadius;
        }
        return pos;
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null || target == null) return;

        Vector3 shootDir = (target.position - firePoint.position).normalized;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(shootDir));

        ProjectileMoveScript moveScript = proj.GetComponent<ProjectileMoveScript>();
        if (moveScript != null)
        {
            moveScript.owner = GetComponent<PlayerInfo>();
            // Gán hướng nếu bạn có hàm set
            // moveScript.SetDirection(transform.forward); ← nếu cần
        }
        else
        {
            Debug.LogError("Projectile prefab is missing ProjectileMoveScript script!", proj);
        }

        // Nếu bạn vẫn muốn dùng physics: bật Interpolate, tắt Gravity
        Rigidbody projRb = proj.GetComponent<Rigidbody>();
        if (projRb != null)
        {
            projRb.velocity = shootDir;
        }
        lastFireTime = Time.time;
    }


    void ResetAttack()
    {
        canAttack = true;
    }

void SearchTarget()
{
    Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, targetLayerMask);
    float closestDist = float.MaxValue;
    Transform best = null;

    foreach (var hit in hits)
    {
        if (hit == null || hit.transform == null || !hit.gameObject.activeSelf)
            continue;

        // ✅ Bỏ qua chính bản thân bot
        if (hit.gameObject == gameObject)
            continue;

        // Kiểm tra nếu là Player (PlayerInfo)
        var player = hit.GetComponent<PlayerInfo>();
        if (player != null && player.hasDied)
            continue;

        // Kiểm tra nếu là Bot (BotInfomation)
        var bot = hit.GetComponent<BotInfomation>();
        if (bot != null && bot.hasdie)
            continue;

        // Bỏ qua nếu cả hai đều null
        if (player == null && bot == null)
            continue;

        float dist = Vector3.Distance(transform.position, hit.transform.position);
        if (dist < closestDist)
        {
            closestDist = dist;
            best = hit.transform;
        }
    }

    target = best;

    if (target == null && currentState != BotState.Patrol)
    {
        currentState = BotState.Patrol;
        PickRandomDirection();
    }
    else if (target != null && currentState == BotState.Patrol)
    {
        currentState = BotState.Chase;
    }
}


    void PickRandomDirection()
    {
        if (target != null) return;

        Vector3 randomOffset = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        Vector3 targetPoint = transform.position + randomOffset;
        moveDirection = (targetPoint - transform.position).normalized;

        Invoke(nameof(PickRandomDirection), Random.Range(2f, 4f));
    }

    void LookAt(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void UpdateAnimation()
    {
        if (anim == null) return;
        float speed = moveDirection.magnitude * moveSpeed;
        anim.SetFloat("Speed", speed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
