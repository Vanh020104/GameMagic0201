// using UnityEngine;
// using UnityEngine.AI;
// using System.Collections.Generic;
// using System.Collections;

// public class BotAI : MonoBehaviour
// {
//     [Header("Settings")]
//     public float detectRange = 60f;
//     public float attackRange = 40f;
//     public float cooldownMoveDistance = 45f;
//     public float moveSpeed = 15f;
//     public float attackCooldown = 2f;

//     [Header("Runtime")]
//     public Transform target;

//     private Rigidbody rb;
//     private NavMeshPath path;
//     private List<Vector3> pathPoints = new List<Vector3>();
//     private int currentPathIndex = 0;
//     private float lastAttackTime = Mathf.NegativeInfinity;
//     private Animator anim;
//     private Vector3 wanderTarget;
//     private Vector3 cooldownTarget;
//     private State currentState;

//     private float pointThreshold = 1.2f;

//     private float stuckTimer = 0f;
//     private Vector3 lastPos;
//     private float noMoveTimer = 0f;

//     [Header("Attack Settings")]
//     public GameObject projectilePrefab;
//     public Transform projectileSpawnPoint;
//     public float projectileLifetime = 2f;
//     private BotStats botStats;
//     private GameObject worldUI;
//     enum State { Wander, Chase, Attack, Cooldown, Die }

//     void Start()
//     {
//         rb = GetComponent<Rigidbody>();
//         anim = GetComponent<Animator>();
//         path = new NavMeshPath();
//         botStats = GetComponent<BotStats>();
//         InvokeRepeating(nameof(ChooseWanderTarget), 0f, 5f);
//         InvokeRepeating(nameof(RegenMana), 1f, 1f);
//         lastPos = transform.position;
//     }

//     void Update()
//     {
//         if (botStats.currentHP <= 0 && currentState != State.Die)
//         {
//             HandleDeath();
//             return;
//         }

//         if (currentState == State.Die) return;
//         ForceMoveIfIdle();
//         CheckStuckAndResetIfNeeded();
//         UpdateStateLogic();

//         switch (currentState)
//         {
//             case State.Wander: HandleWander(); break;
//             case State.Chase: HandleChase(); break;
//             case State.Attack: HandleAttack(); break;
//             case State.Cooldown: HandleCooldown(); break;
//         }
//     }

//     void ForceMoveIfIdle()
//     {
//         if (anim.GetFloat("Speed") <= 0.01f)
//             noMoveTimer += Time.deltaTime;
//         else
//             noMoveTimer = 0f;

//         if (noMoveTimer > 0.2f)
//         {
//             Debug.LogWarning("⛔ Bot bị đứng yên > 0.2s, ép di chuyển");
//             Vector3 emergencyTarget = transform.position + Random.insideUnitSphere * 16f;
//             emergencyTarget.y = transform.position.y;

//             if (NavMesh.SamplePosition(emergencyTarget, out NavMeshHit hit, 20f, NavMesh.AllAreas))
//             {
//                 pathPoints = new List<Vector3> { transform.position, hit.position };
//                 currentPathIndex = 1;
//             }
//             noMoveTimer = 0f;
//         }
//     }

//     void CheckStuckAndResetIfNeeded()
//     {
//         if (Vector3.Distance(transform.position, lastPos) < 0.1f)
//         {
//             stuckTimer += Time.deltaTime;
//         }
//         else
//         {
//             stuckTimer = 0f;
//             lastPos = transform.position;
//         }

//         if (stuckTimer > 2f)
//         {
//             Debug.LogWarning("⚠️ Bot bị đứng yên quá lâu, reset path");

//             if (currentState == State.Wander)
//                 ChooseWanderTarget();
//             else if (currentState == State.Cooldown)
//                 cooldownTarget = GetCooldownDestination();

//             pathPoints.Clear();
//             currentPathIndex = 0;
//             stuckTimer = 0f;
//         }
//     }

//     void UpdateStateLogic()
//     {
//         GameObject playerObj = GameObject.FindWithTag("Player");

//         if (playerObj == null)
//         {
//             target = null;
//             currentState = State.Wander;
//             return;
//         }
//         var playerInfo = playerObj.GetComponent<PlayerInfo>();
//         if (playerInfo != null && playerInfo.hasDied)
//         {
//             target = null;
//             currentState = State.Wander;
//             return;
//         }
//         target = playerObj.transform;
//         float dist = Vector3.Distance(transform.position, target.position);

//         if (Time.time - lastAttackTime < attackCooldown)
//         {
//             if (currentState != State.Cooldown)
//             {
//                 cooldownTarget = GetCooldownDestination();
//                 pathPoints.Clear();
//                 currentPathIndex = 0;
//                 currentState = State.Cooldown;
//             }
//             return;
//         }

