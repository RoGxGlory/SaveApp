using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace SaveApp
{
    public static class AccountMigration
    {
        public static void MigrateAccountsCollection()
        {
            var collection = MongoService.Database.GetCollection<BsonDocument>("accounts");
            var filter = Builders<BsonDocument>.Filter.Exists("PasswordHash");
            var accountsToMigrate = collection.Find(filter).ToList();
            foreach (var doc in accountsToMigrate)
            {
                var update = Builders<BsonDocument>.Update
                    .Set("PasswordHashB64", doc["PasswordHash"])
                    .Set("SaltB64", doc["PasswordSalt"])
                    .Unset("PasswordHash")
                    .Unset("PasswordSalt");
                collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]), update);
            }
            Console.WriteLine($"Migration terminée : {accountsToMigrate.Count} comptes mis à jour.");
        }
    }
}
// ...existing code...
