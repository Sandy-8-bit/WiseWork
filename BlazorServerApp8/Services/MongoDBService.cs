using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MongoDBService
{
    private readonly IMongoCollection<FavoriteCity> _favoriteCities;

    public MongoDBService(IConfiguration configuration)
    {
        var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
        var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        _favoriteCities = database.GetCollection<FavoriteCity>(configuration["MongoDB:CollectionName"]);
    }

    // Add a city to the favorites list
    public async Task AddFavoriteCityAsync(string userId, string cityName)
    {
        try
        {
            var existing = await _favoriteCities
                .Find(x => x.UserId == userId && x.CityName == cityName)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                await _favoriteCities.InsertOneAsync(new FavoriteCity { UserId = userId, CityName = cityName });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding favorite city: {ex.Message}");
        }
    }

    // Retrieve favorite cities for a specific user
    public async Task<List<FavoriteCity>> GetFavoriteCitiesAsync(string userId)
    {
        try
        {
            return await _favoriteCities.Find(city => city.UserId == userId).ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching favorite cities: {ex.Message}");
            return new List<FavoriteCity>();
        }
    }

    // Remove a city from the favorites list
    public async Task RemoveFavoriteCityAsync(string userId, string cityName)
    {
        try
        {
            await _favoriteCities.DeleteOneAsync(city => city.UserId == userId && city.CityName == cityName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing favorite city: {ex.Message}");
        }
    }
}

// FavoriteCity model
public class FavoriteCity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("UserId")]
    public string? UserId { get; set; }

    [BsonElement("CityName")]
    public string? CityName { get; set; }
}
