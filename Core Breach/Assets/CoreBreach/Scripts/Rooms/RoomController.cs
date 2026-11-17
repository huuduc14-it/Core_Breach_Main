//<<<<<<< Updated upstream
//using System.Collections;
//=======
//using System;
//>>>>>>> Stashed changes
//using System.Collections.Generic;
//using System.Diagnostics;
//using UnityEngine;

///// <summary>
///// Quản lý chu trình hoạt động của một phòng chiến đấu trong Core Breach chuẩn phong cách Soul Knight:
///// - Hệ thống Multi-Wave Combat: Đánh hết đợt 1 -> Thông báo chiến đấu -> Đợt 2 -> Đợt 3 (Final Wave) -> Mở Vault Portal
///// - Hỗ trợ đầy đủ 4 chủng loại quái: Sentry Drone, Turret Bot, Kamikaze Bot, Sniper Mech (+ Boss B5)
///// - Quản lý hiệu năng: Staggered Spawn mượt mà, dọn rác bộ nhớ tự động
///// - Thưởng Scrap và Điểm chiến dịch khi vượt qua các đợt quái
///// </summary>
//public class RoomController : MonoBehaviour
//{
//    [Header("Room Info")]
//    [SerializeField] private string roomName = "Combat Room (Vault Sector)";

//    [Header("Player Spawn")]
//    [Tooltip("Điểm xuất phát của Unit trong phòng")]
//    [SerializeField] private Transform playerSpawnPoint;
//    [SerializeField] private GameObject playerPrefab;

//    [Header("Enemy Spawn Points")]
//    [SerializeField] private Transform[] enemySpawnPoints;

//    [Header("Enemy Prefabs")]
//    [SerializeField] private GameObject[] enemyPrefabs;
//    [SerializeField] private GameObject kamikazeBotPrefab;
//    [SerializeField] private GameObject sniperMechPrefab;
//    [SerializeField] private GameObject bossPrefab;

//    [Header("Wave Configuration (Soul Knight Style)")]
//    [Tooltip("Số đợt quái trong phòng (tự động điều chỉnh theo độ sâu Depth)")]
//    [SerializeField] private int totalWaves = 2;
//    [SerializeField] private float waveInterval = 0.9f; // Thời gian nghỉ giữa 2 wave chiến thuật

//    [Tooltip("Số kẻ địch tối đa trong một phòng khi Depth tăng")]
//    [SerializeField] private int maxEnemies = 8;

//    [Header("Exit")]
//    [SerializeField] private GameObject exit;

//    [Header("Debug / Testing")]
//    [SerializeField] private bool autoStartOnPlay = true;

//    // Trạng thái Wave
//    private int currentWaveIndex = 0;
//    private bool isWaveInProgress = false;
//    private bool isWaveTransitioning = false;

//    // Danh sách enemy đang tồn tại trong wave hiện tại
//    private List<GameObject> spawnedEnemies = new List<GameObject>();

//    private bool roomStarted = false;
//    private bool roomCompleted = false;

//    public Transform PlayerSpawnPoint => playerSpawnPoint;
//    public string RoomName => roomName;
//    public int CurrentWave => currentWaveIndex;
//    public int TotalWaves => totalWaves;
//    public bool IsWaveInProgress => isWaveInProgress;
//    public bool IsWaveTransitioning => isWaveTransitioning;

//    private void Start()
//    {
//<<<<<<< Updated upstream
//        // Chờ người chơi chọn nhân vật xong nếu menu chọn nhân vật đang mở
//        if (CharacterSelectSystem.Instance != null && CharacterSelectSystem.Instance.IsSelectionOpen)
//        {
//            return;
//        }

//        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
//        {
//            return;
//        }

//        if (autoStartOnPlay)
//=======
//        // RoomManager đã gọi StartRoom() ngay sau Instantiate nên cần kiểm tra roomStarted
//        if (autoStartOnPlay && !roomStarted)
//>>>>>>> Stashed changes
//        {
//            StartRoom();
//        }
//    }

//    // =========================================================
//    // START ROOM & MULTI-WAVE LOOP
//    // =========================================================

//    public void StartRoom()
//    {
//        if (roomStarted)
//        {
//            UnityEngine.Debug.LogWarning("[RoomController] Room đã bắt đầu trước đó: " + roomName);
//            return;
//        }

//        roomStarted = true;
//        roomCompleted = false;
//        currentWaveIndex = 0;
//        isWaveInProgress = false;
//        isWaveTransitioning = false;

//<<<<<<< Updated upstream
//        // Định cấu hình số đợt quái dựa theo độ sâu Depth
//        int depth = GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1;
//        bool isBossLevel = depth > 0 && depth % 5 == 0;
//=======
//        UnityEngine.Debug.Log($"[RoomController] Bắt đầu phòng: {roomName}");
//>>>>>>> Stashed changes

//        if (isBossLevel)
//        {
//            totalWaves = 1; // Tầng Boss là 1 đợt Boss hoành tráng
//        }
//        else
//        {
//            totalWaves = (depth <= 1) ? 2 : 3; // Tầng 1: 2 đợt; Tầng 2+: 3 đợt dồn dập
//        }

//        Debug.Log($"[RoomController] Bắt đầu phòng: {roomName} | Độ sâu B{depth} | Tổng số Wave: {totalWaves}");

//        // Định vị Player
//        PositionPlayerAtSpawn();

//        // Khóa cửa Exit
//        CloseExit();

//        // Nạp đầy đủ các loại quái mới nếu chưa gán Inspector
//        EnsureEnemyDatabase();

