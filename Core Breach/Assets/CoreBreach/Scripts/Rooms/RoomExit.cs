using UnityEngine;

/// <summary>
/// Cửa thoát (Exit) của phòng trong Core Breach:
/// Khi phòng được dọn sạch toàn bộ địch, cửa mở ra (active).
/// Khi Unit-7 bước vào cổng exit, kích hoạt chuyển sang phòng kế tiếp (hoặc phòng Boss sau mỗi 5 tầng).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomExit : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Khoảng cách tối thiểu hoặc hiệu ứng chuyển cảnh")]
    [SerializeField] private bool autoTriggerOnTouch = true;

    private RoomController parentRoomController;

    private void Awake()
    {
        parentRoomController = GetComponentInParent<RoomController>();
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!autoTriggerOnTouch) return;

        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            OnPlayerEnterExit();
        }
    }

    private void OnPlayerEnterExit()
    {
        Debug.Log("[RoomExit] Unit-7 đã bước vào cổng Exit! Tiến lên tầng tiếp theo.");

        // Thông báo cho GameManager tăng độ sâu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AdvanceDepth();
        }

        // Thông báo cho RoomManager chuyển phòng tiếp theo
        RoomManager roomManager = FindObjectOfType<RoomManager>();
        if (roomManager != null)
        {
            roomManager.CompleteRoom();
        }
        else if (parentRoomController != null)
        {
            parentRoomController.ResetAndStartNextWave();
        }
    }
}
