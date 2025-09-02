using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherApi.Services;
using WeatherApi.Settings;

namespace WeatherApi.Controllers;

[ApiController]
[Route("weather")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IWeatherService _weatherService;

    public WeatherForecastController(
        ILogger<WeatherForecastController> logger,
        IWeatherService weatherService)
    {
        _logger = logger;
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest(new { error = "city is required" });
        }

        var result = await _weatherService.GetWeatherJsonAsync(city);
        if (result.StatusCode == 200)
        {
            return Content(result.Body, "application/json");
        }
        if (result.StatusCode == 404)
        {
            return NotFound(new { error = "city not found" });
        }
        if (result.StatusCode == 400)
        {
            return BadRequest(new { error = result.Error });
        }
        return StatusCode(result.StatusCode, new { error = result.Error });
    }
}