//        // Bắt đầu chuỗi Wave
//        StartCoroutine(MultiWaveMasterRoutine(isBossLevel));
//    }

//    public void ResetAndStartNextWave()
//    {
//        roomStarted = false;
//        roomCompleted = false;

//        // Xóa sạch enemy cũ nếu còn sót
//        for (int i = 0; i < spawnedEnemies.Count; i++)
//        {
//            if (spawnedEnemies[i] != null) Destroy(spawnedEnemies[i]);
//        }
//        spawnedEnemies.Clear();

//        StartRoom();
//    }

//    private void EnsureEnemyDatabase()
//    {
//        // Tải quái mới từ Resources / Prefabs nếu thiếu
//        if (kamikazeBotPrefab == null) kamikazeBotPrefab = Resources.Load<GameObject>("Enemies/KamikazeBot");
//        if (kamikazeBotPrefab == null) kamikazeBotPrefab = Resources.Load<GameObject>("KamikazeBot");

//        if (sniperMechPrefab == null) sniperMechPrefab = Resources.Load<GameObject>("Enemies/SniperMech");
//        if (sniperMechPrefab == null) sniperMechPrefab = Resources.Load<GameObject>("SniperMech");

//        if (bossPrefab == null) bossPrefab = Resources.Load<GameObject>("CoreGuardianBoss");
//    }

//    // =========================================================
//    // MULTI-WAVE MASTER COROUTINE
//    // =========================================================

//    private IEnumerator MultiWaveMasterRoutine(bool isBossLevel)
//    {
//        // 1. NẾU LÀ TẦNG BOSS
//        if (isBossLevel)
//        {
//            currentWaveIndex = 1;
//            Vector3 center = transform.position + new Vector3(0f, 2f, 0f);

//            CombatFeedback.ShowNotice(center, "🚨 WARNING: CORE GUARDIAN BOSS AWAKENS! 🚨", Color.red);
//            if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.2f, 0.25f);
//            if (AudioManager.Instance != null) AudioManager.Instance.PlayBossRoar();

//            yield return new WaitForSeconds(1.2f);

//            if (bossPrefab != null)
//            {
//                CombatFeedback.SpawnSparks(center, Vector2.up, Color.red, 25);
//                GameObject boss = Instantiate(bossPrefab, center, Quaternion.identity, transform);
//                spawnedEnemies.Add(boss);

//                EnemyHealth bHp = boss.GetComponent<EnemyHealth>();
//                if (bHp != null)
//                {
//                    bHp.OnDeath += () => OnSingleEnemyDied(boss);
//                }
//            }
//<<<<<<< Updated upstream

//            // Chờ tiêu diệt Boss
//            yield return new WaitUntil(() => GetRemainingEnemyCount() == 0);

//            CompleteRoom();
//            yield break;
//=======
//            UnityEngine.Debug.Log($"[RoomController] Đã di chuyển Unit-7 tới vị trí Spawn: {spawnPos}");
//>>>>>>> Stashed changes
//        }

//        // 2. PHÒNG THƯỜNG: VÒNG LẶP CÁC ĐỢT QUÁI (WAVE 1 -> WAVE 2 -> WAVE 3)
//        while (currentWaveIndex < totalWaves)
//        {
//<<<<<<< Updated upstream
//            currentWaveIndex++;
//            isWaveInProgress = true;
//            isWaveTransitioning = false;

//            // Thông báo bắt đầu Wave mới trên màn hình (Soul Knight Screen Banner)
//            string waveTitle = currentWaveIndex == totalWaves 
//                ? $"🔥 FINAL WAVE [{currentWaveIndex}/{totalWaves}] - ĐỢT TỔNG LỰC! 🔥" 
//                : $"⚠️ WAVE [{currentWaveIndex}/{totalWaves}] - BẮT ĐẦU XÂM NHẬP! ⚠️";

//            string waveSubtitle = currentWaveIndex == totalWaves
//                ? "CẢNH BÁO: ROBOT XẠ THỦ & CẢM TỬ XUẤT HIỆN"
//                : "QUÉT SẠCH KẺ ĐỊCH ĐỂ MỞ KHÓA TẦNG TIẾP THEO";

//            Color waveColor = currentWaveIndex == totalWaves ? new Color(1f, 0.25f, 0.35f) : new Color(1f, 0.82f, 0.15f);

//            PlayerHUD.ShowWaveBanner(waveTitle, waveSubtitle, waveColor, 2.2f);

//            if (AudioManager.Instance != null)
//            {
//                AudioManager.Instance.PlayShoot();
//            }
//            if (CameraFollow.Instance != null)
//            {
//                CameraFollow.Instance.Shake(0.08f, 0.12f);
//            }

//            yield return new WaitForSeconds(0.35f);

//            // Sinh quái cho Wave hiện tại với hiệu ứng Warp mượt mà không giật
//            yield return StartCoroutine(SpawnEnemiesForWave(currentWaveIndex));

//            // Chờ cho đến khi toàn bộ quái trong wave bị tiêu diệt
//            yield return new WaitUntil(() => GetRemainingEnemyCount() == 0);

//            isWaveInProgress = false;

//            // Nếu còn Wave tiếp theo
//            if (currentWaveIndex < totalWaves)
//            {
//                isWaveTransitioning = true;
//                PlayerHUD.ShowWaveBanner(
//                    $"✓ HOÀN THÀNH WAVE {currentWaveIndex}!", 
//                    "CHUẨN BỊ CHIẾN ĐẤU ĐỢT TIẾP THEO...", 
//                    Color.green, 
//                    1.2f
//                );

