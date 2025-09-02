namespace WeatherApi.Settings;

public class WeatherApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Units { get; set; } = "metric"; // metric or us
}


