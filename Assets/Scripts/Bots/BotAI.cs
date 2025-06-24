using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class BotAI : MonoBehaviour
{
    [Header("Settings")]
    public float detectRange = 60f;
    public float attackRange = 40f;
    public float minAttackDistance = 15f;
    public float fleeDistance = 25f;
    public float cooldownMoveDistance = 50f;
    public float moveSpeed = 15f;
    public float attackCooldown = 2f;
    [SerializeField] float healSearchRange = 80f;

    [Header("Runtime")]
    public Transform target;

    private Rigidbody rb;
    private Animator anim;
    public BotStats botStats;
    private GameObject worldUI;

    private NavMeshPath path;
    private List<Vector3> pathPoints = new List<Vector3>();
    private int currentPathIndex = 0;

    private Vector3 wanderTarget;
    private Vector3 cooldownTarget;
    private Vector3 lastPos;
    private float lastAttackTime = Mathf.NegativeInfinity;

    private float stuckTimer = 0f;
    private float noMoveTimer = 0f;
    private float fleeCooldownTimer = 0f;

    private float pointThreshold = 1.2f;

    enum State { Wander, Chase, Attack, Cooldown, Die, SeekHeal }
    private State currentState;

    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileLifetime = 1f;
    private float aiTickTimer = 0f;
    private float aiTickRate = 0.1f;

    private float pathTickTimer = 0f;
    private float pathTickRate = 0.05f;
    [SerializeField] float healCooldown = 5f;
    private float lastHealTime = -999f;
    private float animSpeed = 0f;
    private float lastEscapeFromBehindTime = -10f;
    private float escapeCooldown = 1.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        botStats = GetComponent<BotStats>();
        path = new NavMeshPath();
        lastPos = transform.position;
        aiTickRate += Random.Range(-0.05f, 0.05f);
        InvokeRepeating(nameof(ChooseWanderTarget), 0f, 5f);
        InvokeRepeating(nameof(RegenMana), 1f, 1f);
    }

    void Update()
    {
        if (botStats.currentHP <= 0 && currentState != State.Die)
        {
            HandleDeath();
            return;
        }
        if (currentState == State.Die) return;

        aiTickTimer += Time.deltaTime;
        if (aiTickTimer >= aiTickRate)
        {
            aiTickTimer = 0f;
            UpdateStateLogic();
        }

        ForceMoveIfIdle();
        CheckStuckAndResetIfNeeded();

        // 🧠 Phản ứng khi bị dí từ phía sau
        if (currentState == State.Cooldown && Time.time - lastEscapeFromBehindTime > escapeCooldown)
        {
            if (IsEnemyTooCloseBehind(out Vector3 safeDir))
            {
                cooldownTarget = ClampToNavMesh(transform.position + safeDir * cooldownMoveDistance * Random.Range(0.8f, 1.2f)); // Thêm ngẫu nhiên
                pathPoints.Clear();
                currentPathIndex = 0;
                lastEscapeFromBehindTime = Time.time;
            }
        }


        switch (currentState)
        {
            case State.Wander: HandleWander(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
            case State.Cooldown: HandleCooldown(); break;
            case State.SeekHeal: HandleSeekHeal(); break;
        }
    }

    bool IsEnemyTooCloseBehind(out Vector3 safeDir)
    {
        safeDir = Vector3.zero;
        Vector3 myBack = -transform.forward;
        int count = 0;

        foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            var info = obj.GetComponent<PlayerInfo>();
            if (info == null || info.hasDied) continue;

            Vector3 toEnemy = obj.transform.position - transform.position;
            float dist = toEnemy.magnitude;
            if (dist < 9f && Vector3.Dot(toEnemy.normalized, myBack) > 0.7f)
            {
                Vector3 perpendicular = Vector3.Cross(toEnemy, Vector3.up).normalized;
                safeDir += (perpendicular + myBack).normalized;
                count++;
            }
        }

        if (count > 0)
        {
            safeDir = safeDir.normalized;
            return true;
        }
        return false;
    }

    void LookAtTargetSmooth()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
            // 🌀 Quay nhanh hơn để phản ứng nhanh khi tấn công
            Quaternion smoothRot = Quaternion.Slerp(rb.rotation, targetRot, Time.deltaTime * 40f);
            rb.MoveRotation(smoothRot);
        }
    }


    void UpdateStateLogic()
    {
        if (Time.time - lastHealTime < healCooldown)
            return;
        if (botStats.currentHP < botStats.maxHP * 0.3f)
        {
            float playerDist = target != null ? Vector3.Distance(transform.position, target.position) : Mathf.Infinity;

            if (playerDist <= attackRange && botStats.currentMana >= 5f)
            {
                currentState = State.Attack;
                return;
            }

            if (Random.value < 0.8f)
            {
                GameObject heal = FindClosestHeal();
                if (heal != null)
                {
                    cooldownTarget = heal.transform.position;
                    currentState = State.SeekHeal;
                    lastHealTime = Time.time;
                    return;
                }
            }
        }

        // Logic flee khi địch quá gần (quan trọng để bot không bị "dí chết")
        if (target != null && Vector3.Distance(transform.position, target.position) < fleeDistance)
        {
            // Nếu có thể bắn dù gần, hãy ưu tiên bắn
            if (botStats.currentMana >= 5f && IsFacingTarget() && HasClearLineOfSightTo())
            {
                currentState = State.Attack;
                return;
            }

            currentState = State.Cooldown; // chạy trốn
            cooldownTarget = GetFleePosition();
            return;
        }

        Transform newTarget = FindClosestTarget();
        if (newTarget == null)
        {
            target = null;
            currentState = State.Wander;
            return;
        }

        if (target == null || IsTargetDead(target))
        {
            target = newTarget;
        }
        else
        {
            float currDist = Vector3.Distance(transform.position, target.position);
            float newDist = Vector3.Distance(transform.position, newTarget.position);

            if (newTarget != target && newDist + 5f < currDist) // Giảm nhạy
            {
                target = newTarget;
            }
        }


        float dist = Vector3.Distance(transform.position, target.position);

        // Bỏ điều kiện minAttackDistance để bot luôn có thể bắn nếu đủ điều kiện
        if (dist <= attackRange && CanShootNow()) // Kiểm tra CanShootNow ở đây
        {
            currentState = State.Attack;
            return;
        }

        // Nếu không thể bắn (do cooldown, mana, hoặc không đối mặt/chắn tầm nhìn)
        if (Time.time - lastAttackTime < attackCooldown || botStats.currentMana < 5f || !IsFacingTarget() || !HasClearLineOfSightTo())
        {
            if (currentState != State.Cooldown) // Tránh chuyển Cooldown liên tục
            {
                cooldownTarget = GetCooldownDestination();
                pathPoints.Clear();
                currentPathIndex = 0;
                currentState = State.Cooldown;
                 isForcingMovement = false;
            }
            return;
        }

        if (dist <= detectRange)
            currentState = State.Chase;
        else
            currentState = State.Wander;
    }

