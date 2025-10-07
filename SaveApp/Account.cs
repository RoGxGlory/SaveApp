// Classe représentant un compte utilisateur avec gestion du stockage et de la récupération
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

public class Account
{
    // Nom d'utilisateur unique
    public string Username { get; set; }
    // Hash sécurisé du mot de passe
    public string PasswordHash { get; set; }
    // Sel utilisé pour le hash du mot de passe
    public string PasswordSalt { get; set; }

    // Nom du fichier où sont stockés tous les comptes
    public static string AccountsFile => "accounts.json";

    // Constructeur par défaut (utilisé pour la désérialisation JSON)
    public Account() { }

    // Constructeur pour créer un compte avec nom, hash et sel
    public Account(string username, string hash, string salt)
    {
        Username = username;
        PasswordHash = hash;
        PasswordSalt = salt;
    }

    // Charge la liste des comptes depuis le fichier JSON
    public static List<Account> LoadAccounts()
    {
        if (!File.Exists(AccountsFile))
            return new List<Account>(); // Aucun compte si le fichier n'existe pas
        try
        {
            string json = File.ReadAllText(AccountsFile);
            return JsonSerializer.Deserialize<List<Account>>(json) ?? new List<Account>();
        }
        catch
        {
            // Si le fichier est corrompu ou illisible, retourne une liste vide
            return new List<Account>();
        }
    }

    // Sauvegarde la liste des comptes dans le fichier JSON
    public static void SaveAccounts(List<Account> accounts)
    {
        string json = JsonSerializer.Serialize(accounts);
        File.WriteAllText(AccountsFile, json);
    }
}
