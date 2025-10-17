using UnityEngine;
using SaveAppCore;
using System.Threading.Tasks;

/// <summary>
/// Manages the current game session, including loading, saving, and performing actions.
/// </summary>
public class GameSessionManager : MonoBehaviour
{
    // The current game instance for this session
    public Game CurrentGame { get; private set; }

    // Reference to account manager for authentication context
    public AccountManager accountManager;

    // Stores the session password for secure actions
    public string SessionPassword { get; private set; }

    /// <summary>
    /// Sets the session password for use in save/load operations.
    /// </summary>
    public void SetSessionPassword(string password)
    {
        SessionPassword = password;
    }

    /// <summary>
    /// Clears the current game and session password.
    /// </summary>
    public void ClearSession()
    {
        CurrentGame = null;
        SessionPassword = null;
    }

    /// <summary>
    /// Starts a new game session and initializes the game state.
    /// </summary>
    public void StartNewGame()
    {
        CurrentGame = new Game();
        CurrentGame.StartNewGame();
    }

    /// <summary>
    /// Loads a saved game for the given user credentials.
    /// </summary>
    public async Task<bool> LoadGame(string username, string password)
    {
        CurrentGame = await Game.LoadEncryptedAsync(username, password);
        return CurrentGame != null && CurrentGame.InProgress;
    }

    /// <summary>
    /// Saves the current game state for the given user credentials and account.
    /// </summary>
    public async Task SaveGame(string username, string password, Account account)
    {
        // Save the current game using encrypted save logic
        await Game.SaveEncryptedAsync(CurrentGame, username, password, account);
    }

    /// <summary>
    /// Performs a game action (move, fight, pick up) and returns the result string.
    /// </summary>
    public string PerformAction(string action)
    {
        if (CurrentGame == null)
        {
            Debug.LogWarning("PerformAction called but CurrentGame is null.");
            return string.Empty;
        }
        if (accountManager == null)
        {
            Debug.LogWarning("PerformAction called but AccountManager reference is missing.");
            return string.Empty;
        }

        var username = accountManager.Username;
        var account = accountManager.CurrentAccount;
        // Call the PlayTurn method in Game to process the action
        string result = CurrentGame.PlayTurn(action, username, SessionPassword, account);
        return result;
    }

    // Convenience wrappers for UI buttons
    public string MoveUp() => PerformAction("up");
    public string MoveDown() => PerformAction("down");
    public string MoveLeft() => PerformAction("left");
    public string MoveRight() => PerformAction("right");
    public string Fight() => PerformAction("fight");
    public string PickUp() => PerformAction("pick");
}