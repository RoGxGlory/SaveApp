using UnityEngine;
using System.Collections.Generic;
using SaveAppCore;

/// <summary>
/// Manages all arenas (rooms) in the game, including transitions and updating monster movement in all rooms.
/// </summary>
public class ArenaManager : MonoBehaviour
{
    // Dictionary of all rooms by their unique ID
    public Dictionary<string, Arena> Rooms = new();
    // The currently active arena (room)
    public Arena CurrentArena;
    // The ID of the current room
    public string CurrentRoomId;
    
    public int DefaultWidth = 24;
    public int DefaultHeight = 8;

    /// <summary>
    /// Constructor: creates the initial room and sets it as current.
    /// </summary>
    public ArenaManager()
    {
        // Create initial room
        CurrentRoomId = "Start";
        CurrentArena = new Arena(DefaultWidth, DefaultHeight, CurrentRoomId);
        Rooms[CurrentRoomId] = CurrentArena;
    }

    /// <summary>
    /// Moves the player to the specified room, creating it if it doesn't exist.
    /// </summary>
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

    /// <summary>
    /// Updates all rooms by moving monsters in each arena.
    /// </summary>
    public void UpdateAllRooms()
    {
        foreach (var arena in Rooms.Values)
            arena.MoveMonsters();
    }
}
