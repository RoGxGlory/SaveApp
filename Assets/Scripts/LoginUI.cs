using System;
using UnityEngine;
using TMPro;

public class LoginUI : MonoBehaviour
{
    private static readonly int Success = Animator.StringToHash("Success?");
    private static readonly int ShouldPlay = Animator.StringToHash("ShouldPlay?");
    private string _sessionPassword;
    public TMP_InputField usernameLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_InputField emailField;
    public AccountManager accountManager;
    public PanelManager panelManager;
    public GameSessionManager gameSessionManager;
    public LeaderboardManager leaderboardManager;
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject mainMenuPanel;
    public GameObject gamePanel;
    public GameObject leaderboardPanel;
    [HideInInspector] Animator loginPanelAnimator;
    [HideInInspector] Animator registerPanelAnimator;
    [HideInInspector] Animator gamePanelAnimator;
    [HideInInspector] Animator mainMenuPanelAnimator;
    [HideInInspector] Animator leaderboardPanelAnimator;
    
    private void Awake()
    {
        loginPanelAnimator = loginPanel.GetComponent<Animator>();
        gamePanelAnimator = gamePanel.GetComponent<Animator>();
        registerPanelAnimator = registerPanel.GetComponent<Animator>();
        mainMenuPanelAnimator = mainMenuPanel.GetComponent<Animator>();
        leaderboardPanelAnimator = leaderboardPanel.GetComponent<Animator>();
    }

    public async void OnLoginButton()
    {
        bool success = await accountManager.Login(usernameLoginField.text, passwordLoginField.text);
        if (success)
        {
            Debug.Log("Login successful!");
            _sessionPassword = passwordLoginField.text;
            // Share session password with the session manager for future actions/saves
            if (gameSessionManager != null)
            {
                gameSessionManager.SetSessionPassword(_sessionPassword);
            }
            loginPanelAnimator.SetBool(Success, true);
            loginPanelAnimator.SetBool(ShouldPlay, true);
            StartCoroutine(PlayLoginAnimationAndSwitchPanel());
        }
        else
        {
            Debug.Log("Login failed.");
            loginPanelAnimator.SetBool(Success, false);
            loginPanelAnimator.SetBool(ShouldPlay, true);
            StartCoroutine(ResetLoginAnimation());
        }
    }
    
    public async void OnRegisterButton()
    {
        string username = usernameField.text;
        string password = passwordField.text;
        string email = emailField.text;

        bool success = await accountManager.Register(username, password, email);
        if (success)
        {
            Debug.Log("Register successful!");
            // Set session password and start a new game for new accounts
            _sessionPassword = password;
            if (gameSessionManager != null)
            {
                gameSessionManager.SetSessionPassword(_sessionPassword);
                gameSessionManager.StartNewGame();
            }
            registerPanelAnimator.SetBool(Success, true);
            registerPanelAnimator.SetBool(ShouldPlay, true);
            StartCoroutine(PlayRegisterAnimationAndSwitchPanel());
        }
        else
        {
            Debug.Log("Register failed.");
            registerPanelAnimator.SetBool(Success, false);
            registerPanelAnimator.SetBool(ShouldPlay, true);
            StartCoroutine(ResetRegisterAnimation());
        }
    }
    
    // Save game progress
    public async void OnSaveButton()
    {
        if (accountManager.CurrentAccount != null && gameSessionManager.CurrentGame != null)
        {
            await gameSessionManager.SaveGame(
                accountManager.Username, _sessionPassword, accountManager.CurrentAccount
            );
            Debug.Log("Game saved!");
            gamePanelAnimator.SetBool(Success, true);
            gamePanelAnimator.SetBool(ShouldPlay, true);
            StartCoroutine(ResetSaveAnimation());
            await leaderboardManager.FetchAndDisplayLeaderboard();
        }
        else
        {
            Debug.Log("Save failed.");
            gamePanelAnimator.SetBool(Success, false);
            gamePanelAnimator.SetBool(ShouldPlay, true);
            StartCoroutine(ResetSaveAnimation());
        }
    }
    
    public void OnDisconnectButton()
    {
        accountManager.Logout();
        _sessionPassword = null;
        if (gameSessionManager != null)
        {
            gameSessionManager.ClearSession();
        }
        // Return to login panel
        if (panelManager != null)
        {
            panelManager.CloseCurrent();
            panelManager.OpenPanel(loginPanelAnimator);
        }
    }
    
    public async void OnLeaderboardButton()
    {
        panelManager.CloseCurrent();
        panelManager.OpenPanel(leaderboardPanelAnimator);
        await leaderboardManager.FetchAndDisplayLeaderboard();
    }
    
    private System.Collections.IEnumerator PlayRegisterAnimationAndSwitchPanel()
    {
        yield return new WaitForSeconds(1.0f); // Match your animation length
        registerPanelAnimator.SetBool(ShouldPlay, false);
        panelManager.CloseCurrent();
        panelManager.OpenPanel(mainMenuPanelAnimator);
    }

    private System.Collections.IEnumerator ResetRegisterAnimation()
    {
        yield return new WaitForSeconds(1.0f); // Match your animation length
        registerPanelAnimator.SetBool(ShouldPlay, false);
    }
    
    private System.Collections.IEnumerator PlayLoginAnimationAndSwitchPanel()
    {
        yield return new WaitForSeconds(1.0f); // Match your animation length
        loginPanelAnimator.SetBool(ShouldPlay, false);
        panelManager.CloseCurrent();
        panelManager.OpenPanel(gamePanelAnimator);
    }
    
    private System.Collections.IEnumerator ResetLoginAnimation()
    {
        yield return new WaitForSeconds(1.0f); // Match your animation length
        loginPanelAnimator.SetBool(ShouldPlay, false);
    }
    
    private System.Collections.IEnumerator ResetSaveAnimation()
    {
        yield return new WaitForSeconds(1.0f); // Match your animation length
        gamePanelAnimator.SetBool(ShouldPlay, false);
    }
}