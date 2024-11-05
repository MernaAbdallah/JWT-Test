using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Controllers
{

    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")]
    public class WeatherVersionTwoController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "v2", "v2", "v2", "v2", "v2", "v2", "v2", "v2", "v2", "v2"
        };

        private readonly ILogger<WeatherVersionTwoController> _logger;

        public WeatherVersionTwoController(ILogger<WeatherVersionTwoController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecastV2")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}