private bool isForcingMovement = false;
    bool HasClearLineOfSightTo(Transform targetToCheck = null)
    {
        if (targetToCheck == null) targetToCheck = target;
        if (!targetToCheck) return false;

        Vector3 from = projectileSpawnPoint ? projectileSpawnPoint.position : transform.position + Vector3.up;
        Vector3 to = targetToCheck.position + Vector3.up;
        Vector3 dir = (to - from).normalized;
        float dist = Vector3.Distance(from, to);

        int obstacleMask = LayerMask.GetMask("Obstacle", "Wall");
        if (Physics.Raycast(from, dir, out RaycastHit hit, dist, obstacleMask))
        {
            if (!hit.transform.IsChildOf(targetToCheck))
                return false;
        }

        return true;
    }


    void HandleSeekHeal()
    {
        if (cooldownTarget != Vector3.zero)
        {
            GameObject targetHeal = GameObject.FindGameObjectsWithTag("HealPrefab")
                .FirstOrDefault(h => Vector3.Distance(h.transform.position, cooldownTarget) < 1f);

            if (targetHeal == null)
            {
                currentState = State.Wander;
                return;
            }
        }

        if (target != null && Vector3.Distance(transform.position, target.position) < minAttackDistance)
        {
            cooldownTarget = GetFleePosition();
            currentState = State.Cooldown;
            return;
        }

        ResetPathIfTargetChanged(cooldownTarget);
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);

            if (dist <= attackRange && botStats.currentMana >= 5f)
            {
                currentState = State.Attack;
                return;
            }
        }

        MoveAlongPath(cooldownTarget);

        if (Vector3.Distance(transform.position, cooldownTarget) < 2f)
        {
            currentState = State.Wander;
        }
    }


    GameObject FindClosestHeal()
    {
        GameObject[] heals = GameObject.FindGameObjectsWithTag("HealPrefab");

        float closestDist = Mathf.Infinity;
        GameObject closest = null;

        foreach (var h in heals)
        {
            var healComp = h.GetComponent<HeartPickup>();
            if (healComp == null || healComp.isClaimed) continue;

            float dist = Vector3.Distance(transform.position, h.transform.position);

            // Chỉ xét những heal nằm trong phạm vi cho phép
            if (dist < healSearchRange && dist < closestDist)
            {
                closest = h;
                closestDist = dist;
            }
        }

        if (closest != null)
            closest.GetComponent<HeartPickup>().isClaimed = true;

        return closest;
    }



    Transform FindClosestTarget()
    {
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (obj == gameObject || obj.GetComponent<PlayerInfo>()?.hasDied == true) continue;
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = obj.transform;
            }
        }

        foreach (var bot in GameObject.FindGameObjectsWithTag("Bot"))
        {
            if (bot == gameObject || bot.GetComponent<BotStats>()?.currentHP <= 0) continue;
            float dist = Vector3.Distance(transform.position, bot.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = bot.transform;
            }
        }

        return closest;
    }

    void HandleWander()
    {
        ResetPathIfTargetChanged(wanderTarget);
        MoveAlongPath(wanderTarget);
        if (Vector3.Distance(transform.position, wanderTarget) < 4f)
        {
            ChooseWanderTarget();
            pathPoints.Clear();
            currentPathIndex = 0;
        }
    }

    void ChooseWanderTarget()
    {
        for (int i = 0; i < 10; i++) // Tăng số lần thử
        {
            Vector3 randomDir = Random.insideUnitSphere * 80f;
            randomDir.y = 0;
            Vector3 candidate = transform.position + randomDir;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 80f, NavMesh.AllAreas)) // Tăng range
            {
                wanderTarget = hit.position;
                return;
            }
        }
        // Fallback nếu không tìm được điểm xa
        wanderTarget = ClampToNavMesh(transform.position + Random.insideUnitSphere * 20f); // Luôn đảm bảo có điểm
    }
    void HandleChase()
    {
        if (target == null) return;
        Vector3 chasePos = ClampToNavMesh(target.position);
        ResetPathIfTargetChanged(chasePos);
        MoveAlongPath(chasePos);
    }

    void HandleAttack()
    {
        if (target == null || IsTargetDead(target) || botStats.currentMana < 5f)
        {
            currentState = State.Wander;
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        // Nếu không có tầm nhìn, di chuyển để tìm góc bắn
        if (!HasClearLineOfSightTo())
        {
            cooldownTarget = GetCooldownDestination();
            currentState = State.Cooldown;
            pathPoints.Clear();
            currentPathIndex = 0;
            return;
        }

        // Nếu không thể bắn do cooldown hoặc chưa quay mặt ĐỦ
        if (Time.time - lastAttackTime < attackCooldown || !IsFacingTarget())
        {
            // Vẫn ở trạng thái Attack nhưng không bắn, và chuyển sang Cooldown nếu cần né
            if (dist < minAttackDistance) // Nếu quá gần thì vẫn né ra
            {
                cooldownTarget = GetFleePosition();
                currentState = State.Cooldown;
            }
            else // Nếu không quá gần nhưng chưa đủ điều kiện bắn, vẫn ở trạng thái Attack nhưng không di chuyển hoặc di chuyển nhẹ
            {
                // Có thể thêm logic di chuyển nhẹ nhàng xung quanh target để duy trì tầm nhìn
                if (Random.value < 0.5f)
                {
                    cooldownTarget = ClampToNavMesh(target.position + (transform.position - target.position).normalized * (minAttackDistance + Random.Range(2f, 5f)));
                }
                else
                {
                    cooldownTarget = GetCooldownDestination();
                }
                currentState = State.Cooldown;
                ResetPathIfTargetChanged(cooldownTarget);
            }
            return;
        }

        // Đã facing + đủ mana + có tầm nhìn → bắn
        LookAtTargetSmooth();
        Shoot();
        lastAttackTime = Time.time;

        // Sau khi bắn, chuyển sang Cooldown để né tránh hoặc chuẩn bị cho lần bắn tiếp theo
        if (dist <= minAttackDistance) // Nếu quá gần sau khi bắn, lùi ra
        {
            cooldownTarget = GetFleePosition();
        }
        else
        {
            cooldownTarget = GetCooldownDestination(); // Lùi về vị trí cooldown
        }
        currentState = State.Cooldown;
    }

    void HandleCooldown()
    {
        ResetPathIfTargetChanged(cooldownTarget);
        MoveAlongPath(cooldownTarget);
        if (Vector3.Distance(transform.position, cooldownTarget) < 4f)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
                currentState = State.Chase;
            else
            {
                cooldownTarget = GetCooldownDestination();
                pathPoints.Clear();
                currentPathIndex = 0;
            }
        }

    }

    void Shoot()
    {
        botStats.currentMana -= 5;
        if (projectilePrefab && projectileSpawnPoint)
        {

            SpawnProjectile();
        }
    }

    void SpawnProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null) return;

        GameObject newProjectile = Instantiate(projectilePrefab, projectileSpawnPoint);
        newProjectile.GetComponent<ProjectileMoveScript>().ownerBot = botStats;

        newProjectile.transform.forward = transform.forward;
        newProjectile.transform.SetParent(null);
        newProjectile.transform.position = projectileSpawnPoint.position;

        Destroy(newProjectile, projectileLifetime);
    }


    void MoveAlongPath(Vector3 destination)
    {
        if (pathPoints.Count == 0 || currentPathIndex >= pathPoints.Count)
        {
            anim.SetFloat("Speed", 0f);
            return;
        }

        Vector3 currentPos = ClampToNavMesh(transform.position);
        Vector3 targetPoint = pathPoints[currentPathIndex];
        Vector3 dir = targetPoint - currentPos;

        if (dir.magnitude < pointThreshold)
        {
            currentPathIndex++;
            if (currentPathIndex >= pathPoints.Count)
            {
                anim.SetFloat("Speed", 0f);
                pathPoints.Clear();
                currentPathIndex = 0;
                return;
            }

            targetPoint = pathPoints[currentPathIndex];
            dir = targetPoint - currentPos;
        }

        dir.y = 0; // Đảm bảo không xoay theo trục Y nếu không cần thiết
        dir.Normalize();

        // Di chuyển bot
        rb.MovePosition(currentPos + dir * moveSpeed * Time.deltaTime);

        // Quay bot mượt mà hơn
        if (dir.sqrMagnitude > 0.01f)
        {
            // Điều chỉnh tốc độ quay để mượt mà hơn, ví dụ 20f
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)); // Chỉ xoay trên mặt phẳng XZ
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, Time.deltaTime * 12f); // Đặt trực tiếp rb.rotation cho mượt hơn
        }

        anim.SetFloat("Speed", 1f);
    }

    void ResetPathIfTargetChanged(Vector3 destination)
    {
        pathTickTimer += Time.deltaTime;
        if (pathTickTimer < pathTickRate) return;
        pathTickTimer = 0f;

        if (pathPoints.Count == 0 || Vector3.Distance(destination, pathPoints[^1]) > 2f)
        {
            // reset path ngay khi cần (không nên delay)
            Vector3 currentPos = ClampToNavMesh(transform.position);
            Vector3 destPos = ClampToNavMesh(destination);
            if (NavMesh.CalculatePath(currentPos, destPos, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                pathPoints = new List<Vector3>(path.corners);
                currentPathIndex = 1;
            }
            else
            {
                pathPoints.Clear(); // tránh path lỗi
                currentPathIndex = 0;
            }
        }

    }


    Vector3 ClampToNavMesh(Vector3 pos)
    {
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 12f, NavMesh.AllAreas))
            return hit.position;

        // fallback về giữa map
        if (NavMesh.SamplePosition(Vector3.zero, out hit, 30f, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }


    Vector3 GetCooldownDestination()
    {
        if (target == null)
            return transform.position + Random.insideUnitSphere * 12f;

        Vector3 away = (transform.position - target.position).normalized;

        float angleOffset = Random.Range(-30f, 30f);
        away = Quaternion.Euler(0, angleOffset, 0) * away;

        Vector3 dest = transform.position + away * cooldownMoveDistance;

        // 🛑 Nếu NavMesh không hợp lệ thì fallback
        Vector3 safePos;

        if (!NavMesh.SamplePosition(dest, out NavMeshHit hit, 8f, NavMesh.AllAreas))
        {
            // fallback đi gần trung tâm map
            Vector3 fallback = transform.position + (Vector3.zero - transform.position).normalized * 30f;

            if (!NavMesh.SamplePosition(fallback, out hit, 12f, NavMesh.AllAreas))
            {
                safePos = transform.position + Random.insideUnitSphere * 10f;
                safePos.y = 0;
            }
            else
            {
                safePos = hit.position;
            }
        }
        else
        {
            safePos = hit.position;
        }

        return safePos;

    }



    Vector3 GetFleePosition()
    {
        Vector3 fleeDir = Vector3.zero;
        int count = 0;

        float spacingFactor = 0.6f;
        float separationRadius = attackRange * spacingFactor;

        // Né bot khác
        foreach (var bot in GameObject.FindGameObjectsWithTag("Bot"))
        {
            if (bot == gameObject) continue;

            float dist = Vector3.Distance(transform.position, bot.transform.position);
            if (dist < separationRadius)
            {
                Vector3 away = (transform.position - bot.transform.position).normalized;
                float weight = separationRadius - dist;
                fleeDir += away * weight;
                count++;
            }
        }

        // Né player
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var info = player.GetComponent<PlayerInfo>();
            if (info != null && info.hasDied) continue;

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < separationRadius)
            {
                Vector3 away = (transform.position - player.transform.position).normalized;
                float weight = separationRadius - dist;
                fleeDir += away * weight;
                count++;
            }
        }

        if (count > 0)
        {
            fleeDir /= count;
            Vector3 fleeTarget = transform.position + fleeDir.normalized * fleeDistance;
            return ClampToNavMesh(fleeTarget);
        }

        // fallback
        Vector3 randomDir = Random.insideUnitSphere * fleeDistance;
        randomDir.y = 0;
        return ClampToNavMesh(transform.position + randomDir);
    }





    void RegenMana()
    {
        if (botStats == null) return;
        botStats.currentMana += 3f;
        if (botStats.currentMana > botStats.maxMana) botStats.currentMana = botStats.maxMana;
    }

    void HandleDeath()
    {
        FindObjectOfType<KillInfoUIHandler>()?.PlayerDied();
        currentState = State.Die;
        anim.SetTrigger("Die");
        rb.velocity = Vector3.zero;
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.isKinematic = true;
        rb.detectCollisions = false;

        pathPoints.Clear();
        currentPathIndex = 0;

        if (worldUI != null)
            Destroy(worldUI, 5f);
        Destroy(gameObject, 5f);
    }


    void ForceMoveIfIdle()
    {
        if (anim.GetFloat("Speed") <= 0.01f && currentState != State.Die) // Chỉ kiểm tra khi bot không chết
            noMoveTimer += Time.deltaTime;
        else
            noMoveTimer = 0f;

        // Giảm thời gian chờ để bot phản ứng nhanh hơn
        if (noMoveTimer > 0.3f) // ⏱ Tăng lên để nhạy hơn
        {
            // Khi bot đứng yên quá lâu, ép nó di chuyển đến một vị trí ngẫu nhiên hoặc vị trí cooldown
            cooldownTarget = GetCooldownDestination();
            currentState = State.Cooldown;
            pathPoints.Clear();
            currentPathIndex = 0;
            noMoveTimer = 0f; // Reset timer
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 4f);

        if (cooldownTarget != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, cooldownTarget);
            Gizmos.DrawSphere(cooldownTarget, 1f);
        }
    }

    void CheckStuckAndResetIfNeeded()
    {
        // Kiểm tra xem phía trước bot có bị chặn bởi NavMesh không
        if (!NavMesh.SamplePosition(transform.position + transform.forward * 4f, out NavMeshHit hit, 4f, NavMesh.AllAreas))
        {
            // Nếu bị chặn, ép bot lùi lại hoặc đổi hướng
            cooldownTarget = GetFleePosition() + Random.insideUnitSphere * 8f; // Thêm ngẫu nhiên
            currentState = State.Cooldown;
            pathPoints.Clear();
            currentPathIndex = 0;
            return;
        }

        // Kiểm tra xem bot có di chuyển không
        if (Vector3.Distance(transform.position, lastPos) < 0.1f)
            stuckTimer += Time.deltaTime;
        else
        {
            stuckTimer = 0f;
            lastPos = transform.position;
        }

        // Nếu bot bị kẹt quá lâu
        if (stuckTimer > 1.5f && !isForcingMovement) // Giảm thời gian kẹt xuống
        {
            cooldownTarget = GetFleePosition(); // Hoặc một điểm đến ngẫu nhiên khác
            currentState = State.Cooldown;
            isForcingMovement = true;
            pathPoints.Clear();
            currentPathIndex = 0;
            stuckTimer = 0f; // Reset timer
        }
    }
    bool CanShootNow()
    {
        return Time.time - lastAttackTime >= attackCooldown
            && botStats.currentMana >= 5
            && IsFacingTarget()
            && HasClearLineOfSightTo();
    }


    public void SetWorldUI(GameObject ui) => worldUI = ui;

    bool IsFacingTarget()
    {
        if (!target) return false;
        Vector3 dir = (target.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, dir) > 0.55f;
    }

    bool IsTargetDead(Transform t)
    {
        var p = t.GetComponent<PlayerInfo>();
        if (p != null) return p.hasDied;
        var b = t.GetComponent<BotStats>();
        return b != null && b.currentHP <= 0;
    }


    private float originalSpeed;

    void Awake()
    {
        originalSpeed = moveSpeed;
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetSpeed()
    {
        moveSpeed = originalSpeed;
    }

    public void ForceDie()
    {
        if (botStats.currentHP <= 0) return;
        botStats.currentHP = 0;
        HandleDeath();
    }

}


