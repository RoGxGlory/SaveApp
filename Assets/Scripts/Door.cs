/// <summary>
/// Represents a door in the arena, allowing transition to another room.
/// </summary>
public class Door
{
    public int X { get; set; } // X position of the door
    public int Y { get; set; } // Y position of the door
    public string TargetRoomId { get; set; } // ID of the room this door leads to

    public Door(int x, int y, string targetRoomId)
    {
        X = x;
        Y = y;
        TargetRoomId = targetRoomId;
    }
}
