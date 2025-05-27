

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

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

    [Header("Runtime")]
    public Transform target;

    private Rigidbody rb;
    private Animator anim;
    private BotStats botStats;
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

    enum State { Wander, Chase, Attack, Cooldown, Die }
    private State currentState;

    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileLifetime = 2f;
    private float aiTickTimer = 0f;
    private float aiTickRate = 0.2f;

    private float pathTickTimer = 0f;
    private float pathTickRate = 0.5f;

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
        }
    }

    void UpdateStateLogic()
    {
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
        Vector3 away = (transform.position - target.position).normalized;
        Vector3 candidate = transform.position + away * fleeDistance * 1.5f + Random.insideUnitSphere * 4f;
        return ClampToNavMesh(candidate);
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
}
