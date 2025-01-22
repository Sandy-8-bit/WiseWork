using BlazorWasmApp.Models;
using System.Linq;
using System.Text.Json;

public class WeatherChatService
{
    private readonly WeatherService _weatherService;
    private readonly HttpClient _httpClient;
    private readonly string _geminiApiKey;

    public WeatherChatService(WeatherService weatherService, HttpClient httpClient, IConfiguration configuration)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _httpClient = httpClient;
        _geminiApiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("GeminiApiKey");
    }

    private class GeminiRequest
    {
        public List<ChatMessage> Messages { get; set; } = new();
    }

    private class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }

    public async Task<(string Message, WeatherResponse? Weather)> ProcessMessageAsync(string userMessage)
    {
        try
        {
            var location = ExtractLocation(userMessage.ToLower());
            if (string.IsNullOrEmpty(location))
            {
                return ("I can help you check the weather! Just ask me about the weather in a specific city.", null);
            }

            var weatherData = await _weatherService.GetWeatherAsync(location);
            if (weatherData == null)
            {
                return ($"I couldn't find weather data for {location}. Please check the city name and try again.", null);
            }

            // Create context for Gemini with weather data
            var weatherContext = CreateWeatherContext(weatherData);
            var geminiResponse = await GetGeminiResponseAsync(userMessage, weatherContext);

            return (geminiResponse, weatherData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
            return ("I'm having trouble getting the weather information right now. Please try again later.", null);
        }
    }

    private string CreateWeatherContext(WeatherResponse weather)
    {
        var sunriseTime = DateTimeOffset.FromUnixTimeSeconds(weather.sys.sunrise)
            .ToLocalTime().ToString("h:mm tt");
        var sunsetTime = DateTimeOffset.FromUnixTimeSeconds(weather.sys.sunset)
            .ToLocalTime().ToString("h:mm tt");

        return $@"Current weather data for {weather.name}, {weather.sys.country}:
Temperature: {weather.main.temp}°C (feels like {weather.main.feels_like}°C)
Conditions: {weather.weather.FirstOrDefault()?.description}
Humidity: {weather.main.humidity}%
Wind Speed: {weather.wind.speed} m/s
Pressure: {weather.main.pressure} hPa
Visibility: {weather.visibility / 1000.0} km
Sunrise: {sunriseTime}
Sunset: {sunsetTime}
Cloud Cover: {weather.clouds.all}%";
    }

    private async Task<string> GetGeminiResponseAsync(string userMessage, string weatherContext)
    {
        try
        {
            var systemPrompt = @"You are a helpful weather assistant. Use the provided weather data to:
1. Answer questions about current conditions
2. Suggest activities based on the weather
3. Provide relevant weather safety tips if conditions are severe
4. Compare conditions to typical weather for the region
5. Explain meteorological phenomena in simple terms
Keep responses friendly and conversational.";

            var messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "system", Content = systemPrompt },
                new ChatMessage { Role = "system", Content = "Current weather data:\n" + weatherContext },
                new ChatMessage { Role = "user", Content = userMessage }
            };

            var request = new GeminiRequest { Messages = messages };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_geminiApiKey}",
                request);

            if (!response.IsSuccessStatusCode)
            {
                // Fallback to basic weather response if Gemini fails
                return GenerateBasicWeatherResponse(weatherContext);
            }

            var content = await response.Content.ReadFromJsonAsync<JsonDocument>();
            var responseText = content?.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return responseText ?? GenerateBasicWeatherResponse(weatherContext);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting Gemini response: {ex.Message}");
            return GenerateBasicWeatherResponse(weatherContext);
        }
    }

    private string GenerateBasicWeatherResponse(string weatherContext)
    {
        // Parse the weather context back into meaningful parts
        var lines = weatherContext.Split('\n');
        var basicInfo = string.Join("\n", lines.Take(4));

        return basicInfo + "\n\nBased on these conditions, it's a good idea to check the forecast before planning outdoor activities.";
    }

    private string ExtractLocation(string message)
    {
        string[] patterns = {
            "weather in ",
            "weather at ",
            "weather for ",
            "temperature in ",
            "temperature at ",
            "how's the weather in ",
            "what's the weather in ",
            "what is the weather in ",
            "how is the weather in ",
            "forecast for ",
            "conditions in "
        };

        foreach (var pattern in patterns)
        {
            var index = message.IndexOf(pattern);
            if (index != -1)
            {
                return message.Substring(index + pattern.Length).Trim();
            }
        }

        if (message.Split(' ').Length <= 2)
        {
            return message.Trim();
        }

        return string.Empty;
    }
}
