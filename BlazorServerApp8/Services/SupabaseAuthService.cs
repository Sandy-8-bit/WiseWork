using Supabase.Gotrue;

public class SupabaseAuthService
{
    private readonly Supabase.Client _client;

    public SupabaseAuthService()
    {
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
        await _client.InitializeAsync();
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        try
        {
            var session = await _client.Auth.SignIn(email, password);
            return session?.User;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
            throw; // Rethrow the exception to propagate it
        }
    }

    public async Task<User?> RegisterAsync(string email, string password)
    {
        try
        {
            var session = await _client.Auth.SignUp(email, password);
            return session?.User;
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
    }

    public User? GetCurrentUser()
    {
        return _client.Auth.CurrentUser;
    }
}
