//<<<<<<< Updated upstream
//using System.Collections;
//using UnityEngine;

///// <summary>
///// Cổng dịch chuyển (Vault Portal) chuẩn phong cách Soul Knight:
///// - Chỉ kích hoạt khi Unit-7 thực sự bước vào sát tâm cổng (khoảng cách <= 0.65m).
///// - Thực hiện chuỗi hiệu ứng chuyển tầng điện ảnh (Warp Sequence - 0.55s):
/////   1. Khóa tạm thời điều khiển người chơi (tránh trượt ra ngoài).
/////   2. Lực hút xoáy lỗ sâu (Vortex Suction): Hút Unit-7 vào tâm cổng, xoay tròn và thu nhỏ kích thước (1.0 -> 0) trong 0.55s.
/////   3. Rung giật camera và phát âm thanh dịch chuyển siêu công nghệ (SFX_PortalWarp).
/////   4. Bùng nổ hạt năng lượng (Cyan/Magenta Sparks Burst) và lóe sáng toàn màn hình qua PlayerHUD.
/////   5. Tăng độ sâu Tầng (B2, B3...), đặt lại nhân vật tại Spawn Point, khôi phục kích thước 1.0 & điều khiển.
/////   6. Bật Wave Banner chúc mừng hoành tráng thông baáo tầng mới.
//=======
//using System.Diagnostics;
//using UnityEngine;

///// <summary>2
///// Cửa thoát (Exit) của phòng trong Core Breach:
///// Khi phòng được dọn sạch toàn bộ địch, cửa mở ra (active).
///// Khi Unit-7 bước vào cổng exit, kích hoạt chuyển sang phòng kế tiếp (hoặc phòng Boss sau mỗi 5 tầng).
//>>>>>>> Stashed changes
///// </summary>
//[RequireComponent(typeof(Collider2D))]
//public class RoomExit : MonoBehaviour
//{
//    [Header("Settings")]
//    [Tooltip("Khoảng cách kích hoạt tối đa tới tâm cổng")]
//    [SerializeField] private float activationDistance = 0.65f;
//    [Tooltip("Thời lượng hoạt ảnh hút vào cổng dịch chuyển")]
//    [SerializeField] private float warpDuration = 0.55f;

//    private RoomController parentRoomController;
//<<<<<<< Updated upstream
//    private bool isWarping = false;
//=======
//    private bool used = false; // chống kích hoạt nhiều lần (tăng Depth 2 lần)
//>>>>>>> Stashed changes

//    private void Awake()
//    {
//        parentRoomController = GetComponentInParent<RoomController>();
//        if (parentRoomController == null)
//        {
//            parentRoomController = FindFirstObjectByType<RoomController>();
//        }

//        Collider2D col = GetComponent<Collider2D>();
//        if (col != null)
//        {
//            col.isTrigger = true;
//            if (col is CircleCollider2D circleCol)
//            {
//                circleCol.radius = 0.35f;
//            }
//        }
//    }

//    private void OnEnable()
//    {
//<<<<<<< Updated upstream
//        isWarping = false;
//    }

//    private void OnTriggerStay2D(Collider2D collision)
//    {
//        if (isWarping) return;
//=======
//        if (!autoTriggerOnTouch || used) return;
//>>>>>>> Stashed changes

//        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
//        {
//            float dist = Vector2.Distance(collision.transform.position, transform.position);
//            if (dist <= activationDistance)
//            {
//                isWarping = true;

//                // Khởi chạy Coroutine trên GameManager để không bao giờ bị gián đoạn khi cổng Exit bị SetActive(false)
//                if (GameManager.Instance != null)
//                {
//                    GameManager.Instance.StartCoroutine(WarpPlayerSequenceRoutine(collision.gameObject));
//                }
//                else
//                {
//                    StartCoroutine(WarpPlayerSequenceRoutine(collision.gameObject));
//                }
//            }
//        }
//    }

//    private IEnumerator WarpPlayerSequenceRoutine(GameObject player)
//    {
//<<<<<<< Updated upstream
//        if (player == null)
//        {
//            isWarping = false;
//            yield break;
//        }
//=======
//        if (used) return;
//        used = true;

//        UnityEngine.Debug.Log("[RoomExit] Unit-7 đã bước vào cổng Exit! Tiến lên tầng tiếp theo.");
//>>>>>>> Stashed changes

//        PortalVisual portalVisual = GetComponent<PortalVisual>();
//        if (portalVisual != null)
//        {
//            portalVisual.IsWarping = true;
//        }

//        // 1. Tạm thời khóa di chuyển & ngắm bắn
//        PlayerMovement pm = player.GetComponent<PlayerMovement>();
//        if (pm != null) pm.enabled = false;

//        PlayerAim pa = player.GetComponent<PlayerAim>();
//        if (pa != null) pa.enabled = false;

//        WeaponController wc = player.GetComponent<WeaponController>();
//        if (wc != null) wc.enabled = false;

//        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
//        if (rb != null) rb.linearVelocity = Vector2.zero;

