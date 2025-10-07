// Classe représentant la logique du jeu, la gestion de la partie et la sauvegarde chiffrée

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SaveApp;

public class SaveData
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    [JsonPropertyName("salt")]
    public string Salt { get; set; } = string.Empty;
    [JsonPropertyName("nonce")]
    public string Nonce { get; set; } = string.Empty;
    [JsonPropertyName("tag")]
    public string Tag { get; set; } = string.Empty;
    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
}

public class Game
{
    // Score total du joueur (nombre de parties gagnées)
    public int Score { get; set; }
    // Nombre à deviner dans la partie en cours
    public int NumberToGuess { get; set; }
    // Nombre d'essais effectués dans la partie en cours
    public int Attempts { get; set; }
    // Indique si une partie est en cours
    public bool InProgress { get; set; }
    // Générateur de nombres aléatoires pour le jeu
    private static Random _rng = new Random();

    // Constructeur par défaut (nouvelle partie vierge)
    public Game()
    {
        Score = 0;
        Attempts = 0;
        InProgress = false;
        NumberToGuess = 0;
    }

    // Démarre une nouvelle partie (génère un nouveau nombre à deviner)
    public void StartNewGame()
    {
        NumberToGuess = _rng.Next(1, 101); // 1 à 100
        Attempts = 0;
        InProgress = true;
    }

    // Logique de jeu : traite une proposition de l'utilisateur
    public string Play(int guess)
    {
        if (!InProgress)
            return "Aucune partie en cours. Lancez une nouvelle partie.\n";
        Attempts++;
        if (guess < NumberToGuess)
            return "Trop petit !\n";
        if (guess > NumberToGuess)
            return "Trop grand !\n";
        InProgress = false;
        Score++;
        return $"Bravo ! Vous avez trouvé en {Attempts} essais.\n";
    }

    // Sauvegarde chiffrée de la partie via l'API
    public static async Task SaveEncryptedAsync(Game game, string username, string password, Account currentAccount, List<Account> allAccounts)
    {
        string json = JsonSerializer.Serialize(game);
        byte[] plaintext = Encoding.UTF8.GetBytes(json);
        byte[] salt = RandomNumberGenerator.GetBytes(16); // 128 bits
        var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256).GetBytes(32); // 256 bits
        using var aes = new AesGcm(key);
        byte[] nonce = RandomNumberGenerator.GetBytes(12); // 96 bits
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[16]; // 128 bits
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        var save = new SaveData
        {
            Username = username,
            Salt = Convert.ToBase64String(salt),
            Nonce = Convert.ToBase64String(nonce),
            Tag = Convert.ToBase64String(tag),
            Data = Convert.ToBase64String(ciphertext)
        };
        await ApiClient.SaveGameAsync(save);
        // Synchronisation du score du compte
        if (currentAccount != null && allAccounts != null)
        {
            currentAccount.Score = game.Score;
            currentAccount.ScoreDateUtc = DateTime.UtcNow;
            currentAccount.ScoreSignature = Account.GenerateScoreSignature(currentAccount.Score, Account.GetServerSecretKey());
            await Account.SaveAccountsAsync(allAccounts);
        }
    }

    // Charge une sauvegarde chiffrée via l'API
    public static async Task<Game> LoadEncryptedAsync(string username, string password)
    {
        var save = await ApiClient.LoadGameAsync(username, password);
        if (save == null)
            return new Game();
        try
        {
            // Console.WriteLine("Starting decryption for user: " + username);
            byte[] salt = Convert.FromBase64String(save.Salt);
            byte[] nonce = Convert.FromBase64String(save.Nonce);
            byte[] tag = Convert.FromBase64String(save.Tag);
            byte[] ciphertext = Convert.FromBase64String(save.Data);
            var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256).GetBytes(32);
            using var aes = new AesGcm(key);
            byte[] plaintext = new byte[ciphertext.Length];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            string json = Encoding.UTF8.GetString(plaintext);
            // Console.WriteLine("Decryption successful, json = " + json);
            var deserializedGame = JsonSerializer.Deserialize<Game>(json);
            // Console.WriteLine("Deserialized game.Score = " + deserializedGame?.Score);
            return deserializedGame ?? new Game();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Decryption failed: " + ex.Message);
            return new Game();
        }
    }
}