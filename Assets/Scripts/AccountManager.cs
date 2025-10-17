using UnityEngine;
using SaveAppCore;
using System.Threading.Tasks;

/// <summary>
/// Manages user account authentication, registration, and session state.
/// </summary>
public class AccountManager : MonoBehaviour
{
    // The currently logged-in account
    public Account CurrentAccount { get; private set; }
    // The username of the current account (empty if not logged in)
    public string Username => CurrentAccount?.Username ?? "";

    /// <summary>
    /// Attempts to log in with the given identifier (username or email) and password.
    /// </summary>
    public async Task<bool> Login(string identifier, string password)
    {
        object loginResult;
        // Use email login if identifier contains '@', otherwise use username login
        if (identifier.Contains("@"))
        {
            loginResult = await ApiClient.LoginByEmailAsync(identifier, password); // ApiResult
        }
        else
        {
            loginResult = await ApiClient.LoginAsync(identifier, password); // LoginResult
        }

        // Handle both possible result types
        if (loginResult is SaveAppCore.ApiResult apiRes)
        {
            Debug.Log($"ApiResult: Success={apiRes.Success}, Account={apiRes.Account}");
            if (apiRes.Success && apiRes.Account != null)
            {
                CurrentAccount = apiRes.Account;
                return true;
            }
        }
        else if (loginResult is SaveAppCore.LoginResult loginRes)
        {
            Debug.Log($"LoginResult: Success={loginRes.Success}, Account={loginRes.Account}");
            if (loginRes.Success && loginRes.Account != null)
            {
                CurrentAccount = loginRes.Account;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Attempts to register a new account with the given username, password, and email.
    /// </summary>
    public async Task<bool> Register(string username, string password, string email)
    {
        var result = await ApiClient.RegisterAsync(username, password, email);
        if (result.Success && result.Account != null)
        {
            CurrentAccount = result.Account;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Logs out the current account and clears session state.
    /// </summary>
    public void Logout()
    {
        CurrentAccount = null;
    }
}