// using UnityEngine;
//  using UnityEngine.AI;
//  using System.Collections.Generic;
//  using System.Linq;

//  public class BotAI : MonoBehaviour
//  {
//      [Header("Settings")]
//      public float detectRange = 60f;
//      public float attackRange = 40f;
//      public float minAttackDistance = 15f;
//      public float fleeDistance = 25f;
//      public float cooldownMoveDistance = 50f;
//      public float moveSpeed = 15f;
//      public float attackCooldown = 2f;
//      [SerializeField] float healSearchRange = 80f;

//      [Header("Runtime")]
//      public Transform target;

//      private Rigidbody rb;
//      private Animator anim;
//      public BotStats botStats;
//      private GameObject worldUI;

//      private NavMeshPath path;
//      private List<Vector3> pathPoints = new List<Vector3>();
//      private int currentPathIndex = 0;

//      private Vector3 wanderTarget;
//      private Vector3 cooldownTarget;
//      private Vector3 lastPos;
//      private float lastAttackTime = Mathf.NegativeInfinity;

//      private float stuckTimer = 0f;
//      private float noMoveTimer = 0f;
//      private float fleeCooldownTimer = 0f;

//      private float pointThreshold = 2.5f; // Increased for smoother path following, bot won't stop at every tiny point

//      enum State { Wander, Chase, Attack, Cooldown, Die, SeekHeal }
//      private State currentState;

