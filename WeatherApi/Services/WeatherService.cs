using Microsoft.Extensions.Options;
using StackExchange.Redis;
using WeatherApi.Settings;

namespace WeatherApi.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConnectionMultiplexer _redis;
    private readonly WeatherApiOptions _weatherOptions;
    private readonly RedisOptions _redisOptions;

    public WeatherService(
        IHttpClientFactory httpClientFactory,
        IConnectionMultiplexer redis,
        IOptions<WeatherApiOptions> weatherOptions,
        IOptions<RedisOptions> redisOptions)
    {
        _httpClientFactory = httpClientFactory;
        _redis = redis;
        _weatherOptions = weatherOptions.Value;
        _redisOptions = redisOptions.Value;
    }

    public async Task<WeatherResult> GetWeatherJsonAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return new WeatherResult(400, string.Empty, "city is required");
        }

        var normalizedCity = city.Trim().ToLowerInvariant();
        var cacheKey = $"weather:{normalizedCity}";
        var db = _redis.GetDatabase();

        var cached = await db.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            return new WeatherResult(200, cached!);
        }

        var client = _httpClientFactory.CreateClient();
        var url = $"{_weatherOptions.BaseUrl}/{Uri.EscapeDataString(city)}?unitGroup={_weatherOptions.Units}&key={_weatherOptions.ApiKey}&contentType=json";
        try
        {
            using var resp = await client.GetAsync(url);
            var status = (int)resp.StatusCode;
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new WeatherResult(404, string.Empty, "city not found");
                }
                return new WeatherResult(status, string.Empty, body);
            }

            if (!string.IsNullOrWhiteSpace(body))
            {
                await db.StringSetAsync(cacheKey, body, TimeSpan.FromSeconds(_redisOptions.DefaultTtlSeconds));
            }
            return new WeatherResult(200, body);
        }
        catch (HttpRequestException)
        {
            return new WeatherResult(503, string.Empty, "service unavailable");
        }
    }
}


