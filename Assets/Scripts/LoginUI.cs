using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles the login and registration UI logic, including panel switching, password masking, and animation feedback.
/// </summary>
public class LoginUI : MonoBehaviour
{
    // Animator parameter hashes for controlling feedback animations (Success? and ShouldPlay?)
    private static readonly int Success = Animator.StringToHash("Success?");
    private static readonly int ShouldPlay = Animator.StringToHash("ShouldPlay?");
    // Stores the session password for use after login/register (not persisted)
    private string _sessionPassword;

    // UI references for login and registration fields (set in Inspector)
    public TMP_InputField usernameLoginField; // Username/email input for login
    public TMP_InputField passwordLoginField; // Password input for login
    public TMP_InputField usernameField;      // Username input for registration
    public TMP_InputField passwordField;      // Password input for registration
    public TMP_InputField emailField;         // Email input for registration

    // References to managers and panels (set in Inspector)
    public AccountManager accountManager;         // Handles login/register logic
    public PanelManager panelManager;             // Handles panel switching
    public GameSessionManager gameSessionManager; // Handles game state and session
    public LeaderboardManager leaderboardManager; // Handles leaderboard fetching/display
    public GameObject loginPanel;                 // Login panel GameObject
    public GameObject registerPanel;              // Register panel GameObject
    public GameObject mainMenuPanel;              // Main menu panel GameObject
    public GameObject gamePanel;                  // Game panel GameObject
    public GameObject leaderboardPanel;           // Leaderboard panel GameObject

    // Animators for each panel (set in Awake for animation feedback)
    [HideInInspector] Animator loginPanelAnimator;
    [HideInInspector] Animator registerPanelAnimator;
    [HideInInspector] Animator gamePanelAnimator;
    [HideInInspector] Animator mainMenuPanelAnimator;
    [HideInInspector] Animator leaderboardPanelAnimator;
    
    private void Awake()
    {
        // Cache animator references for each panel for later use
        loginPanelAnimator = loginPanel.GetComponent<Animator>();
        gamePanelAnimator = gamePanel.GetComponent<Animator>();
        registerPanelAnimator = registerPanel.GetComponent<Animator>();
        mainMenuPanelAnimator = mainMenuPanel.GetComponent<Animator>();
        leaderboardPanelAnimator = leaderboardPanel.GetComponent<Animator>();
        
        // Ensure password fields are masked with '*' for privacy
        if (passwordLoginField != null)
        {
            passwordLoginField.contentType = TMP_InputField.ContentType.Password;
            passwordLoginField.asteriskChar = '*';
            passwordLoginField.ForceLabelUpdate(); // Update label to apply masking
        }
        if (passwordField != null)
        {
            passwordField.contentType = TMP_InputField.ContentType.Password;
            passwordField.asteriskChar = '*';
            passwordField.ForceLabelUpdate(); // Update label to apply masking
        }
    }

    /// <summary>
    /// Called when the login button is pressed. Attempts login and plays feedback animation.
    /// </summary>
    public async void OnLoginButton()
    {
        // Attempt to log in using the provided username/email and password
        bool success = await accountManager.Login(usernameLoginField.text, passwordLoginField.text);
        if (success)
        {
            Debug.Log("Login successful!");
            _sessionPassword = passwordLoginField.text; // Store password for session
            // Share session password with the session manager for future actions/saves
            if (gameSessionManager != null)
            {
                gameSessionManager.SetSessionPassword(_sessionPassword);
            }
            // Set animator parameters for success and play the animation
            loginPanelAnimator.SetBool(Success, true);
            loginPanelAnimator.SetBool(ShouldPlay, true);
            // Wait for animation, then switch to the game panel
            StartCoroutine(PlayLoginAnimationAndSwitchPanel());
        }
        else
        {
            Debug.Log("Login failed.");
            // Set animator parameters for failure and play the animation
            loginPanelAnimator.SetBool(Success, false);
            loginPanelAnimator.SetBool(ShouldPlay, true);
            // Wait for animation, then reset
            StartCoroutine(ResetLoginAnimation());
        }
    }
    
    /// <summary>
    /// Called when the register button is pressed. Attempts registration and plays feedback animation.
    /// </summary>
    public async void OnRegisterButton()
    {
        // Gather registration info from input fields
        string username = usernameField.text;
        string password = passwordField.text;
        string email = emailField.text;

        // Attempt to register a new account
        bool success = await accountManager.Register(username, password, email);
        if (success)
        {
            Debug.Log("Register successful!");
            // Store password for session and start a new game for new accounts
            _sessionPassword = password;
            if (gameSessionManager != null)
            {
                gameSessionManager.SetSessionPassword(_sessionPassword);
                gameSessionManager.StartNewGame();
            }
            // Set animator parameters for success and play the animation
            registerPanelAnimator.SetBool(Success, true);
            registerPanelAnimator.SetBool(ShouldPlay, true);
            // Wait for animation, then switch to main menu
            StartCoroutine(PlayRegisterAnimationAndSwitchPanel());
        }
        else
        {
            Debug.Log("Register failed.");
            // Set animator parameters for failure and play the animation
            registerPanelAnimator.SetBool(Success, false);
            registerPanelAnimator.SetBool(ShouldPlay, true);
            // Wait for animation, then reset
            StartCoroutine(ResetRegisterAnimation());
        }
    }
    