//      [Header("Attack Settings")]
//      public GameObject projectilePrefab;
//      public Transform projectileSpawnPoint;
//      public float projectileLifetime = 1f;
//      private float aiTickTimer = 0f;
//      private float aiTickRate = 0.15f; // Slightly increased for less frequent state checks

//      private float pathTickTimer = 0f;
//      private float pathTickRate = 0.5f; // Reduced path recalculations for smoothness
//      [SerializeField] float healCooldown = 5f;
//      private float lastHealTime = -999f;
//      private float animSpeed = 0f;
//      private float lastEscapeFromBehindTime = -10f;
//      private float escapeCooldown = 1.2f;

//      // New smoothing parameters
//      [Header("Movement Smoothing")]
//      public float rotationSpeed = 15f; // Increased rotation speed for responsiveness
//      public float movementSmoothTime = 0.1f; // How quickly the movement catches up to target speed
//      private Vector3 currentVelocity = Vector3.zero;
//      private Vector3 smoothDampTargetVelocity;


//      void Start()
//      {
//          rb = GetComponent<Rigidbody>();
//          anim = GetComponent<Animator>();
//          botStats = GetComponent<BotStats>();
//          path = new NavMeshPath();
//          lastPos = transform.position;
//          aiTickRate += Random.Range(-0.05f, 0.05f);
//          InvokeRepeating(nameof(ChooseWanderTarget), 0f, 5f);
//          InvokeRepeating(nameof(RegenMana), 1f, 1f);

//          // Ensure Rigidbody settings are conducive to smooth movement
//          rb.interpolation = RigidbodyInterpolation.Interpolate;
//          rb.freezeRotation = true; // Prevent physics engine from rotating the bot uncontrollably
//      }

//      void Update()
//      {
//          if (botStats.currentHP <= 0 && currentState != State.Die)
//          {
//              HandleDeath();
//              return;
//          }
//          if (currentState == State.Die) return;

//          aiTickTimer += Time.deltaTime;
//          if (aiTickTimer >= aiTickRate)
//          {
//              aiTickTimer = 0f;
//              UpdateStateLogic();
//          }

//          // Only check for stuck and idle if the bot is supposed to be moving
//          if (currentState != State.Attack) // Bot can be idle during attack for aiming
//          {
//              ForceMoveIfIdle();
//              CheckStuckAndResetIfNeeded();
//          }


//          // 🧠 Phản ứng khi bị dí từ phía sau - prioritize this in Cooldown
//          if (currentState == State.Cooldown && Time.time - lastEscapeFromBehindTime > escapeCooldown)
//          {
//              if (IsEnemyTooCloseBehind(out Vector3 safeDir))
//              {
//                  cooldownTarget = ClampToNavMesh(transform.position + safeDir * cooldownMoveDistance * Random.Range(0.8f, 1.2f));
//                  pathPoints.Clear();
//                  currentPathIndex = 0;
//                  lastEscapeFromBehindTime = Time.time;
//              }
//          }

