using UnityEngine;
using UnityEngine.UI;
using SaveAppCore;

/// <summary>
/// Handles displaying the arena and player stats in the UI, and delegates button actions to the game session manager.
/// </summary>
public class ArenaDisplay : MonoBehaviour
{
    public Text arenaText; // UI Text for the arena visual (assign in Inspector)
    public Text statsText; // UI Text for player stats (assign in Inspector)
    public GameSessionManager gameSessionManager; // Reference to the session manager

    void Update()
    {
        // Get the current game from the session manager
        var game = gameSessionManager != null ? gameSessionManager.CurrentGame : null;
        if (game != null)
        {
            if (!game.InProgress)
            {
                // Show death message if game is over
                arenaText.text = "You Died!";
            }
            else if (game.Arena != null)
            {
                // Show the current arena visual
                arenaText.text = game.Arena.GetVisual();
            }

            // Always show stats if game and arena exist
            if (game.Arena != null)
            {
                statsText.text = $"Kills : {game.MonstersKilled}\nDistance : {game.DistanceTraveled}m\nHP : {game.Arena.Player.Health}";
            }
            else
            {
                statsText.text = "";
            }
        }
        else
        {
            // Clear UI if no game
            arenaText.text = "";
            statsText.text = "";
        }
    }

    // UI Button handlers - delegate to GameSessionManager wrappers
    /// <summary>
    /// Handle the Up button click - moves the player up in the arena.
    /// </summary>
    public void OnUpButton()
    {
        var res = gameSessionManager?.MoveUp();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }

    /// <summary>
    /// Handle the Down button click - moves the player down in the arena.
    /// </summary>
    public void OnDownButton()
    {
        var res = gameSessionManager?.MoveDown();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }

    /// <summary>
    /// Handle the Left button click - moves the player left in the arena.
    /// </summary>
    public void OnLeftButton()
    {
        var res = gameSessionManager?.MoveLeft();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }

    /// <summary>
    /// Handle the Right button click - moves the player right in the arena.
    /// </summary>
    public void OnRightButton()
    {
        var res = gameSessionManager?.MoveRight();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }

    /// <summary>
    /// Handle the Fight button click - initiates a fight with an enemy.
    /// </summary>
    public void OnFightButton()
    {
        var res = gameSessionManager?.Fight();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }

    /// <summary>
    /// Handle the Pick Up button click - picks up an item or loot.
    /// </summary>
    public void OnPickUpButton()
    {
        var res = gameSessionManager?.PickUp();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }
}