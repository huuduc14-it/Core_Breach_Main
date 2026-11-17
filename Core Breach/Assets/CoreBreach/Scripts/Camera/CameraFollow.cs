using UnityEngine;

/// <summary>
/// Camera mượt mà bám theo Player chuẩn phong cách Soul Knight trong không gian 2D Top-down:
/// - Góc nhìn 3/4 thoải mái: Offset nhẹ Y (+0.35f) và Ortho Size ~4.5 giúp nhìn rõ toàn thân, chân bước và vung vũ khí
/// - Dẫn hướng động (Dynamic Lead): Camera hướng nhẹ theo chiều di chuyển và hướng ngắm/kẻ địch
/// - SmoothDamp mượt mà tuyệt đối không khựng giật
/// - Rung chấn Camera Shake tinh tế, mượt mà
/// - Hỗ trợ con lăn chuột (Mouse Scroll) phóng to / thu nhỏ tự do (3.2x - 5.8x)
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Framing & Smooth Settings (Soul Knight Style)")]
    [SerializeField] private float smoothTime = 0.12f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0.35f, -10f); // 3/4 angle cozy offset
    [SerializeField] private float velocityLeadMultiplier = 0.22f;

    [Header("Zoom Settings (Góc nhìn & Kích thước)")]
    [SerializeField] private float targetOrthoSize = 4.5f; // Zoom cân bằng hoàn hảo Soul Knight
    [SerializeField] private float zoomSpeed = 4f;

    [Header("Bounds (Tùy chọn giới hạn trong phòng)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-20f, -20f);
    [SerializeField] private Vector2 maxBounds = new Vector2(20f, 20f);

    private Camera cam;
    private Rigidbody2D targetRb;
    private PlayerAim playerAim;
    private Vector3 currentVelocity = Vector3.zero;

    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;

    public float OrthoSize => cam != null ? cam.orthographicSize : targetOrthoSize;

    private void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = targetOrthoSize;
        }
    }

    private void Start()
    {
        if (target == null)
        {
            FindPlayerTarget();
        }
    }

    private void LateUpdate()
    {
        HandleZoom();

        if (target == null)
        {
            FindPlayerTarget();
            if (target == null) return;
        }

        // 1. Tính toán Lead di chuyển theo chuyển động của Player (Cố định, không bị giật theo hướng ngắm quái)
        Vector3 velocityLead = Vector3.zero;
        if (targetRb != null)
        {
            velocityLead = Vector3.ClampMagnitude(targetRb.linearVelocity * velocityLeadMultiplier, 1.0f);
        }

        Vector3 desiredPosition = target.position + offset + velocityLead;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        // 2. Nội suy mượt mà vị trí Camera bằng SmoothDamp (Zero Stutter)
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        smoothedPosition.z = offset.z; // Giữ nguyên độ sâu Z của Camera 2D

        // 3. Xử lý rung màn hình (Camera Shake)
        if (shakeTimer > 0f)
        {
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude;
            smoothedPosition.x += shakeOffset.x;
            smoothedPosition.y += shakeOffset.y;
            shakeTimer -= Time.deltaTime;
        }

        transform.position = smoothedPosition;
    }

    private void HandleZoom()
    {
        if (cam == null || !cam.orthographic) return;

        // Cho phép người chơi lăn chuột để tùy chỉnh cự ly gần/xa theo ý thích
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.05f)
        {
            targetOrthoSize = Mathf.Clamp(targetOrthoSize - scroll * 0.4f, 3.2f, 5.8f);
        }

        // Nội suy mượt mà kích thước Camera
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize, zoomSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Kích hoạt rung màn hình tinh tế (Game Feel / Juiciness mà không gây chóng mặt)
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        shakeTimer = Mathf.Min(duration, 0.12f);
        shakeMagnitude = Mathf.Min(magnitude, 0.06f);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
            playerAim = target.GetComponent<PlayerAim>();
            Vector3 targetPos = target.position + offset;
            transform.position = new Vector3(targetPos.x, targetPos.y, offset.z);
        }
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        useBounds = true;
        minBounds = min;
        maxBounds = max;
    }

    public void DisableBounds()
    {
        useBounds = false;
    }

    private void FindPlayerTarget()
    {
        if (PlayerMovement.Instance != null)
        {
            SetTarget(PlayerMovement.Instance.transform);
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SetTarget(player.transform);
        }
        else
        {
            PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null)
            {
                SetTarget(pm.transform);
            }
        }
    }
}
