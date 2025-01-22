using Supabase.Gotrue;
using System.Text.Json;
using Microsoft.JSInterop;

public class SupabaseAuthService
{
    private readonly Supabase.Client _client;
    private readonly IJSRuntime _jsRuntime;

    public SupabaseAuthService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;

        var options = new Supabase.ClientOptions
        {
            AutoRefreshToken = true,
            PersistSession = true
        };

        _client = new Supabase.Client(
            "https://zmbwovmoriuapqluaxjn.supabase.co",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InptYndvdm1vcml1YXBxbHVheGpuIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTczNzM1MTE3MSwiZXhwIjoyMDUyOTI3MTcxfQ.GprfFxMF6eUjFu9x0ecFqYlTsghZc5ml0wV5-S6wIvY",
            options
        );
    }

    public async Task InitializeAsync()
    {
        // Check if JS runtime is available
        if (_jsRuntime is not null)
        {
            await _client.InitializeAsync();

            var sessionJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "supabaseSession");
            if (!string.IsNullOrEmpty(sessionJson))
            {
                try
                {
                    var session = JsonSerializer.Deserialize<Session>(sessionJson);
                    if (session != null)
                    {
                        await _client.Auth.SetSession(session.AccessToken, session.RefreshToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to restore session: {ex.Message}");
                }
            }
        }
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        try
        {
            var session = await _client.Auth.SignIn(email, password);

            if (session != null)
            {
                var sessionJson = JsonSerializer.Serialize(session);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "supabaseSession", sessionJson);
                return session.User;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
            throw;
        }
    }

    public async Task<User?> RegisterAsync(string email, string password)
    {
        try
        {
            var session = await _client.Auth.SignUp(email, password);

            if (session != null)
            {
                var sessionJson = JsonSerializer.Serialize(session);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "supabaseSession", sessionJson);
                return session.User;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registration failed: {ex.Message}");
            throw;
        }
    }

    public async Task LogoutAsync()
    {
        await _client.Auth.SignOut();
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "supabaseSession");
    }

    public User? GetCurrentUser()
    {
        return _client.Auth.CurrentUser;
    }
}