//          // State machine for movement logic
//          switch (currentState)
//          {
//              case State.Wander: HandleWander(); break;
//              case State.Chase: HandleChase(); break;
//              case State.Attack: HandleAttack(); break;
//              case State.Cooldown: HandleCooldown(); break;
//              case State.SeekHeal: HandleSeekHeal(); break;
//          }

//          // Apply movement and rotation after state logic has determined desired movement
//          ApplyMovementAndRotation();
//      }

//      bool IsEnemyTooCloseBehind(out Vector3 safeDir)
//      {
//          safeDir = Vector3.zero;
//          Vector3 myBack = -transform.forward;
//          int count = 0;

//          foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
//          {
//              var info = obj.GetComponent<PlayerInfo>();
//              if (info == null || info.hasDied) continue;

//              Vector3 toEnemy = obj.transform.position - transform.position;
//              float dist = toEnemy.magnitude;
//              if (dist < 9f && Vector3.Dot(toEnemy.normalized, myBack) > 0.7f)
//              {
//                  // Move more directly away from the enemy, or perpendicular
//                  Vector3 perpendicular = Vector3.Cross(toEnemy, Vector3.up).normalized * Random.Range(-1f, 1f); // Random side dodge
//                  safeDir += (perpendicular + myBack).normalized; // Combine direct back and perpendicular dodge
//                  count++;
//              }
//          }

//          if (count > 0)
//          {
//              safeDir = safeDir.normalized;
//              return true;
//          }
//          return false;
//      }

//      void LookAtTargetSmooth()
//      {
//          if (target == null) return;

//          Vector3 dir = (target.position - transform.position);
//          dir.y = 0;
//          if (dir.sqrMagnitude > 0.001f)
//          {
//              Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
//              rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, Time.deltaTime * rotationSpeed);
//          }
//      }


//      void UpdateStateLogic()
//      {
//          // Priority 1: Low HP - Seek Heal or Flee/Attack if conditions met
//          if (botStats.currentHP < botStats.maxHP * 0.3f)
//          {
//              float playerDist = target != null ? Vector3.Distance(transform.position, target.position) : Mathf.Infinity;

//              // If low HP AND can attack (and is close enough), prioritize attacking
//              if (playerDist <= attackRange && botStats.currentMana >= 5f && CanShootNow())
//              {
//                  currentState = State.Attack;
//                  return;
//              }

//              // Otherwise, try to find a heal or flee
//              if (Time.time - lastHealTime >= healCooldown && Random.value < 0.8f)
//              {
//                  GameObject heal = FindClosestHeal();
//                  if (heal != null)
//                  {
//                      cooldownTarget = heal.transform.position;
//                      currentState = State.SeekHeal;
//                      lastHealTime = Time.time;
//                      return;
//                  }
//              }

//              // If can't heal, and enemy is too close, prioritize fleeing
//              if (target != null && Vector3.Distance(transform.position, target.position) < fleeDistance)
//              {
//                  currentState = State.Cooldown;
//                  cooldownTarget = GetFleePosition();
//                  return;
//              }
//          }

//          // Priority 2: Flee if enemy too close (even if not low HP, for tactical retreat)
//          if (target != null && Vector3.Distance(transform.position, target.position) < fleeDistance)
//          {
//              // If can shoot while fleeing, maybe take a shot
//              if (botStats.currentMana >= 5f && IsFacingTarget() && HasClearLineOfSightTo())
//              {
//                  currentState = State.Attack;
//                  return;
//              }
//              currentState = State.Cooldown; // Flee
//              cooldownTarget = GetFleePosition();
//              return;
//          }

//          // Priority 3: Find new target or maintain current target
//          Transform newTarget = FindClosestTarget();
//          if (newTarget == null)
//          {
//              target = null;
//              currentState = State.Wander;
//              return;
//          }

//          // Logic to switch targets
//          if (target == null || IsTargetDead(target) || (HasClearLineOfSightTo(newTarget) && !HasClearLineOfSightTo(target)))
//          {
//              target = newTarget;
//          }
//          else
//          {
//              float currDist = Vector3.Distance(transform.position, target.position);
//              float newDist = Vector3.Distance(transform.position, newTarget.position);
//              if (newDist + 3f < currDist) // More sensitive to closer targets
//                  target = newTarget;
//          }

//          float dist = Vector3.Distance(transform.position, target.position);

//          // Priority 4: Attack if conditions met
//          if (dist <= attackRange && CanShootNow())
//          {
//              currentState = State.Attack;
//              return;
//          }

//          // Priority 5: Cooldown/Reposition if cannot attack or too close
//          if (Time.time - lastAttackTime < attackCooldown || botStats.currentMana < 5f || !IsFacingTarget(0.6f) || !HasClearLineOfSightTo())
//          {
//              if (currentState != State.Cooldown)
//              {
//                  cooldownTarget = GetCooldownDestination();
//                  pathPoints.Clear();
//                  currentPathIndex = 0;
//              }
//              currentState = State.Cooldown;
//              return;
//          }

//          // Priority 6: Chase if within detection range
//          if (dist <= detectRange)
//              currentState = State.Chase;
//          else
//              currentState = State.Wander;
//      }


//      bool HasClearLineOfSightTo(Transform targetToCheck = null)
//      {
//          if (targetToCheck == null) targetToCheck = target;
//          if (!targetToCheck) return false;

//          Vector3 from = projectileSpawnPoint ? projectileSpawnPoint.position : transform.position + Vector3.up;
//          Vector3 to = targetToCheck.position + Vector3.up;
//          Vector3 dir = (to - from).normalized;
//          float dist = Vector3.Distance(from, to);

//          int obstacleMask = LayerMask.GetMask("Obstacle", "Wall"); // Ensure these layers are correctly set up
//          if (Physics.Raycast(from, dir, out RaycastHit hit, dist, obstacleMask))
//          {
//              // If the hit object is not the target itself (or a child of it), then line of sight is blocked
//              if (!hit.transform.IsChildOf(targetToCheck) && hit.transform != targetToCheck)
//                  return false;
//          }

//          return true;
//      }

//      void HandleSeekHeal()
//      {
//          // Re-evaluate heal target in case it's gone or claimed
//          GameObject targetHeal = GameObject.FindGameObjectsWithTag("HealPrefab")
//              .FirstOrDefault(h => Vector3.Distance(h.transform.position, cooldownTarget) < 1f && h.GetComponent<HeartPickup>()?.isClaimed == true);

//          if (targetHeal == null) // Heal pickup is gone or claimed by another bot/player
//          {
//              currentState = State.Wander;
//              return;
//          }

