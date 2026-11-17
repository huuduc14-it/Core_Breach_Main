using System.Collections;
using UnityEngine;

/// <summary>
/// AI cho Kamikaze Bot (Robot Cảm Tử Nổ Chùm) chuẩn phong cách Roguelite / Soul Knight:
/// - Tốc độ cao (4.8f), lao nhanh tới áp sát Player
/// - Khi vào phạm vi 1.8m: Dừng lại, nhấp nháy đỏ rực cực nhanh trong 0.65s (Warning Countdown)
/// - Kích nổ diện rộng (AOE 2.5m, DMG 25): Gây sát thương và thổi văng cả Player lẫn quái địch xung quanh!
/// - Cơ chế Chain-Reaction: Nếu bị Player bắn chết trước khi phát nổ, nó vẫn phát nổ tại chỗ tiêu diệt các quái lân cận!
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class KamikazeBot : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 4.8f;
    [SerializeField] private float detectRadius = 12f;
    [SerializeField] private float triggerDistance = 1.8f;
    [SerializeField] private float fuseDuration = 0.65f;
    [SerializeField] private float explosionRadius = 2.6f;
    [SerializeField] private float explosionDamage = 25f;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private SpriteRenderer sr;
    private EnemyHealth health;

    private bool isPrimed = false;
    private bool hasExploded = false;
    private Color originalColor = Color.white;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath += OnKilledByPlayer;
        }
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (PlayerMovement.PlayerTransform != null)
        {
            playerTransform = PlayerMovement.PlayerTransform;
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    private void Update()
    {
        if (hasExploded) return;

        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        if (!isPrimed)
        {
            if (dist <= triggerDistance)
            {
                StartCoroutine(DetonationCountdown());
            }
        }
    }

    private void FixedUpdate()
    {
        if (hasExploded || isPrimed)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (playerTransform == null) return;

        //float dist = Vector2.Distance(transform.position, playerTransform.position);
        //if (dist <= detectRadius)
        float dist = Vector2.Distance(transform.position, playerTransform.position);

        float detectionMultiplier = 1f;

        if (RunModifierSystem.Instance != null)
        {
            detectionMultiplier =
                RunModifierSystem.Instance.GetEnemyDetectionMultiplier();
        }

        if (dist <= detectRadius * detectionMultiplier)
        {
            Vector2 dir = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
            //rb.linearVelocity = dir * moveSpeed;
            float speedMultiplier = 1f;

            if (RunModifierSystem.Instance != null)
            {
                speedMultiplier =
                    RunModifierSystem.Instance.GetEnemySpeedMultiplier();
            }

            rb.linearVelocity = dir * moveSpeed * speedMultiplier;

            // Lật mặt theo hướng di chuyển
            if (sr != null)
            {
                sr.flipX = dir.x < 0;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator DetonationCountdown()
    {
        isPrimed = true;
        rb.linearVelocity = Vector2.zero;

        // Phát âm thanh cảnh báo bíp bíp
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOverheatWarning();
        }

        float elapsed = 0f;
        while (elapsed < fuseDuration)
        {
            elapsed += Time.deltaTime;
            // Nhấp nháy đỏ rực cảnh báo (Flash Warning)
            float flash = Mathf.PingPong(elapsed * 16f, 1f);
            if (sr != null)
            {
                sr.color = Color.Lerp(originalColor, new Color(1f, 0.1f, 0.1f, 1f), flash);
            }
            yield return null;
        }

        Explode();
    }

    private void OnKilledByPlayer()
    {
        if (!hasExploded)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        StopAllCoroutines();

        Vector3 pos = transform.position;

        // Hiệu ứng nổ lửa hoành tráng & Khói bụi
        CombatFeedback.SpawnSparks(pos, Vector2.up, new Color(1f, 0.45f, 0.05f), 24);
        CombatFeedback.SpawnSparks(pos, Vector2.down, Color.red, 16);
        CombatFeedback.ShowNotice(pos + Vector3.up * 0.5f, "💥 KABOOM!", Color.red);

        // Rung chấn màn hình
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(0.12f, 0.18f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }

        // Sát thương nổ diện rộng AOE (Quét trúng cả Player lẫn Kẻ địch khác)
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, explosionRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || hit.gameObject == gameObject) continue;

            // 1. Sát thương Player nếu không kịp né
            PlayerHealth pHp = hit.GetComponent<PlayerHealth>();
            if (pHp != null)
            {
                pHp.TakeDamage(explosionDamage);
                Rigidbody2D pRb = hit.GetComponent<Rigidbody2D>();
                if (pRb != null)
                {
                    Vector2 pushDir = ((Vector2)hit.transform.position - (Vector2)pos).normalized;
                    pRb.AddForce(pushDir * 7f, ForceMode2D.Impulse);
                }
            }

            // 2. Sát thương kẻ địch khác (Chain Reaction kích nổ dây chuyền)
            EnemyHealth eHp = hit.GetComponent<EnemyHealth>();
            if (eHp != null && eHp.CurrentHealth > 0f)
            {
                eHp.TakeDamage(explosionDamage * 1.5f); // Quái dính đạn nổ chịu x1.5 sát thương
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
