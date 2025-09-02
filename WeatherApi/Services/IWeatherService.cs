namespace WeatherApi.Services;

public record WeatherResult(int StatusCode, string Body, string? Error = null);

public interface IWeatherService
{
    Task<WeatherResult> GetWeatherJsonAsync(string city);
}


