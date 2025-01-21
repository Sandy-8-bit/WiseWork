using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorWasmApp.Models;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string apiKey = "0396fe1fc3e16a3fcca570677d66ae8d"; // Your OpenWeather API key
    private readonly Dictionary<string, WeatherResponse> _weatherCache = new(); // Cache for weather data

    public List<string> FavoriteCities { get; private set; } = new List<string>();

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Fetch current weather data by city
    public async Task<WeatherResponse?> GetWeatherAsync(string city)
    {
        // Check if the weather data is already cached
        if (_weatherCache.ContainsKey(city))
        {
            Console.WriteLine($"Returning cached weather data for {city}.");
            return _weatherCache[city];
        }

        try
        {
            var response = await _httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric");

            if (response.IsSuccessStatusCode)
            {
                var weather = await response.Content.ReadFromJsonAsync<WeatherResponse>();
                if (weather != null)
                {
                    // Cache the weather data for the city
                    _weatherCache[city] = weather;
                    return weather;
                }
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, Content: {content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching weather data for {city}: {ex.Message}");
        }

        return null;
    }

    // Fetch 5-day weather forecast for a city
    public async Task<ForecastResponse?> GetFiveDayForecastAsync(string city)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={apiKey}&units=metric");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ForecastResponse>();
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, Content: {content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching forecast data for {city}: {ex.Message}");
        }

        return null;
    }

    // Add a city to favorites
    public void AddToFavorites(string city)
    {
        if (!FavoriteCities.Contains(city))
        {
            FavoriteCities.Add(city);
            Console.WriteLine($"City {city} added to favorites.");
        }
    }

    // Remove a city from favorites
    public void RemoveFromFavorites(string city)
    {
        if (FavoriteCities.Contains(city))
        {
            FavoriteCities.Remove(city);
            Console.WriteLine($"City {city} removed from favorites.");
        }
    }

    // Check if a city is already in favorites
    public bool IsCityInFavorites(string city)
    {
        return FavoriteCities.Contains(city);
    }

    // Fetch weather data for a city from the favorite list
    public async Task<WeatherResponse?> GetWeatherFromFavoriteCity(string city)
    {
        if (!FavoriteCities.Contains(city))
        {
            Console.WriteLine($"City {city} is not in the favorites list.");
            return null;
        }

        return await GetWeatherAsync(city); // Reusing GetWeatherAsync method
    }

    // Clear the cache (useful in cases where the data might need refreshing)
    public void ClearCache()
    {
        _weatherCache.Clear();
        Console.WriteLine("Weather data cache cleared.");
    }
}