//        // 2. Phát âm thanh dịch chuyển & rung chấn camera
//        if (AudioManager.Instance != null)
//        {
//            AudioManager.Instance.PlayPortalWarp();
//        }
//        if (CameraFollow.Instance != null)
//        {
//            CameraFollow.Instance.Shake(0.22f, warpDuration);
//        }

//        // 3. Hiệu ứng hút vào hố đen năng lượng (Vortex Suction & Shrink)
//        Vector3 startPos = player.transform.position;
//        Vector3 portalCenter = transform.position;

//        float elapsed = 0f;
//        while (elapsed < warpDuration)
//        {
//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / warpDuration);
//            float easeInT = t * t; // Gia tốc hút mạnh dần vào tâm

//            // Kéo dần Unit-7 về tâm cổng
//            if (player != null)
//            {
//                player.transform.position = Vector3.Lerp(startPos, portalCenter, easeInT);
//                player.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, easeInT);
//                player.transform.Rotate(0f, 0f, -480f * Time.deltaTime);

//                if (CombatFeedback.Instance != null && Random.value > 0.4f)
//                {
//                    Vector2 dir = (portalCenter - player.transform.position).normalized;
//                    CombatFeedback.SpawnSparks(player.transform.position, dir, Color.cyan, 2);
//                }
//            }

//            yield return null;
//        }

//        // 4. Bùng nổ hạt năng lượng cực đại tại tâm cổng & lóe sáng toàn màn hình
//        PlayerHUD.FlashScreen(new Color(0.85f, 1f, 1f), 0.85f);
//        if (CombatFeedback.Instance != null)
//        {
//            CombatFeedback.SpawnSparks(portalCenter, Vector2.up, new Color(0f, 1f, 1f, 1f), 35);
//            CombatFeedback.SpawnSparks(portalCenter, Vector2.down, new Color(0.9f, 0.2f, 1f, 1f), 20);
//        }

//        // 5. KHÔI PHỤC KÍCH THƯỚC VÀ TRẠNG THÁI CHO NGƯỜI CHƠI (Tuyệt đối không để scale = 0)
//        if (player != null)
//        {
//            player.transform.localScale = Vector3.one;
//            player.transform.rotation = Quaternion.identity;
//            if (pm != null) pm.enabled = true;
//            if (pa != null) pa.enabled = true;
//            if (wc != null) wc.enabled = true;
//        }

//        if (portalVisual != null)
//        {
//            portalVisual.IsWarping = false;
//        }

//        int nextDepth = (GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1) + 1;
//        Debug.Log($"[RoomExit] Unit-7 hoàn tất dịch chuyển! Tiến vào tầng B{nextDepth}.");

//        // 6. Cập nhật độ sâu và giao diện
//        if (GameManager.Instance != null)
//        {
//            GameManager.Instance.AdvanceDepth();
//        }

//        PlayerHUD.ShowWaveBanner($"🚪 TIẾN VÀO TẦNG B{nextDepth}!", "KHU VỰC CHIẾN ĐẤU MỚI ĐÃ SẴN SÀNG", Color.cyan, 2.5f);

//        // 7. Chuyển phòng hoặc reset wave phòng mới (sẽ gọi PositionPlayerAtSpawn)
//        RoomManager roomManager = FindFirstObjectByType<RoomManager>();
//        if (roomManager != null)
//        {
//            roomManager.CompleteRoom();
//        }
//        else if (parentRoomController != null)
//        {
//            parentRoomController.ResetAndStartNextWave();
//        }

//        isWarping = false;
//    }
//}
using System.Collections;
using UnityEngine;

