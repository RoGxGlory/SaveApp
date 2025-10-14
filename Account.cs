// Classe représentant un compte utilisateur avec gestion du stockage et de la récupération

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SaveApp;

namespace SaveApp;

public class Account
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string PasswordHashB64 { get; set; } = default!;
    public string SaltB64 { get; set; } = default!;
    public DateTime CreatedUtc { get; set; }
    public int MonstersKilled { get; set; }
    public int DistanceTraveled { get; set; }
    public DateTime MonstersKilledDateUtc { get; set; }
    public string MonstersKilledSignature { get; set; } = string.Empty;
    public string Integrity { get; set; } = string.Empty;

    // Constructeur par défaut
    public Account() { }

    // Constructeur pour créer un compte avec nom, hash et sel
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

    // Load all accounts from the API
    public static async Task<List<Account>> LoadAccountsAsync()
    {
        return await ApiClient.GetAccountsAsync();
    }

    // Save all accounts via the API
    public static async Task SaveAccountsAsync(List<Account> accounts)
    {
        await ApiClient.SaveAccountsAsync(accounts);
    }

    // HMAC key for public deployment
    private static string? _cachedSecretKey = null;

    // Get the server secret key from environment variable
    public static string GetServerSecretKey()
    {
        if (_cachedSecretKey == null)
        {
            _cachedSecretKey = Environment.GetEnvironmentVariable("SERVER_SECRET_KEY") ?? "default_fallback_key";
        }
        return _cachedSecretKey;
    }

    // Génère la signature HMAC du MonstersKilled
    public static string GenerateMonstersKilledSignature(int MonstersKilled, string secretKey)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secretKey));
        var MonstersKilledBytes = BitConverter.GetBytes(MonstersKilled);
        var hash = hmac.ComputeHash(MonstersKilledBytes);
        return Convert.ToBase64String(hash);
    }

    // Vérifie la signature HMAC du MonstersKilled
    public static bool VerifyMonstersKilledSignature(int MonstersKilled, string signature, string secretKey)
    {
        var expected = GenerateMonstersKilledSignature(MonstersKilled, secretKey);
        return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(signature),
            Convert.FromBase64String(expected)
        );
    }
}