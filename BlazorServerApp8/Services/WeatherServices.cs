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
    public List<string> FavoriteCities { get; private set; } = new List<string>();

    public WeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Fetch current weather data by city
    public async Task<WeatherResponse?> GetWeatherAsync(string city)
    {
        var response = await _httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<WeatherResponse>();
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error: {response.StatusCode}, Content: {content}");
            return null;
        }
    }

    // Fetch 5-day weather forecast for a city
    public async Task<ForecastResponse?> GetFiveDayForecastAsync(string city)
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
            return null;
        }
    }

    // Fetch weather data for a city from the favorite list
    public async Task<WeatherResponse?> GetWeatherFromFavoriteCity(string city)
    {
        return await GetWeatherAsync(city); // Reusing GetWeatherAsync method
    }

    // Add a city to favorites
    public void AddToFavorites(string city)
    {
        if (!FavoriteCities.Contains(city))
        {
            FavoriteCities.Add(city);
        }
    }

    // Remove a city from favorites
    public void RemoveFromFavorites(string city)
    {
        if (FavoriteCities.Contains(city))
        {
            FavoriteCities.Remove(city);
        }
    }

    // Check if a city is already in favorites
    public bool IsCityInFavorites(string city)
    {
        return FavoriteCities.Contains(city);
    }
}
