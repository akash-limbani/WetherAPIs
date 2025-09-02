using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;
using WeatherApi.Services;
using WeatherApi.Settings;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configuration: bind options for external services
builder.Services.Configure<WeatherApiOptions>(builder.Configuration.GetSection("WeatherApi"));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));
builder.Services.AddHttpClient();

// Redis connection multiplexer
var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
}
else
{
    // Lazy local default to avoid nulls; will throw on first use if misconfigured
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect("localhost:6379"));
}

// Services
builder.Services.AddSingleton<IWeatherService, WeatherService>();

// Rate limiting (fixed window: 60 req/min per IP)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.PermitLimit = 60;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit = 0;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();
