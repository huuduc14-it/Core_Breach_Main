using System.Collections;
using UnityEngine;

/// <summary>
/// AI cho Sniper Mech (Robot Bắn Tỉa Tầm Xa) chuẩn phong cách Roguelite / Soul Knight:
/// - Giữ khoảng cách chiến thuật (5m - 8m), lùi lại nếu Player áp sát
/// - Chiếu tia ngắm laser đỏ mỏng nhắm thẳng vào Player trong 1.1s (Telegraphing rõ nét)
/// - Bắn một phát đạn Plasma Cao Tốc (Piercing Sniper Round) tốc độ cao (18f, DMG 20)
/// - Cơ chế đấu trí: Người chơi quan sát vệt laser đỏ để canh thời điểm bấm [SPACE] Lướt Né ngoạn mục!
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SniperMech : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 2.2f;
    [SerializeField] private float optimalDistance = 6.5f;
    [SerializeField] private float minDistance = 3.5f;
    [SerializeField] private float aimDuration = 1.1f;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float sniperDamage = 20f;
    [SerializeField] private float bulletSpeed = 18f;

    [Header("Prefabs & References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private SpriteRenderer sr;
    private EnemyHealth health;

    private bool isAiming = false;
    private float cooldownTimer = 0f;
    private Vector2 lockedAimDir = Vector2.right;

    private static Texture2D laserTex;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();

        if (firePoint == null) firePoint = transform;
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
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return;
        }

        if (cooldownTimer > 0f)
        {
            //cooldownTimer -= Time.deltaTime;
            float fireMultiplier = 1f;

            if (RunModifierSystem.Instance != null)
            {
                fireMultiplier =
                    RunModifierSystem.Instance.GetEnemyFireRateMultiplier();
            }

            cooldownTimer -= Time.deltaTime;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);

        //if (!isAiming && cooldownTimer <= 0f && dist <= optimalDistance + 3f)
        //{
        //    StartCoroutine(AimAndShootRoutine());
        //}
        float rangeMultiplier = 1f;

        if (RunModifierSystem.Instance != null)
        {
            rangeMultiplier =
                RunModifierSystem.Instance.GetEnemyRangeMultiplier();
        }

        if (!isAiming &&
            cooldownTimer <= 0f &&
            dist <= (optimalDistance + 3f) * rangeMultiplier)
        {
            StartCoroutine(AimAndShootRoutine());
        }
    }

    private void FixedUpdate()
    {
        float speedMultiplier = 1f;

        if (RunModifierSystem.Instance != null)
        {
            speedMultiplier =
                RunModifierSystem.Instance.GetEnemySpeedMultiplier();
        }
        if (isAiming)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        Vector2 toPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;

        // Nếu quá gần: Lùi lại giữ khoảng cách
        if (dist < minDistance)
        {
            //rb.linearVelocity = -toPlayer * (moveSpeed * 1.1f);
            rb.linearVelocity = -toPlayer * (moveSpeed * speedMultiplier * 1.1f);
        }
        // Nếu quá xa: Tiến lại gần
        else if (dist > optimalDistance + 1f)
        {
            //rb.linearVelocity = toPlayer * moveSpeed;
            rb.linearVelocity = toPlayer * moveSpeed * speedMultiplier;
        }
        else
        {
            // Đi ngang để né đạn (Strafing)
            Vector2 perp = new Vector2(-toPlayer.y, toPlayer.x);
            //rb.linearVelocity = perp * (moveSpeed * 0.7f);
            rb.linearVelocity = perp * (moveSpeed * speedMultiplier * 0.7f);
        }

        if (sr != null && toPlayer.x != 0)
        {
            sr.flipX = toPlayer.x < 0;
        }
    }

    private IEnumerator AimAndShootRoutine()
    {
        isAiming = true;
        rb.linearVelocity = Vector2.zero;

        float elapsed = 0f;
        while (elapsed < aimDuration)
        {
            elapsed += Time.deltaTime;
            if (playerTransform != null)
            {
                // Bám sát hướng ngắm vào Player
                lockedAimDir = ((Vector2)playerTransform.position - (Vector2)firePoint.position).normalized;
                if (sr != null) sr.flipX = lockedAimDir.x < 0;
            }
            yield return null;
        }

        // Bắn phát súng ngắm cao tốc
        FireSniperBullet();

        //cooldownTimer = attackCooldown;
        float fireMultiplier = 1f;

        if (RunModifierSystem.Instance != null)
        {
            fireMultiplier =
                RunModifierSystem.Instance.GetEnemyFireRateMultiplier();
        }

        cooldownTimer = attackCooldown * fireMultiplier;
        isAiming = false;
    }

    private void FireSniperBullet()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        // Khói súng & tia chớp đầu nòng
        CombatFeedback.SpawnSparks(spawnPos, lockedAimDir, new Color(1f, 0.2f, 0.8f), 12);
        CombatFeedback.SpawnSparks(spawnPos, -lockedAimDir, Color.white, 6);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShoot();
        }

        // Tạo đạn Sniper
        GameObject bulletObj = null;
        if (bulletPrefab != null)
        {
            bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Tận dụng Bullet prefab có sẵn trong Scene
            Bullet existing = FindFirstObjectByType<Bullet>();
            if (existing != null) bulletObj = Instantiate(existing.gameObject, spawnPos, Quaternion.identity);
        }

        if (bulletObj != null)
        {
            bulletObj.name = "SniperBullet";
            bulletObj.tag = "EnemyBullet";

            // Xoay hướng đạn
            float angle = Mathf.Atan2(lockedAimDir.y, lockedAimDir.x) * Mathf.Rad2Deg;
            bulletObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Cấu hình đạn
            Bullet b = bulletObj.GetComponent<Bullet>();
            if (b != null)
            {
                b.Initialize(lockedAimDir, sniperDamage, false);
            }
            else
            {
                Rigidbody2D bRb = bulletObj.GetComponent<Rigidbody2D>();
                if (bRb != null) bRb.linearVelocity = lockedAimDir * bulletSpeed;
            }

            SpriteRenderer bSr = bulletObj.GetComponent<SpriteRenderer>();
            if (bSr != null)
            {
                bSr.color = new Color(1f, 0.15f, 0.35f, 1f); // Đạn tím đỏ neon
            }
        }
    }

    private static void EnsureLaserTex()
    {
        if (laserTex == null)
        {
            laserTex = new Texture2D(1, 1);
            laserTex.SetPixel(0, 0, Color.white);
            laserTex.Apply();
        }
    }

    private void OnGUI()
    {
        if (!isAiming || playerTransform == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        EnsureLaserTex();

        Vector3 gunWorld = firePoint != null ? firePoint.position : transform.position;
        Vector3 targetWorld = playerTransform.position;

        Vector3 screenStart = cam.WorldToScreenPoint(gunWorld);
        Vector3 screenEnd = cam.WorldToScreenPoint(targetWorld);

        if (screenStart.z > 0 && screenEnd.z > 0)
        {
            Vector2 p1 = new Vector2(screenStart.x, Screen.height - screenStart.y);
            Vector2 p2 = new Vector2(screenEnd.x, Screen.height - screenEnd.y);

            // Vẽ tia laser đỏ cảnh báo ngắm bắn
            float pulse = Mathf.PingPong(Time.time * 12f, 1f);
            Color laserCol = new Color(1f, 0.15f, 0.25f, 0.45f + pulse * 0.45f);

            GUI.color = laserCol;
            Vector2 d = p2 - p1;
            float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            GUIUtility.RotateAroundPivot(angle, p1);
            GUI.DrawTexture(new Rect(p1.x, p1.y - 1f, d.magnitude, 2.5f), laserTex);
            GUIUtility.RotateAroundPivot(-angle, p1);
            GUI.color = Color.white;
        }
    }
}
