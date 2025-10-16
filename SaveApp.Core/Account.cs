// Class representing a user account with storage and retrieval management

namespace SaveAppCore;

/// <summary>
/// Represents a user account, including credentials, progression, and integrity data.
/// </summary>
public class Account
{
    // Simple identifier (no MongoDB attributes on client)
    public string Id { get; set; } = string.Empty;
    // Username chosen by the user
    public string Username { get; set; } = string.Empty;
    // Email address of the user
    public string Email { get; set; } = string.Empty;
    // Password hash (base64 encoded)
    public string PasswordHashB64 { get; set; } = string.Empty;
    // Salt used for password hashing (base64 encoded)
    public string SaltB64 { get; set; } = string.Empty;
    // UTC creation date of the account
    public DateTime CreatedUtc { get; set; }
    // Number of monsters killed by the user
    public int MonstersKilled { get; set; }
    // Total distance traveled by the user
    public int DistanceTraveled { get; set; }
    // Date of last monsters killed update (UTC)
    public DateTime MonstersKilledDateUtc { get; set; }
    // HMAC signature for monsters killed
    public string MonstersKilledSignature { get; set; } = string.Empty;
    // Data integrity field
    public string Integrity { get; set; } = string.Empty;

    // Default constructor
    public Account() { }

    // Constructor to create an account with username, hash and salt
    public Account(string username, string hashB64, string saltB64)
    {
        Username = username;
        PasswordHashB64 = hashB64;
        SaltB64 = saltB64;
        CreatedUtc = DateTime.UtcNow;
        MonstersKilled = 0;
        DistanceTraveled = 0;
        MonstersKilledDateUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Loads all accounts from the API.
    /// </summary>
    public static async Task<List<Account>> LoadAccountsAsync()
    {
        return await ApiClient.GetAccountsAsync();
    }

    /// <summary>
    /// Saves all accounts via the API.
    /// </summary>
    public static async Task SaveAccountsAsync(List<Account> accounts)
    {
        await ApiClient.SaveAccountsAsync(accounts);
    }

    // HMAC key for public deployment (cached)
    private static string? _cachedSecretKey = null;

    /// <summary>
    /// Gets the server secret key from environment variable or fallback.
    /// </summary>
    public static string GetServerSecretKey()
    {
        if (_cachedSecretKey == null)
        {
            _cachedSecretKey = Environment.GetEnvironmentVariable("SERVER_SECRET_KEY") ?? "default_fallback_key";
        }
        return _cachedSecretKey;
    }

    /// <summary>
    /// Generates the HMAC signature for MonstersKilled.
    /// </summary>
    public static string GenerateMonstersKilledSignature(int MonstersKilled, string secretKey)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secretKey));
        var MonstersKilledBytes = BitConverter.GetBytes(MonstersKilled);
        var hash = hmac.ComputeHash(MonstersKilledBytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Verifies the HMAC signature for MonstersKilled.
    /// </summary>
    public static bool VerifyMonstersKilledSignature(int MonstersKilled, string signature, string secretKey)
    {
        var expected = GenerateMonstersKilledSignature(MonstersKilled, secretKey);
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(signature),
            Convert.FromBase64String(expected)
        );
    }
}