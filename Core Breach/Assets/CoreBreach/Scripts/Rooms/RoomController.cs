using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý chu trình hoạt động của một phòng chiến đấu trong Core Breach:
/// - Đặt vị trí Unit-7 (Player) tại PlayerSpawn
/// - Tạo quái tại các EnemySpawnPoints (Sentry Drone, Turret Bot, Boss...)
/// - Theo dõi số lượng quái còn sống
/// - Mở cửa Exit khi đã tiêu diệt toàn bộ kẻ địch
/// </summary>
public class RoomController : MonoBehaviour
{
    [Header("Room Info")]
    [SerializeField] private string roomName = "Combat Room";

    [Header("Player Spawn")]
    [Tooltip("Điểm xuất phát của Unit-7 trong phòng")]
    [SerializeField] private Transform playerSpawnPoint;
    [Tooltip("Prefab Unit-7 nếu cần spawn mới khi vào phòng")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Enemy Spawn Points")]
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Exit")]
    [SerializeField] private GameObject exit;

    [Header("Debug / Testing")]
    [Tooltip("Bật để room tự động bắt đầu khi Play (dùng để test, tắt khi dùng RoomManager)")]
    [SerializeField] private bool autoStartOnPlay = true;

    // Danh sách enemy đang tồn tại trong room
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    // Room đã bắt đầu chưa?
    private bool roomStarted = false;

    // Room đã hoàn thành chưa?
    private bool roomCompleted = false;

    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public string RoomName => roomName;

    // =========================================================
    // UNITY LIFECYCLE
    // =========================================================

    private void Start()
    {
        if (autoStartOnPlay)
        {
            StartRoom();
        }
    }

    // =========================================================
    // START ROOM
    // =========================================================

    public void StartRoom()
    {
        if (roomStarted)
        {
            Debug.LogWarning("[RoomController] Room đã bắt đầu trước đó: " + roomName);
            return;
        }

        roomStarted = true;
        roomCompleted = false;

        Debug.Log($"[RoomController] Bắt đầu phòng: {roomName}");

        // Định vị hoặc spawn Player tại PlayerSpawnPoint
        PositionPlayerAtSpawn();

        // Khóa Exit khi bắt đầu room
        CloseExit();

        // Spawn enemy
        SpawnEnemies();
    }

    public void ResetAndStartNextWave()
    {
        roomStarted = false;
        roomCompleted = false;

        // Xóa sạch enemy cũ nếu còn sót
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] != null) Destroy(spawnedEnemies[i]);
        }
        spawnedEnemies.Clear();

        StartRoom();
    }

    // =========================================================
    // PLAYER POSITIONING / SPAWN
    // =========================================================

    private void PositionPlayerAtSpawn()
    {
        Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : transform.position;

        // Tìm Player đã có trong Scene
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer == null)
        {
            PlayerMovement pm = FindObjectOfType<PlayerMovement>();
            if (pm != null) existingPlayer = pm.gameObject;
        }

        if (existingPlayer != null)
        {
            existingPlayer.transform.position = spawnPos;
            Rigidbody2D rb = existingPlayer.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            Debug.Log($"[RoomController] Đã di chuyển Unit-7 tới vị trí Spawn: {spawnPos}");
        }
        else if (playerPrefab != null)
        {
            GameObject newPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            newPlayer.name = "Player (Unit-7)";
            Debug.Log($"[RoomController] Đã tạo mới Unit-7 tại: {spawnPos}");
        }

        // Cập nhật Camera bám theo Player
        CameraFollow camFollow = FindObjectOfType<CameraFollow>();
        if (camFollow != null)
        {
            GameObject p = existingPlayer ?? GameObject.FindGameObjectWithTag("Player");
            if (p != null) camFollow.SetTarget(p.transform);
        }
    }

    // =========================================================
    // SPAWN ENEMIES
    // =========================================================

    private void SpawnEnemies()
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogWarning("[RoomController] Không có enemy spawn points trong: " + roomName);
            CheckRoomCompleted();
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[RoomController] Không có enemy prefabs trong: " + roomName);
            CheckRoomCompleted();
            return;
        }

        for (int i = 0; i < enemySpawnPoints.Length; i++)
        {
            Transform spawnPoint = enemySpawnPoints[i];
            if (spawnPoint == null) continue;

            // Phân bổ enemy prefab theo danh sách
            GameObject enemyPrefab = enemyPrefabs[i % enemyPrefabs.Length];
            if (enemyPrefab == null) continue;

            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity, transform);
            spawnedEnemies.Add(enemy);
        }

        Debug.Log($"[RoomController] Đã spawn {spawnedEnemies.Count} kẻ địch trong {roomName}");
    }

    // =========================================================
    // UPDATE & COMPLETION CHECK
    // =========================================================

    private void Update()
    {
        if (!roomStarted || roomCompleted) return;

        CheckRoomCompleted();
    }

    private void CheckRoomCompleted()
    {
        // Loại bỏ các enemy đã bị tiêu diệt (null)
        spawnedEnemies.RemoveAll(enemy => enemy == null);

        if (spawnedEnemies.Count > 0) return;

        // Toàn bộ kẻ địch đã bị tiêu diệt
        CompleteRoom();
    }

    private void CompleteRoom()
    {
        if (roomCompleted) return;
        roomCompleted = true;

        Debug.Log($"[RoomController] PHÒNG HOÀN THÀNH: {roomName}! Mở cửa Exit.");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRoomClear();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(50);
        }

        OpenExit();
    }

    // =========================================================
    // EXIT
    // =========================================================

    private void OpenExit()
    {
        if (exit == null)
        {
            Debug.LogWarning("[RoomController] Exit chưa được gán trong: " + roomName);
            return;
        }

        exit.SetActive(true);

        // Đảm bảo Exit có Collider2D và RoomExit script để đón Player
        if (exit.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = exit.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1.0f;
        }
        if (exit.GetComponent<RoomExit>() == null)
        {
            exit.AddComponent<RoomExit>();
        }

        Debug.Log("[RoomController] Cửa Exit đã mở và sẵn sàng!");
    }

    private void CloseExit()
    {
        if (exit == null) return;
        exit.SetActive(false);
    }

    // =========================================================
    // PUBLIC GETTERS
    // =========================================================

    public bool IsRoomStarted() => roomStarted;
    public bool IsRoomCompleted() => roomCompleted;
    public int GetRemainingEnemyCount()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        return spawnedEnemies.Count;
    }
}
