using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LogAnalytics.Client;

using Microsoft.AspNetCore.Mvc;

namespace ApiDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly LogAnalyticsClient _logger;

        public WeatherForecastController(LogAnalyticsClient logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            var rng = new Random();
            var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToList();

            // send weather forecasts in to Log Analytics
            await _logger.SendLogEntries(forecast, "forecasts");

            return forecast;
        }
    }
}
