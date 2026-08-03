using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace FitnessCenterr.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"]!;
        var databaseName     = configuration["MongoDB:Database"]!;
        var client           = new MongoClient(connectionString);
        _database            = client.GetDatabase(databaseName);
    }

    public IMongoCollection<T> GetCollection<T>(string name) =>
        _database.GetCollection<T>(name);
}
