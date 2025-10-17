using UnityEngine;

/// <summary>
/// Handles main menu UI actions such as starting a new game, loading a game, and switching panels.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public GameSessionManager gameSessionManager; // Reference to the game session manager
    public AccountManager accountManager; // Reference to the account manager
    public PanelManager panelManager; // Reference to the panel manager
    public Animator gamePanelAnimator; // Animator for the game panel

    /// <summary>
    /// Called when the New Game button is pressed. Starts a new game and switches to the game panel.
    /// </summary>
    public void OnNewGameButton()
    {
        gameSessionManager.StartNewGame();
        // ArenaDisplay reads from GameSessionManager, no need to assign the game directly
        if (panelManager != null && gamePanelAnimator != null)
        {
            panelManager.CloseCurrent(true);
            panelManager.OpenPanel(gamePanelAnimator, true);
        }
    }

    /// <summary>
    /// Called when the Load Game button is pressed. Loads the saved game and switches to the game panel if successful.
    /// </summary>
    public async void OnLoadGameButton()
    {
        var username = accountManager?.Username;
        var password = gameSessionManager?.SessionPassword;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Cannot load game: missing username or session password.");
            return;
        }
        bool loaded = await gameSessionManager.LoadGame(username, password);
        if (loaded)
        {
            // ArenaDisplay reads from GameSessionManager, no need to assign the game directly
            if (panelManager != null && gamePanelAnimator != null)
            {
                panelManager.CloseCurrent();
                panelManager.OpenPanel(gamePanelAnimator);
            }
        }
    }

    // Add similar methods for Save, Show Score, Leaderboard, Logout, etc.
}