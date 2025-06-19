

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
    private float aiTickRate = 0.2f;

    private float pathTickTimer = 0f;
    private float pathTickRate = 0.5f;
    [SerializeField] float healCooldown = 5f;
    private float lastHealTime = -999f;
    private float animSpeed = 0f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        botStats = GetComponent<BotStats>();
        path = new NavMeshPath();
        lastPos = transform.position;

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

        switch (currentState)
        {
            case State.Wander: HandleWander(); break;
            case State.Chase: HandleChase(); break;
            case State.Attack: HandleAttack(); break;
            case State.Cooldown: HandleCooldown(); break;
            case State.SeekHeal: HandleSeekHeal(); break;

        }
    }

    void UpdateStateLogic()
    {
        if (ZoneManager.Instance != null && !ZoneManager.Instance.IsInsideZone(transform.position))
        {
            cooldownTarget = ZoneManager.Instance.GetSafePoint(transform.position);
            currentState = State.Cooldown;
            return;
        }
        if (Time.time - lastHealTime < healCooldown)
            return;
        if (botStats.currentHP < botStats.maxHP * 0.3f)
        {
            float playerDist = target != null ? Vector3.Distance(transform.position, target.position) : Mathf.Infinity;

            // Nếu có kẻ địch gần và đủ mana thì bắn trước đã
            if (playerDist <= attackRange && botStats.currentMana >= 5f)
            {
                currentState = State.Attack;
                return;
            }

            if (Random.value < 0.6f)
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
        if (target != null && Vector3.Distance(transform.position, target.position) < fleeDistance)
        {
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
            if (newDist + 4f < currDist)
                target = newTarget;
        }

        float dist = Vector3.Distance(transform.position, target.position);
        bool isTooClose = dist < minAttackDistance;

        if (isTooClose)
        {
            cooldownTarget = GetFleePosition();
            currentState = State.Cooldown;
            return;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            if (currentState != State.Cooldown)
            {
                cooldownTarget = GetCooldownDestination();
                pathPoints.Clear();
                currentPathIndex = 0;
                currentState = State.Cooldown;
            }
            return;
        }

        if (dist <= attackRange)
            currentState = State.Attack;
        else if (dist <= detectRange)
            currentState = State.Chase;
        else
            currentState = State.Wander;
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
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * 80f;
            randomDir.y = 0;
            Vector3 candidate = transform.position + randomDir;
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 40f, NavMesh.AllAreas))
            {
                wanderTarget = hit.position;
                return;
            }
        }
        wanderTarget = transform.position + Random.insideUnitSphere * 12f;
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
        if (target == null || IsTargetDead(target))
        {
            target = null;
            currentState = State.Wander;
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist < minAttackDistance)
        {
            cooldownTarget = GetFleePosition();
            currentState = State.Cooldown;
            return;
        }

        LookAtTargetSmooth();
        if (Time.time - lastAttackTime < attackCooldown)
            return;
        if (IsFacingTarget() && botStats.currentMana >= 5)
        {
            Shoot();
            lastAttackTime = Time.time;
            cooldownTarget = GetCooldownDestination();
            pathPoints.Clear();
            currentPathIndex = 0;
            currentState = State.Cooldown;
        }
    }

    void HandleCooldown()
    {
        ResetPathIfTargetChanged(cooldownTarget);
        MoveAlongPath(cooldownTarget);
        if (Vector3.Distance(transform.position, cooldownTarget) < 4f && Time.time - lastAttackTime >= attackCooldown)
            currentState = State.Chase;
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

    void LookAtTargetSmooth()
    {
        if (target == null) return;
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            Quaternion smoothRot = Quaternion.Slerp(rb.rotation, targetRot, Time.deltaTime * 30f);
            rb.MoveRotation(smoothRot);

        }
    }

    void MoveAlongPath(Vector3 destination)
    {
        if (currentPathIndex >= pathPoints.Count)
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
                return;
            }
            targetPoint = pathPoints[currentPathIndex];
            dir = targetPoint - currentPos;
        }

        dir.Normalize();
        rb.MovePosition(currentPos + dir * moveSpeed * Time.deltaTime);
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, lookRot, Time.deltaTime * 20f);
            rb.MoveRotation(newRotation);

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
            Vector3 currentPos = ClampToNavMesh(transform.position);
            Vector3 destPos = ClampToNavMesh(destination);
            if (NavMesh.CalculatePath(currentPos, destPos, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                pathPoints = new List<Vector3>(path.corners);
                currentPathIndex = 1;
            }
        }
    }


    Vector3 ClampToNavMesh(Vector3 pos)
    {
        return NavMesh.SamplePosition(pos, out NavMeshHit hit, 8f, NavMesh.AllAreas) ? hit.position : transform.position;
    }

    Vector3 GetCooldownDestination()
    {
        if (target == null) return transform.position + Random.insideUnitSphere * 12f;
        Vector3 away = (transform.position - target.position).normalized;
        Vector3 dest = transform.position + away * cooldownMoveDistance;
        return ClampToNavMesh(dest);
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
        if (anim.GetFloat("Speed") <= 0.01f)
            noMoveTimer += Time.deltaTime;
        else
            noMoveTimer = 0f;

        if (noMoveTimer > 0.2f)
        {
            Vector3 emergencyTarget = transform.position + Random.insideUnitSphere * 16f;
            if (NavMesh.SamplePosition(emergencyTarget, out NavMeshHit hit, 20f, NavMesh.AllAreas))
            {
                pathPoints = new List<Vector3> { transform.position, hit.position };
                currentPathIndex = 1;
            }
            noMoveTimer = 0f;
        }
    }

    void CheckStuckAndResetIfNeeded()
    {
        if (!NavMesh.SamplePosition(transform.position + transform.forward * 4f, out NavMeshHit hit, 4f, NavMesh.AllAreas))
        {
            // Gần mép map, thì chuyển sang chạy ngược
            cooldownTarget = GetFleePosition() + Random.insideUnitSphere * 4f;
            currentState = State.Cooldown;
            return;
        }

        if (Vector3.Distance(transform.position, lastPos) < 0.1f)
            stuckTimer += Time.deltaTime;
        else
        {
            stuckTimer = 0f;
            lastPos = transform.position;
        }

        if (stuckTimer > 2f)
        {
            cooldownTarget = GetFleePosition();
            currentState = State.Cooldown;
            pathPoints.Clear();
            currentPathIndex = 0;
            stuckTimer = 0f;
        }
    }

    public void SetWorldUI(GameObject ui) => worldUI = ui;

    bool IsFacingTarget()
    {
        if (!target) return false;
        Vector3 dir = (target.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, dir) > 0.95f;
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