//          // If enemy gets too close while seeking heal, flee
//          if (target != null && Vector3.Distance(transform.position, target.position) < minAttackDistance)
//          {
//              cooldownTarget = GetFleePosition();
//              currentState = State.Cooldown;
//              return;
//          }

//          MoveAlongPath(cooldownTarget);

//          if (Vector3.Distance(transform.position, cooldownTarget) < 2f)
//          {
//              currentState = State.Wander; // Reached heal point, go back to wandering
//          }
//      }


//      GameObject FindClosestHeal()
//      {
//          GameObject[] heals = GameObject.FindGameObjectsWithTag("HealPrefab");

//          float closestDist = Mathf.Infinity;
//          GameObject closest = null;

//          foreach (var h in heals)
//          {
//              var healComp = h.GetComponent<HeartPickup>();
//              if (healComp == null || healComp.isClaimed) continue;

//              float dist = Vector3.Distance(transform.position, h.transform.position);

//              if (dist < healSearchRange && dist < closestDist)
//              {
//                  closest = h;
//                  closestDist = dist;
//              }
//          }

//          // Only claim if a heal is actually found
//          if (closest != null)
//              closest.GetComponent<HeartPickup>().isClaimed = true;

//          return closest;
//      }


//      Transform FindClosestTarget()
//      {
//          float closestDistance = detectRange; // Only consider targets within detectRange
//          Transform closest = null;

//          // Check players
//          foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
//          {
//              if (obj == gameObject || obj.GetComponent<PlayerInfo>()?.hasDied == true) continue;
//              float dist = Vector3.Distance(transform.position, obj.transform.position);
//              if (dist < closestDistance)
//              {
//                  closestDistance = dist;
//                  closest = obj.transform;
//              }
//          }

//          // Check other bots
//          foreach (var bot in GameObject.FindGameObjectsWithTag("Bot"))
//          {
//              if (bot == gameObject || bot.GetComponent<BotStats>()?.currentHP <= 0) continue;
//              float dist = Vector3.Distance(transform.position, bot.transform.position);
//              if (dist < closestDistance)
//              {
//                  closestDistance = dist;
//                  closest = bot.transform;
//              }
//          }

//          return closest;
//      }

//      void HandleWander()
//      {
//          ResetPathIfTargetChanged(wanderTarget);
//          MoveAlongPath(wanderTarget);
//          if (Vector3.Distance(transform.position, wanderTarget) < 4f)
//          {
//              ChooseWanderTarget();
//              pathPoints.Clear(); // Clear path to force recalculation for new wander target
//              currentPathIndex = 0;
//          }
//      }

//      void ChooseWanderTarget()
//      {
//          Vector3 currentPos = ClampToNavMesh(transform.position);
//          for (int i = 0; i < 15; i++) // Increased attempts
//          {
//              Vector3 randomDir = Random.insideUnitSphere * 80f;
//              randomDir.y = 0;
//              Vector3 candidate = currentPos + randomDir;
//              if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 80f, NavMesh.AllAreas))
//              {
//                  wanderTarget = hit.position;
//                  return;
//              }
//          }
//          // Fallback if no distant point found, pick something closer but valid
//          wanderTarget = ClampToNavMesh(currentPos + Random.insideUnitSphere * 20f);
//      }

//      void HandleChase()
//      {
//          if (target == null) return;
//          Vector3 chasePos = ClampToNavMesh(target.position);
//          ResetPathIfTargetChanged(chasePos);
//          MoveAlongPath(chasePos);
//      }

//      void HandleAttack()
//      {
//          if (target == null || IsTargetDead(target) || botStats.currentMana < 5f)
//          {
//              currentState = State.Wander;
//              return;
//          }

//          float dist = Vector3.Distance(transform.position, target.position);

//          // Prioritize line of sight and facing target
//          if (!HasClearLineOfSightTo() || !IsFacingTarget())
//          {
//              // If not facing or blocked, try to reposition
//              cooldownTarget = GetCooldownDestination();
//              currentState = State.Cooldown;
//              pathPoints.Clear();
//              currentPathIndex = 0;
//              return;
//          }

//          // If can shoot (faced, clear line of sight, enough mana, not on cooldown)
//          if (CanShootNow())
//          {
//              LookAtTargetSmooth();
//              Shoot();
//              lastAttackTime = Time.time;

//              // After shooting, immediately transition to Cooldown for repositioning
//              if (dist <= minAttackDistance)
//              {
//                  cooldownTarget = GetFleePosition();
//              }
//              else
//              {
//                  cooldownTarget = GetCooldownDestination();
//              }
//              currentState = State.Cooldown;
//          }
//          else if (dist < minAttackDistance) // Too close, even if can't shoot yet
//          {
//              cooldownTarget = GetFleePosition();
//              currentState = State.Cooldown;
//          }
//          else // Not too close, but can't shoot (e.g., still on cooldown, waiting to face)
//          {
//              // Maintain current position or subtle movement while waiting to shoot
//              // No explicit movement here, just let LookAtTargetSmooth handle rotation
//              // The bot will naturally try to stay in attack range if it's not too close.
//          }
//      }


//      void HandleCooldown()
//      {
//          ResetPathIfTargetChanged(cooldownTarget);
//          MoveAlongPath(cooldownTarget);

//          // If bot reaches cooldown destination or cooldown expires, re-evaluate
//          if (Vector3.Distance(transform.position, cooldownTarget) < pointThreshold)
//          {
//              // If attack cooldown is over, go back to chasing
//              if (Time.time - lastAttackTime >= attackCooldown)
//              {
//                  currentState = State.Chase;
//              }
//              else // Still on cooldown, find a new cooldown spot to keep moving
//              {
//                  cooldownTarget = GetCooldownDestination();
//                  pathPoints.Clear();
//                  currentPathIndex = 0;
//              }
//          }
//      }

//      void Shoot()
//      {
//          if (botStats.currentMana < 5) return; // Double check mana
//          botStats.currentMana -= 5;
//          if (projectilePrefab && projectileSpawnPoint)
//          {
//              SpawnProjectile();
//          }
//      }

//      void SpawnProjectile()
//      {
//          if (projectilePrefab == null || projectileSpawnPoint == null) return;

//          GameObject newProjectile = Instantiate(projectilePrefab, projectileSpawnPoint);
//          newProjectile.GetComponent<ProjectileMoveScript>().ownerBot = botStats;

//          newProjectile.transform.forward = transform.forward;
//          newProjectile.transform.SetParent(null);
//          newProjectile.transform.position = projectileSpawnPoint.position;

//          Destroy(newProjectile, projectileLifetime);
//      }


//      void MoveAlongPath(Vector3 destination)
//      {
//          if (pathPoints.Count == 0 || currentPathIndex >= pathPoints.Count)
//          {
//              smoothDampTargetVelocity = Vector3.zero; // Stop movement
//              anim.SetFloat("Speed", 0f);
//              return;
//          }

