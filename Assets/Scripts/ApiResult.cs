using SaveAppCore;

/// <summary>
/// Represents the result of an API call, including success status, account info, and error message.
/// </summary>
public class ApiResult
{
    public bool Success { get; set; } // Was the API call successful?
    public Account Account { get; set; } // The account returned by the API (if any)
    public string ErrorMessage { get; set; } // Error message if the call failed

    public ApiResult() { }

    /// <summary>
    /// Constructs an ApiResult from a LoginResult.
    /// </summary>
    public ApiResult(LoginResult loginResult)
    {
        Success = loginResult.Success;
        Account = loginResult.Account;
        ErrorMessage = loginResult is { Success: false } ? "Login failed." : string.Empty;
    }
}