//                if (AudioManager.Instance != null)
//                {
//                    AudioManager.Instance.PlayScrapCollect();
//                }

//                // Thưởng nhẹ điểm & thời gian nghỉ chiến thuật ngắn (0.9s)
//                if (GameManager.Instance != null) GameManager.Instance.AddScore(25);

//                yield return new WaitForSeconds(waveInterval);
//            }
//        }

//        // Toàn bộ các đợt quái đã bị quét sạch hoàn toàn!
//=======
//            GameObject newPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
//            newPlayer.name = "Player (Unit-7)";
//            UnityEngine.Debug.Log($"[RoomController] Đã tạo mới Unit-7 tại: {spawnPos}");
//        }

//        // Cập nhật Camera bám theo Player
//        CameraFollow camFollow = FindObjectOfType<CameraFollow>();
//        if (camFollow != null)
//        {
//            GameObject p = existingPlayer ?? GameObject.FindGameObjectWithTag("Player");
//            if (p != null) camFollow.SetTarget(p.transform);
//        }
//    }

//    // =========================================================
//    // SPAWN ENEMIES
//    // =========================================================

//    private void SpawnEnemies()
//    {
//        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
//        {
//            UnityEngine.Debug.LogWarning("[RoomController] Không có enemy spawn points trong: " + roomName);
//            CheckRoomCompleted();
//            return;
//        }

//        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
//        {
//            UnityEngine.Debug.LogWarning("[RoomController] Không có enemy prefabs trong: " + roomName);
//            CheckRoomCompleted();
//            return;
//        }

//        // Độ khó tăng theo Depth: cứ 2 tầng thêm 1 kẻ địch (tối đa maxEnemies)
//        int depth = GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1;
//        int total = Mathf.Min(enemySpawnPoints.Length + Mathf.Max(0, depth - 1) / 2, maxEnemies);
//        total = Mathf.Max(total, 1);

//        for (int i = 0; i < total; i++)
//        {
//            Transform spawnPoint = enemySpawnPoints[i % enemySpawnPoints.Length];
//            if (spawnPoint == null) continue;

//            // Phân bổ enemy prefab theo danh sách
//            GameObject enemyPrefab = enemyPrefabs[i % enemyPrefabs.Length];
//            if (enemyPrefab == null) continue;

//            // Quái thêm (vượt số spawn point) được lệch vị trí để không chồng lên nhau
//            Vector3 offset = i < enemySpawnPoints.Length ? Vector3.zero : (Vector3)(UnityEngine.Random.insideUnitCircle * 1.5f);
//            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position + offset, Quaternion.identity, transform);
//            spawnedEnemies.Add(enemy);
//        }

//        UnityEngine.Debug.Log($"[RoomController] Đã spawn {spawnedEnemies.Count} kẻ địch trong {roomName}");
//    }

//    // =========================================================
//    // UPDATE & COMPLETION CHECK
//    // =========================================================

//    private void Update()
//    {
//        if (!roomStarted || roomCompleted) return;

//        CheckRoomCompleted();
//    }

//    private void CheckRoomCompleted()
//    {
//        // Loại bỏ các enemy đã bị tiêu diệt (null)
//        spawnedEnemies.RemoveAll(enemy => enemy == null);

//        if (spawnedEnemies.Count > 0) return;

//        // Toàn bộ kẻ địch đã bị tiêu diệt
//>>>>>>> Stashed changes
//        CompleteRoom();
//    }

//    private IEnumerator SpawnEnemiesForWave(int waveNum)
//    {
//        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0) yield break;

//        // Xây dựng danh sách quái cho đợt hiện tại
//        List<GameObject> wavePool = BuildEnemyPoolForWave(waveNum);
//        if (wavePool.Count == 0) yield break;

//        int spawnCount = Mathf.Clamp(3 + waveNum * 2, 3, 10);
//        List<Vector3> spawnPositions = new List<Vector3>(spawnCount);

//        // 1. Tính toán toạ độ và Bắn tia sáng/Warp rune cảnh báo trước tại tất cả vị trí (Telegraphing)
//        for (int i = 0; i < spawnCount; i++)
//        {
//            Transform sp = enemySpawnPoints[i % enemySpawnPoints.Length];
//            Vector3 basePos = sp != null ? sp.position : transform.position;
//            Vector3 jitter = (Vector3)(Random.insideUnitCircle * (i >= enemySpawnPoints.Length ? 1.2f : 0.3f));
//            Vector3 pos = basePos + jitter;
//            spawnPositions.Add(pos);

//            // Tia sáng màu xanh cảnh báo xuất hiện
//            CombatFeedback.SpawnSparks(pos, Vector2.up, new Color(0.1f, 0.95f, 1f, 0.9f), 6);
//        }

//        // Chờ 0.35s cho người chơi nhận biết vị trí quái sắp hiện (chuẩn Soul Knight)
//        yield return new WaitForSeconds(0.35f);

//        // 2. Xuất hiện đồng loạt toàn bộ quái vật không bị ngắt quãng giật khung hình
//        for (int i = 0; i < spawnCount; i++)
//        {
//            GameObject prefabToSpawn = wavePool[i % wavePool.Count];
//            if (prefabToSpawn == null) continue;

//            Vector3 pos = spawnPositions[i];

//            GameObject enemy = Instantiate(prefabToSpawn, pos, Quaternion.identity, transform);
//            spawnedEnemies.Add(enemy);

