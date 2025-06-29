using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class BotAI : MonoBehaviour
{
    [Header("Settings")]
    public float detectRange = 90f;
    public float attackRange = 60f;
    public float minAttackDistance = 3f;
    public float fleeDistance = 8f;
    public float cooldownMoveDistance = 40f;
    public float moveSpeed = 25f;
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

    private float pointThreshold = 1.9f;

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
    private float lastAttackCooldownSetTime = -999f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        botStats = GetComponent<BotStats>();
        path = new NavMeshPath();
        lastPos = transform.position;
        aiTickRate = 0.2f + Random.Range(-0.03f, 0.03f);
        pathTickRate = 0.2f;

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
            ForceMoveIfIdle(); // Đưa lên đây
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
            Quaternion smoothRot = Quaternion.Slerp(rb.rotation, targetRot, Time.deltaTime * 50f);
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
            // Luôn luôn phản ứng bằng cách di chuyển
            Vector3 fleeDir = (transform.position - target.position).normalized;
            if (fleeDir.sqrMagnitude < 0.01f)
            {
                // fallback nếu Player sát bot quá
                fleeDir = Random.insideUnitSphere;
                fleeDir.y = 0;
            }

            cooldownTarget = ClampToNavMesh(transform.position + fleeDir.normalized * cooldownMoveDistance);
            currentState = State.Cooldown;
            pathPoints.Clear();
            currentPathIndex = 0;
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

            if (newTarget != target && newDist + 3f < currDist) // Giảm nhạy
            {
                target = newTarget;
            }
        }


        float dist = Vector3.Distance(transform.position, target.position);

        if (dist <= attackRange)
        {
            if (CanShootNow())
            {
                currentState = State.Attack;
                return;
            }
            else
            {
                // 🧠 Nếu quá gần thì di chuyển ra xa 1 chút để bắn
                float currentDist = Vector3.Distance(transform.position, target.position);
                if (currentDist < minAttackDistance)
                {
                    cooldownTarget = ClampToNavMesh(transform.position - (target.position - transform.position).normalized * (minAttackDistance + 4f));
                }

                else
                {
                    // Di chuyển ngang hoặc xung quanh để giữ khoảng cách
                    Vector3 dir = (transform.position - target.position).normalized;
                    Vector3 side = Quaternion.Euler(0, Random.Range(-70f, 70f), 0) * dir;
                    cooldownTarget = ClampToNavMesh(transform.position + side * 20f);
                }

                currentState = State.Cooldown;
                ResetPathIfTargetChanged(cooldownTarget);
                return;
            }
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



    // Transform FindClosestTarget()
    // {
    //     float closestDistance = Mathf.Infinity;
    //     Transform closest = null;

    //     // 🎯 Tính xác suất ưu tiên Player
    //     bool preferPlayer = Random.value < 0.5f; // 50% ưu tiên Player

    //     if (preferPlayer)
    //     {
    //         // Ưu tiên Player trước
    //         foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
    //         {
    //             if (obj == gameObject || obj.GetComponent<PlayerInfo>()?.hasDied == true) continue;
    //             float dist = Vector3.Distance(transform.position, obj.transform.position);
    //             if (dist < closestDistance)
    //             {
    //                 closestDistance = dist;
    //                 closest = obj.transform;
    //             }
    //         }

    //         // Nếu không có Player hoặc Player chết, fallback tìm bot
    //         if (closest == null)
    //         {
    //             foreach (var bot in GameObject.FindGameObjectsWithTag("Bot"))
    //             {
    //                 if (bot == gameObject || bot.GetComponent<BotStats>()?.currentHP <= 0) continue;
    //                 float dist = Vector3.Distance(transform.position, bot.transform.position);
    //                 if (dist < closestDistance)
    //                 {
    //                     closestDistance = dist;
    //                     closest = bot.transform;
    //                 }
    //             }
    //         }
    //     }
    //     else
    //     {
    //         // Ưu tiên bot trước
    //         foreach (var bot in GameObject.FindGameObjectsWithTag("Bot"))
    //         {
    //             if (bot == gameObject || bot.GetComponent<BotStats>()?.currentHP <= 0) continue;
    //             float dist = Vector3.Distance(transform.position, bot.transform.position);
    //             if (dist < closestDistance)
    //             {
    //                 closestDistance = dist;
    //                 closest = bot.transform;
    //             }
    //         }

    //         // Nếu không có bot nào sống, fallback tìm Player
    //         if (closest == null)
    //         {
    //             foreach (var obj in GameObject.FindGameObjectsWithTag("Player"))
    //             {
    //                 if (obj == gameObject || obj.GetComponent<PlayerInfo>()?.hasDied == true) continue;
    //                 float dist = Vector3.Distance(transform.position, obj.transform.position);
    //                 if (dist < closestDistance)
    //                 {
    //                     closestDistance = dist;
    //                     closest = obj.transform;
    //                 }
    //             }
    //         }
    //     }

    //     return closest;
    // }
Transform FindClosestTarget()
{
    float closestDistance = Mathf.Infinity;
    Transform closest = null;

    // Gộp tất cả "Bot" và "Player" vào cùng danh sách
    var allTargets = GameObject.FindGameObjectsWithTag("Bot")
                    .Concat(GameObject.FindGameObjectsWithTag("Player"));

    foreach (var obj in allTargets)
    {
        if (obj == gameObject) continue;

        if (obj.CompareTag("Bot") && obj.GetComponent<BotStats>()?.currentHP <= 0) continue;
        if (obj.CompareTag("Player") && obj.GetComponent<PlayerInfo>()?.hasDied == true) continue;

        float dist = Vector3.Distance(transform.position, obj.transform.position);
        if (dist < closestDistance)
        {
            closestDistance = dist;
            closest = obj.transform;
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
    private float attackHoldTimer = 0f;

    void HandleAttack()
    {
        if (target == null || IsTargetDead(target) || botStats.currentMana < 5f)
        {
            currentState = State.Wander;
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        if (!HasClearLineOfSightTo())
        {
            cooldownTarget = GetCooldownDestination();
            currentState = State.Cooldown;
            pathPoints.Clear();
            currentPathIndex = 0;
            return;
        }

        // ✅ Luôn LookAtTarget nếu chưa facing đủ
        if (!IsFacingTarget())
        {
            LookAtTargetSmooth(); // ép quay nhanh hơn
        }


        if (CanShootNow())
        {
            Shoot();
            lastAttackTime = Time.time;

            // 👉 Sau khi bắn xong, ép cooldownTarget là NGƯỢC hướng với enemy
            Vector3 offset = (transform.position - target.position).normalized;
            Vector3 escapeDir = Quaternion.Euler(0, Random.Range(-30f, 30f), 0) * offset;

            cooldownTarget = ClampToNavMesh(transform.position + escapeDir * cooldownMoveDistance);
            lastAttackCooldownSetTime = Time.time;

            // ✅ Ép quay NGAY lập tức về hướng trốn
            Quaternion escapeRotation = Quaternion.LookRotation(escapeDir);
            rb.rotation = escapeRotation;

            // 👉 Ép state chuyển luôn sang Cooldown (không chờ ai)
            currentState = State.Cooldown;
            pathPoints.Clear();
            currentPathIndex = 0;
            ResetPathIfTargetChanged(cooldownTarget);
            return; // Ngắt tại đây
        }


        // 💡 Không set cooldownTarget lại liên tục nếu chưa bắn
        if (Time.time - lastAttackCooldownSetTime < 0.1f)
        {
            ResetPathIfTargetChanged(cooldownTarget);
            MoveAlongPath(cooldownTarget); // ✅ Vẫn phải move dù đang attack chưa bắn được
        }

    }


    void HandleCooldown()
    {
        ResetPathIfTargetChanged(cooldownTarget);
        MoveAlongPath(cooldownTarget);

        // Nếu đến nơi rồi, thì chọn điểm cooldown mới
        if (Vector3.Distance(transform.position, cooldownTarget) < 4f)
        {
            // 👉 Nếu đã hết cooldown và đủ mana thì quay lại chase
            if (Time.time - lastAttackTime >= attackCooldown && botStats.currentMana >= 5f)
            {
                currentState = State.Chase;
                return;
            }

            // 🌀 Tiếp tục cooldown - chọn hướng mới
            cooldownTarget = GetCooldownDestination();
            pathPoints.Clear();
            currentPathIndex = 0;
            ResetPathIfTargetChanged(cooldownTarget);
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
            cooldownTarget = GetCooldownDestination();
            ResetPathIfTargetChanged(cooldownTarget);
            rb.velocity = Vector3.zero; // 🛑 Dừng đứng yên nếu không có đường
            anim.SetFloat("Speed", 0f);
            return;
        }

        Vector3 currentPos = ClampToNavMesh(transform.position);
        Vector3 targetPoint = pathPoints[currentPathIndex];

        // ❗ Bỏ qua nếu điểm tới quá gần chướng ngại
        if (!NavMesh.SamplePosition(targetPoint, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
        {
            // Skip điểm này vì nó nằm gần chướng ngại / ngoài NavMesh
            currentPathIndex++;
            if (currentPathIndex >= pathPoints.Count)
            {
                rb.velocity = Vector3.zero;
                pathPoints.Clear();
                currentPathIndex = 0;
                anim.SetFloat("Speed", 0f);
                return;
            }
            targetPoint = pathPoints[currentPathIndex];
        }

        Vector3 dir = targetPoint - currentPos;

        if (dir.magnitude < pointThreshold)
        {
            currentPathIndex++;
            if (currentPathIndex >= pathPoints.Count)
            {
                anim.SetFloat("Speed", 0f);
                rb.velocity = Vector3.zero;
                pathPoints.Clear();
                currentPathIndex = 0;
                return;
            }

            targetPoint = pathPoints[currentPathIndex];
            dir = targetPoint - currentPos;
        }

        dir.y = 0;
        dir.Normalize();

        Vector3 desiredVelocity = dir * moveSpeed;
        Vector3 velocity = Vector3.Lerp(rb.velocity, desiredVelocity, Time.deltaTime * 15f);

        if (Vector3.Distance(currentPos, targetPoint) < 2f)
            velocity *= 0.5f; // Giảm tốc khi gần đích

        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

        // ✅ Ép bot xoay theo hướng di chuyển NGAY CẢ khi đang cooldown
        Vector3 moveDir = rb.velocity;
        moveDir.y = 0;
        if (moveDir.sqrMagnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(moveDir);

            // 🔥 Tăng tốc độ quay nếu trong Cooldown
            float rotSpeed = currentState == State.Cooldown ? 30f : 15f;
            rb.rotation = Quaternion.Slerp(rb.rotation, lookRot, Time.deltaTime * rotSpeed);
        }


        anim.SetFloat("Speed", rb.velocity.magnitude);
    }


    void ResetPathIfTargetChanged(Vector3 destination)
    {
        pathTickTimer += Time.deltaTime;
        if (pathTickTimer < pathTickRate) return;
        pathTickTimer = 0f;

        if (pathPoints.Count == 0 || Vector3.Distance(destination, pathPoints[^1]) > 2f)
        {
            Vector3 currentPos = ClampToNavMesh(transform.position);
            Vector3 destPos = ClampToNavMesh(destination);
            if (!NavMesh.SamplePosition(destPos, out var sample, 5f, NavMesh.AllAreas))
            {
                destPos = ClampToNavMesh(transform.position + Random.insideUnitSphere * 15f);
            }

            if (!NavMesh.CalculatePath(currentPos, destPos, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete)
            {
                // ❌ Không có đường hợp lệ, chọn hướng random tránh đứng yên
                cooldownTarget = ClampToNavMesh(transform.position + Random.insideUnitSphere * 10f);
                pathPoints.Clear();
                currentPathIndex = 0;
                return;
            }

            pathPoints = new List<Vector3>(path.corners);
            currentPathIndex = 1;
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
        Vector3 away;

        if (target != null)
        {
            // Luôn né sang hướng chéo (tránh bị ngược hoàn toàn dễ stuck)
            Vector3 dir = (transform.position - target.position).normalized;
            float angle = Random.Range(-80f, 80f);
            away = Quaternion.Euler(0, angle, 0) * dir;
        }
        else
        {
            away = Random.insideUnitSphere;
        }

        away.y = 0;

        Vector3 dest = transform.position + away.normalized * cooldownMoveDistance;

        if (!NavMesh.SamplePosition(dest, out NavMeshHit hit, 8f, NavMesh.AllAreas))
        {
            // fallback hướng trung tâm bản đồ
            Vector3 fallback = (Vector3.zero - transform.position).normalized * 25f;
            dest = transform.position + fallback;
            if (!NavMesh.SamplePosition(dest, out hit, 12f, NavMesh.AllAreas))
            {
                dest = transform.position + Random.insideUnitSphere * 10f;
                dest.y = 0;
            }
            else
            {
                dest = hit.position;
            }
        }
        else
        {
            dest = hit.position;
        }

        return dest;
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
        if (stuckTimer > 0.8f)
        {
            cooldownTarget = ClampToNavMesh(transform.position + Random.insideUnitSphere * 20f);
            currentState = State.Cooldown;
            pathPoints.Clear();
            currentPathIndex = 0;
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
        // rb.velocity = Vector3.zero;
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
        float moveDelta = Vector3.Distance(transform.position, lastPos);
        lastPos = transform.position;

        if (moveDelta < 0.05f)
            noMoveTimer += Time.deltaTime;
        else
            noMoveTimer = 0f;

        if (noMoveTimer > 0.3f)
        {
            Vector3 dir = (target != null) ? (transform.position - target.position).normalized : Random.insideUnitSphere;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.01f) dir = Random.onUnitSphere;

            cooldownTarget = ClampToNavMesh(transform.position + dir.normalized * cooldownMoveDistance);
            currentState = State.Cooldown;

            pathPoints.Clear();
            currentPathIndex = 0;

            ResetPathIfTargetChanged(cooldownTarget);
            noMoveTimer = 0f;
        }
    }

    void CheckStuckAndResetIfNeeded()
    {
        // Kiểm tra xem phía trước bot có bị chặn bởi NavMesh không
        if (!NavMesh.SamplePosition(transform.position + transform.forward * 4f, out NavMeshHit hit, 4f, NavMesh.AllAreas))
        {
            // Nếu bị chặn, ép bot lùi lại hoặc đổi hướng
            Vector3 fallback = transform.position + Random.insideUnitSphere * 10f;
            cooldownTarget = ClampToNavMesh(fallback);

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
        if (stuckTimer > 0.8f && !isForcingMovement)
        {
            Vector3 dir = (target != null) ? (transform.position - target.position).normalized : Random.insideUnitSphere;
            if (dir.sqrMagnitude < 0.01f) dir = Random.insideUnitSphere;

            cooldownTarget = ClampToNavMesh(transform.position + dir.normalized * cooldownMoveDistance);
            currentState = State.Cooldown;
            isForcingMovement = true;
            pathPoints.Clear();
            currentPathIndex = 0;
            stuckTimer = 0f;
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
        return Vector3.Dot(transform.forward, dir) > 0.4f;
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

