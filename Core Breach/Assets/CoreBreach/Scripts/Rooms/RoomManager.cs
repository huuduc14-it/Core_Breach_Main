using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Room Database")]
    public RoomData[] normalRooms;

    public RoomData bossRoom;

    [Header("Room Spawn")]
    public Transform roomSpawnPoint;

    private GameObject currentRoom;

    private int currentDepth = 0;

    private void Start()
    {
        StartRun();
    }

    public void StartRun()
    {
        currentDepth = 1;

        LoadNextRoom();
    }

    public void LoadNextRoom()
    {
        RoomData selectedRoom;

        if (currentDepth % 5 == 0)
        {
            selectedRoom = bossRoom;

            Debug.Log("BOSS ROOM!");
        }
        else
        {
            selectedRoom = GetRandomNormalRoom();
        }

        SpawnRoom(selectedRoom);
    }

    private RoomData GetRandomNormalRoom()
    {
        if (normalRooms == null || normalRooms.Length == 0)
        {
            Debug.LogError("No normal rooms assigned!");
            return null;
        }

        int index = Random.Range(0, normalRooms.Length);

        return normalRooms[index];
    }

    private void SpawnRoom(RoomData roomData)
    {
        if (roomData == null)
            return;

        if (currentRoom != null)
        {
            Destroy(currentRoom);
        }

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

        Debug.Log(
            "Loaded room: " +
            roomData.roomName +
            " | Depth: " +
            currentDepth
        );
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