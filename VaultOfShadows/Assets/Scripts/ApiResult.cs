using SaveAppCore;

public class ApiResult
{
    public bool Success { get; set; }
    public Account Account { get; set; }
    public string ErrorMessage { get; set; }

    public ApiResult() { }

    public ApiResult(LoginResult loginResult)
    {
        Success = loginResult.Success;
        Account = loginResult.Account;
        ErrorMessage = loginResult is { Success: false } ? "Login failed." : string.Empty;
    }
}