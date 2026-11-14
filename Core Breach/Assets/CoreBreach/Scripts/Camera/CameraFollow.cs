using UnityEngine;

/// <summary>
/// Camera mượt mà bám theo Unit-7 (Player) trong không gian 2D Top-down
/// Hỗ trợ giới hạn khung nhìn (Bounds) theo kích thước phòng nếu cần.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Smooth Settings")]
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Bounds (Tùy chọn giới hạn trong phòng)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-20f, -20f);
    [SerializeField] private Vector2 maxBounds = new Vector2(20f, 20f);

    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;

    private void Awake()
    {
        Instance = this;
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
        if (target == null)
        {
            FindPlayerTarget();
            if (target == null) return;
        }

        Vector3 desiredPosition = target.position + offset;

        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        // Nội suy mượt mà vị trí Camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        smoothedPosition.z = offset.z; // Giữ nguyên độ sâu Z của Camera 2D

        // Xử lý rung màn hình (Camera Shake)
        if (shakeTimer > 0f)
        {
            Vector2 shakeOffset = Random.insideUnitCircle * shakeMagnitude;
            smoothedPosition.x += shakeOffset.x;
            smoothedPosition.y += shakeOffset.y;
            shakeTimer -= Time.deltaTime;
        }

        transform.position = smoothedPosition;
    }

    /// <summary>
    /// Kích hoạt rung màn hình (Game Feel / Juiciness)
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            PlayerMovement pm = FindObjectOfType<PlayerMovement>();
            if (pm != null)
            {
                target = pm.transform;
            }
        }
    }
}
