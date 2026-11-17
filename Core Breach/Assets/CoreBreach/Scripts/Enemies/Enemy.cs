using UnityEngine;

/// <summary>
/// AI cho Sentry Drone (Kẻ địch cận chiến) theo Game Concept Core Breach:
/// FSM: Tuần tra (Patrol) -> Đuổi theo (Chase) -> Lao vào húc (Rush) -> Hồi chiêu (Recover)
/// Chỉ số theo Game Concept: HP 20, ATK 5, Speed 3.5, Rush Speed 6.0 - 7.0
/// </summary>

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Rush,
        Recover
    }

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float rushSpeed = 7f;

    [Header("Detection")]
    [SerializeField] private float detectRadius = 6f;
    [SerializeField] private float loseTargetRadius = 9f;
    [SerializeField] private float rushDistance = 2.5f;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float rushDuration = 0.45f;
    [SerializeField] private float recoverDuration = 1.2f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Patrol")]
    [SerializeField] private float patrolChangeTime = 2f;

    private Rigidbody2D rb;
    private Transform player;

    private EnemyState currentState = EnemyState.Patrol;

    private Vector2 patrolDirection;
    private Vector2 rushDirection;

    private float patrolTimer;
    private float stateTimer;
    private float nextAttackTime;

    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        patrolDirection = Random.insideUnitCircle.normalized;
        patrolTimer = patrolChangeTime;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );
        float detectionMultiplier = 1f;

        if (RunModifierSystem.Instance != null)
        {
            detectionMultiplier =
                RunModifierSystem.Instance.GetEnemyDetectionMultiplier();
        }

        float effectiveDetectRadius =
            detectRadius * detectionMultiplier;

        float effectiveLoseTargetRadius =
            loseTargetRadius * detectionMultiplier;

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (distance <= effectiveDetectRadius)
                    ChangeState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                if (distance > effectiveLoseTargetRadius)
                    ChangeState(EnemyState.Patrol);

                else if (distance <= rushDistance)
                    StartRush();
                break;

            case EnemyState.Rush:
                stateTimer -= Time.deltaTime;

                if (stateTimer <= 0f)
                    ChangeState(EnemyState.Recover);
                break;

            case EnemyState.Recover:
                stateTimer -= Time.deltaTime;

                if (stateTimer <= 0f)
                    ChangeState(
                        distance <= loseTargetRadius
                        ? EnemyState.Chase
                        : EnemyState.Patrol
                    );
                break;
        }
    }

    private void FixedUpdate()
    {
        //switch (currentState)
        //{
        //    case EnemyState.Patrol:
        //        Patrol();
        //        break;

        //    case EnemyState.Chase:
        //        MoveTowardsPlayer(chaseSpeed);
        //        break;

        //    case EnemyState.Rush:
        //        rb.linearVelocity = rushDirection * rushSpeed;
        //        break;

        //    case EnemyState.Recover:
        //        rb.linearVelocity = Vector2.zero;
        //        break;
        //}
        float speedMultiplier = 1f;

        if (RunModifierSystem.Instance != null)
        {
            speedMultiplier =
                RunModifierSystem.Instance.GetEnemySpeedMultiplier();
        }

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol(speedMultiplier);
                break;

            case EnemyState.Chase:
                MoveTowardsPlayer(chaseSpeed * speedMultiplier);
                break;

            case EnemyState.Rush:
                rb.linearVelocity =
                    rushDirection * rushSpeed * speedMultiplier;
                break;

            case EnemyState.Recover:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    private void FindPlayer()
    {
        if (PlayerMovement.PlayerTransform != null)
        {
            player = PlayerMovement.PlayerTransform;
        }
        else
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null)
                player = obj.transform;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        currentState = newState;

        if (newState == EnemyState.Recover)
        {
            stateTimer = recoverDuration;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Patrol(float speedMultiplier)
    {
        patrolTimer -= Time.fixedDeltaTime;

        if (patrolTimer <= 0f)
        {
            patrolDirection = Random.insideUnitCircle.normalized;
            patrolTimer = patrolChangeTime;
        }

        //rb.linearVelocity = patrolDirection * patrolSpeed;
        rb.linearVelocity = patrolDirection * patrolSpeed * speedMultiplier;
        RotateTowards(patrolDirection);
    }

    private void MoveTowardsPlayer(float speed)
    {
        if (player == null) return;

        Vector2 direction =
            ((Vector2)player.position - rb.position).normalized;

        rb.linearVelocity = direction * speed;

        RotateTowards(direction);
    }

    private void StartRush()
    {
        if (player == null) return;

        currentState = EnemyState.Rush;

        rushDirection =
            ((Vector2)player.position - rb.position).normalized;

        stateTimer = rushDuration;
        nextAttackTime = 0f;

        RotateTowards(rushDirection);
    }

    private void RotateTowards(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rb.rotation = angle;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerContact(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandlePlayerContact(collision.gameObject);
    }

    private void HandlePlayerContact(GameObject target)
    {
        if (!target.CompareTag("Player") && target.GetComponent<PlayerHealth>() == null) return;
        if (Time.time < nextAttackTime) return;

        PlayerHealth health = target.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(attackDamage);
            nextAttackTime = Time.time + attackCooldown;
            Debug.Log($"[Enemy] Sentry Drone húc trúng Unit-7! Gây {attackDamage} sát thương.");
        }

        // Bật lùi lại và chuyển sang Recover
        ChangeState(EnemyState.Recover);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rushDistance);
    }
}
