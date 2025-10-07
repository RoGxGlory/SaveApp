// Classe représentant la logique du jeu, la gestion de la partie et la sauvegarde chiffrée
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SaveApp;

public class SaveData
{
    [BsonId]
    [BsonIgnoreIfDefault]
    public ObjectId Id { get; set; } // Ignoré si ObjectId.Empty
    public string Username { get; set; }
    public string Salt { get; set; }
    public string Nonce { get; set; }
    public string Tag { get; set; }
    public string Data { get; set; }
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

    // Sauvegarde chiffrée de la partie dans MongoDB
    public static void SaveEncrypted(Game game, string username, string password, Account currentAccount = null, List<Account> allAccounts = null)
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
        var collection = MongoService.Database.GetCollection<SaveData>("saves");
        var filter = Builders<SaveData>.Filter.Eq(x => x.Username, username);
        collection.ReplaceOne(filter, save, new ReplaceOptions { IsUpsert = true });
        // Synchronisation du score du compte
        if (currentAccount != null && allAccounts != null)
        {
            currentAccount.Score = game.Score;
            currentAccount.ScoreDateUtc = DateTime.UtcNow;
            // Mise à jour de la signature d'intégrité du score
            currentAccount.ScoreSignature = Account.GenerateScoreSignature(currentAccount.Score, typeof(Account).GetField("ServerSecretKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.GetValue(null)?.ToString() ?? "SuperSecretKeyChangeMe!");
            Account.SaveAccounts(allAccounts);
        }
    }

    // Charge une sauvegarde chiffrée depuis MongoDB
    public static Game LoadEncrypted(string username, string password)
    {
        var collection = MongoService.Database.GetCollection<SaveData>("saves");
        var filter = Builders<SaveData>.Filter.Eq(x => x.Username, username);
        var save = collection.Find(filter).FirstOrDefault();
        if (save == null)
            return new Game();
        try
        {
            byte[] salt = Convert.FromBase64String(save.Salt);
            byte[] nonce = Convert.FromBase64String(save.Nonce);
            byte[] tag = Convert.FromBase64String(save.Tag);
            byte[] ciphertext = Convert.FromBase64String(save.Data);
            var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256).GetBytes(32);
            using var aes = new AesGcm(key);
            byte[] plaintext = new byte[ciphertext.Length];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            string json = Encoding.UTF8.GetString(plaintext);
            return JsonSerializer.Deserialize<Game>(json) ?? new Game();
        }
        catch
        {
            // Si le contenu est corrompu ou le mot de passe incorrect, retourne une nouvelle partie
            return new Game();
        }
    }
}