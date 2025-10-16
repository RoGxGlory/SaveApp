public class Door
{
    public int X { get; set; }
    public int Y { get; set; }
    public string TargetRoomId { get; set; }

    public Door(int x, int y, string targetRoomId)
    {
        X = x;
        Y = y;
        TargetRoomId = targetRoomId;
    }
}
