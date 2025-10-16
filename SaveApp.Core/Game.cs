// Class representing the game logic, game management, and encrypted save

using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SaveAppCore;

/// <summary>
/// Data structure for storing encrypted game save information.
/// </summary>
public class SaveData
{
    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;
    [JsonProperty("salt")]
    public string Salt { get; set; } = string.Empty;
    [JsonProperty("iv")]
    public string IV { get; set; } = string.Empty; // Changed from Nonce/Tag to IV
    [JsonProperty("data")]
    public string Data { get; set; } = string.Empty;
}

public class Door
{
    public int X { get; set; }
    public int Y { get; set; }
    public string TargetRoomId { get; set; }

    public Door(int x, int y, string targetRoomId)
    {
        X = x;
        Y = y;
        TargetRoomId = targetRoomId;
    }
}

/// <summary>
/// Represents the game arena, including player, monsters, items, and exploration state.
/// </summary>
public class Arena
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Adventurer Player { get; set; }
    public List<Monster> Monsters { get; set; }
    public List<Item> Items { get; set; }
    [JsonIgnore]
    public bool[,] Explored { get; set; }
    public string RoomId { get; set; }
    private static Random _rng = new Random();
    public Door RoomDoor { get; set; }

    /// <summary>
    /// Initializes a new arena with monsters and items.
    /// </summary>
    public Arena(int width, int height, string id = null, string targetRoomId = null)
    {
        Width = width;
        Height = height;
        RoomId = id ?? Guid.NewGuid().ToString();
        Explored = new bool[width, height];
        Player = new Adventurer { X = width / 2, Y = height / 2 };
        Monsters = new List<Monster>();
        Items = new List<Item>();
        GenerateMonsters();
        GenerateItems();
        SpawnDoor(targetRoomId ?? Guid.NewGuid().ToString());
    }
    
    private void SpawnDoor(string targetRoomId)
    {
        int border = Arena._rng.Next(4);
        int x = 0, y = 0;
        switch (border)
        {
            case 0: // Top wall
                x = Arena._rng.Next(this.Width);
                y = -1;
                break;
            case 1: // Bottom wall
                x = Arena._rng.Next(this.Width);
                y = this.Height;
                break;
            case 2: // Left wall
                x = -1;
                y = Arena._rng.Next(this.Height);
                break;
            case 3: // Right wall
                x = this.Width;
                y = Arena._rng.Next(this.Height);
                break;
        }
        this.RoomDoor = new Door(x, y, targetRoomId);
    }


    /// <summary>
    /// Randomly generates monsters in the arena.
    /// </summary>
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
                Attack = _rng.Next(1, 4) // Reduced attack
            });
        }
    }

    /// <summary>
    /// Randomly generates items in the arena.
    /// </summary>
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

    /// <summary>
    /// Checks if there is a monster at the given coordinates.
    /// </summary>
    public bool IsMonsterAt(int x, int y) => Monsters.Any(m => m.X == x && m.Y == y && m.Health > 0);
    /// <summary>
    /// Gets the monster at the given coordinates.
    /// </summary>
    public Monster? GetMonsterAt(int x, int y) => Monsters.FirstOrDefault(m => m.X == x && m.Y == y && m.Health > 0);
    /// <summary>
    /// Checks if there is an item at the given coordinates.
    /// </summary>
    public bool IsItemAt(int x, int y) => Items.Any(i => i.X == x && i.Y == y && !i.PickedUp);
    /// <summary>
    /// Gets the item at the given coordinates.
    /// </summary>
    public Item? GetItemAt(int x, int y) => Items.FirstOrDefault(i => i.X == x && i.Y == y && !i.PickedUp);

    /// <summary>
    /// Moves monsters towards the player.
    /// </summary>
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
            if (newX >= 0 && newX < Width && newY >= 0 && newY < Height && !Monsters.Any(m => m != monster && m.X == newX && m.Y == newY && m.Health > 0))
            {
                monster.X = newX;
                monster.Y = newY;
            }
        }
    }

    /// <summary>
    /// Gets a visual representation of the arena.
    /// </summary>
    public string GetVisual()
    {
        var sb = new StringBuilder();

        // Top border
        for (int x = 0; x < this.Width + 2; x++)
        {
            if (this.RoomDoor != null && this.RoomDoor.Y == -1 && this.RoomDoor.X + 1 == x)
                sb.Append('D');
            else
                sb.Append('#');
        }
        sb.AppendLine();

        // Arena rows
        for (int y = 0; y < this.Height; y++)
        {
            // Left border
            if (this.RoomDoor != null && this.RoomDoor.X == -1 && this.RoomDoor.Y == y)
                sb.Append('D');
            else
                sb.Append('#');

            // Arena content
            for (int x = 0; x < this.Width; x++)
            {
                if (this.Player.X == x && this.Player.Y == y)
                    sb.Append('@');
                else if (this.Monsters.Any(m => m.X == x && m.Y == y && m.Health > 0))
                    sb.Append('M');
                else if (this.Items.Any(i => i.X == x && i.Y == y && !i.PickedUp))
                    sb.Append('?');
                else
                    sb.Append('.');
            }

            // Right border
            if (this.RoomDoor != null && this.RoomDoor.X == this.Width && this.RoomDoor.Y == y)
                sb.Append('D');
            else
                sb.Append('#');

            sb.AppendLine();
        }

        // Bottom border
        for (int x = 0; x < this.Width + 2; x++)
        {
            if (this.RoomDoor != null && this.RoomDoor.Y == this.Height && this.RoomDoor.X + 1 == x)
                sb.Append('D');
            else
                sb.Append('#');
        }
        sb.AppendLine();

        return sb.ToString();
    }
}

