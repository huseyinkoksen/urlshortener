using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UrlShortener.Api.Models;

namespace UrlShortener.Api.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<ShortUrl> ShortUrls => _database.GetCollection<ShortUrl>("ShortUrls");
        public IMongoCollection<ClickAnalytics> ClickAnalytics => _database.GetCollection<ClickAnalytics>("ClickAnalytics");
    }
}