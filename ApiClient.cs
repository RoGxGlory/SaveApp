using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SaveApp;

public class AccountSaveRequest
{
    public List<Account> Dto { get; set; }
    public AccountSaveRequest(List<Account> dto) => Dto = dto;
}

public class LeaderboardEntry
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    [JsonPropertyName("score")]
    public int Score { get; set; }
    [JsonPropertyName("scoreDateUtc")]
    public string ScoreDateUtc { get; set; } = string.Empty;
    [JsonPropertyName("integrity")]
    public string Integrity { get; set; } = string.Empty;
}

public static class ApiClient
{
    private static readonly HttpClient HttpClient = new();
    private static readonly string ApiBaseUrl = "https://saveapp-r3dt.onrender.com/api";

    public static async Task<List<Account>> GetAccountsAsync()
    {
        var response = await HttpClient.GetAsync($"{ApiBaseUrl}/account/list");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Account>>(json) ?? new List<Account>();
    }

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

    public static async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
    {
        var response = await HttpClient.GetAsync($"{ApiBaseUrl}/leaderboard");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<LeaderboardEntry>>(json) ?? new List<LeaderboardEntry>();
    }

    public static async Task SaveGameAsync(SaveData save)
    {
        var response = await HttpClient.PostAsJsonAsync($"{ApiBaseUrl}/game/save", save);
        response.EnsureSuccessStatusCode();
    }

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

    public static async Task<LoginResult> LoginAsync(string username, string password)
    {
        var payload = new { Username = username, Password = password };
        var response = await HttpClient.PostAsJsonAsync($"{ApiBaseUrl}/account/login", payload);
        if (!response.IsSuccessStatusCode)
        {
            return new LoginResult { Success = false };
        }
        var account = await response.Content.ReadFromJsonAsync<Account>();
        return new LoginResult { Success = true, Account = account };
    }

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

    public static async Task SaveAccountAsync(string username, int score)
    {
        var payload = new { Username = username, Score = score };
        var response = await HttpClient.PostAsJsonAsync($"{ApiBaseUrl}/account/save", payload);
        response.EnsureSuccessStatusCode();
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
