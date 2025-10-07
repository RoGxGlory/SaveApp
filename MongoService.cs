using MongoDB.Bson;
using MongoDB.Driver;

// Service statique pour la connexion à MongoDB
namespace SaveApp.SaveApp
{
    public static class MongoService
    {
        // Client MongoDB
        public static MongoClient Client { get; private set; }
        // Base de données MongoDB
        public static IMongoDatabase Database { get; private set; }

        // Initialisation de la connexion et création des collections
        public static async Task InitializeAsync()
        {
            // Read connection string from environment variable for security
            string connectionString = Environment.GetEnvironmentVariable("MONGODB_CONN_STRING") ?? "";
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("MongoDB connection string is missing. Set the MONGODB_CONN_STRING environment variable.");
            }
            Client = new MongoClient(connectionString);
            Database = Client.GetDatabase("game");
            // Création des collections si elles n'existent pas
            var collections = await Database.ListCollectionNames().ToListAsync();
            if (!collections.Contains("accounts"))
                await Database.CreateCollectionAsync("accounts");
            if (!collections.Contains("saves"))
                await Database.CreateCollectionAsync("saves");
            // Vérification de la connexion et affichage
            try
            {
                var testCollections = await Database.ListCollectionNames().ToListAsync();
                Console.WriteLine($"Connexion MongoDB OK. Collections dans 'game': {string.Join(", ", testCollections)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de connexion à MongoDB: {ex.Message}");
            }
        }

        // Affiche tous les IDs de la collection spécifiée
        public static void PrintAllIds(string collectionName)
        {
            var dbCollection = Database.GetCollection<BsonDocument>(collectionName);
            var docs = dbCollection.Find(Builders<BsonDocument>.Filter.Empty).ToList();
            Console.WriteLine($"IDs de la collection '{collectionName}':");
            foreach (var doc in docs)
            {
                if (doc.Contains("_id"))
                    Console.WriteLine(doc["_id"].ToString());
            }
        }
    }
}
