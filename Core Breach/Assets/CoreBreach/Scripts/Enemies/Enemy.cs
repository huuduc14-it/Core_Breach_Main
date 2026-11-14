//using UnityEngine;

///// <summary>
///// AI cho Sentry Drone (Kẻ địch cận chiến) theo Game Concept Core Breach:
///// FSM: Tuần tra (Patrol) -> Phát hiện (Chase) -> Lao vào húc (Rush)
///// Tốc độ: 3.5f, Tốc độ lao: 6.0f, Sát thương: 5
///// </summary>
//[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
//public class Enemy : MonoBehaviour
//{
//    public enum EnemyState { Patrol, Chase, Rush }

//    [Header("Stats theo Game Concept")]
//    [SerializeField] private float patrolSpeed = 3.5f;
//    [SerializeField] private float rushSpeed = 6.0f;
//    [SerializeField] private float attackDamage = 5f;

//    [Header("Detection Settings")]
//    [SerializeField] private float detectRadius = 6.0f;
//    [SerializeField] private float rushDistance = 3.0f;

//    private Rigidbody2D rb;
//    private Transform playerTransform;
//    private EnemyState currentState = EnemyState.Patrol;

//    private Vector2 patrolDirection = Vector2.right;
//    private float patrolTimer = 0f;

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        rb.gravityScale = 0f;
//        rb.freezeRotation = true;
//    }

//    private void Start()
//    {
//        FindPlayer();
//    }

//    private void Update()
//    {
//        if (playerTransform == null)
//        {
//            FindPlayer();
//            return;
//        }

//        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

//        // Chuyển đổi trạng thái FSM
//        switch (currentState)
//        {
//            case EnemyState.Patrol:
//                if (distanceToPlayer <= detectRadius)
//                {
//                    currentState = EnemyState.Chase;
//                }
//                break;

//            case EnemyState.Chase:
//                if (distanceToPlayer <= rushDistance)
//                {
//                    currentState = EnemyState.Rush;
//                }
//                else if (distanceToPlayer > detectRadius * 1.3f)
//                {
//                    currentState = EnemyState.Patrol;
//                }
//                break;

//            case EnemyState.Rush:
//                if (distanceToPlayer > rushDistance * 1.5f)
//                {
//                    currentState = EnemyState.Chase;
//                }
//                break;
//        }
//    }

//    private void FixedUpdate()
//    {
//        switch (currentState)
//        {
//            case EnemyState.Patrol:
//                PatrolLogic();
//                break;

//            case EnemyState.Chase:
//                ChaseLogic(patrolSpeed);
//                break;

//            case EnemyState.Rush:
//                ChaseLogic(rushSpeed);
//                break;
//        }
//    }

//    private void FindPlayer()
//    {
//        GameObject playerObj = GameObject.FindWithTag("Player");
//        if (playerObj != null)
//        {
//            playerTransform = playerObj.transform;
//        }
//    }

//    private void PatrolLogic()
//    {
//        patrolTimer -= Time.fixedDeltaTime;
//        if (patrolTimer <= 0f)
//        {
//            // Đổi hướng ngẫu nhiên sau 1.5 - 3 giây
//            patrolDirection = Random.insideUnitCircle.normalized;
//            patrolTimer = Random.Range(1.5f, 3.0f);
//        }

//        rb.linearVelocity = patrolDirection * (patrolSpeed * 0.5f);
//    }

//    private void ChaseLogic(float speed)
//    {
//        if (playerTransform == null) return;

//        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
//        rb.linearVelocity = direction * speed;

//        // Xoay mặt nhìn về phía Player
//        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//        transform.rotation = Quaternion.Euler(0f, 0f, angle);
//    }

//    private void OnCollisionEnter2D(Collision2D collision)
//    {
//        if (collision.gameObject.CompareTag("Player"))
//        {
//            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
//            if (playerHealth != null)
//            {
//                playerHealth.TakeDamage(attackDamage);
//            }
//        }
//    }
//}


using UnityEngine;

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

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (distance <= detectRadius)
                    ChangeState(EnemyState.Chase);
                break;

            case EnemyState.Chase:
                if (distance > loseTargetRadius)
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
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;

            case EnemyState.Chase:
                MoveTowardsPlayer(chaseSpeed);
                break;

            case EnemyState.Rush:
                rb.linearVelocity = rushDirection * rushSpeed;
                break;

            case EnemyState.Recover:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    private void FindPlayer()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");

        if (obj != null)
            player = obj.transform;
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

    private void Patrol()
    {
        patrolTimer -= Time.fixedDeltaTime;

        if (patrolTimer <= 0f)
        {
            patrolDirection = Random.insideUnitCircle.normalized;
            patrolTimer = patrolChangeTime;
        }

        rb.linearVelocity = patrolDirection * patrolSpeed;
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
        if (currentState != EnemyState.Rush) return;

        if (!collision.gameObject.CompareTag("Player")) return;

        if (Time.time < nextAttackTime) return;

        PlayerHealth health =
            collision.gameObject.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(attackDamage);
            nextAttackTime = Time.time + attackCooldown;
        }

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
