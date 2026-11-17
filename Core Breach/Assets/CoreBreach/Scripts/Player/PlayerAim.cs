using UnityEngine;

/// <summary>
/// Quản lý hướng ngắm bắn và cơ chế Soft-Lock (Tự động khóa mục tiêu) chuẩn phong cách Soul Knight:
/// - Khi có quái xuất hiện trong phòng: Súng và hướng ngắm tự động khóa mục tiêu kẻ địch gần nhất/nguy hiểm nhất
/// - TRẠNG THÁI NHÂN VẬT VẪN HOÀN TOÀN ĐỨNG YÊN (IDLE): Không tự di chuyển, không tự bắn bậy
/// - Người chơi hoàn toàn làm chủ: WASD để bước đi, Click chuột trái để nổ súng / vung kiếm chém
/// - Khi rê chuột mạnh hoặc không có quái: Mượt mà chuyển sang bám theo con trỏ chuột
/// - Tâm ngắm viền đỏ Sci-Fi khóa chặt quái vật
/// </summary>
public class PlayerAim : MonoBehaviour
{
    [Header("Aim Settings")]
    [Tooltip("Transform của vũ khí/điểm xoay súng (WeaponPivot)")]
    [SerializeField] private Transform aimTransform;

    [Header("Soul Knight Auto-Aim / Soft-Lock")]
    [SerializeField] private bool enableSoftLock = true;
    [SerializeField] private float autoLockRadius = 8.5f;

    [Header("Laser Sight")]
    [SerializeField] private bool showLaserSight = false; // Tắt mặc định để màn hình sạch đẹp như Soul Knight

    private Camera mainCamera;
    private Vector2 currentAimDirection = Vector2.right;
    private float currentAimAngle = 0f;

    private Transform currentLockTarget = null;
    private float manualAimTimer = 0f;
    private Vector3 lastMousePos;

    public bool ShowLaserSight
    {
        get => showLaserSight;
        set => showLaserSight = value;
    }

    public Vector2 CurrentAimDirection => currentAimDirection;
    public float CurrentAimAngle => currentAimAngle;
    public Transform AimTransform => aimTransform;
    public Transform CurrentLockTarget => currentLockTarget;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (aimTransform == null || aimTransform == transform)
        {
            Transform pivot = transform.Find("WeaponPivot");
            if (pivot == null)
            {
                GameObject newPivot = new GameObject("WeaponPivot");
                newPivot.transform.SetParent(transform, false);
                newPivot.transform.localPosition = new Vector3(0.18f, -0.05f, 0f);
                aimTransform = newPivot.transform;
            }
            else
            {
                aimTransform = pivot;
            }
        }