/// <summary>
/// Represents the player character.
/// </summary>
public class Adventurer
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Health { get; set; } = 40; // Increased HP
    public int Attack { get; set; } = 3;
    public List<Item> Inventory { get; set; } = new();
}

/// <summary>
/// Represents a monster in the arena.
/// </summary>
public class Monster
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Health { get; set; }
    public int Attack { get; set; }
}

/// <summary>
/// Types of items that can be found in the arena.
/// </summary>
public enum ItemType
{
    Potion,
    Treasure
}

/// <summary>
/// Represents an item in the arena.
/// </summary>
public class Item
{
    public int X { get; set; }
    public int Y { get; set; }
    public ItemType Type { get; set; }
    public bool PickedUp { get; set; } = false;
}

/// <summary>
/// Manages the game state, progression, and interactions.
/// </summary>
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

    /// <summary>
    /// Initializes a new game instance.
    /// </summary>
    public Game()
    {
        Arena = new Arena(DefaultWidth, DefaultHeight);
        InProgress = false;
        Turn = 0;
        MonstersKilled = 0;
        DistanceTraveled = 0;
    }

    /// <summary>
    /// Starts a new game with a fresh arena.
    /// </summary>
    public void StartNewGame()
    {
        Arena = new Arena(DefaultWidth, DefaultHeight);
        InProgress = true;
        Turn = 0;
        MonstersKilled = 0;
        DistanceTraveled = 0;
    }

    /// <summary>
    /// Plays a turn of the game, processing the given action.
    /// </summary>
    public string PlayTurn(string action, string username, string password, Account? currentAccount = null, List<Account>? allAccounts = null)
    {
        if (!InProgress)
            return "No game in progress. Start a new game.\n";
        string result = "";
        bool actionEffectuee = false;
        switch (action.ToLower())
        {
            case "up":
            case "down":
            case "left":
            case "right":
                var moveResult = MovePlayer(action);
                result = moveResult.Item1;
                actionEffectuee = moveResult.Item2;
                break;
            case "fight":
                result = FightMonster();
                actionEffectuee = true;
                break;
            case "pick":
                result = PickupItem();
                actionEffectuee = true;
                break;
            default:
                result = "Unknown action.\n";
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
                result += $"A monster automatically attacks you (-{monsterOnPlayer.Attack} HP) !\n";
            }
            if (Arena.Player.Health <= 0)
            {
                InProgress = false;
                result += $"\nYou are dead ! Monsters killed : {MonstersKilled} | Distance traveled : {DistanceTraveled}\n";
                // Automatic save on player death
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    _ = Game.SaveEncryptedAsync(this, username, password, currentAccount, allAccounts);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Moves the player in the given direction.
    /// </summary>
    public (string, bool) MovePlayer(string direction)
    {
        int x = this.Arena.Player.X;
        int y = this.Arena.Player.Y;
        switch (direction.ToLower())
        {
            case "up":
                --y;
                break;
            case "down":
                ++y;
                break;
            case "left":
                --x;
                break;
            case "right":
                ++x;
                break;
        }
        if (this.Arena.RoomDoor != null && x == this.Arena.RoomDoor.X && y == this.Arena.RoomDoor.Y)
        {
            // Save old player stats
            var oldPlayer = this.Arena.Player;

            // Create new arena
            this.Arena = new Arena(this.Arena.Width, this.Arena.Height, this.Arena.RoomDoor.TargetRoomId);

            // Transfer stats to new player
            this.Arena.Player.Health = oldPlayer.Health;
            this.Arena.Player.Attack = oldPlayer.Attack;
            this.Arena.Player.Inventory = oldPlayer.Inventory;
      
            return ($"You go through the door to room {this.Arena.RoomId}!\n", true);
        }
        if (x < 0 || x >= this.Arena.Width || y < 0 || y >= this.Arena.Height)
            return ("Cannot leave the arena!\n", false);
        this.Arena.Player.X = x;
        this.Arena.Player.Y = y;
        this.Arena.Explored[x, y] = true;
        ++this.DistanceTraveled;
        string str = $"You move to ({x},{y}). Distance traveled: {this.DistanceTraveled}\n";
        if (this.Arena.IsItemAt(x, y))
            str += "There is something here...\n";
        return (str, true);
    }

    /// <summary>
    /// Initiates combat with a monster.
    /// </summary>
    public string FightMonster()
    {
        var monster = Arena.GetMonsterAt(Arena.Player.X, Arena.Player.Y);
        if (monster == null)
            return "No monster here.\n";
        monster.Health -= Arena.Player.Attack;
        if (monster.Health > 0)
        {
            Arena.Player.Health -= monster.Attack;
            return $"You injure the monster ! It retaliates (-{monster.Attack} HP).\n";
        }
        else
        {
            MonstersKilled++;
            return "Monster defeated !\n";
        }
    }

    /// <summary>
    /// Picks up an item from the arena.
    /// </summary>
    public string PickupItem()
    {
        var item = Arena.GetItemAt(Arena.Player.X, Arena.Player.Y);
        if (item == null)
            return "No item here.\n";
        item.PickedUp = true;
        Arena.Player.Inventory.Add(item);
        switch (item.Type)
        {
            case ItemType.Potion:
                Arena.Player.Health += 10; // More effective potion
                return "You pick up a potion (+10 HP).\n";
            case ItemType.Treasure:
                MonstersKilled++;
                return "You pick up a treasure (+1 fictitious monster killed).\n";
            default:
                return "Item picked up.\n";
        }
    }

    /// <summary>
    /// Saves the game state encrypted to the server and locally.
    /// </summary>
    public static async Task SaveEncryptedAsync(Game game, string username, string password, Account? currentAccount = null, List<Account>? allAccounts = null)
    {
        string json = JsonConvert.SerializeObject(game);
        byte[] plaintext = Encoding.UTF8.GetBytes(json);

        // Generate salt (16 bytes)
        byte[] salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256).GetBytes(32);

        using var aes = new AesManaged
        {
            KeySize = 256,
            BlockSize = 128,
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7
        };
        aes.Key = key;
        aes.GenerateIV();
        byte[] iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, iv);
        byte[] ciphertext = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);

        var save = new SaveData
        {
            Username = username,
            Salt = Convert.ToBase64String(salt),
            IV = Convert.ToBase64String(iv),
            Data = Convert.ToBase64String(ciphertext)
        };
        // Console.WriteLine($"DEBUG: SaveData before API call: Username={save.Username}, Salt={save.Salt}, Nonce={save.Nonce}, Tag={save.Tag}, DataLength={save.Data.Length}");
        await ApiClient.SaveGameAsync(save);
        // Local/account sync
        if (currentAccount != null)
        {
            currentAccount.MonstersKilled = game.MonstersKilled;
            currentAccount.DistanceTraveled = game.DistanceTraveled;
            currentAccount.MonstersKilledDateUtc = DateTime.UtcNow;
            currentAccount.MonstersKilledSignature = Account.GenerateMonstersKilledSignature(
                currentAccount.MonstersKilled, Account.GetServerSecretKey());

            await ApiClient.SaveAccountAsync(username, currentAccount.Email, currentAccount.MonstersKilled, currentAccount.DistanceTraveled);
        }
    }

    /// <summary>
    /// Loads a game state from encrypted data.
    /// </summary>
    public static async Task<Game> LoadEncryptedAsync(string username, string password)
    {
        var save = await ApiClient.LoadGameAsync(username, password);
        if (save == null)
            return new Game();
        try
        {
            byte[] salt = Convert.FromBase64String(save.Salt);
            byte[] iv = Convert.FromBase64String(save.IV);
            byte[] ciphertext = Convert.FromBase64String(save.Data);
            var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256).GetBytes(32);

            using var aes = new AesManaged
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
            aes.Key = key;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] plaintext = decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
            string json = Encoding.UTF8.GetString(plaintext);
            var deserializedGame = JsonConvert.DeserializeObject<Game>(json);
            return deserializedGame ?? new Game();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Decryption failed: " + ex.Message);
            return new Game();
        }
    }

    /// <summary>
    /// Saves the game and account data locally.
    /// </summary>
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
        var json = JsonConvert.SerializeObject(data);
        File.WriteAllText(fullPath, json);
    }

    /// <summary>
    /// Synchronizes local save data with the server if valid.
    /// </summary>
    public static async Task SyncLocalIfValid(string filePath)
    {
        if (!File.Exists(filePath)) return;
        var json = File.ReadAllText(filePath);
        var data = JsonConvert.DeserializeObject<LocalSaveData>(json);
        if (data == null) return;
        // Check the signature
        if (Account.VerifyMonstersKilledSignature(
            data.Account.MonstersKilled,
            data.Account.MonstersKilledSignature,
            Account.GetServerSecretKey()))
        {
            // Send to server
            var serverAccount = await ApiClient.GetAccountAsync(data.Account.Username);
            if (serverAccount != null &&
                data.Account.MonstersKilled > serverAccount.MonstersKilled &&
                data.Account.DistanceTraveled > serverAccount.DistanceTraveled)
            {
                await ApiClient.SaveAccountAsync(
                    data.Account.Username,
                    data.Account.Email,
                    data.Account.MonstersKilled,
                    data.Account.DistanceTraveled);
            }
        }
    }

    public class LocalSaveData
    {
        public Game Game { get; set; } = new Game();
        public Account Account { get; set; } = new Account();
    }
}
