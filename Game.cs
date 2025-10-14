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

public class Arena
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Adventurer Player { get; set; }
    public List<Monster> Monsters { get; set; }
    public List<Item> Items { get; set; }
    [JsonIgnore]
    public bool[,] Explored { get; set; }
    private static Random _rng = new Random();

    public Arena(int width, int height)
    {
        Width = width;
        Height = height;
        Explored = new bool[width, height];
        Player = new Adventurer { X = width / 2, Y = height / 2 };
        Monsters = new List<Monster>();
        Items = new List<Item>();
        GenerateMonsters();
        GenerateItems();
    }

    private void GenerateMonsters()
    {
        int count = _rng.Next(3, 7);
        for (int i = 0; i < count; i++)
        {
            Monsters.Add(new Monster
            {
                X = _rng.Next(Width),
                Y = _rng.Next(Height),
                Health = _rng.Next(5, 15),
                Attack = _rng.Next(1, 4) // Attaque réduite
            });
        }
    }

    private void GenerateItems()
    {
        int count = _rng.Next(3, 7);
        for (int i = 0; i < count; i++)
        {
            Items.Add(new Item
            {
                X = _rng.Next(Width),
                Y = _rng.Next(Height),
                Type = (ItemType)_rng.Next(0, 2)
            });
        }
    }

    public bool IsMonsterAt(int x, int y) => Monsters.Any(m => m.X == x && m.Y == y && m.Health > 0);
    public Monster? GetMonsterAt(int x, int y) => Monsters.FirstOrDefault(m => m.X == x && m.Y == y && m.Health > 0);
    public bool IsItemAt(int x, int y) => Items.Any(i => i.X == x && i.Y == y && !i.PickedUp);
    public Item? GetItemAt(int x, int y) => Items.FirstOrDefault(i => i.X == x && i.Y == y && !i.PickedUp);

    public void MoveMonsters()
    {
        foreach (var monster in Monsters.Where(m => m.Health > 0))
        {
            int dx = Player.X - monster.X;
            int dy = Player.Y - monster.Y;
            int newX = monster.X;
            int newY = monster.Y;
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                if (dx > 0) newX++;
                else if (dx < 0) newX--;
            }
            else if (dy != 0)
            {
                if (dy > 0) newY++;
                else if (dy < 0) newY--;
            }
            // Vérifie que la nouvelle position est dans l'arène et non occupée par un autre monstre
            if (newX >= 0 && newX < Width && newY >= 0 && newY < Height && !Monsters.Any(m => m != monster && m.X == newX && m.Y == newY && m.Health > 0))
            {
                monster.X = newX;
                monster.Y = newY;
            }
        }
    }

    public string GetVisual()
    {
        var sb = new StringBuilder();
        // Ligne du haut
        sb.Append('#', Width + 2).AppendLine();
        for (int y = 0; y < Height; y++)
        {
            sb.Append('#');
            for (int x = 0; x < Width; x++)
            {
                if (Player.X == x && Player.Y == y)
                    sb.Append('@');
                else if (Monsters.Any(m => m.X == x && m.Y == y && m.Health > 0))
                    sb.Append('M');
                else if (Items.Any(i => i.X == x && i.Y == y && !i.PickedUp))
                    sb.Append('?');
                else
                    sb.Append('.');
            }
            sb.Append('#').AppendLine();
        }
        // Ligne du bas
        sb.Append('#', Width + 2).AppendLine();
        return sb.ToString();
    }
}

public class Adventurer
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Health { get; set; } = 40; // PV augmentés
    public int Attack { get; set; } = 3;
    public List<Item> Inventory { get; set; } = new();
}

public class Monster
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Health { get; set; }
    public int Attack { get; set; }
}

public enum ItemType
{
    Potion,
    Treasure
}

public class Item
{
    public int X { get; set; }
    public int Y { get; set; }
    public ItemType Type { get; set; }
    public bool PickedUp { get; set; } = false;
}