        lastMousePos = Input.mousePosition;
    }

    private void Update()
    {
        // Khóa ngắm bắn nếu không phải trạng thái Playing
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        // Khóa nếu đang mở bảng chọn nhân vật
        if (CharacterSelectSystem.Instance != null && CharacterSelectSystem.Instance.IsSelectionOpen) return;

        // Kiểm tra xem người chơi có đang chủ động di chuyển chuột không
        Vector3 mouseDelta = Input.mousePosition - lastMousePos;
        lastMousePos = Input.mousePosition;
        if (mouseDelta.sqrMagnitude > 9f)
        {
            manualAimTimer = 0.5f; // Tạm ưu tiên chuột trong 0.5 giây khi người chơi rê chuột
        }
        else if (manualAimTimer > 0f)
        {
            manualAimTimer -= Time.deltaTime;
        }

        // Cập nhật ngắm bắn
        UpdateAimOrientation();

        // Phím tắt [L] để bật/tắt tia laser chiến thuật
        if (Input.GetKeyDown(KeyCode.L))
        {
            showLaserSight = !showLaserSight;
            CombatFeedback.ShowNotice(transform.position + Vector3.up * 0.8f, 
                showLaserSight ? "LASER SIGHT: ON [L]" : "LASER SIGHT: OFF [L]", 
                new Color(0f, 0.9f, 1f));
        }
    }

    private void UpdateAimOrientation()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Vector3 myPos = transform.position;
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Transform bestTarget = null;
        if (enableSoftLock && manualAimTimer <= 0f)
        {
            bestTarget = FindBestEnemyTarget(myPos, mouseWorld);
        }

        currentLockTarget = bestTarget;

        Vector2 targetDir;
        if (currentLockTarget != null)
        {
            // Hướng ngắm bám chuẩn vào kẻ địch
            targetDir = ((Vector2)currentLockTarget.position - (Vector2)myPos).normalized;
        }
        else
        {
            // Hướng ngắm bám theo con trỏ chuột
            targetDir = ((Vector2)mouseWorld - (Vector2)myPos).normalized;
        }

        if (targetDir.sqrMagnitude > 0.001f)
        {
            // Nội suy góc quay mượt mà (không giật cục)
            currentAimDirection = Vector2.Lerp(currentAimDirection, targetDir, Time.deltaTime * 18f).normalized;
            currentAimAngle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;
        }

        // Xoay cánh tay và súng (WeaponPivot)
        if (aimTransform != null && aimTransform != transform)
        {
            aimTransform.rotation = Quaternion.Euler(0f, 0f, currentAimAngle);
        }
    }

    private Transform FindBestEnemyTarget(Vector3 origin, Vector3 mousePos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, autoLockRadius);
        Transform best = null;
        float bestScore = float.MaxValue;

        // Giữ mục tiêu cũ nếu vẫn còn sống và trong tầm (Target Hysteresis tránh rung giật)
        if (currentLockTarget != null)
        {
            EnemyHealth prevHp = currentLockTarget.GetComponent<EnemyHealth>();
            if (prevHp != null && prevHp.CurrentHealth > 0f && Vector2.Distance(origin, currentLockTarget.position) <= autoLockRadius * 1.15f)
            {
                best = currentLockTarget;
                bestScore = Vector2.Distance(origin, currentLockTarget.position) - 1.8f; // Điểm ưu tiên giữ mục tiêu cũ
            }
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D col = colliders[i];
            if (col == null) continue;

            EnemyHealth hp = col.GetComponent<EnemyHealth>();
            if (hp == null || hp.CurrentHealth <= 0f) continue;

            // Bỏ qua TurretBot nếu đã bị hack thành đồng minh
            TurretBot turret = col.GetComponent<TurretBot>();
            if (turret != null && turret.enabled == false) continue;

            Vector3 enemyPos = col.transform.position;
            float distToPlayer = Vector2.Distance(origin, enemyPos);
            float distToMouse = Vector2.Distance(mousePos, enemyPos);

            // Điểm số ưu tiên: Gần Player + Thiên hướng theo chuột
            float score = distToPlayer + distToMouse * 0.4f;

            if (score < bestScore)
            {
                bestScore = score;
                best = col.transform;
            }
        }

        return best;
    }

    public Vector2 GetAimDirection()
    {
        if (currentLockTarget != null)
        {
            return ((Vector2)currentLockTarget.position - (Vector2)transform.position).normalized;
        }
        return currentAimDirection;
    }

    // =========================================================
    // SOUL KNIGHT TARGET RETICLE & SCI-FI CROSSHAIR
    // =========================================================

    private static Texture2D laserTex;

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
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        // Ẩn khi không phải trạng thái đang chơi
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

        // Ẩn khi đang ở Menu chọn nhân vật
        if (CharacterSelectSystem.Instance != null && CharacterSelectSystem.Instance.IsSelectionOpen) return;

        EnsureLaserTex();

        // 1. VẼ TÂM KHÓA MỤC TIÊU (SOUL KNIGHT LOCK-ON RETICLE)
        if (currentLockTarget != null)
        {
            Vector3 enemyScreen = mainCamera.WorldToScreenPoint(currentLockTarget.position);
            if (enemyScreen.z > 0)
            {
                Vector2 center = new Vector2(enemyScreen.x, Screen.height - enemyScreen.y);
                float size = 18f;
                float half = size * 0.5f;

                // 4 góc khung ngắm đỏ rực khóa mục tiêu
                GUI.color = new Color(1f, 0.2f, 0.35f, 0.95f);
                // Top-Left
                GUI.DrawTexture(new Rect(center.x - half, center.y - half, 6, 2), laserTex);
                GUI.DrawTexture(new Rect(center.x - half, center.y - half, 2, 6), laserTex);
                // Top-Right
                GUI.DrawTexture(new Rect(center.x + half - 6, center.y - half, 6, 2), laserTex);
                GUI.DrawTexture(new Rect(center.x + half - 2, center.y - half, 2, 6), laserTex);
                // Bottom-Left
                GUI.DrawTexture(new Rect(center.x - half, center.y + half - 2, 6, 2), laserTex);
                GUI.DrawTexture(new Rect(center.x - half, center.y + half - 6, 2, 6), laserTex);
                // Bottom-Right
                GUI.DrawTexture(new Rect(center.x + half - 6, center.y + half - 2, 6, 2), laserTex);
                GUI.DrawTexture(new Rect(center.x + half - 2, center.y + half - 6, 2, 6), laserTex);

                // Chấm đỏ chính giữa mục tiêu
                GUI.DrawTexture(new Rect(center.x - 2, center.y - 2, 4, 4), laserTex);
                GUI.color = Color.white;
            }
        }

        // 2. TIA LASER CHIẾN THUẬT (CHỈ HIỂN THỊ KHI BẬT [L])
        if (showLaserSight)
        {
            Vector3 gunWorldPos = aimTransform != null ? aimTransform.position : transform.position;
            Vector3 gunScreen = mainCamera.WorldToScreenPoint(gunWorldPos);
            if (gunScreen.z > 0)
            {
                Vector2 start = new Vector2(gunScreen.x, Screen.height - gunScreen.y);
                Vector2 end;

                if (currentLockTarget != null)
                {
                    Vector3 tgtScreen = mainCamera.WorldToScreenPoint(currentLockTarget.position);
                    end = new Vector2(tgtScreen.x, Screen.height - tgtScreen.y);
                }
                else
                {
                    end = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                }

                Color laserColor = currentLockTarget != null 
                    ? new Color(1f, 0.2f, 0.35f, 0.75f) 
                    : new Color(0f, 0.85f, 1f, 0.35f);

                DrawLine(start, end, laserColor, 1.5f);
            }
        }
    }

    private void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
        GUI.color = color;
        Vector2 d = pointB - pointA;
        float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        GUIUtility.RotateAroundPivot(angle, pointA);
        GUI.DrawTexture(new Rect(pointA.x, pointA.y - width * 0.5f, d.magnitude, width), laserTex);
        GUIUtility.RotateAroundPivot(-angle, pointA);
        GUI.color = Color.white;
    }
}