//            // Bắn hiệu ứng nổ năng lượng tại vị trí quái xuất hiện
//            CombatFeedback.SpawnSparks(pos, Vector2.up, new Color(1f, 0.8f, 0.2f), 8);

//            // Hoạt ảnh Pop-in nhanh (Scale 0 -> 1.15 -> 1.0)
//            StartCoroutine(EnemyPopInRoutine(enemy));

//            EnemyHealth hp = enemy.GetComponent<EnemyHealth>();
//            if (hp != null)
//            {
//                hp.OnDeath += () => OnSingleEnemyDied(enemy);
//            }
//        }

//        Debug.Log($"[RoomController] Đã xuất trận {spawnedEnemies.Count} kẻ địch cho Wave {waveNum}/{totalWaves}");
//    }

//    private IEnumerator EnemyPopInRoutine(GameObject enemy)
//    {
//        if (enemy == null) yield break;
//        Transform tr = enemy.transform;
//        Vector3 origScale = tr.localScale;
//        tr.localScale = Vector3.zero;

//        float elapsed = 0f;
//        float duration = 0.20f;
//        while (elapsed < duration)
//        {
//            if (enemy == null) yield break;
//            elapsed += Time.deltaTime;
//            float t = elapsed / duration;
//            float scaleMult = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.12f;
//            tr.localScale = origScale * scaleMult;
//            yield return null;
//        }

//        if (enemy != null) tr.localScale = origScale;
//    }

//    private List<GameObject> BuildEnemyPoolForWave(int waveNum)
//    {
//        List<GameObject> pool = new List<GameObject>();

//        // Lấy danh sách cơ bản từ Inspector nếu có
//        if (enemyPrefabs != null)
//        {
//            for (int i = 0; i < enemyPrefabs.Length; i++)
//            {
//                if (enemyPrefabs[i] != null) pool.Add(enemyPrefabs[i]);
//            }
//        }

//        // Bổ sung quái mới theo từng Wave
//        if (waveNum == 1)
//        {
//            // Wave 1: Quái trinh sát cơ bản
//            if (kamikazeBotPrefab != null && Random.value > 0.5f) pool.Add(kamikazeBotPrefab);
//        }
//        else if (waveNum == 2)
//        {
//            // Wave 2: Thêm quái cảm tử Kamikaze
//            if (kamikazeBotPrefab != null)
//            {
//                pool.Add(kamikazeBotPrefab);
//                pool.Add(kamikazeBotPrefab);
//            }
//            if (sniperMechPrefab != null && Random.value > 0.4f) pool.Add(sniperMechPrefab);
//        }
//        else
//        {
//            // Wave 3 (Final Wave): Đội hình tinh anh tổng lực có Sniper Mech & Kamikaze
//            if (sniperMechPrefab != null) pool.Add(sniperMechPrefab);
//            if (kamikazeBotPrefab != null) pool.Add(kamikazeBotPrefab);
//        }

//        return pool;
//    }

//    private void OnSingleEnemyDied(GameObject enemy)
//    {
//        spawnedEnemies.Remove(enemy);
//    }

//    // =========================================================
//    // ROOM COMPLETION & REWARD
//    // =========================================================

//    private void CompleteRoom()
//    {
//        if (roomCompleted) return;
//        roomCompleted = true;

//<<<<<<< Updated upstream
//        Debug.Log($"[RoomController] PHÒNG HOÀN THÀNH TOÀN BỘ {totalWaves} WAVES: {roomName}! Mở cửa Exit.");

//        PlayerHUD.ShowWaveBanner(
//            "🏆 TOÀN BỘ ĐỢT QUÁI ĐÃ BỊ QUÉT SẠCH!", 
//            "CỔNG DỊCH CHUYỂN VAULT ĐÃ MỞ - BƯỚC VÀO ĐỂ TIẾN LÊN TẦNG TIẾP THEO", 
//            Color.cyan, 
//            3.5f
//        );
//=======
//        UnityEngine.Debug.Log($"[RoomController] PHÒNG HOÀN THÀNH: {roomName}! Mở cửa Exit.");
//>>>>>>> Stashed changes

//        if (AudioManager.Instance != null)
//        {
//            AudioManager.Instance.PlayRoomClear();
//        }

//        if (GameManager.Instance != null)
//        {
//            GameManager.Instance.AddScore(100);
//            GameManager.Instance.AddScrap(10 + totalWaves * 5); // Thưởng Scrap lớn khi hoàn thành mọi đợt quái
//        }

//        OpenExit();
//    }

//    // =========================================================
//    // EXIT & PLAYER POSITIONING
//    // =========================================================

//    private void PositionPlayerAtSpawn()
//    {
//        Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : transform.position;

//        PlayerMovement existingPm = PlayerMovement.Instance;
//        GameObject existingPlayer = existingPm != null ? existingPm.gameObject : GameObject.FindGameObjectWithTag("Player");

//        if (existingPlayer != null)
//        {
//            existingPlayer.transform.position = spawnPos;
//            existingPlayer.transform.localScale = Vector3.one;
//            existingPlayer.transform.rotation = Quaternion.identity;

//            PlayerMovement pm = existingPlayer.GetComponent<PlayerMovement>();
//            if (pm != null) pm.enabled = true;

//            PlayerAim pa = existingPlayer.GetComponent<PlayerAim>();
//            if (pa != null) pa.enabled = true;

//            WeaponController wc = existingPlayer.GetComponent<WeaponController>();
//            if (wc != null) wc.enabled = true;

//            Rigidbody2D rb = existingPlayer.GetComponent<Rigidbody2D>();
//            if (rb != null) rb.linearVelocity = Vector2.zero;

