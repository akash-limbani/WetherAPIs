namespace WeatherApi.Settings;

public class RedisOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int DefaultTtlSeconds { get; set; } = 43200; // 12 hours
}


