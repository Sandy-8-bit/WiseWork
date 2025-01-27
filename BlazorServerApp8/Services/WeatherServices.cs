using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using BlazorWasmApp.Models;

public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string apiKey = "0396fe1fc3e16a3fcca570677d66ae8d"; // Your OpenWeather API key
    private readonly Dictionary<string, WeatherResponse> _weatherCache = new(); // Cache for weather data
    private readonly IConfiguration _configuration;
    private readonly SupabaseAuthService _authService;

    public List<string> FavoriteCities { get; private set; } = new List<string>();

    public WeatherService(HttpClient httpClient, IConfiguration configuration, SupabaseAuthService authService)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _authService = authService;  // Inject SupabaseAuthService here
    }

    // Fetch current weather data by city
    public async Task<WeatherResponse?> GetWeatherAsync(string city)
    {
        try
        {
            Console.WriteLine($"Attempting to fetch weather for: {city}");
            var response = await _httpClient.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric");

            Console.WriteLine($"API Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var weather = await response.Content.ReadFromJsonAsync<WeatherResponse>();
                if (weather != null)
                {
                    _weatherCache[city] = weather;

                    // Check if rain is present in the weather data
                    if (weather.weather.Any(w => w.main == "Rain"))
                    {
                        var user = _authService.GetCurrentUser();  // Use the injected instance of SupabaseAuthService
                        if (user != null)
                        {
                            // Send rain notification email
                            await SendRainNotificationEmail(user.Email!, city, weather.weather.FirstOrDefault()?.description);
                        }
                    }

                    return weather;
                }
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error Response: {content}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetWeatherAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        return null;
    }

    // Send a rain notification email to the user
    private async Task SendRainNotificationEmail(string userEmail, string cityName, string? description)
    {
        try
        {
            var smtpClient = new SmtpClient(_configuration["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_configuration["EmailSettings:Port"]),
                Credentials = new NetworkCredential(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:FromAddress"]),
                Subject = $"Rain Alert for {cityName}",
                Body = $"Hello, there is rain in {cityName}. Description: {description ?? "No description available"}. Please take necessary precautions.",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(userEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending rain email: {ex.Message}");
        }
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
