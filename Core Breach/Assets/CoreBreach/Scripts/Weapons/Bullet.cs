using UnityEngine;

/// <summary>
/// Đạn bắn ra từ súng của Unit-7 (Player) hoặc Kẻ địch (Turret, Boss)
/// Tự động di chuyển theo hướng bắn, gây sát thương khi trúng mục tiêu và tự hủy khi trúng vật cản hoặc hết thời gian sống.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private bool isPlayerBullet = true;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Start()
    {
        // Tự huỷ sau thời gian tồn tại để giải phóng bộ nhớ
        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// Khởi tạo và bắn đạn (dành cho súng người chơi)
    /// </summary>
    public void Launch(Vector2 direction, float bulletDamage = -1f)
    {
        Initialize(direction, bulletDamage > 0f ? bulletDamage : damage, true);
    }

    /// <summary>
    /// Khởi tạo tổng quát (cả người chơi và kẻ địch)
    /// </summary>
    public void Initialize(Vector2 direction, float bulletDamage, bool playerBullet)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        damage = bulletDamage;
        isPlayerBullet = playerBullet;

        Vector2 normDir = direction.normalized;
        rb.linearVelocity = normDir * speed;

        // Xoay đạn theo hướng bay
        float angle = Mathf.Atan2(normDir.y, normDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Bỏ qua nếu đạn chạm vào chính người bắn
        if (isPlayerBullet && collision.CompareTag("Player")) return;
        if (!isPlayerBullet && collision.CompareTag("Enemy")) return;

        // Đạn Player trúng Enemy
        if (isPlayerBullet && collision.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // Đạn Enemy trúng Player
        if (!isPlayerBullet && collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // Đạn chạm vào Tường hoặc các vật cản khác
        if (!collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
