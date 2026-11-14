using UnityEngine;

/// <summary>
/// Xoay nhân vật hoặc súng theo vị trí con trỏ chuột trên màn hình
/// </summary>
public class PlayerAim : MonoBehaviour
{
    [Header("Aim Settings")]
    [Tooltip("Transform của vũ khí/điểm xoay súng (nếu để trống sẽ tự xoay toàn bộ nhân vật)")]
    [SerializeField] private Transform aimTransform;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (aimTransform == null)
        {
            aimTransform = transform;
        }
    }

    private void Update()
    {
        RotateTowardsMouse();
    }

    private void RotateTowardsMouse()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // Lấy tọa độ chuột trên World space
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        mouseWorldPosition.z = 0f;

        // Tính vector hướng từ tâm súng/nhân vật đến chuột
        Vector3 direction = (mouseWorldPosition - aimTransform.position).normalized;

        // Tính góc xoay (độ)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Gán góc xoay trục Z
        aimTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Lấy hướng ngắm hiện tại để truyền cho đạn khi bắn (chuẩn xác 100% theo vị trí con trỏ chuột)
    /// </summary>
    public Vector2 GetAimDirection()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
            mouseWorldPosition.z = 0f;
            Vector2 dir = (mouseWorldPosition - aimTransform.position).normalized;
            if (dir != Vector2.zero) return dir;
        }
        return aimTransform.right;
    }
}