//          Vector3 currentPos = transform.position; // Use transform.position here, Rigidbody.MovePosition is in FixedUpdate
//          Vector3 targetPoint = pathPoints[currentPathIndex];
//          Vector3 dir = targetPoint - currentPos;

//          // Check if we're close enough to the current path point to move to the next
//          if (dir.magnitude < pointThreshold)
//          {
//              currentPathIndex++;
//              if (currentPathIndex >= pathPoints.Count)
//              {
//                  smoothDampTargetVelocity = Vector3.zero;
//                  anim.SetFloat("Speed", 0f);
//                  pathPoints.Clear();
//                  currentPathIndex = 0;
//                  return;
//              }
//              targetPoint = pathPoints[currentPathIndex];
//              dir = targetPoint - currentPos;
//          }

//          dir.y = 0;
//          if (dir.sqrMagnitude > 0.01f) // Ensure direction is valid
//          {
//              dir.Normalize();
//              smoothDampTargetVelocity = dir * moveSpeed; // Set desired velocity for smooth damp
//              anim.SetFloat("Speed", 1f); // Set speed for animation
//          }
//          else
//          {
//              smoothDampTargetVelocity = Vector3.zero;
//              anim.SetFloat("Speed", 0f); // Stop animation
//          }
//      }

//      // New function to apply movement and rotation smoothly
//      void ApplyMovementAndRotation()
//      {
//          // Smoothly move the Rigidbody
//          Vector3 newVelocity = Vector3.SmoothDamp(rb.velocity, smoothDampTargetVelocity, ref currentVelocity, movementSmoothTime);
//          rb.velocity = newVelocity;

//          // Smoothly rotate the Rigidbody
//          if (smoothDampTargetVelocity.sqrMagnitude > 0.01f) // Only rotate if moving
//          {
//              Quaternion targetRot = Quaternion.LookRotation(new Vector3(smoothDampTargetVelocity.x, 0, smoothDampTargetVelocity.z).normalized);
//              rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, Time.deltaTime * rotationSpeed);
//          }
//          else if (currentState == State.Attack && target != null) // If attacking, always face target
//          {
//              LookAtTargetSmooth();
//          }
//      }


//      void ResetPathIfTargetChanged(Vector3 destination)
//      {
//          pathTickTimer += Time.deltaTime;
//          // Only recalculate path if enough time has passed OR current path is invalid/empty AND destination has changed significantly
//          if (pathTickTimer < pathTickRate && pathPoints.Count > 0 && Vector3.Distance(destination, pathPoints[pathPoints.Count - 1]) < 2f)
//          {
//              return; // Don't recalculate if recently done and destination is similar
//          }

//          pathTickTimer = 0f; // Reset timer after attempt to recalculate

//          Vector3 currentPos = ClampToNavMesh(transform.position);
//          Vector3 destPos = ClampToNavMesh(destination);

//          if (NavMesh.CalculatePath(currentPos, destPos, NavMesh.AllAreas, path))
//         {
//             if (path.status == NavMeshPathStatus.PathComplete)
//             {
//                 pathPoints = new List<Vector3>(path.corners);
//                 currentPathIndex = 1;
//             }
//             else // Path Partial or Invalid
//             {
//                 Debug.LogWarning($"Path status for {name}: {path.status}. Destination: {destPos}. Clearing path.");
//                 pathPoints.Clear();
//                 currentPathIndex = 0;
//                 // Fallback: If path is partial, maybe try to move towards the first valid point on the partial path
//                 // Or, if truly stuck, enter a special "Stuck" state to re-evaluate.
//             }
//         }
//         else
//         {
//             Debug.LogWarning($"NavMesh.CalculatePath failed for {name}. Clearing path.");
//             pathPoints.Clear();
//             currentPathIndex = 0;
//         }
//      }


//      Vector3 ClampToNavMesh(Vector3 pos)
//      {
//          if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 12f, NavMesh.AllAreas))
//              return hit.position;

//          // Fallback to a close point on NavMesh if original position is invalid
//          if (NavMesh.SamplePosition(transform.position, out hit, 12f, NavMesh.AllAreas))
//              return hit.position;

//          // Last resort: try center of the map
//          if (NavMesh.SamplePosition(Vector3.zero, out hit, 30f, NavMesh.AllAreas))
//              return hit.position;

//          Debug.LogWarning("ClampToNavMesh failed to find a valid position for " + name + ". Returning current transform position.");
//          return transform.position;
//      }


//      Vector3 GetCooldownDestination()
//      {
//          if (target == null)
//              return ClampToNavMesh(transform.position + Random.insideUnitSphere * 12f);

//          Vector3 away = (transform.position - target.position).normalized;

//          // Introduce more directional variance for dodging/cooldown
//          float angleOffset = Random.Range(-45f, 45f); // Increased range
//          away = Quaternion.Euler(0, angleOffset, 0) * away;

//          Vector3 dest = transform.position + away * cooldownMoveDistance * Random.Range(0.8f, 1.2f); // Randomize distance slightly

//          Vector3 safePos;
//          if (NavMesh.SamplePosition(dest, out NavMeshHit hit, 10f, NavMesh.AllAreas)) // Increased range
//          {
//              safePos = hit.position;
//          }
//          else
//          {
//              // Fallback: try moving to a random point within a smaller radius, or towards map center
//              Vector3 fallback = transform.position + Random.insideUnitSphere * 20f;
//              if (NavMesh.SamplePosition(fallback, out hit, 20f, NavMesh.AllAreas))
//              {
//                  safePos = hit.position;
//              }
//              else // Last resort, just move a bit
//              {
//                  safePos = ClampToNavMesh(transform.position + Random.insideUnitSphere * 5f);
//              }
//          }
//          return safePos;
//      }


//      Vector3 GetFleePosition()
//      {
//          Vector3 fleeDir = Vector3.zero;
//          int count = 0;

//          float separationRadius = fleeDistance * 0.8f; // Use a slightly smaller radius for "separation"
//          float avoidanceForce = 1f; // How strongly to avoid
//          float maxAvoidDistance = fleeDistance + 5f; // Max distance to consider for avoidance

//          // Avoid other bots
//          foreach (var bot in GameObject.FindGameObjectsWithTag("Bot"))
//          {
//              if (bot == gameObject) continue;

//              float dist = Vector3.Distance(transform.position, bot.transform.position);
//              if (dist < separationRadius)
//              {
//                  Vector3 away = (transform.position - bot.transform.position).normalized;
//                  float weight = (separationRadius - dist) / separationRadius; // Stronger avoidance closer
//                  fleeDir += away * weight * avoidanceForce;
//                  count++;
//              }
//          }

