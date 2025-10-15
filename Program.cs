// Main program for the console game with multi-account management, encrypted save, and interactive menu

using System.Text.RegularExpressions;

namespace SaveApp;

/// <summary>
/// Entry point and main loop for the SaveApp console game.
/// Handles user authentication, game session management, and menu navigation.
/// </summary>
class Program
{
    static string username = ""; // Ajout de la variable username globale pour la session

    /// <summary>
    /// Displays the main menu options to the user.
    /// </summary>
    static void ShowMenu()
    {
        Console.WriteLine("\n=== MAIN MENU ===");
        Console.WriteLine("1. New game");
        Console.WriteLine("2. Load game");
        Console.WriteLine("3. Save");
        Console.WriteLine("4. Show score");
        Console.WriteLine("5. New save");
        Console.WriteLine("6. Quit");
        Console.WriteLine("7. Log out");
        Console.WriteLine("8. Show leaderboard");
        Console.Write("Choice: ");
    }

    /// <summary>
    /// Displays the in-game menu for exploration and actions.
    /// </summary>
    static void ShowInGameMenu()
    {
        Console.WriteLine("\n=== IN-GAME MENU ===");
        Console.WriteLine("1. Move (up/down/left/right)");
        Console.WriteLine("2. Fight");
        Console.WriteLine("3. Pick up an item");
        Console.WriteLine("4. Return to main menu");
        Console.Write("Choice: ");
    }

    /// <summary>
    /// Displays the leaderboard with player statistics.
    /// </summary>
    static async Task ShowLeaderboardAsync()
    {
        var accounts = ApiClient.SortLeaderboard(await ApiClient.GetLeaderboardAsync());
        Console.WriteLine("\n=== LEADERBOARD ===");
        Console.WriteLine($"{"User",-18} | {"Monsters killed",-13} | {"Distance",-10} | {"Score date",-19} | {"Integrity",-9}");
        Console.WriteLine(new string('-', 18) + " | " + new string('-', 13) + " | " + new string('-', 10) + " | " + new string('-', 19) + " | " + new string('-', 9));
        foreach (var acc in accounts.OrderByDescending(a => a.MonstersKilled))
        {
            string integrity = string.IsNullOrEmpty(acc.Integrity) ? "UNKNOWN" : acc.Integrity;
            string dateStr = acc.ScoreDateUtc;
            if (DateTime.TryParse(acc.ScoreDateUtc, out var date))
                dateStr = date.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine($"{acc.Username,-18} | {acc.MonstersKilled,-13} | {acc.DistanceTraveled,-10} | {dateStr,-19} | {integrity,-9}");
        }
        Console.WriteLine();
    }
    
