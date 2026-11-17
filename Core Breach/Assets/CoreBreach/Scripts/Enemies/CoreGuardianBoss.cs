using System.Collections;
using UnityEngine;

/// <summary>
/// AI cho Core Guardian (Mini-boss xuất hiện mỗi 5 phòng) theo Game Concept Core Breach:
/// - HP: 180, ATK: 15-25, Speed: 2.0
/// - FSM 3 Pha chuyển tiếp theo % HP:
///   + Pha 1 (HP > 50%): Bắn chùm đạn năng lượng thẳng về phía Player
///   + Pha 2 (HP <= 50%): Triệu hồi 2 Sentry Drone hỗ trợ, tăng tốc độ bắn
///   + Pha 3 (HP < 25%): Trạng thái Overdrive cuồng nộ, tăng tốc lao xả thân húc Player
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth))]
public class CoreGuardianBoss : MonoBehaviour
{
    public enum BossPhase { Phase1_ShootBeam, Phase2_SummonMinions, Phase3_BerserkRush }

    [Header("Boss Stats")]
    [SerializeField] private float baseSpeed = 2.0f;
    [SerializeField] private float berserkSpeed = 5.5f;
    [SerializeField] private float contactDamage = 20f;

    [Header("Phase 1 & 2: Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float phase1FireInterval = 1.8f;
    [SerializeField] private float phase2FireInterval = 1.1f;
    [SerializeField] private int burstCount = 3;

    [Header("Phase 2: Summoning")]
    [SerializeField] private GameObject sentryDronePrefab;
    [SerializeField] private Transform[] summonPoints;

    [Header("Phase 3: Berserk")]
    [SerializeField] private Color berserkColor = new Color(1f, 0.2f, 0.2f, 1f);

    private BossPhase currentPhase = BossPhase.Phase1_ShootBeam;
    private EnemyHealth health;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    private float fireTimer = 0f;
    private bool hasSummonedPhase2 = false;

    public BossPhase CurrentPhase => currentPhase;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (firePoint == null) firePoint = transform;
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        UpdateBossPhases();

        // Xử lý hành vi theo từng pha
        switch (currentPhase)
        {
            case BossPhase.Phase1_ShootBeam:
                HandlePhase1();
                break;
            case BossPhase.Phase2_SummonMinions:
                HandlePhase2();
                break;
            case BossPhase.Phase3_BerserkRush:
                HandlePhase3();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        if (currentPhase == BossPhase.Phase3_BerserkRush)
        {
            // Pha 3: Lao thẳng vào người chơi với tốc độ cao
            Vector2 dir = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = dir * berserkSpeed;
        }
        else
        {
            // Pha 1 & 2: Di chuyển chậm rãi áp sát vừa phải
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            if (dist > 4.0f)
            {
                Vector2 dir = (playerTransform.position - transform.position).normalized;
                rb.linearVelocity = dir * baseSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void UpdateBossPhases()
    {
        if (health == null) return;

        float hpPercent = (health.CurrentHealth / health.MaxHealth) * 100f;

        if (hpPercent <= 25f && currentPhase != BossPhase.Phase3_BerserkRush)
        {
            EnterPhase3();
        }
        else if (hpPercent <= 50f && hpPercent > 25f && currentPhase == BossPhase.Phase1_ShootBeam)
        {
            EnterPhase2();
        }
    }

    // =========================================================
    // PHASE IMPLEMENTATIONS
    // =========================================================

    private void HandlePhase1()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            StartCoroutine(FireBurst(burstCount, 0.15f));
            fireTimer = phase1FireInterval;
        }
    }

    private void EnterPhase2()
    {
        currentPhase = BossPhase.Phase2_SummonMinions;
        Debug.Log("[CoreGuardianBoss] CHUYỂN PHA 2: Kích hoạt triệu hồi Sentry Drone hỗ trợ!");

        if (!hasSummonedPhase2)
        {
            hasSummonedPhase2 = true;
            SummonMinions();
        }
    }

    private void HandlePhase2()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            StartCoroutine(FireSpreadShot(5, 30f)); // Bắn chùm 5 viên toả rộng
            fireTimer = phase2FireInterval;
        }
    }

    private void EnterPhase3()
    {
        currentPhase = BossPhase.Phase3_BerserkRush;
        Debug.Log("[CoreGuardianBoss] CẢNH BÁO: CHUYỂN PHA 3 - OVERDRIVE CUỒNG NỘ!");

        if (spriteRenderer != null)
        {
            spriteRenderer.color = berserkColor;
        }
    }

    private void HandlePhase3()
    {
        // Bắn đạn ngẫu nhiên xung quanh khi đang cuồng nộ
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            StartCoroutine(FireRadialNova(8));
            fireTimer = 1.5f;
        }
    }

    // =========================================================
    // ATTACK COROUTINES
    // =========================================================

    private IEnumerator FireBurst(int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            if (playerTransform == null) yield break;
            Vector2 dir = (playerTransform.position - firePoint.position).normalized;
            SpawnBullet(dir, 15f);
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator FireSpreadShot(int bullets, float totalAngle)
    {
        if (playerTransform == null) yield break;
        Vector2 baseDir = (playerTransform.position - firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - totalAngle / 2f;
        float step = totalAngle / (bullets - 1);

        for (int i = 0; i < bullets; i++)
        {
            float curAngle = startAngle + step * i;
            Vector2 dir = new Vector2(Mathf.Cos(curAngle * Mathf.Deg2Rad), Mathf.Sin(curAngle * Mathf.Deg2Rad));
            SpawnBullet(dir, 18f);
        }
        yield return null;
    }

    private IEnumerator FireRadialNova(int count)
    {
        float step = 360f / count;
        for (int i = 0; i < count; i++)
        {
            float curAngle = step * i;
            Vector2 dir = new Vector2(Mathf.Cos(curAngle * Mathf.Deg2Rad), Mathf.Sin(curAngle * Mathf.Deg2Rad));
            SpawnBullet(dir, 20f);
        }
        yield return null;
    }

    private void SpawnBullet(Vector2 direction, float damage)
    {
        if (bulletPrefab == null)
        {
            Bullet existing = FindFirstObjectByType<Bullet>();
            if (existing != null) bulletPrefab = existing.gameObject;
        }

        if (bulletPrefab == null) return;
        GameObject bObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet b = bObj.GetComponent<Bullet>();
        if (b != null)
        {
            b.Initialize(direction, damage, false);
        }
    }

    private void SummonMinions()
    {
        if (sentryDronePrefab == null) return;

        if (summonPoints != null && summonPoints.Length >= 2)
        {
            Instantiate(sentryDronePrefab, summonPoints[0].position, Quaternion.identity);
            Instantiate(sentryDronePrefab, summonPoints[1].position, Quaternion.identity);
        }
        else
        {
            // Spawn 2 bên cạnh Boss
            Instantiate(sentryDronePrefab, transform.position + new Vector3(-2f, 1f, 0f), Quaternion.identity);
            Instantiate(sentryDronePrefab, transform.position + new Vector3(2f, 1f, 0f), Quaternion.identity);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth pHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(contactDamage);
            }
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }
}
