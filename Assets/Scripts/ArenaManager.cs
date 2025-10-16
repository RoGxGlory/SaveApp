using UnityEngine;
using System.Collections.Generic;
using SaveAppCore;

public class ArenaManager : MonoBehaviour
{
    public Dictionary<string, Arena> Rooms = new();
    public Arena CurrentArena;
    public string CurrentRoomId;
    
    public int DefaultWidth = 24;
    public int DefaultHeight = 8;

    public ArenaManager()
    {
        // Create initial room
        CurrentRoomId = "Start";
        CurrentArena = new Arena(DefaultWidth, DefaultHeight, CurrentRoomId);
        Rooms[CurrentRoomId] = CurrentArena;
    }

    public void MoveToRoom(string roomId)
    {
        if (!Rooms.ContainsKey(roomId))
        {
            var newArena = new Arena(DefaultWidth, DefaultHeight, roomId);
            Rooms[roomId] = newArena;
        }
        CurrentRoomId = roomId;
        CurrentArena = Rooms[roomId];
    }

    public void UpdateAllRooms()
    {
        foreach (var arena in Rooms.Values)
            arena.MoveMonsters();
    }
}