//         if (dist <= attackRange)
//             currentState = State.Attack;
//         else if (dist <= detectRange)
//             currentState = State.Chase;
//         else
//             currentState = State.Wander;
//     }

//     void HandleWander()
//     {
//         ResetPathIfTargetChanged(wanderTarget);
//         MoveAlongPath(wanderTarget);

//         if (Vector3.Distance(transform.position, wanderTarget) < 4f)
//         {
//             ChooseWanderTarget();
//             pathPoints.Clear();
//             currentPathIndex = 0;
//         }
//     }

//     void HandleChase()
//     {
//         if (target == null) return;

//         Vector3 chasePos = ClampToNavMesh(target.position);
//         ResetPathIfTargetChanged(chasePos);
//         MoveAlongPath(chasePos);
//     }

//     void HandleAttack()
//     {
//         if (target == null) return;
//         var info = target.GetComponent<PlayerInfo>();
//         if (info != null && info.hasDied)
//         {
//             target = null;
//             currentState = State.Wander;
//             return;
//         }
//         FaceTarget();
//         Shoot();

//         lastAttackTime = Time.time;
//         cooldownTarget = GetCooldownDestination();
//         pathPoints.Clear();
//         currentPathIndex = 0;
//         currentState = State.Cooldown;
//     }

//     void HandleCooldown()
//     {
//         ResetPathIfTargetChanged(cooldownTarget);
//         MoveAlongPath(cooldownTarget);

//         if (Vector3.Distance(transform.position, cooldownTarget) < 4f)
//         {
//             if (Time.time - lastAttackTime >= attackCooldown)
//                 currentState = State.Chase;
//             else
//             {
//                 cooldownTarget = GetCooldownDestination();
//                 pathPoints.Clear();
//                 currentPathIndex = 0;
//             }
//         }
//     }

//     void ChooseWanderTarget()
//     {
//         for (int i = 0; i < 5; i++)
//         {
//             Vector3 randomDir = Random.insideUnitSphere * 80f;
//             randomDir.y = 0;
//             Vector3 candidate = transform.position + randomDir;

//             if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 40f, NavMesh.AllAreas))
//             {
//                 wanderTarget = hit.position;
//                 return;
//             }
//         }

//         wanderTarget = transform.position + Random.insideUnitSphere * 12f;
//         wanderTarget.y = transform.position.y;
//     }

//     Vector3 GetCooldownDestination()
//     {
//         if (target == null)
//             return transform.position + Random.insideUnitSphere * 12f;

//         Vector3 away = (transform.position - target.position).normalized;
//         Vector3 dest = transform.position + away * cooldownMoveDistance;

//         if (NavMesh.SamplePosition(dest, out NavMeshHit hit, 8f, NavMesh.AllAreas))
//             return hit.position;

//         return transform.position + Random.insideUnitSphere * 12f;
//     }

//     void ResetPathIfTargetChanged(Vector3 destination)
//     {
//         Vector3 currentDestination = (pathPoints.Count > 0) ? pathPoints[^1] : Vector3.zero;
//         if (pathPoints.Count == 0 || Vector3.Distance(destination, currentDestination) > 2f)
//         {
//             NavMeshPath newPath = new NavMeshPath();
//             Vector3 currentPos = ClampToNavMesh(transform.position);
//             Vector3 destPos = ClampToNavMesh(destination);

//             if (NavMesh.CalculatePath(currentPos, destPos, NavMesh.AllAreas, newPath)
//                 && newPath.status == NavMeshPathStatus.PathComplete)
//             {
//                 pathPoints = new List<Vector3>(newPath.corners);
//                 currentPathIndex = 1;
//             }
//         }
//     }

//     void MoveAlongPath(Vector3 destination)
//     {
//         if (currentPathIndex >= pathPoints.Count)
//         {
//             anim.SetFloat("Speed", 0f);
//             return;
//         }

//         Vector3 currentPos = ClampToNavMesh(transform.position);
//         Vector3 targetPoint = pathPoints[currentPathIndex];
//         Vector3 dir = targetPoint - currentPos;

//         if (dir.magnitude < pointThreshold)
//         {
//             currentPathIndex++;
//             if (currentPathIndex >= pathPoints.Count)
//             {
//                 anim.SetFloat("Speed", 0f);
//                 return;
//             }
//             targetPoint = pathPoints[currentPathIndex];
//             dir = targetPoint - currentPos;
//         }

//         dir.Normalize();
//         rb.MovePosition(transform.position + dir * moveSpeed * Time.deltaTime);
//         transform.forward = dir;
//         anim.SetFloat("Speed", 1f);
//     }

//     void FaceTarget()
//     {
//         if (target == null) return;
//         Vector3 lookDir = (target.position - transform.position).normalized;
//         lookDir.y = 0;
//         if (lookDir != Vector3.zero)
//             transform.forward = lookDir;
//     }

