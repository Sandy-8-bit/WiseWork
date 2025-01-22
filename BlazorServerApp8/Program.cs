using BlazorServerApp8.Data;
using Blazored.LocalStorage;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<WeatherService>(); // Assuming WeatherService is defined
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddScoped<SupabaseAuthService>();

// Register HttpClient
builder.Services.AddScoped<WeatherChatService>();
// Register HttpClient for dependency injection
builder.Services.AddHttpClient(); // This registers HttpClient with the default settings

// Add Blazored Local Storage
builder.Services.AddBlazoredLocalStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
