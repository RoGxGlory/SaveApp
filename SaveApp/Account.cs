// Classe représentant un compte utilisateur avec gestion du stockage et de la récupération
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using SaveApp;
using System;
using System.Collections.Generic;

public class Account
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string PasswordHashB64 { get; set; } = default!;
    public string SaltB64 { get; set; } = default!;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public int Score { get; set; } = 0;
    public DateTime ScoreDateUtc { get; set; } = DateTime.UtcNow;

    // Constructeur par défaut
    public Account() { }

    // Constructeur pour créer un compte avec nom, hash et sel
    public Account(string username, string hashB64, string saltB64)
    {
        Username = username;
        PasswordHashB64 = hashB64;
        SaltB64 = saltB64;
        CreatedUtc = DateTime.UtcNow;
        Score = 0;
        ScoreDateUtc = DateTime.UtcNow;
    }

    // Charge tous les comptes depuis MongoDB
    public static List<Account> LoadAccounts()
    {
        var collection = MongoService.Database.GetCollection<Account>("accounts");
        return collection.Find(Builders<Account>.Filter.Empty).ToList();
    }

    // Sauvegarde la liste des comptes dans MongoDB (remplace tout)
    public static void SaveAccounts(List<Account> accounts)
    {
        var collection = MongoService.Database.GetCollection<Account>("accounts");
        collection.DeleteMany(Builders<Account>.Filter.Empty);
        if (accounts.Count > 0)
            collection.InsertMany(accounts);
    }
}