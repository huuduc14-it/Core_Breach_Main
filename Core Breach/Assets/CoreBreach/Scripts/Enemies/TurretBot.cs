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

    //private void Update()
    //{
    //    if (isHacked)
    //    {
    //        HandleHackedState();
    //        return;
    //    }

    //    if (playerTransform == null)
    //    {
    //        FindPlayer();
    //        return;
    //    }

    //    float distance = Vector2.Distance(transform.position, playerTransform.position);

    //    // Kích hoạt Hack Terminal bằng phím E khi Player áp sát
    //    //if (!isHacked && canBeHacked && distance <= 2.5f)
    //    //{
    //    //    if (Input.GetKeyDown(KeyCode.E))
    //    //    {
    //    //        HackTerminal();
    //    //        return;
    //    //    }
    //    //}

    //    if (distance <= attackRange)
    //    {
    //        // Xoay hướng ngắm vào Unit-7
    //        RotateTowards(playerTransform.position);

    //        // Bắn đạn theo chu kỳ
    //        //fireTimer -= Time.deltaTime;
    //        //if (fireTimer <= 0f)
    //        //{
    //        //    Shoot(playerTransform.position, false);
    //        //    fireTimer = fireRate;
    //        //}
    //        float fireMultiplier = 1f;
    //        float rangeMultiplier = 1f;

    //        if (RunModifierSystem.Instance != null)
    //        {
    //            fireMultiplier =
    //                RunModifierSystem.Instance.GetEnemyFireRateMultiplier();

    //            rangeMultiplier =
    //                RunModifierSystem.Instance.GetEnemyRangeMultiplier();
    //        }

    //        fireTimer -= Time.deltaTime;

    //        if (fireTimer <= 0f)
    //        {
    //            Shoot(playerTransform.position, false);

    //            fireTimer = fireRate * fireMultiplier;
    //        }
    //    }
    //}

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

        float distance = Vector2.Distance(
            transform.position,
            playerTransform.position
        );

        // Lấy hệ số từ Run Modifier
        float fireMultiplier = 1f;
        float rangeMultiplier = 1f;

        if (RunModifierSystem.Instance != null)
        {
            fireMultiplier =
                RunModifierSystem.Instance.GetEnemyFireRateMultiplier();

            rangeMultiplier =
                RunModifierSystem.Instance.GetEnemyRangeMultiplier();
        }

        // Tầm tấn công thực tế
        float effectiveAttackRange =
            attackRange * rangeMultiplier;

        // Kiểm tra Player có nằm trong tầm bắn không
        if (distance <= effectiveAttackRange)
        {
            // Xoay hướng ngắm vào Player
            RotateTowards(playerTransform.position);

            // Đếm thời gian giữa các lần bắn
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0f)
            {
                Shoot(playerTransform.position, false);

                // Điều chỉnh tốc độ bắn theo Modifier
                fireTimer = fireRate * fireMultiplier;
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
            Bullet existing = FindFirstObjectByType<Bullet>();
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
        canBeHacked = false;
        hackedTimer = hackedDuration;
        gameObject.tag = "Friendly"; // Đổi tag thành đồng minh

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(0f, 1f, 0.75f, 1f); // Xanh mint đồng minh

        CombatFeedback.ShowNotice(transform.position, "SYSTEM OVERRIDE!", new Color(0f, 1f, 0.75f));
        CombatFeedback.HitStop(0.04f);

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
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
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
        if (PlayerMovement.PlayerTransform != null)
        {
            playerTransform = PlayerMovement.PlayerTransform;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
    }

    private static Texture2D telegraphTex;

    private void OnGUI()
    {
        Camera cam = Camera.main;
        if (cam == null || playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // 1. Tia laser cảnh báo ngắm bắn (Telegraph Laser) trước khi xả đạn
        if (!isHacked && distance <= attackRange && fireTimer <= 0.45f)
        {
            if (telegraphTex == null)
            {
                telegraphTex = new Texture2D(1, 1);
                telegraphTex.SetPixel(0, 0, Color.white);
                telegraphTex.Apply();
            }

            Vector3 startScreen = cam.WorldToScreenPoint(firePoint.position);
            Vector3 targetScreen = cam.WorldToScreenPoint(playerTransform.position);

            if (startScreen.z > 0 && targetScreen.z > 0)
            {
                Vector2 pA = new Vector2(startScreen.x, Screen.height - startScreen.y);
                Vector2 pB = new Vector2(targetScreen.x, Screen.height - targetScreen.y);

                float pulse = Mathf.Sin(Time.time * 30f) > 0 ? 0.9f : 0.4f;
                GUI.color = new Color(1f, 0.15f, 0.15f, pulse);

                Vector2 d = pB - pA;
                float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                GUIUtility.RotateAroundPivot(angle, pA);
                GUI.DrawTexture(new Rect(pA.x, pA.y - 1f, d.magnitude, 2.5f), telegraphTex);
                GUIUtility.RotateAroundPivot(-angle, pA);
                GUI.color = Color.white;
            }
        }

        // 2. Thông báo phím tương tác Hack Terminal
        if (isHacked || !canBeHacked) return;

        if (distance <= 2.5f)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position + Vector3.up * 0.8f);
            if (screenPos.z > 0)
            {
                GUI.color = new Color(0f, 1f, 0.7f, 1f);
                GUIStyle promptStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 12
                };
                GUI.Label(new Rect(screenPos.x - 70, Screen.height - screenPos.y - 12, 140, 24), "[E] HACK TERMINAL", promptStyle);
                GUI.color = Color.white;
            }
        }
    }
}