//     void Shoot()
//     {
//         if (botStats.currentMana < 5) return;

//         Debug.Log($"{botStats.botName} shoot");
//         anim.SetTrigger("AttackNormal");
//         botStats.currentMana -= 5;
//         SpawnProjectile();
//     }

//     void SpawnProjectile()
//     {
//         if (projectilePrefab == null || projectileSpawnPoint == null) return;

//         GameObject newProjectile = Instantiate(projectilePrefab, projectileSpawnPoint);
//         newProjectile.GetComponent<ProjectileMoveScript>().ownerBot = botStats;

//         newProjectile.transform.forward = transform.forward;
//         newProjectile.transform.SetParent(null);
//         newProjectile.transform.position = projectileSpawnPoint.position;

//         Destroy(newProjectile, projectileLifetime);
//     }


//     Vector3 ClampToNavMesh(Vector3 position)
//     {
//         if (NavMesh.SamplePosition(position, out NavMeshHit hit, 8f, NavMesh.AllAreas))
//             return hit.position;
//         return transform.position;
//     }

//     void RegenMana()
//     {
//         if (botStats == null) return;

//         botStats.currentMana += 3f;
//         if (botStats.currentMana > botStats.maxMana)
//             botStats.currentMana = botStats.maxMana;
//     }


//     void HandleDeath()
//     {
//         currentState = State.Die;
//         anim.SetTrigger("Die");
//         rb.velocity = Vector3.zero;
//         pathPoints.Clear();
//         currentPathIndex = 0;
//         Collider col = GetComponent<Collider>();
//         if (col != null) col.enabled = false;
//         if (worldUI != null)
//         Destroy(worldUI,5f);
//         Destroy(gameObject, 5f);
//     }

//     public void SetWorldUI(GameObject ui)
//     {
//         worldUI = ui;
//     }


//     void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.green;
//         Gizmos.DrawWireSphere(transform.position, detectRange);

//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, attackRange);

//         Gizmos.color = Color.cyan;
//         Gizmos.DrawWireSphere(transform.position, cooldownMoveDistance);

//         if (Application.isPlaying)
//         {
//             Gizmos.color = Color.yellow;
//             Gizmos.DrawSphere(wanderTarget, 0.5f);

//             Gizmos.color = Color.blue;
//             Gizmos.DrawSphere(cooldownTarget, 0.5f);

//             Gizmos.color = Color.white;
//             for (int i = 1; i < pathPoints.Count; i++)
//             {
//                 Gizmos.DrawLine(pathPoints[i - 1], pathPoints[i]);
//             }
//         }
//     }
    
    

// }