//            // Bắn chùm tia năng lượng báo hiệu Unit-7 xuất hiện tại điểm Spawn tầng mới
//            CombatFeedback.SpawnSparks(spawnPos, Vector2.up, Color.cyan, 20);
//        }
//        else if (playerPrefab != null)
//        {
//            GameObject newPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
//            newPlayer.name = "Player (Unit-7)";
//            newPlayer.transform.localScale = Vector3.one;
//        }

//        CameraFollow camFollow = CameraFollow.Instance != null ? CameraFollow.Instance : FindFirstObjectByType<CameraFollow>();
//        if (camFollow != null)
//        {
//            GameObject p = existingPlayer ?? GameObject.FindGameObjectWithTag("Player");
//            if (p != null) camFollow.SetTarget(p.transform);
//        }
//    }

//    private void OpenExit()
//    {
//        if (exit == null)
//        {
//<<<<<<< Updated upstream
//            // Tự động tìm Exit trong hierarchy nếu chưa gán
//            Transform exitTr = transform.Find("Exit");
//            if (exitTr != null) exit = exitTr.gameObject;
//        }

//        if (exit == null)
//        {
//            Debug.LogWarning("[RoomController] Exit chưa được gán trong: " + roomName);
//=======
//            UnityEngine.Debug.LogWarning("[RoomController] Exit chưa được gán trong: " + roomName);
//>>>>>>> Stashed changes
//            return;
//        }

//        // Đặt vị trí Portal ngay trong tầm mắt của người chơi tại trung tâm phía trên phòng
//        exit.transform.position = transform.position + new Vector3(0f, 2.2f, 0f);
//        exit.SetActive(true);

//        if (exit.GetComponent<PortalVisual>() == null)
//        {
//            exit.AddComponent<PortalVisual>();
//        }
//        if (exit.GetComponent<Collider2D>() == null)
//        {
//            CircleCollider2D col = exit.AddComponent<CircleCollider2D>();
//            col.isTrigger = true;
//            col.radius = 0.35f;
//        }
//        if (exit.GetComponent<RoomExit>() == null)
//        {
//            exit.AddComponent<RoomExit>();
//        }

//<<<<<<< Updated upstream
//        Debug.Log("[RoomController] Cửa Exit Portal đã mở và sẵn sàng tại vị trí trung tâm!");
//=======
//        UnityEngine.Debug.Log("[RoomController] Cửa Exit đã mở và sẵn sàng!");
//>>>>>>> Stashed changes
//    }

//    private void CloseExit()
//    {
//        if (exit == null) return;
//        exit.SetActive(false);
//    }

//    // =========================================================
//    // PUBLIC GETTERS
//    // =========================================================

//    public bool IsRoomStarted() => roomStarted;
//    public bool IsRoomCompleted() => roomCompleted;
//    public int GetRemainingEnemyCount()
//    {
//        spawnedEnemies.RemoveAll(enemy => enemy == null);
//        return spawnedEnemies.Count;
//    }
//}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Quản lý chu trình hoạt động của một phòng chiến đấu trong Core Breach chuẩn phong cách Soul Knight:
/// - Hệ thống Multi-Wave Combat: Đánh hết đợt 1 -> Thông báo chiến đấu -> Đợt 2 -> Đợt 3 (Final Wave) -> Mở Vault Portal
/// - Hỗ trợ đầy đủ 4 chủng loại quái: Sentry Drone, Turret Bot, Kamikaze Bot, Sniper Mech (+ Boss B5)
/// - Quản lý hiệu năng: Staggered Spawn mượt mà, dọn rác bộ nhớ tự động
/// - Thưởng Scrap và Điểm chiến dịch khi vượt qua các đợt quái
/// </summary>
public class RoomController : MonoBehaviour
{
    [Header("Room Info")]
    [SerializeField] private string roomName = "Combat Room (Vault Sector)";

