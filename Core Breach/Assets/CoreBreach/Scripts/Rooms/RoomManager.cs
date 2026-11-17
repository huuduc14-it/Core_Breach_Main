using System.Diagnostics;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>1
/// Quản lý vòng lặp phòng vô tận (Depth):
/// - Chọn phòng thường ngẫu nhiên theo trọng số (weight), không lặp lại phòng vừa rồi
/// - Cứ mỗi (bossEveryNDepth) tầng thì vào phòng Boss (nếu chưa gán Boss thì dùng phòng thường)
/// - Giới hạn khung nhìn camera theo kích thước phòng
/// </summary>
public class RoomManager : MonoBehaviour
{
    [Header("Room Database")]
    public RoomData[] normalRooms;

    public RoomData bossRoom;

    [Header("Room Spawn")]
    public Transform roomSpawnPoint;

    [Header("Progression")]
    [SerializeField] private int bossEveryNDepth = 4;
    [SerializeField] private bool limitCameraToRoom = true;

    private GameObject currentRoom;
    private RoomData lastRoomData;

    private int currentDepth = 0;

    //private void Start()
    //{
    //    StartRun();
    //}

    public void StartRun()
    {
        currentDepth = 1;
        lastRoomData = null;

        LoadNextRoom();
    }

    public void LoadNextRoom()
    {
        RoomData selectedRoom = null;

        bool isBossDepth = bossEveryNDepth > 0 && currentDepth % bossEveryNDepth == 0;

        if (isBossDepth)
        {
            if (bossRoom != null && bossRoom.roomPrefab != null)
            {
                selectedRoom = bossRoom;
                UnityEngine.Debug.Log("BOSS ROOM!");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[RoomManager] Chưa gán Boss Room, dùng phòng thường thay thế.");
            }
        }

        if (selectedRoom == null)
        {
            selectedRoom = GetRandomNormalRoom();
        }

        SpawnRoom(selectedRoom);
    }

    private RoomData GetRandomNormalRoom()
    {
        if (normalRooms == null || normalRooms.Length == 0)
        {
            UnityEngine.Debug.LogError("No normal rooms assigned!");
            return null;
        }

        // Tổng trọng số của các phòng hợp lệ (bỏ phòng vừa chơi nếu còn lựa chọn khác)
        int total = 0;
        for (int i = 0; i < normalRooms.Length; i++)
        {
            if (IsPickable(normalRooms[i])) total += Mathf.Max(1, normalRooms[i].weight);
        }

        if (total <= 0)
        {
            // Chỉ có 1 phòng duy nhất hoặc tất cả đều bị loại -> chọn phòng đầu tiên hợp lệ
            for (int i = 0; i < normalRooms.Length; i++)
            {
                if (normalRooms[i] != null && normalRooms[i].roomPrefab != null) return normalRooms[i];
            }
            return null;
        }

        int roll = UnityEngine.Random.Range(0, total);
        for (int i = 0; i < normalRooms.Length; i++)
        {
            if (!IsPickable(normalRooms[i])) continue;

            roll -= Mathf.Max(1, normalRooms[i].weight);
            if (roll < 0) return normalRooms[i];
        }

        return null;
    }

    private bool IsPickable(RoomData data)
    {
        if (data == null || data.roomPrefab == null) return false;
        if (data == lastRoomData && CountValidRooms() > 1) return false;
        return true;
    }

    private int CountValidRooms()
    {
        int count = 0;
        for (int i = 0; i < normalRooms.Length; i++)
        {
            if (normalRooms[i] != null && normalRooms[i].roomPrefab != null) count++;
        }
        return count;
    }

    private void SpawnRoom(RoomData roomData)
    {
        if (roomData == null || roomData.roomPrefab == null)
        {
            UnityEngine.Debug.LogError("[RoomManager] Không có phòng hợp lệ để tạo!");
            return;
        }

        if (currentRoom != null)
        {
            Destroy(currentRoom);
        }

        lastRoomData = roomData;

        Vector3 spawnPos = roomSpawnPoint != null ? roomSpawnPoint.position : transform.position;

        currentRoom = Instantiate(
            roomData.roomPrefab,
            spawnPos,
            Quaternion.identity
        );

        RoomController controller =
            currentRoom.GetComponent<RoomController>();

        if (controller != null)
        {
            controller.StartRoom();
        }

        if (limitCameraToRoom)
        {
            ApplyCameraBounds(currentRoom);
        }

        UnityEngine.Debug.Log(
            "Loaded room: " +
            roomData.roomName +
            " | Depth: " +
            currentDepth
        );
    }

    /// <summary>
    /// Giới hạn camera trong phạm vi các Tilemap của phòng để không lộ vùng trống bên ngoài.
    /// </summary>
    private void ApplyCameraBounds(GameObject room)
    {
        CameraFollow follow = CameraFollow.Instance;
        Camera cam = Camera.main;
        if (follow == null || cam == null || !cam.orthographic) return;

        TilemapRenderer[] renderers = room.GetComponentsInChildren<TilemapRenderer>();
        if (renderers.Length == 0) return;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector2 min = new Vector2(b.min.x + halfW, b.min.y + halfH);
        Vector2 max = new Vector2(b.max.x - halfW, b.max.y - halfH);

        // Phòng nhỏ hơn khung nhìn: khóa camera ở giữa phòng
        if (min.x > max.x) { min.x = b.center.x; max.x = b.center.x; }
        if (min.y > max.y) { min.y = b.center.y; max.y = b.center.y; }

        follow.SetBounds(min, max);
    }

    public void CompleteRoom()
    {
        currentDepth++;

        LoadNextRoom();
    }

    public int GetCurrentDepth()
    {
        return currentDepth;
    }
}