using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public class BotAI : MonoBehaviour
{
    [Header("Settings")]
    public float detectRange = 60f;
    public float attackRange = 40f;
    public float minAttackDistance = 5f;
    public float fleeDistance = 10f;
    public float cooldownMoveDistance = 45f;
    public float moveSpeed = 15f;
    public float attackCooldown = 2f;

    [Header("Runtime")]
    public Transform target;

    private Rigidbody rb;
    private NavMeshPath path;
    private List<Vector3> pathPoints = new List<Vector3>();
    private int currentPathIndex = 0;
    private float lastAttackTime = Mathf.NegativeInfinity;
    private Animator anim;
    private Vector3 wanderTarget;
    private Vector3 cooldownTarget;
    private State currentState;

    private float pointThreshold = 1.2f;
    private float stuckTimer = 0f;
    private Vector3 lastPos;
    private float noMoveTimer = 0f;
    private float fleeCooldownTimer = 0f;

    [Header("Attack Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileLifetime = 2f;
    private BotStats botStats;
    private GameObject worldUI;

    enum State { Wander, Chase, Attack, Cooldown, Die }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        path = new NavMeshPath();
        botStats = GetComponent<BotStats>();
        InvokeRepeating(nameof(ChooseWanderTarget), 0f, 5f);
        InvokeRepeating(nameof(RegenMana), 1f, 1f);
        lastPos = transform.position;
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
    wanderTarget.y = transform.position.y;
}

    void Update()
    {
        if (botStats.currentHP <= 0 && currentState != State.Die)
        {
            HandleDeath();
            return;
        }

        if (currentState == State.Die) return;

        ForceMoveIfIdle();
        CheckStuckAndResetIfNeeded();
        UpdateStateLogic();

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
        Transform closestTarget = FindClosestTarget();
        if (closestTarget == null)
        {
            target = null;
            currentState = State.Wander;
            return;
        }

        target = closestTarget;
        float dist = Vector3.Distance(transform.position, target.position);

        if (dist < fleeDistance)
        {
            fleeCooldownTimer += Time.deltaTime;
            if (fleeCooldownTimer > 0.2f)
            {
                cooldownTarget = GetFleePosition();
                currentState = State.Cooldown;
                fleeCooldownTimer = 0f;
                return;
            }
        }
        else fleeCooldownTimer = 0f;

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
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Player"); // có thể là player hoặc bot
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (var obj in allTargets)
        {
            if (obj == gameObject) continue; // bỏ qua chính nó
            if (obj.GetComponent<PlayerInfo>()?.hasDied == true) continue;

            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = obj.transform;
            }
        }

        GameObject[] allBots = GameObject.FindGameObjectsWithTag("Bot"); // tag các bot
        foreach (var bot in allBots)
        {
            if (bot == gameObject) continue;
            if (bot.GetComponent<BotStats>()?.currentHP <= 0) continue;

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

    void HandleChase()
    {
        if (target == null) return;
        Vector3 chasePos = ClampToNavMesh(target.position);
        ResetPathIfTargetChanged(chasePos);
        MoveAlongPath(chasePos);
    }

    void HandleAttack()
    {
        if (target == null || target.GetComponent<PlayerInfo>()?.hasDied == true)
        {
            target = null;
            currentState = State.Wander;
            return;
        }

        if (Vector3.Distance(transform.position, target.position) < fleeDistance)
        {
            cooldownTarget = GetFleePosition();
            currentState = State.Cooldown;
            return;
        }

        FaceTargetSmooth();
        Shoot();
        lastAttackTime = Time.time;
        cooldownTarget = GetCooldownDestination();
        pathPoints.Clear();
        currentPathIndex = 0;
        currentState = State.Cooldown;
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
        if (botStats.currentMana < 5) return;
        botStats.currentMana -= 5;
        SpawnProjectile();
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

        if (dir == Vector3.zero || dir.magnitude < 0.1f)
            dir = (Random.insideUnitSphere * 5f).normalized;

        dir.Normalize();
        rb.MovePosition(transform.position + dir * moveSpeed * Time.deltaTime);
        transform.forward = dir;
        anim.SetFloat("Speed", 1f);
    }

    void FaceTargetSmooth()
    {
        if (target == null) return;
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void ResetPathIfTargetChanged(Vector3 destination)
    {
        Vector3 currentDestination = (pathPoints.Count > 0) ? pathPoints[^1] : Vector3.zero;
        if (pathPoints.Count == 0 || Vector3.Distance(destination, currentDestination) > 2f)
        {
            NavMeshPath newPath = new NavMeshPath();
            Vector3 currentPos = ClampToNavMesh(transform.position);
            Vector3 destPos = ClampToNavMesh(destination);

            if (NavMesh.CalculatePath(currentPos, destPos, NavMesh.AllAreas, newPath) && newPath.status == NavMeshPathStatus.PathComplete)
            {
                pathPoints = new List<Vector3>(newPath.corners);
                currentPathIndex = 1;
            }
        }
    }

    Vector3 ClampToNavMesh(Vector3 position)
    {
        return NavMesh.SamplePosition(position, out NavMeshHit hit, 8f, NavMesh.AllAreas) ? hit.position : transform.position;
    }

    Vector3 GetCooldownDestination()
    {
        if (target == null) return transform.position + Random.insideUnitSphere * 12f;
        Vector3 away = (transform.position - target.position).normalized;
        Vector3 dest = transform.position + away * cooldownMoveDistance;
        return NavMesh.SamplePosition(dest, out NavMeshHit hit, 8f, NavMesh.AllAreas) ? hit.position : transform.position + Random.insideUnitSphere * 12f;
    }

    Vector3 GetFleePosition()
    {
        Vector3 away = (transform.position - target.position).normalized;
        Vector3 candidate = transform.position + away * fleeDistance * 1.5f + Random.insideUnitSphere * 4f;
        candidate.y = transform.position.y;
        return NavMesh.SamplePosition(candidate, out NavMeshHit hit, 15f, NavMesh.AllAreas) ? hit.position : transform.position + Random.insideUnitSphere * 6f;
    }

    void RegenMana()
    {
        if (botStats == null) return;
        botStats.currentMana += 3f;
        if (botStats.currentMana > botStats.maxMana) botStats.currentMana = botStats.maxMana;
    }

    void HandleDeath()
    {
        currentState = State.Die;
        anim.SetTrigger("Die");
        rb.velocity = Vector3.zero;
        pathPoints.Clear();
        currentPathIndex = 0;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        if (worldUI != null) Destroy(worldUI, 5f);
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
            emergencyTarget.y = transform.position.y;
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

    public void SetWorldUI(GameObject ui)
    {
        worldUI = ui;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cooldownMoveDistance);
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(wanderTarget, 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(cooldownTarget, 0.5f);
            Gizmos.color = Color.white;
            for (int i = 1; i < pathPoints.Count; i++)
                Gizmos.DrawLine(pathPoints[i - 1], pathPoints[i]);
        }
    }
}