    [Header("Player Spawn")]
    [Tooltip("Điểm xuất phát của Unit trong phòng")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameObject playerPrefab;

    [Header("Enemy Spawn Points")]
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject kamikazeBotPrefab;
    [SerializeField] private GameObject sniperMechPrefab;
    [SerializeField] private GameObject bossPrefab;

    [Header("Wave Configuration (Soul Knight Style)")]
    [Tooltip("Số đợt quái trong phòng (tự động điều chỉnh theo độ sâu Depth)")]
    [SerializeField] private int totalWaves = 2;
    [SerializeField] private float waveInterval = 0.9f; // Thời gian nghỉ giữa 2 wave chiến thuật

    [Tooltip("Số kẻ địch tối đa trong một phòng khi Depth tăng")]
    [SerializeField] private int maxEnemies = 8;

    [Header("Exit")]
    [SerializeField] private GameObject exit;
    [SerializeField] private Transform exitSpawnPoint;
    [Header("Debug / Testing")]
    [SerializeField] private bool autoStartOnPlay = true;

    // Trạng thái Wave
    private int currentWaveIndex = 0;
    private bool isWaveInProgress = false;
    private bool isWaveTransitioning = false;

    // Danh sách enemy đang tồn tại trong wave hiện tại
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private bool roomStarted = false;
    private bool roomCompleted = false;
    private bool isBossRoom = false;

    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public string RoomName => roomName;
    public int CurrentWave => currentWaveIndex;
    public int TotalWaves => totalWaves;
    public bool IsWaveInProgress => isWaveInProgress;
    public bool IsWaveTransitioning => isWaveTransitioning;

    private void Start()
    {
        // Chờ người chơi chọn nhân vật xong nếu menu chọn nhân vật đang mở
        if (CharacterSelectSystem.Instance != null && CharacterSelectSystem.Instance.IsSelectionOpen)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        // RoomManager đã gọi StartRoom() ngay sau Instantiate nên cần kiểm tra !roomStarted
        if (autoStartOnPlay && !roomStarted)
        {
            StartRoom();
        }
    }

    // =========================================================
    // START ROOM & MULTI-WAVE LOOP
    // =========================================================

    public void StartRoom()
    {
        if (roomStarted)
        {
            UnityEngine.Debug.LogWarning("[RoomController] Room đã bắt đầu trước đó: " + roomName);
            return;
        }

        roomStarted = true;
        roomCompleted = false;
        currentWaveIndex = 0;
        isWaveInProgress = false;
        isWaveTransitioning = false;

        // Định cấu hình số đợt quái dựa theo độ sâu Depth
        int depth = GameManager.Instance != null ? GameManager.Instance.CurrentDepth : 1;
        isBossRoom = depth > 0 && depth % 4 == 0;

        if (isBossRoom)
        {
            totalWaves = 1; // Tầng Boss là 1 đợt Boss hoành tráng
        }
        else
        {
            totalWaves = (depth <= 1) ? 2 : 3; // Tầng 1: 2 đợt; Tầng 2+: 3 đợt dồn dập
        }

        UnityEngine.Debug.Log($"[RoomController] Bắt đầu phòng: {roomName} | Độ sâu B{depth} | Tổng số Wave: {totalWaves}");

        // Định vị Player
        PositionPlayerAtSpawn();

        // Khóa cửa Exit
        CloseExit();

        // Nạp đầy đủ các loại quái mới nếu chưa gán Inspector
        EnsureEnemyDatabase();

        // Bắt đầu chuỗi Wave
        StartCoroutine(MultiWaveMasterRoutine(isBossRoom));
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

    private void EnsureEnemyDatabase()
    {
        // Tải quái mới từ Resources / Prefabs nếu thiếu
        if (kamikazeBotPrefab == null) kamikazeBotPrefab = Resources.Load<GameObject>("Enemies/KamikazeBot");
        if (kamikazeBotPrefab == null) kamikazeBotPrefab = Resources.Load<GameObject>("KamikazeBot");

        if (sniperMechPrefab == null) sniperMechPrefab = Resources.Load<GameObject>("Enemies/SniperMech");
        if (sniperMechPrefab == null) sniperMechPrefab = Resources.Load<GameObject>("SniperMech");

        if (bossPrefab == null) bossPrefab = Resources.Load<GameObject>("CoreGuardianBoss");
    }

    // =========================================================
    // MULTI-WAVE MASTER COROUTINE
    // =========================================================

    private IEnumerator MultiWaveMasterRoutine(bool isBossLevel)
    {
        // 1. NẾU LÀ TẦNG BOSS
        if (isBossLevel)
        {
            currentWaveIndex = 1;
            Vector3 center = transform.position + new Vector3(0f, 2f, 0f);

            CombatFeedback.ShowNotice(center, "🚨 WARNING: CORE GUARDIAN BOSS AWAKENS! 🚨", Color.red);
            if (CameraFollow.Instance != null) CameraFollow.Instance.Shake(0.2f, 0.25f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayBossRoar();

            yield return new WaitForSeconds(1.2f);

            if (bossPrefab != null)
            {
                CombatFeedback.SpawnSparks(center, Vector2.up, Color.red, 25);
                GameObject boss = Instantiate(bossPrefab, center, Quaternion.identity, transform);
                spawnedEnemies.Add(boss);

                EnemyHealth bHp = boss.GetComponent<EnemyHealth>();
                if (bHp != null)
                {
                    bHp.OnDeath += () => OnSingleEnemyDied(boss);
                }
            }

            // Chờ tiêu diệt Boss
            yield return new WaitUntil(() => GetRemainingEnemyCount() == 0);

            CompleteRoom();
            yield break;
        }

        // 2. PHÒNG THƯỜNG: VÒNG LẶP CÁC ĐỢT QUÁI (WAVE 1 -> WAVE 2 -> WAVE 3)
        while (currentWaveIndex < totalWaves)
        {
            currentWaveIndex++;
            isWaveInProgress = true;
            isWaveTransitioning = false;

            // Thông báo bắt đầu Wave mới trên màn hình (Soul Knight Screen Banner)
            string waveTitle = currentWaveIndex == totalWaves
                ? $"🔥 FINAL WAVE [{currentWaveIndex}/{totalWaves}] - ĐỢT TỔNG LỰC! 🔥"
                : $"⚠️ WAVE [{currentWaveIndex}/{totalWaves}] - BẮT ĐẦU XÂM NHẬP! ⚠️";

            string waveSubtitle = currentWaveIndex == totalWaves
                ? "CẢNH BÁO: ROBOT XẠ THỦ & CẢM TỬ XUẤT HIỆN"
                : "QUÉT SẠCH KẺ ĐỊCH ĐỂ MỞ KHÓA TẦNG TIẾP THEO";

            Color waveColor = currentWaveIndex == totalWaves ? new Color(1f, 0.25f, 0.35f) : new Color(1f, 0.82f, 0.15f);

            PlayerHUD.ShowWaveBanner(waveTitle, waveSubtitle, waveColor, 2.2f);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShoot();
            }
            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.Shake(0.08f, 0.12f);
            }

            yield return new WaitForSeconds(0.35f);

            // Sinh quái cho Wave hiện tại với hiệu ứng Warp mượt mà không giật
            yield return StartCoroutine(SpawnEnemiesForWave(currentWaveIndex));

            // Chờ cho đến khi toàn bộ quái trong wave bị tiêu diệt
            yield return new WaitUntil(() => GetRemainingEnemyCount() == 0);

            isWaveInProgress = false;

            // Nếu còn Wave tiếp theo
            if (currentWaveIndex < totalWaves)
            {
                isWaveTransitioning = true;
                PlayerHUD.ShowWaveBanner(
                    $"✓ HOÀN THÀNH WAVE {currentWaveIndex}!",
                    "CHUẨN BỊ CHIẾN ĐẤU ĐỢT TIẾP THEO...",
                    Color.green,
                    1.2f
                );

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayScrapCollect();
                }

                // Thưởng nhẹ điểm & thời gian nghỉ chiến thuật ngắn (0.9s)
                if (GameManager.Instance != null) GameManager.Instance.AddScore(25);

                yield return new WaitForSeconds(waveInterval);
            }
        }

        // Toàn bộ các đợt quái đã bị quét sạch hoàn toàn!
        CompleteRoom();
    }

