using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SaveAppCore;

/// <summary>
/// Handles API communication for account, leaderboard, and game data
/// </summary>
public class AccountSaveRequest
{
    // List of accounts to be sent in the API request
    public List<Account> Dto { get; set; }
    // Constructor for AccountSaveRequest
    public AccountSaveRequest(List<Account> dto) => Dto = dto;
}

public class LeaderboardEntry
{
    // Username of the player
    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;
    // Number of monsters killed by the player
    [JsonProperty("monstersKilled")]
    public int MonstersKilled { get; set; }
    // Total distance traveled by the player
    [JsonProperty("distanceTraveled")]
    public int DistanceTraveled { get; set; }
    // Date of the score
    [JsonProperty("scoreDateUtc")]
    public string ScoreDateUtc { get; set; } = string.Empty;
    // Data integrity signature
    [JsonProperty("integrity")]
    public string Integrity { get; set; } = string.Empty;
}

public static class ApiClient
{
    // HTTP client for API requests
    private static readonly HttpClient HttpClient = new HttpClient();
    // Base URL for the API endpoints
    private static readonly string ApiBaseUrl = "https://saveapp-r3dt.onrender.com/api";

    private static StringContent JsonContent(object payload)
        => new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

    private static async Task<T?> ParseJsonAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// Retrieves the list of all accounts from the API.
    /// </summary>
    public static async Task<List<Account>> GetAccountsAsync()
    {
        var response = await HttpClient.GetAsync($"{ApiBaseUrl}/account/list");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<Account>>(json) ?? new List<Account>();
    }

    /// <summary>
    /// Saves the provided list of accounts to the API.
    /// </summary>
    public static async Task SaveAccountsAsync(List<Account> accounts)
    {
        var payload = new AccountSaveRequest(accounts);
        var response = await HttpClient.PostAsync($"{ApiBaseUrl}/account/save", JsonContent(payload));
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
            throw new HttpRequestException($"API Error: {response.StatusCode} - {errorContent}");
        }
    }

    /// <summary>
    /// Retrieves the leaderboard data from the API.
    /// </summary>
    public static async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
    {
        var response = await HttpClient.GetAsync($"{ApiBaseUrl}/leaderboard");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<LeaderboardEntry>>(json) ?? new List<LeaderboardEntry>();
    }

    /// <summary>
    /// Saves the game data to the API.
    /// </summary>
    public static async Task SaveGameAsync(SaveData save)
    {
        var response = await HttpClient.PostAsync($"{ApiBaseUrl}/game/save", JsonContent(save));
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Loads the game data for a specific user.
    /// </summary>
    public static async Task<SaveData?> LoadGameAsync(string username, string password)
    {
        var payload = new { Username = username, Password = password };
        var response = await HttpClient.PostAsync($"{ApiBaseUrl}/game/load", JsonContent(payload));
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return null;
        }
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<SaveData>(json);
    }

    /// <summary>
    /// Logs in a user with the provided credentials.
    /// </summary>
    public static async Task<LoginResult> LoginAsync(string identifier, string password, bool isEmail = false)
    {
        object payload = isEmail ? new { Email = identifier, Password = password } : new { Username = identifier, Password = password };
        var response = await HttpClient.PostAsync($"{ApiBaseUrl}/account/login", JsonContent(payload));
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Login error: {response.StatusCode} - {error}");
            return new LoginResult { Success = false };
        }
        var account = await ParseJsonAsync<Account>(response);
        return new LoginResult { Success = true, Account = account };
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    public static async Task<RegisterResult> RegisterAsync(string username, string password, string email)
    {
        var payload = new { Username = username, Password = password, Email = email };
        var response = await HttpClient.PostAsync($"{ApiBaseUrl}/account/register", JsonContent(payload));
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API Register error: {response.StatusCode} - {error}");
            return new RegisterResult { Success = false };
        }
        var account = await ParseJsonAsync<Account>(response);
        return new RegisterResult { Success = true, Account = account };
    }

    /// <summary>
    /// Saves the account data, including progression information.
    /// </summary>
    public static async Task SaveAccountAsync(string username, string email, int monstersKilled, int distanceTraveled)
    {
        var payload = new { Username = username, Email = email, MonstersKilled = monstersKilled, DistanceTraveled = distanceTraveled };
        try
        {
            var response = await HttpClient.PostAsync($"{ApiBaseUrl}/account/save", JsonContent(payload));
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Progression inférieure à celle sur le serveur : Données non sauvegardées. Détails : {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves a specific account by username.
    /// </summary>
    public static async Task<Account?> GetAccountAsync(string username)
    {
        var accounts = await GetAccountsAsync();
        if (accounts == null || accounts.Count == 0)
        {
            return null;
        }
        return accounts
            .Where(account => account != null)
            .FirstOrDefault(account => account.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sorts the leaderboard entries by monsters killed and distance traveled.
    /// </summary>
    public static List<LeaderboardEntry> SortLeaderboard(List<LeaderboardEntry> leaderboard)
    {
        return leaderboard
            .OrderByDescending(entry => entry.MonstersKilled)
            .ThenByDescending(entry => entry.DistanceTraveled)
            .ToList();
    }
    
    public static async Task<ApiResult> LoginByEmailAsync(string email, string password)
    {
        var payload = new { Email = email, Password = password };
        var response = await HttpClient.PostAsync($"{ApiBaseUrl}/account/login", JsonContent(payload));
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return new ApiResult
            {
                Success = false,
                Account = null,
                ErrorMessage = $"API Login error: {response.StatusCode} - {error}"
            };
        }
        var account = await ParseJsonAsync<Account>(response);
        return new ApiResult
        {
            Success = true,
            Account = account,
            ErrorMessage = string.Empty
        };
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public Account? Account { get; set; }
}

public class RegisterResult
{
    public bool Success { get; set; }
    public Account? Account { get; set; }
}

public class LoginPayload
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
}

public class ApiResult
{
    public bool Success { get; set; }
    public Account Account { get; set; }
    public string ErrorMessage { get; set; }
}