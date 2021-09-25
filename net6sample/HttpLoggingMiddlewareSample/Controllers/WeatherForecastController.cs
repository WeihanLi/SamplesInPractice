using Microsoft.AspNetCore.Mvc;

namespace HttpLoggingMiddlewareSample.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpPost]
    public IActionResult Post(System.Text.Json.JsonElement element) => Ok(element);

    [HttpPost("form")]
    public IActionResult Form([FromForm] string title)
    {
        return Ok(new
        {
            title
        });
    }

    [HttpPost("file")]
    public IActionResult PostFile([FromForm]string title, IFormFile file)
    {
        return Ok(new
        {
            title,
            file.FileName,
            file.Name,
            file.ContentType,
            file.ContentDisposition
        });
    }
}
