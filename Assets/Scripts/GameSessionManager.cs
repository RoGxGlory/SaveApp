using UnityEngine;
using SaveAppCore;
using System.Threading.Tasks;

public class GameSessionManager : MonoBehaviour
{
    public Game CurrentGame { get; private set; }

    // Reference to account for actions that require auth context
    public AccountManager accountManager;

    // Store session password for actions/save without exposing broadly
    public string SessionPassword { get; private set; }

    public void SetSessionPassword(string password)
    {
        SessionPassword = password;
    }

    public void ClearSession()
    {
        CurrentGame = null;
        SessionPassword = null;
    }

    public void StartNewGame()
    {
        CurrentGame = new Game();
        CurrentGame.StartNewGame();
    }

    public async Task<bool> LoadGame(string username, string password)
    {
        CurrentGame = await Game.LoadEncryptedAsync(username, password);
        return CurrentGame != null && CurrentGame.InProgress;
    }

    public async Task SaveGame(string username, string password, Account account)
    {
        // Rely on default for optional parameters, if any
        await Game.SaveEncryptedAsync(CurrentGame, username, password, account);
    }

    // Wrapper to perform a turn/action without modifying Game.cs
    // Returns the result string (if any) from the action
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
        // Some actions in Game may return a string describing the outcome
        // We call the existing API similar to the console version
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