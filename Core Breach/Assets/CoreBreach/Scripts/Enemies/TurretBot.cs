using UnityEngine;

/// <summary>
/// AI cho Turret Bot (Kẻ địch tầm xa) theo Game Concept Core Breach:
/// - HP: 15, Speed: 0 (đứng yên)
/// - Bắn đạn năng lượng chu kỳ 1.2s khi Player nằm trong tầm bắn
/// - Tính năng Hack Terminal độc quyền: Khi bị hạ, có thể bị Unit-7 "Hack" để trở thành đồng minh bắn hỗ trợ!
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TurretBot : MonoBehaviour
{
    [Header("Stats theo Game Concept")]
    [SerializeField] private float fireRate = 1.2f;        // Bắn theo chu kỳ 1.2 giây
    [SerializeField] private float attackDamage = 7f;      // Sát thương 7
    [SerializeField] private float attackRange = 8.0f;     // Tầm bắn
    [SerializeField] private float rotationSpeed = 5.0f;

    [Header("Bullet Settings")]
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Hack Terminal Mechanic")]
    [SerializeField] private bool canBeHacked = true;
    [SerializeField] private float hackedDuration = 6.0f;  // Thời gian hỗ trợ khi bị hack

    private float fireTimer = 0f;
    private Transform playerTransform;
    private bool isHacked = false;
    private float hackedTimer = 0f;
    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (isHacked)
        {
            HandleHackedState();
            return;
        }

        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= attackRange)
        {
            // Xoay hướng ngắm vào Unit-7
            RotateTowards(playerTransform.position);

            // Bắn đạn theo chu kỳ
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                Shoot(playerTransform.position, false);
                fireTimer = fireRate;
            }
        }
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector2 dir = (targetPosition - transform.position).normalized;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void Shoot(Vector3 targetPosition, bool isFriendly)
    {
        if (enemyBulletPrefab == null)
        {
            Bullet existing = FindObjectOfType<Bullet>();
            if (existing != null) enemyBulletPrefab = existing.gameObject;
        }

        if (enemyBulletPrefab == null) return;

        Vector2 dir = (targetPosition - firePoint.position).normalized;
        GameObject bulletObj = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(dir, attackDamage, isFriendly);
        }
    }

    /// <summary>
    /// Kích hoạt cơ chế Hack Terminal: Turret đổi phe hỗ trợ Unit-7 bắn quái
    /// </summary>
    public void HackTerminal()
    {
        if (!canBeHacked || isHacked) return;

        isHacked = true;
        hackedTimer = hackedDuration;
        gameObject.tag = "Player"; // Đổi tag thành đồng minh
        Debug.Log("[TurretBot] HACK THÀNH CÔNG! Turret Bot tạm thời hỗ trợ Unit-7.");
    }

    private void HandleHackedState()
    {
        hackedTimer -= Time.deltaTime;
        if (hackedTimer <= 0f)
        {
            // Hết giờ hack -> nổ tung giải phóng Scrap
            Destroy(gameObject);
            return;
        }

        // Tìm quái gần nhất để bắn hỗ trợ Player
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy closestEnemy = null;
        float minDist = attackRange;

        foreach (var enemy in enemies)
        {
            if (enemy.gameObject == gameObject) continue;
            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy != null)
        {
            RotateTowards(closestEnemy.transform.position);
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                Shoot(closestEnemy.transform.position, true); // Đạn đồng minh
                fireTimer = fireRate * 0.8f;
            }
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }
}