public class Game
{
    private const int DefaultWidth = 24;
    private const int DefaultHeight = 8;
    public Arena Arena { get; set; }
    public bool InProgress { get; set; }
    public int Turn { get; set; }
    public int MonstersKilled { get; set; }
    public int DistanceTraveled { get; set; }
    private static Random _rng = new Random();

    public Game()
    {
        Arena = new Arena(DefaultWidth, DefaultHeight);
        InProgress = false;
        Turn = 0;
        MonstersKilled = 0;
        DistanceTraveled = 0;
    }

    public void StartNewGame()
    {
        Arena = new Arena(DefaultWidth, DefaultHeight);
        InProgress = true;
        Turn = 0;
        MonstersKilled = 0;
        DistanceTraveled = 0;
    }

    public string PlayTurn(string action, string username, string password, Account? currentAccount = null, List<Account>? allAccounts = null)
    {
        if (!InProgress)
            return "Aucune partie en cours. Lancez une nouvelle partie.\n";
        string result = "";
        bool actionEffectuee = false;
        switch (action.ToLower())
        {
            case "haut":
            case "bas":
            case "gauche":
            case "droite":
                var moveResult = MovePlayer(action);
                result = moveResult.Item1;
                actionEffectuee = moveResult.Item2;
                break;
            case "combattre":
                result = FightMonster();
                actionEffectuee = true;
                break;
            case "ramasser":
                result = PickupItem();
                actionEffectuee = true;
                break;
            default:
                result = "Action inconnue.\n";
                break;
        }
        if (actionEffectuee)
        {
            Turn++;
            Arena.MoveMonsters();
            var monsterOnPlayer = Arena.GetMonsterAt(Arena.Player.X, Arena.Player.Y);
            if (monsterOnPlayer != null && monsterOnPlayer.Health > 0)
            {
                Arena.Player.Health -= monsterOnPlayer.Attack;
                result += $"Un monstre vous attaque automatiquement (-{monsterOnPlayer.Attack} PV) !\n";
            }
            if (Arena.Player.Health <= 0)
            {
                InProgress = false;
                result += $"\nVous êtes mort ! Monstres tués : {MonstersKilled} | Distance parcourue : {DistanceTraveled}\n";
                // Sauvegarde automatique à la mort du joueur
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    _ = Game.SaveEncryptedAsync(this, username, password, currentAccount, allAccounts);
                }
            }
        }
        return result;
    }

    private (string, bool) MovePlayer(string direction)
    {
        int x = Arena.Player.X;
        int y = Arena.Player.Y;
        switch (direction.ToLower())
        {
            case "haut": y--; break;
            case "bas": y++; break;
            case "gauche": x--; break;
            case "droite": x++; break;
        }
        if (x < 0 || x >= Arena.Width || y < 0 || y >= Arena.Height)
            return ("Impossible de sortir de l'arène !\n", false);
        Arena.Player.X = x;
        Arena.Player.Y = y;
        Arena.Explored[x, y] = true;
        DistanceTraveled++; // incrémente à chaque déplacement valide
        string msg = $"Vous vous déplacez vers ({x},{y}). Distance parcourue : {DistanceTraveled}\n";
        if (Arena.IsItemAt(x, y))
            msg += "Il y a quelque chose ici...\n";
        return (msg, true);
    }

    private string FightMonster()
    {
        var monster = Arena.GetMonsterAt(Arena.Player.X, Arena.Player.Y);
        if (monster == null)
            return "Aucun monstre ici.\n";
        monster.Health -= Arena.Player.Attack;
        if (monster.Health > 0)
        {
            Arena.Player.Health -= monster.Attack;
            return $"Vous blessez le monstre ! Il riposte (-{monster.Attack} PV).\n";
        }
        else
        {
            MonstersKilled++;
            return "Monstre vaincu !\n";
        }
    }

    private string PickupItem()
    {
        var item = Arena.GetItemAt(Arena.Player.X, Arena.Player.Y);
        if (item == null)
            return "Aucun objet ici.\n";
        item.PickedUp = true;
        Arena.Player.Inventory.Add(item);
        switch (item.Type)
        {
            case ItemType.Potion:
                Arena.Player.Health += 10; // Potion plus efficace
                return "Vous ramassez une potion (+10 PV).\n";
            case ItemType.Treasure:
                MonstersKilled++;
                return "Vous ramassez un trésor (+1 monstre tué fictif).\n";
            default:
                return "Objet ramassé.\n";
        }
    }

    public static async Task SaveEncryptedAsync(Game game, string username, string password, Account? currentAccount = null, List<Account>? allAccounts = null)
    {
        string json = JsonSerializer.Serialize(game);
        byte[] plaintext = Encoding.UTF8.GetBytes(json);
        byte[] salt = RandomNumberGenerator.GetBytes(16); // 128 bits
        var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256).GetBytes(32); // 256 bits
        using var aes = new AesGcm(key, 16); // tag size 16 bytes
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
        // Sauvegarde locale
        if (currentAccount != null)
        {
            // Sauvegarde sur le serveur
            currentAccount.MonstersKilled = game.MonstersKilled;
            currentAccount.DistanceTraveled = game.DistanceTraveled;
            currentAccount.MonstersKilledDateUtc = DateTime.UtcNow;
            currentAccount.MonstersKilledSignature = Account.GenerateMonstersKilledSignature(
                currentAccount.MonstersKilled, Account.GetServerSecretKey());

            await ApiClient.SaveAccountAsync(username, currentAccount.MonstersKilled, currentAccount.DistanceTraveled, DateTime.Now);
        }
    }

    public static async Task<Game> LoadEncryptedAsync(string username, string password)
    {
        var save = await ApiClient.LoadGameAsync(username, password);
        if (save == null)
            return new Game();
        try
        {
            byte[] salt = Convert.FromBase64String(save.Salt);
            byte[] nonce = Convert.FromBase64String(save.Nonce);
            byte[] tag = Convert.FromBase64String(save.Tag);
            byte[] ciphertext = Convert.FromBase64String(save.Data);
            var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256).GetBytes(32);
            using var aes = new AesGcm(key, 16);
            byte[] plaintext = new byte[ciphertext.Length];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            string json = Encoding.UTF8.GetString(plaintext);
            var deserializedGame = JsonSerializer.Deserialize<Game>(json);
            return deserializedGame ?? new Game();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Decryption failed: " + ex.Message);
            return new Game();
        }
    }

    public static void SaveLocal(Game game, Account account, string filePath)
    {
        string savesDirectory = Path.Combine(AppContext.BaseDirectory, "Saves");
        if (!Directory.Exists(savesDirectory))
        {
            Directory.CreateDirectory(savesDirectory);
        }
        string fullPath = Path.Combine(savesDirectory, filePath);
        var data = new {
            Game = game,
            Account = account
        };
        var json = JsonSerializer.Serialize(data);
        File.WriteAllText(fullPath, json);
    }

    public static async Task SyncLocalIfValid(string filePath)
    {
        if (!File.Exists(filePath)) return;
        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<LocalSaveData>(json);
        if (data == null) return;
        // Vérifie la signature
        if (Account.VerifyMonstersKilledSignature(
            data.Account.MonstersKilled,
            data.Account.MonstersKilledSignature,
            Account.GetServerSecretKey()))
        {
            // Envoie au serveur
            var serverAccount = await ApiClient.GetAccountAsync(data.Account.Username);
            if (serverAccount != null &&
                data.Account.MonstersKilled > serverAccount.MonstersKilled &&
                data.Account.DistanceTraveled > serverAccount.DistanceTraveled)
            {
                await ApiClient.SaveAccountAsync(
                    data.Account.Username,
                    data.Account.MonstersKilled,
                    data.Account.DistanceTraveled,
                    data.Account.MonstersKilledDateUtc);
            }
        }
    }

    public class LocalSaveData
    {
        public Game Game { get; set; }
        public Account Account { get; set; }
    }
}