/// <summary>
/// Cổng dịch chuyển (Vault Portal) chuẩn phong cách Soul Knight:
/// - Chỉ kích hoạt khi Unit-7 thực sự bước vào sát tâm cổng (khoảng cách <= 0.65m).
/// - Thực hiện chuỗi hiệu ứng chuyển tầng điện ảnh (Warp Sequence - 0.55s):
///   1. Khóa tạm thời điều khiển người chơi (tránh trượt ra ngoài).
///   2. Lực hút xoáy lỗ sâu (Vortex Suction): Hút Unit-7 vào tâm cổng, xoay tròn và thu nhỏ kích thước (1.0 -> 0) trong 0.55s.
///   3. Rung giật camera và phát âm thanh dịch chuyển siêu công nghệ (SFX_PortalWarp).
///   4. Bùng nổ hạt năng lượng (Cyan/Magenta Sparks Burst) và lóe sáng toàn màn hình qua PlayerHUD.
///   5. Tăng độ sâu Tầng (B2, B3...), đặt lại nhân vật tại Spawn Point, khôi phục kích thước 1.0 & điều khiển.
///   6. Bật Wave Banner chúc mừng hoành tráng thông báo tầng mới.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomExit : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Khoảng cách kích hoạt tối đa tới tâm cổng")]
    [SerializeField] private float activationDistance = 0.65f;
    [Tooltip("Thời lượng hoạt ảnh hút vào cổng dịch chuyển")]
    [SerializeField] private float warpDuration = 0.55f;

    private RoomController parentRoomController;
    private bool isWarping = false;

    private void Awake()
    {
        parentRoomController = GetComponentInParent<RoomController>();
        if (parentRoomController == null)
        {
            parentRoomController = FindFirstObjectByType<RoomController>();
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            if (col is CircleCollider2D circleCol)
            {
                circleCol.radius = 0.35f;
            }
        }
    }

    private void OnEnable()
    {
        isWarping = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isWarping) return;

        if (collision.CompareTag("Player") || collision.GetComponent<PlayerMovement>() != null)
        {
            float dist = Vector2.Distance(collision.transform.position, transform.position);
            if (dist <= activationDistance)
            {
                isWarping = true;

                // Khởi chạy Coroutine trên GameManager để không bao giờ bị gián đoạn khi cổng Exit bị SetActive(false)
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.StartCoroutine(WarpPlayerSequenceRoutine(collision.gameObject));
                }
                else
                {
                    StartCoroutine(WarpPlayerSequenceRoutine(collision.gameObject));
                }
            }
        }
    }

    private IEnumerator WarpPlayerSequenceRoutine(GameObject player)
    {
        if (player == null)
        {
            isWarping = false;
            yield break;
        }

        Debug.Log("[RoomExit] Unit-7 đã bước vào cổng Exit! Tiến lên tầng tiếp theo.");

        PortalVisual portalVisual = GetComponent<PortalVisual>();
        if (portalVisual != null)
        {
            portalVisual.IsWarping = true;
        }

        // 1. Tạm thời khóa di chuyển & ngắm bắn
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;

        PlayerAim pa = player.GetComponent<PlayerAim>();
        if (pa != null) pa.enabled = false;

        WeaponController wc = player.GetComponent<WeaponController>();
        if (wc != null) wc.enabled = false;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. Phát âm thanh dịch chuyển & rung chấn camera
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPortalWarp();
        }
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.Shake(0.22f, warpDuration);
        }

        // 3. Hiệu ứng hút vào hố đen năng lượng (Vortex Suction & Shrink)
        Vector3 startPos = player.transform.position;
        Vector3 portalCenter = transform.position;

        float elapsed = 0f;
        while (elapsed < warpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / warpDuration);
            float easeInT = t * t; // Gia tốc hút mạnh dần vào tâm

            // Kéo dần Unit-7 về tâm cổng
            if (player != null)
            {
                player.transform.position = Vector3.Lerp(startPos, portalCenter, easeInT);
                player.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, easeInT);
                player.transform.Rotate(0f, 0f, -480f * Time.deltaTime);

                if (CombatFeedback.Instance != null && Random.value > 0.4f)
                {
                    Vector2 dir = (portalCenter - player.transform.position).normalized;
                    CombatFeedback.SpawnSparks(player.transform.position, dir, Color.cyan, 2);
                }
            }

            yield return null;
        }

        // 4. Bùng nổ hạt năng lượng cực đại tại tâm cổng & lóe sáng toàn màn hình
        PlayerHUD.FlashScreen(new Color(0.85f, 1f, 1f), 0.85f);
        if (CombatFeedback.Instance != null)
        {
            CombatFeedback.SpawnSparks(portalCenter, Vector2.up, new Color(0f, 1f, 1f, 1f), 35);
            CombatFeedback.SpawnSparks(portalCenter, Vector2.down, new Color(0.9f, 0.2f, 1f, 1f), 20);
        }

        // 5. KHÔI PHỤC KÍCH THƯỚC VÀ TRẠNG THÁI CHO NGƯỜI CHƠI (Tuyệt đối không để scale = 0)
        if (player != null)
        {
            player.transform.localScale = Vector3.one;
            player.transform.rotation = Quaternion.identity;
            if (pm != null) pm.enabled = true;
            if (pa != null) pa.enabled = true;
            if (wc != null) wc.enabled = true;
        }

        if (portalVisual != null)
        {
            portalVisual.IsWarping = false;
        }

        int nextDepth = (GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1) + 1;
        Debug.Log($"[RoomExit] Unit-7 hoàn tất dịch chuyển! Tiến vào tầng B{nextDepth}.");

        // 6. Cập nhật độ sâu và giao diện
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AdvanceDepth();
        }

        PlayerHUD.ShowWaveBanner($"🚪 TIẾN VÀO TẦNG B{nextDepth}!", "KHU VỰC CHIẾN ĐẤU MỚI ĐÃ SẴN SÀNG", Color.cyan, 2.5f);

        // 7. Chuyển phòng hoặc reset wave phòng mới (sẽ gọi PositionPlayerAtSpawn)
        RoomManager roomManager = FindFirstObjectByType<RoomManager>();
        if (roomManager != null)
        {
            roomManager.CompleteRoom();
        }
        else if (parentRoomController != null)
        {
            parentRoomController.ResetAndStartNextWave();
        }

        isWarping = false;
    }
}