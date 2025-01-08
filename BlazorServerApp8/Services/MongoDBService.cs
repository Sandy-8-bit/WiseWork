using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MongoDBService
{
    private readonly IMongoCollection<FavoriteCity> _favoritesCollection;

    public MongoDBService(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        _favoritesCollection = database.GetCollection<FavoriteCity>(configuration["MongoDB:CollectionName"]);
    }

    // Add a city to the favorites list
    public async Task AddFavoriteCityAsync(string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
        {
            throw new ArgumentException("City name cannot be null or empty.", nameof(cityName));
        }

        var existingCity = await _favoritesCollection.Find(c => c.Name == cityName).FirstOrDefaultAsync();
        if (existingCity == null)
        {
            await _favoritesCollection.InsertOneAsync(new FavoriteCity { Name = cityName });
        }
    }

    // Retrieve all favorite cities
    public async Task<List<FavoriteCity>> GetFavoriteCitiesAsync()
    {
        return await _favoritesCollection.Find(_ => true).ToListAsync();
    }

    // Remove a city from the favorites list
    public async Task RemoveFavoriteCityAsync(string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
        {
            throw new ArgumentException("City name cannot be null or empty.", nameof(cityName));
        }

        await _favoritesCollection.DeleteOneAsync(c => c.Name == cityName);
    }
}

public class FavoriteCity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("Name")]
    public string? Name { get; set; }
}