    private IEnumerator SpawnEnemiesForWave(int waveNum)
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0) yield break;

        // Xây dựng danh sách quái cho đợt hiện tại
        List<GameObject> wavePool = BuildEnemyPoolForWave(waveNum);
        if (wavePool.Count == 0) yield break;

        int spawnCount = Mathf.Clamp(3 + waveNum * 2, 3, 10);
        List<Vector3> spawnPositions = new List<Vector3>(spawnCount);

        // 1. Tính toán toạ độ và Bắn tia sáng/Warp rune cảnh báo trước tại tất cả vị trí (Telegraphing)
        for (int i = 0; i < spawnCount; i++)
        {
            Transform sp = enemySpawnPoints[i % enemySpawnPoints.Length];
            Vector3 basePos = sp != null ? sp.position : transform.position;
            Vector3 jitter = (Vector3)(UnityEngine.Random.insideUnitCircle * (i >= enemySpawnPoints.Length ? 1.2f : 0.3f));
            Vector3 pos = basePos + jitter;
            spawnPositions.Add(pos);

            // Tia sáng màu xanh cảnh báo xuất hiện
            CombatFeedback.SpawnSparks(pos, Vector2.up, new Color(0.1f, 0.95f, 1f, 0.9f), 6);
        }

        // Chờ 0.35s cho người chơi nhận biết vị trí quái sắp hiện (chuẩn Soul Knight)
        yield return new WaitForSeconds(0.35f);

        // 2. Xuất hiện đồng loạt toàn bộ quái vật không bị ngắt quãng giật khung hình
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefabToSpawn = wavePool[i % wavePool.Count];
            if (prefabToSpawn == null) continue;

            Vector3 pos = spawnPositions[i];

            GameObject enemy = Instantiate(prefabToSpawn, pos, Quaternion.identity, transform);
            spawnedEnemies.Add(enemy);

            // Bắn hiệu ứng nổ năng lượng tại vị trí quái xuất hiện
            CombatFeedback.SpawnSparks(pos, Vector2.up, new Color(1f, 0.8f, 0.2f), 8);

            // Hoạt ảnh Pop-in nhanh (Scale 0 -> 1.15 -> 1.0)
            StartCoroutine(EnemyPopInRoutine(enemy));

            EnemyHealth hp = enemy.GetComponent<EnemyHealth>();
            if (hp != null)
            {
                hp.OnDeath += () => OnSingleEnemyDied(enemy);
            }
        }

        UnityEngine.Debug.Log($"[RoomController] Đã xuất trận {spawnedEnemies.Count} kẻ địch cho Wave {waveNum}/{totalWaves}");
    }

    private IEnumerator EnemyPopInRoutine(GameObject enemy)
    {
        if (enemy == null) yield break;
        Transform tr = enemy.transform;
        Vector3 origScale = tr.localScale;
        tr.localScale = Vector3.zero;

        float elapsed = 0f;
        float duration = 0.20f;
        while (elapsed < duration)
        {
            if (enemy == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scaleMult = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.12f;
            tr.localScale = origScale * scaleMult;
            yield return null;
        }

        if (enemy != null) tr.localScale = origScale;
    }

    private List<GameObject> BuildEnemyPoolForWave(int waveNum)
    {
        List<GameObject> pool = new List<GameObject>();

        // Lấy danh sách cơ bản từ Inspector nếu có
        if (enemyPrefabs != null)
        {
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (enemyPrefabs[i] != null) pool.Add(enemyPrefabs[i]);
            }
        }

        // Bổ sung quái mới theo từng Wave
        if (waveNum == 1)
        {
            // Wave 1: Quái trinh sát cơ bản
            if (kamikazeBotPrefab != null && UnityEngine.Random.value > 0.5f) pool.Add(kamikazeBotPrefab);
        }
        else if (waveNum == 2)
        {
            // Wave 2: Thêm quái cảm tử Kamikaze
            if (kamikazeBotPrefab != null)
            {
                pool.Add(kamikazeBotPrefab);
                pool.Add(kamikazeBotPrefab);
            }
            if (sniperMechPrefab != null && UnityEngine.Random.value > 0.4f) pool.Add(sniperMechPrefab);
        }
        else
        {
            // Wave 3 (Final Wave): Đội hình tinh anh tổng lực có Sniper Mech & Kamikaze
            if (sniperMechPrefab != null) pool.Add(sniperMechPrefab);
            if (kamikazeBotPrefab != null) pool.Add(kamikazeBotPrefab);
        }

        return pool;
    }

    private void OnSingleEnemyDied(GameObject enemy)
    {
        spawnedEnemies.Remove(enemy);
    }

    // =========================================================
    // ROOM COMPLETION & REWARD
    // =========================================================

    //private void CompleteRoom()
    //{
    //    if (roomCompleted) return;
    //    roomCompleted = true;

    //    UnityEngine.Debug.Log($"[RoomController] PHÒNG HOÀN THÀNH TOÀN BỘ {totalWaves} WAVES: {roomName}! Mở cửa Exit.");

    //    PlayerHUD.ShowWaveBanner(
    //        "🏆 TOÀN BỘ ĐỢT QUÁI ĐÃ BỊ QUÉT SẠCH!",
    //        "CỔNG DỊCH CHUYỂN VAULT ĐÃ MỞ - BƯỚC VÀO ĐỂ TIẾN LÊN TẦNG TIẾP THEO",
    //        Color.cyan,
    //        3.5f
    //    );

    //    if (AudioManager.Instance != null)
    //    {
    //        AudioManager.Instance.PlayRoomClear();
    //    }

    //    if (GameManager.Instance != null)
    //    {
    //        GameManager.Instance.AddScore(100);
    //        GameManager.Instance.AddScrap(10 + totalWaves * 5); // Thưởng Scrap lớn khi hoàn thành mọi đợt quái
    //    }

    //    OpenExit();
    //}
    private void CompleteRoom()
    {
        if (roomCompleted) return;

        roomCompleted = true;

        // Thưởng điểm và Scrap
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(100);
            GameManager.Instance.AddScrap(10 + totalWaves * 5);
        }

        // Nếu đây là phòng Boss cuối cùng
        if (isBossRoom)
        {
            UnityEngine.Debug.Log("[RoomController] CORE GUARDIAN DEFEATED!");

            PlayerHUD.ShowWaveBanner(
                "CORE GUARDIAN DEFEATED!",
                "VAULT CLEARED - RUN COMPLETE",
                Color.cyan,
                4f
            );

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRoomClear();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteRun();
            }

            return;
        }

        // Phòng thường: mở Exit để đi tầng kế tiếp
        UnityEngine.Debug.Log(
            $"[RoomController] Hoàn thành phòng {roomName}. Mở Exit."
        );

        PlayerHUD.ShowWaveBanner(
            "TOÀN BỘ ĐỢT QUÁI ĐÃ BỊ QUÉT SẠCH!",
            "CỔNG DỊCH CHUYỂN VAULT ĐÃ MỞ - BƯỚC VÀO ĐỂ TIẾN LÊN TẦNG TIẾP THEO",
            Color.cyan,
            3.5f
        );

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRoomClear();
        }

        OpenExit();
    }

    // =========================================================
    // EXIT & PLAYER POSITIONING
    // =========================================================

    private void PositionPlayerAtSpawn()
    {
        Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : transform.position;

        PlayerMovement existingPm = PlayerMovement.Instance;
        GameObject existingPlayer = existingPm != null ? existingPm.gameObject : GameObject.FindGameObjectWithTag("Player");

        if (existingPlayer != null)
        {
            existingPlayer.transform.position = spawnPos;
            existingPlayer.transform.localScale = Vector3.one;
            existingPlayer.transform.rotation = Quaternion.identity;

            PlayerMovement pm = existingPlayer.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = true;

            PlayerAim pa = existingPlayer.GetComponent<PlayerAim>();
            if (pa != null) pa.enabled = true;

            WeaponController wc = existingPlayer.GetComponent<WeaponController>();
            if (wc != null) wc.enabled = true;

            Rigidbody2D rb = existingPlayer.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // Bắn chùm tia năng lượng báo hiệu Unit-7 xuất hiện tại điểm Spawn tầng mới
            CombatFeedback.SpawnSparks(spawnPos, Vector2.up, Color.cyan, 20);
        }
        else if (playerPrefab != null)
        {
            GameObject newPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            newPlayer.name = "Player (Unit-7)";
            newPlayer.transform.localScale = Vector3.one;
        }

        CameraFollow camFollow = CameraFollow.Instance != null ? CameraFollow.Instance : FindFirstObjectByType<CameraFollow>();
        if (camFollow != null)
        {
            GameObject p = existingPlayer ?? GameObject.FindGameObjectWithTag("Player");
            if (p != null) camFollow.SetTarget(p.transform);
        }
    }

    private void OpenExit()
    {
        if (exit == null)
        {
            // Tự động tìm Exit trong hierarchy nếu chưa gán
            Transform exitTr = transform.Find("Exit");
            if (exitTr != null) exit = exitTr.gameObject;
        }

        if (exit == null)
        {
            UnityEngine.Debug.LogWarning("[RoomController] Exit chưa được gán trong: " + roomName);
            return;
        }

        // Đặt vị trí Portal ngay trong tầm mắt của người chơi tại trung tâm phía trên phòng
        //exit.transform.position = transform.position + new Vector3(0f, 2.2f, 0f);
        exit.SetActive(true);

        if (exit.GetComponent<PortalVisual>() == null)
        {
            exit.AddComponent<PortalVisual>();
        }
        if (exit.GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = exit.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;
        }
        if (exit.GetComponent<RoomExit>() == null)
        {
            exit.AddComponent<RoomExit>();
        }

        UnityEngine.Debug.Log("[RoomController] Cửa Exit Portal đã mở và sẵn sàng tại vị trí trung tâm!");
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