    /// <summary>
    /// Main entry point and game loop. Handles authentication, game session, and menu navigation.
    /// </summary>
    static async Task Main()
    {
        while (true)
        {
            string identifier;
            Account? currentAccount = null;
            string userPassword = null;
            // Loop for login or account creation
            while (true)
            {
                Console.Write("Do you want to log in with (1) username or (2) email? ");
                string choixLogin = Console.ReadLine();
                bool isEmail = false;
                // Lors du login, renseigner username ou email
                if (choixLogin == "2")
                {
                    Console.Write("Enter your email : ");
                    string email = Console.ReadLine() ?? "";
                    while (!IsValidEmail(email))
                    {
                        Console.Write("Invalid email. Try again : ");
                        email = Console.ReadLine() ?? "";
                    }
                    identifier = email;
                    isEmail = true;
                }
                else
                {
                    Console.Write("Enter your username: ");
                    username = Console.ReadLine() ?? "";
                    identifier = username;
                }
                Console.Write("Password: ");
                string password = ReadPassword();
                var loginResult = await ApiClient.LoginAsync(identifier, password, isEmail);
                if (loginResult.Success)
                {
                    currentAccount = loginResult.Account;
                    userPassword = password;
                    Console.WriteLine("Login successful!\n");
                    break;
                }
                else
                {
                    Console.WriteLine("No account found. Do you want to create one? (y/n)");
                    string rep = Console.ReadLine()?.ToLower() ?? "n";
                    // Lors de la création d'un compte, demander username et email
                    if (rep == "o" || rep == "oui" || rep == "y")
                    {
                        string newUsername = "";
                        string newEmail = "";
                        string newPassword = "";
                        if (isEmail)
                        {
                            Console.Write("Choose a username : ");
                            newUsername = Console.ReadLine() ?? "";
                            newEmail = identifier;
                        }
                        else
                        {
                            newUsername = identifier;
                            Console.Write("Enter your email : ");
                            newEmail = Console.ReadLine() ?? "";
                            while (!IsValidEmail(newEmail))
                            {
                                Console.Write("Invalid email. Try again : ");
                                newEmail = Console.ReadLine() ?? "";
                            }
                        }
                        Console.Write("Choose a password : ");
                        newPassword = ReadPassword();
                        var registerResult = await ApiClient.RegisterAsync(newUsername, newPassword, newEmail);
                        if (registerResult.Success)
                        {
                            currentAccount = registerResult.Account;
                            userPassword = newPassword;
                            Console.WriteLine("Account created and logged in!\n");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Error while creating account.\n");
                        }
                    }
                }
            }
            // Load encrypted game save for the logged-in user
            Game game = await Game.LoadEncryptedAsync(username, userPassword);
            // Ensure account score from server is authoritative to avoid overwriting higher server value
            if (currentAccount != null)
            {
                // Synchronize the number of monsters killed
                if (game.MonstersKilled < currentAccount.MonstersKilled)
                    game.MonstersKilled = currentAccount.MonstersKilled;
                else
                    currentAccount.MonstersKilled = Math.Max(currentAccount.MonstersKilled, game.MonstersKilled);
            }
            // Synchronize local saves at startup
            string localSavePath = $"{username}_local_save.json";
            await Game.SyncLocalIfValid(localSavePath);
            bool quitter = false;
            while (!quitter)
            {
                ShowMenu();
                string choix = Console.ReadLine() ?? "";
                switch (choix)
                {
                    case "1":
                        game.StartNewGame();
                        Console.WriteLine("New game started!\n");
                        // Main game loop
                        while (game.InProgress)
                        {
                            Console.WriteLine($"\nTurn {game.Turn + 1} | HP: {game.Arena.Player.Health} | Monsters killed: {game.MonstersKilled} | Distance traveled: {game.DistanceTraveled} | Pos: ({game.Arena.Player.X},{game.Arena.Player.Y})");
                            // Visual display of the arena
                            Console.WriteLine(game.Arena.GetVisual());
                            ShowInGameMenu();
                            string action = Console.ReadLine()?.ToLower() ?? "";
                            string result = "";
                            switch (action)
                            {
                                case "1":
                                    Console.Write("Direction (up/down/left/right): ");
                                    string dir = Console.ReadLine()?.ToLower() ?? "";
                                    result = game.PlayTurn(dir, username, userPassword, currentAccount, null);
                                    break;
                                case "2":
                                    result = game.PlayTurn("fight", username, userPassword, currentAccount, null);
                                    break;
                                case "3":
                                    result = game.PlayTurn("pick", username, userPassword, currentAccount, null);
                                    break;
                                case "4":
                                    Console.WriteLine("Returning to main menu...");
                                    game.InProgress = false;
                                    break;
                                default:
                                    Console.WriteLine("Invalid choice.");
                                    break;
                            }
                            if (!string.IsNullOrWhiteSpace(result))
                                Console.WriteLine(result);
                        }
                        break;
                    case "2":
                        game = await Game.LoadEncryptedAsync(username, userPassword);
                        Console.WriteLine("Game loaded!\n");
                        break;
                    case "3":
                        await Game.SaveEncryptedAsync(game, username, userPassword, currentAccount);
                        Console.WriteLine("Save completed!\n");
                        break;
                    case "4":
                        Console.WriteLine($"Monsters killed: {game.MonstersKilled} | Distance traveled: {game.DistanceTraveled}\n");
                        break;
                    case "5":
                        await Game.SaveEncryptedAsync(game, username, userPassword, currentAccount);
                        Console.WriteLine("New save created!\n");
                        break;
                    case "6":
                        quitter = true;
                        break;
                    case "7":
                        Console.WriteLine("Logging out...");
                        quitter = true;
                        break;
                    case "8":
                        await ShowLeaderboardAsync();
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Utility method to read a password without displaying it on the console.
    /// </summary>
    static string ReadPassword()
    {
        var pwd = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace)
            {
                if (pwd.Length > 0)
                {
                    pwd.Length--;
                    Console.Write("\b \b");
                }
                continue;
            }
            if (!char.IsControl(key.KeyChar))
            {
                pwd.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return pwd.ToString();
    }

    /// <summary>
    /// Validates the email format using a regular expression.
    /// </summary>
    static bool IsValidEmail(string email)
    {
        // Simple regex for email validation
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