    /// <summary>
    /// Called when the save button is pressed. Saves the current game and plays feedback animation.
    /// </summary>
    public async void OnSaveButton()
    {
        // Only save if logged in and a game is in progress
        if (accountManager.CurrentAccount != null && gameSessionManager.CurrentGame != null)
        {
            await gameSessionManager.SaveGame(
                accountManager.Username, _sessionPassword, accountManager.CurrentAccount
            );
            Debug.Log("Game saved!");
            // Set animator parameters for success and play the animation
            gamePanelAnimator.SetBool(Success, true);
            gamePanelAnimator.SetBool(ShouldPlay, true);
            StartCoroutine(ResetSaveAnimation());
            // Refresh leaderboard after saving
            await leaderboardManager.FetchAndDisplayLeaderboard();
        }
        else
        {
            Debug.Log("Save failed.");
            // Set animator parameters for failure and play the animation
            gamePanelAnimator.SetBool(Success, false);
            gamePanelAnimator.SetBool(ShouldPlay, true);
            StartCoroutine(ResetSaveAnimation());
        }
    }
    
    /// <summary>
    /// Called when the disconnect button is pressed. Logs out and returns to the login panel.
    /// </summary>
    public void OnDisconnectButton()
    {
        accountManager.Logout(); // Clear account info
        _sessionPassword = null; // Clear session password
        if (gameSessionManager != null)
        {
            gameSessionManager.ClearSession(); // Clear game session
        }
        // Return to login panel
        if (panelManager != null)
        {
            panelManager.CloseCurrent();
            panelManager.OpenPanel(loginPanelAnimator);
        }
    }
    
    /// <summary>
    /// Called when the leaderboard button is pressed. Switches to the leaderboard panel and fetches data.
    /// </summary>
    public async void OnLeaderboardButton()
    {
        panelManager.CloseCurrent(true); // Instantly close current panel
        await leaderboardManager.FetchAndDisplayLeaderboard(); // Fetch leaderboard data
        panelManager.OpenPanel(leaderboardPanelAnimator, true); // Instantly open leaderboard panel
    }
    
    /// <summary>
    /// Instantly returns to the game panel, closing any current panel.
    /// </summary>
    public void ReturnToGamePanelInstant()
    {
        if (panelManager == null || gamePanelAnimator == null)
            return;
        panelManager.CloseCurrent(true); // Instantly close current panel
        panelManager.OpenPanel(gamePanelAnimator, true); // Instantly open game panel
    }
    
    /// <summary>
    /// Plays the register animation, then switches to the main menu panel.
    /// </summary>
    private System.Collections.IEnumerator PlayRegisterAnimationAndSwitchPanel()
    {
        yield return new WaitForSeconds(1.0f); // Wait for animation to finish (adjust as needed)
        registerPanelAnimator.SetBool(ShouldPlay, false); // Reset animation trigger
        panelManager.CloseCurrent(); // Close register panel
        panelManager.OpenPanel(mainMenuPanelAnimator); // Open main menu panel
    }

    /// <summary>
    /// Resets the register animation after a short delay.
    /// </summary>
    private System.Collections.IEnumerator ResetRegisterAnimation()
    {
        yield return new WaitForSeconds(1.0f); // Wait for animation to finish
        registerPanelAnimator.SetBool(ShouldPlay, false); // Reset animation trigger
    }
    
    /// <summary>
    /// Plays the login animation, then switches to the game panel.
    /// </summary>
    private System.Collections.IEnumerator PlayLoginAnimationAndSwitchPanel()
    {
        yield return new WaitForSeconds(1.0f); // Wait for animation to finish
        loginPanelAnimator.SetBool(ShouldPlay, false); // Reset animation trigger
        panelManager.CloseCurrent(); // Close login panel
        panelManager.OpenPanel(gamePanelAnimator); // Open game panel
    }
    
    /// <summary>
    /// Resets the login animation after a short delay.
    /// </summary>
    private System.Collections.IEnumerator ResetLoginAnimation()
    {
        yield return new WaitForSeconds(1.0f); // Wait for animation to finish
        loginPanelAnimator.SetBool(ShouldPlay, false); // Reset animation trigger
    }
    
    /// <summary>
    /// Resets the save animation after a short delay.
    /// </summary>
    private System.Collections.IEnumerator ResetSaveAnimation()
    {
        yield return new WaitForSeconds(1.0f); // Wait for animation to finish
        gamePanelAnimator.SetBool(ShouldPlay, false); // Reset animation trigger
    }
}