//          // Avoid players
//          foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
//          {
//              var info = player.GetComponent<PlayerInfo>();
//              if (info != null && info.hasDied) continue;

//              float dist = Vector3.Distance(transform.position, player.transform.position);
//              if (dist < separationRadius)
//              {
//                  Vector3 away = (transform.position - player.transform.position).normalized;
//                  float weight = (separationRadius - dist) / separationRadius;
//                  fleeDir += away * weight * avoidanceForce;
//                  count++;
//              }
//          }

//          if (count > 0)
//          {
//              fleeDir /= count;
//              // Add a random component to flee direction to make it less predictable
//              fleeDir = Quaternion.Euler(0, Random.Range(-20f, 20f), 0) * fleeDir;
//              Vector3 fleeTarget = transform.position + fleeDir.normalized * fleeDistance;
//              return ClampToNavMesh(fleeTarget);
//          }

//          // Fallback if no immediate threats: just flee from target or to a random spot
//          if (target != null)
//          {
//              Vector3 awayFromTarget = (transform.position - target.position).normalized;
//              Vector3 randomOffset = Random.insideUnitSphere * 0.5f; // Small random offset
//              randomOffset.y = 0;
//              return ClampToNavMesh(transform.position + (awayFromTarget + randomOffset).normalized * fleeDistance);
//          }
//          else
//          {
//              Vector3 randomDir = Random.insideUnitSphere * fleeDistance;
//              randomDir.y = 0;
//              return ClampToNavMesh(transform.position + randomDir);
//          }
//      }


//      void RegenMana()
//      {
//          if (botStats == null) return;
//          botStats.currentMana += 3f;
//          if (botStats.currentMana > botStats.maxMana) botStats.currentMana = botStats.maxMana;
//      }

//      void HandleDeath()
//      {
//          FindObjectOfType<KillInfoUIHandler>()?.PlayerDied();
//          currentState = State.Die;
//          anim.SetTrigger("Die");
//          rb.velocity = Vector3.zero;
//          Collider col = GetComponent<Collider>();
//          if (col != null)
//              col.enabled = false;

//          rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
//          rb.isKinematic = true;
//          rb.detectCollisions = false;

//          pathPoints.Clear();
//          currentPathIndex = 0;

//          if (worldUI != null)
//              Destroy(worldUI, 5f);
//          Destroy(gameObject, 5f);
//      }


//      void ForceMoveIfIdle()
//      {
//          // Only force move if not actively moving and not already dead or about to die
//          if (anim.GetFloat("Speed") <= 0.01f && currentState != State.Die)
//              noMoveTimer += Time.deltaTime;
//          else
//              noMoveTimer = 0f;

//          if (noMoveTimer > 0.5f) // Increased to 0.5s for less sensitivity
//          {
//              cooldownTarget = GetCooldownDestination();
//              currentState = State.Cooldown;
//              pathPoints.Clear();
//              currentPathIndex = 0;
//              noMoveTimer = 0f;
//          }
//      }

//      void OnDrawGizmos()
//      {
//          Gizmos.color = Color.red;
//          Gizmos.DrawLine(transform.position, transform.position + transform.forward * 4f);

//          if (cooldownTarget != Vector3.zero)
//          {
//              Gizmos.color = Color.yellow;
//              Gizmos.DrawLine(transform.position, cooldownTarget);
//              Gizmos.DrawSphere(cooldownTarget, 1f);
//          }

//          if (pathPoints != null && pathPoints.Count > 0)
//          {
//              Gizmos.color = Color.blue;
//              for (int i = currentPathIndex; i < pathPoints.Count - 1; i++)
//              {
//                  Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
//              }
//              Gizmos.DrawSphere(pathPoints[currentPathIndex], 0.5f); // Current target point
//          }

//          // Draw detection and attack ranges
//          Gizmos.color = Color.green;
//          Gizmos.DrawWireSphere(transform.position, detectRange);
//          Gizmos.color = Color.magenta;
//          Gizmos.DrawWireSphere(transform.position, attackRange);
//      }

//      void CheckStuckAndResetIfNeeded()
//      {
//          // Check if bot is visually stuck (not moving much)
//          if (Vector3.Distance(transform.position, lastPos) < 0.2f) // Increased threshold slightly
//              stuckTimer += Time.deltaTime;
//          else
//          {
//              stuckTimer = 0f;
//              lastPos = transform.position;
//          }

//          if (stuckTimer > 1.5f) // Time before considered stuck
//          {
//              // Try to find a new, random cooldown destination to break free
//              cooldownTarget = GetCooldownDestination();
//              currentState = State.Cooldown;
//              pathPoints.Clear();
//              currentPathIndex = 0;
//              stuckTimer = 0f;
//          }

//          // Additionally, check for NavMesh blockage directly in front
//          // This is a more proactive check for obstacles
//          if (currentState != State.Attack) // Don't interrupt attack if just blocked for a moment
//          {
//              if (!NavMesh.SamplePosition(transform.position + transform.forward * 2f, out NavMeshHit hit, 2f, NavMesh.AllAreas)) // Check closer
//              {
//                  // If the space directly in front is not valid NavMesh, implies an obstacle
//                  if (stuckTimer > 0.5f) // Only react if stuck for a moment
//                  {
//                      cooldownTarget = GetFleePosition(); // Flee from the obstacle
//                      currentState = State.Cooldown;
//                      pathPoints.Clear();
//                      currentPathIndex = 0;
//                      stuckTimer = 0f; // Reset stuck timer
//                  }
//              }
//          }
//      }

//      bool CanShootNow()
//      {
//          return Time.time - lastAttackTime >= attackCooldown
//                 && botStats.currentMana >= 5
//                 && IsFacingTarget() // Use default facing threshold for shooting
//                 && HasClearLineOfSightTo();
//      }


//      public void SetWorldUI(GameObject ui) => worldUI = ui;

//      bool IsFacingTarget(float angleThreshold = 0.85f) // Added optional parameter for threshold
//      {
//          if (!target) return false;
//          Vector3 dir = (target.position - transform.position).normalized;
//          return Vector3.Dot(transform.forward, dir) > angleThreshold;
//      }

//      bool IsTargetDead(Transform t)
//      {
//          var p = t.GetComponent<PlayerInfo>();
//          if (p != null) return p.hasDied;
//          var b = t.GetComponent<BotStats>();
//          return b != null && b.currentHP <= 0;
//      }

//      private float originalSpeed;

//      void Awake()
//      {
//          originalSpeed = moveSpeed;
//      }

//      public void SetSpeed(float newSpeed)
//      {
//          moveSpeed = newSpeed;
//      }

//      public void ResetSpeed()
//      {
//          moveSpeed = originalSpeed;
//      }

//      public void ForceDie()
//      {
//          if (botStats.currentHP <= 0) return;
//          botStats.currentHP = 0;
//          HandleDeath();
//      }
//  }