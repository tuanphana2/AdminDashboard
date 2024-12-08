using AdminDashboard.Models;
using MongoDB.Driver;

namespace AdminDashboard.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(DatabaseSettings databaseSettings)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            _database = client.GetDatabase(databaseSettings.DatabaseName);
        }

        public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("Notifications");
    }
}
