using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SaveApp;

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
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    // Number of monsters killed by the player
    [JsonPropertyName("monstersKilled")]
    public int MonstersKilled { get; set; }
    // Total distance traveled by the player
    [JsonPropertyName("distanceTraveled")]
    public int DistanceTraveled { get; set; }
    // Date of the score
    [JsonPropertyName("scoreDateUtc")]
    public string ScoreDateUtc { get; set; } = string.Empty;
    // Data integrity signature
    [JsonPropertyName("integrity")]
    public string Integrity { get; set; } = string.Empty;
}

public static class ApiClient
{
    // HTTP client for API requests
    private static readonly HttpClient HttpClient = new();
    // Base URL for the API endpoints
    private static readonly string ApiBaseUrl = "https://saveapp-r3dt.onrender.com/api";

    /// <summary>
    /// Retrieves the list of all accounts from the API.
    /// </summary>
    public static async Task<List<Account>> GetAccountsAsync()
    {
        var response = await HttpClient.GetAsync($"{ApiBaseUrl}/account/list");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Account>>(json) ?? new List<Account>();
    }

    /// <summary>
    /// Saves the provided list of accounts to the API.
    /// </summary>
    public static async Task SaveAccountsAsync(List<Account> accounts)
    {
        var payload = new AccountSaveRequest(accounts);
        var response = await HttpClient.PostAsJsonAsync($"{ApiBaseUrl}/account/save", payload);
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
        return JsonSerializer.Deserialize<List<LeaderboardEntry>>(json) ?? new List<LeaderboardEntry>();
    }

    /// <summary>
    /// Saves the game data to the API.
    /// </summary>
    public static async Task SaveGameAsync(SaveData save)
    {
        var response = await HttpClient.PostAsJsonAsync($"{ApiBaseUrl}/game/save", save);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Loads the game data for a specific user.
    /// </summary>
    public static async Task<SaveData?> LoadGameAsync(string username, string password)
    {
        var payload = new { Username = username, Password = password };
        var response = await HttpClient.PostAsJsonAsync($"{ApiBaseUrl}/game/load", payload);
        // Console.WriteLine($"LoadGame response status: {response.StatusCode}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            // Console.WriteLine($"LoadGame error: {error}");
            return null;
        }
        var json = await response.Content.ReadAsStringAsync();
        // Console.WriteLine($"LoadGame json: {json}");
        var save = JsonSerializer.Deserialize<SaveData>(json);
        // Console.WriteLine($"Deserialized save: Username={save?.Username}, Data length={save?.Data?.Length}");
        return save;
    }

    /// <summary>
    /// Logs in a user with the provided credentials.
    /// </summary>
    public static async Task<LoginResult> LoginAsync(string identifier, string password, bool isEmail = false)
    {
        LoginPayload payload;
        if (isEmail)
            payload = new LoginPayload { Email = identifier, Password = password };
        else
            payload = new LoginPayload { Username = identifier, Password = password };
        var response = await HttpClient.PostAsJsonAsync($"{ApiBaseUrl}/account/login", payload);
        if (!response.IsSuccessStatusCode)
        {
            return new LoginResult { Success = false };
        }
        var account = await response.Content.ReadFromJsonAsync<Account>();
        return new LoginResult { Success = true, Account = account };
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    public static async Task<RegisterResult> RegisterAsync(string username, string password)
    {
        var payload = new { Username = username, Password = password };
        var response = await HttpClient.PostAsJsonAsync($"{ApiBaseUrl}/account/register", payload);
        if (!response.IsSuccessStatusCode)
        {
            return new RegisterResult { Success = false };
        }
        var account = await response.Content.ReadFromJsonAsync<Account>();
        return new RegisterResult { Success = true, Account = account };
    }

    /// <summary>
    /// Saves the account data, including progression information.
    /// </summary>
    public static async Task SaveAccountAsync(string username, int monstersKilled, int distanceTraveled, DateTime endDate)
    {
        var payload = new { Username = username, MonstersKilled = monstersKilled, DistanceTraveled = distanceTraveled, EndDate = endDate };
        try
        {
            var response = await HttpClient.PostAsJsonAsync($"{ApiBaseUrl}/account/save", payload);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Progression inférieure à celle sur le serveur : Données non sauvegardées. Détails : {ex.Message}");
            // Optionally: log the error or notify the user
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
            return null; // Returns null if the list is empty or null
        }

        // Filter out null accounts before accessing their properties
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
