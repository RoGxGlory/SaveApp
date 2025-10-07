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
        Console.Write("Choix : ");
    }

    // Affiche le menu en jeu
    static void ShowInGameMenu()
    {
        Console.WriteLine("\n=== MENU EN JEU ===");
        Console.WriteLine("1. Deviner un nombre");
        Console.WriteLine("2. Retour au menu principal");
        Console.WriteLine("3. Afficher le leaderboard");
        Console.Write("Choix : ");
    }

    static async Task ShowLeaderboardAsync()
    {
        var accounts = await ApiClient.GetLeaderboardAsync();
        Console.WriteLine("\n=== LEADERBOARD ===");
        Console.WriteLine($"{"Utilisateur",-18} | {"Score",5} | {"Date du score",-19} | {"Intégrité",-9}");
        Console.WriteLine(new string('-', 18) + " | " + new string('-', 5) + " | " + new string('-', 19) + " | " + new string('-', 9));
        foreach (var acc in accounts.OrderByDescending(a => a.Score))
        {
            if (DateTime.TryParse(acc.ScoreDateUtc, out var date))
            {
                string integrity = string.IsNullOrEmpty(acc.Integrity) ? "UNKNOWN" : acc.Integrity;
                Console.WriteLine($"{acc.Username,-18} | {acc.Score,5} | {date:yyyy-MM-dd HH:mm:ss} | {integrity,-9}");
            }
            else
            {
                string integrity = string.IsNullOrEmpty(acc.Integrity) ? "UNKNOWN" : acc.Integrity;
                Console.WriteLine($"{acc.Username,-18} | {acc.Score,5} | {acc.ScoreDateUtc,-19} | {integrity,-9}");
            }
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
            bool running = true;
            bool inGame = false;

            // Boucle principale du menu utilisateur
            while (running)
            {
                if (!inGame)
                {
                    ShowMenu(); // Affiche le menu principal
                    string? input = Console.ReadLine();
                    switch (input)
                    {
                        case "1":
                            // Nouvelle partie
                            game.StartNewGame();
                            Console.WriteLine("Nouvelle partie démarrée ! Devinez un nombre entre 1 et 100.\n");
                            inGame = true;
                            break;
                        case "2":
                            // Charger la partie sauvegardée
                            game = await Game.LoadEncryptedAsync(username, userPassword);
                            Console.WriteLine("Partie chargée.\n");
                            if (!game.InProgress)
                            {
                                Console.WriteLine("Aucune partie en cours dans la sauvegarde. Nouvelle partie démarrée !\n");
                                game.StartNewGame();
                            }
                            inGame = true;
                            break;
                        case "3":
                            // Sauvegarder la partie en cours (chiffrée)
                            await Game.SaveEncryptedAsync(game, username, userPassword, currentAccount, null);
                            Console.WriteLine("Partie sauvegardée (chiffrée).\n");
                            break;
                        case "4":
                            // Afficher le score et les essais
                            Console.WriteLine($"Score (parties gagnées) : {currentAccount.Score}\n");
                            if (game.InProgress)
                                Console.WriteLine($"Essais dans la partie en cours : {game.Attempts}\n");
                            break;
                        case "5":
                            // Nouvelle sauvegarde (réinitialisation)
                            game = new Game();
                            game.StartNewGame();
                            await Game.SaveEncryptedAsync(game, username, userPassword, currentAccount, null);
                            Console.WriteLine("Nouvelle sauvegarde créée (chiffrée). Partie réinitialisée ! Devinez un nombre entre 1 et 100.\n");
                            inGame = true;
                            break;
                        case "6":
                            // Quitter l'application
                            running = false;
                            await Game.SaveEncryptedAsync(game, username, userPassword, currentAccount, null);
                            Console.WriteLine("Partie sauvegardée (chiffrée) et application quittée.\n");
                            Environment.Exit(0);
                            break;
                        case "7":
                            // Déconnexion (retour à l'écran de connexion)
                            await Game.SaveEncryptedAsync(game, username, userPassword, currentAccount, null);
                            Console.WriteLine("Déconnexion...\n");
                            running = false; // Sort de la boucle du menu pour revenir à la connexion
                            break;
                        default:
                            Console.WriteLine("Choix invalide.\n");
                            break;
                    }
                }
                else // inGame == true
                {
                    ShowInGameMenu(); // Affiche le menu en jeu
                    string? input = Console.ReadLine();
                    switch (input)
                    {
                        case "1":
                            // Deviner un nombre
                            if (!game.InProgress)
                            {
                                Console.WriteLine("Aucune partie en cours. Retour au menu principal.\n");
                                inGame = false;
                                break;
                            }
                            Console.Write("Votre proposition : ");
                            if (int.TryParse(Console.ReadLine(), out int guess))
                            {
                                string result = game.Play(guess);
                                Console.WriteLine(result);
                                if (!game.InProgress)
                                {
                                    // Synchronisation immédiate du score du compte après victoire
                                    await ApiClient.SaveAccountAsync(currentAccount.Username, game.Score);
                                    Console.WriteLine("Partie terminée. Retour au menu principal.\n");
                                    inGame = false;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Entrée invalide.\n");
                            }
                            break;
                        case "2":
                            // Retour au menu principal
                            Console.WriteLine("Retour au menu principal.\n");
                            inGame = false;
                            break;
                        case "3":
                            // Afficher le leaderboard
                            await ShowLeaderboardAsync();
                            break;
                        default:
                            Console.WriteLine("Choix invalide.\n");
                            break;
                    }
                }
            }
        }
    }

    // Lecture masquée du mot de passe (affiche des * à l'écran)
    static string ReadPassword()
    {
        var pwd = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace && pwd.Length > 0)
            {
                pwd.Length--;
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                pwd.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return pwd.ToString();
    }
}