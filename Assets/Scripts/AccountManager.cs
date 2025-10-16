using UnityEngine;
using SaveAppCore;
using System.Threading.Tasks;

public class AccountManager : MonoBehaviour
{
    public Account CurrentAccount { get; private set; }
    public string Username => CurrentAccount?.Username ?? "";

    public async Task<bool> Login(string identifier, string password)
    {
        object loginResult;
        if (identifier.Contains("@"))
        {
            loginResult = await ApiClient.LoginByEmailAsync(identifier, password); // ApiResult
        }
        else
        {
            loginResult = await ApiClient.LoginAsync(identifier, password); // LoginResult
        }

        // Debug and handle both result types
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

    public void Logout()
    {
        CurrentAccount = null;
    }
}