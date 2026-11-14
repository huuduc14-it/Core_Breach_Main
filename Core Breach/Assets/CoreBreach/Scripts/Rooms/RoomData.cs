using UnityEngine;

public enum RoomType
{
    Combat,
    Turret,
    Mixed,
    Elite,
    Boss
}

[CreateAssetMenu(fileName = "RoomData", menuName = "Core Breach/Room Data")]
public class RoomData : ScriptableObject
{
    public string roomName;

    public RoomType roomType;

    public GameObject roomPrefab;

    [Range(1, 100)]
    public int weight = 10;
}