using UnityEngine;
using UnityEngine.UI;
using SaveAppCore;

public class ArenaDisplay : MonoBehaviour
{
    public Text arenaText; // Assign in Inspector
    public Text statsText;
    public GameSessionManager gameSessionManager; // Assign in Inspector

    void Update()
    {
        var game = gameSessionManager != null ? gameSessionManager.CurrentGame : null;
        if (game != null)
        {
            if (!game.InProgress)
            {
                arenaText.text = "You Died!";
            }
            else if (game.Arena != null)
            {
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
            arenaText.text = "";
            statsText.text = "";
        }
    }

    // UI Button handlers - delegate to GameSessionManager wrappers
    public void OnUpButton()
    {
        var res = gameSessionManager?.MoveUp();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }

    public void OnDownButton()
    {
        var res = gameSessionManager?.MoveDown();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }

    public void OnLeftButton()
    {
        var res = gameSessionManager?.MoveLeft();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }

    public void OnRightButton()
    {
        var res = gameSessionManager?.MoveRight();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }

    public void OnFightButton()
    {
        var res = gameSessionManager?.Fight();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }

    public void OnPickUpButton()
    {
        var res = gameSessionManager?.PickUp();
        if (!string.IsNullOrEmpty(res)) Debug.Log(res);
    }
}