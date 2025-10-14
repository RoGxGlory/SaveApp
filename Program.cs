// Programme principal du jeu console avec gestion multi-comptes, sauvegarde chiffrée et menu interactif

namespace SaveApp;

class Program
{
    // Affiche le menu principal
    static void ShowMenu()
    {
        Console.WriteLine("\n=== MENU PRINCIPAL ===");
        Console.WriteLine("1. Nouvelle partie");
        Console.WriteLine("2. Charger la partie");
        Console.WriteLine("3. Sauvegarder");
        Console.WriteLine("4. Afficher le score");
        Console.WriteLine("5. Nouvelle sauvegarde");
        Console.WriteLine("6. Quitter");
        Console.WriteLine("7. Se déconnecter");
        Console.WriteLine("8. Afficher le leaderboard");
        Console.Write("Choix : ");
    }

    // Nouveau menu en jeu pour le gameplay d'exploration
    static void ShowInGameMenu()
    {
        Console.WriteLine("\n=== MENU EN JEU ===");
        Console.WriteLine("1. Se déplacer (haut/bas/gauche/droite)");
        Console.WriteLine("2. Combattre");
        Console.WriteLine("3. Ramasser un objet");
        Console.WriteLine("4. Retour au menu principal");
        Console.Write("Choix : ");
    }

    // Affichage du leaderboard avec alignement correct
    static async Task ShowLeaderboardAsync()
    {
        var accounts = ApiClient.SortLeaderboard(await ApiClient.GetLeaderboardAsync());
        Console.WriteLine("\n=== LEADERBOARD ===");
        Console.WriteLine($"{"Utilisateur",-18} | {"Monstres tués",-13} | {"Distance",-10} | {"Date du score",-19} | {"Intégrité",-9}");
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
    
    // Point d'entrée principal du programme
    static async Task Main()
    {
        while (true)
        {
            string username;
            Account? currentAccount = null;
            string userPassword = null;
            // Boucle de connexion ou création de compte
            while (true)
            {
                Console.Write("Entrez votre nom d'utilisateur : ");
                username = Console.ReadLine() ?? "Invité";
                Console.Write("Mot de passe : ");
                string password = ReadPassword();
                var loginResult = await ApiClient.LoginAsync(username, password);
                if (loginResult.Success)
                {
                    currentAccount = loginResult.Account;
                    userPassword = password;
                    Console.WriteLine("Connexion réussie !\n");
                    break;
                }
                else
                {
                    Console.WriteLine("Aucun compte trouvé. Voulez-vous en créer un ? (o/n)");
                    string rep = Console.ReadLine()?.ToLower() ?? "n";
                    if (rep == "o" || rep == "oui" || rep == "y")
                    {
                        var registerResult = await ApiClient.RegisterAsync(username, password);
                        if (registerResult.Success)
                        {
                            currentAccount = registerResult.Account;
                            userPassword = password;
                            Console.WriteLine("Compte créé et connecté !\n");
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Erreur lors de la création du compte.\n");
                        }
                    }
                }
            }
            // Chargement de la sauvegarde chiffrée du jeu pour l'utilisateur connecté
            Game game = await Game.LoadEncryptedAsync(username, userPassword);
            // Ensure account score from server is authoritative to avoid overwriting higher server value
            if (currentAccount != null)
            {
                // On synchronise le nombre de monstres tués
                if (game.MonstersKilled < currentAccount.MonstersKilled)
                    game.MonstersKilled = currentAccount.MonstersKilled;
                else
                    currentAccount.MonstersKilled = Math.Max(currentAccount.MonstersKilled, game.MonstersKilled);
            }
            // Synchronisation des sauvegardes locales au démarrage
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
                        Console.WriteLine("Nouvelle partie lancée !\n");
                        // Boucle de jeu principale
                        while (game.InProgress)
                        {
                            Console.WriteLine($"\nTour {game.Turn + 1} | PV : {game.Arena.Player.Health} | Monstres tués : {game.MonstersKilled} | Distance parcourue : {game.DistanceTraveled} | Pos : ({game.Arena.Player.X},{game.Arena.Player.Y})");
                            // Affichage visuel de l'arène
                            Console.WriteLine(game.Arena.GetVisual());
                            ShowInGameMenu();
                            string action = Console.ReadLine()?.ToLower() ?? "";
                            string result = "";
                            switch (action)
                            {
                                case "1":
                                    Console.Write("Direction (haut/bas/gauche/droite) : ");
                                    string dir = Console.ReadLine()?.ToLower() ?? "";
                                    result = game.PlayTurn(dir, username, userPassword, currentAccount, null);
                                    break;
                                case "2":
                                    result = game.PlayTurn("combattre", username, userPassword, currentAccount, null);
                                    break;
                                case "3":
                                    result = game.PlayTurn("ramasser", username, userPassword, currentAccount, null);
                                    break;
                                case "4":
                                    Console.WriteLine("Retour au menu principal...");
                                    game.InProgress = false;
                                    break;
                                default:
                                    Console.WriteLine("Choix invalide.");
                                    break;
                            }
                            if (!string.IsNullOrWhiteSpace(result))
                                Console.WriteLine(result);
                        }
                        break;
                    case "2":
                        game = await Game.LoadEncryptedAsync(username, userPassword);
                        Console.WriteLine("Partie chargée !\n");
                        break;
                    case "3":
                        await Game.SaveEncryptedAsync(game, username, userPassword, currentAccount);
                        Console.WriteLine("Sauvegarde effectuée !\n");
                        break;
                    case "4":
                        Console.WriteLine($"Monstres tués : {game.MonstersKilled} | Distance parcourue : {game.DistanceTraveled}\n");
                        break;
                    case "5":
                        await Game.SaveEncryptedAsync(game, username, userPassword, currentAccount);
                        Console.WriteLine("Nouvelle sauvegarde créée !\n");
                        break;
                    case "6":
                        quitter = true;
                        break;
                    case "7":
                        Console.WriteLine("Déconnexion...");
                        quitter = true;
                        break;
                    case "8":
                        await ShowLeaderboardAsync();
                        break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
        }
    }

    // Méthode utilitaire pour lire un mot de passe sans l'afficher
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
}
