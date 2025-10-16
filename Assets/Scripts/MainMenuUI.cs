using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public GameSessionManager gameSessionManager;
    public AccountManager accountManager;
    public PanelManager panelManager;
    public Animator gamePanelAnimator; // Assign in Inspector

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