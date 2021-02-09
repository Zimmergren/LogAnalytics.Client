using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LogAnalytics.Client;

namespace BlazorDemo.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly LogAnalyticsClient _logger;

        public WeatherForecastService(LogAnalyticsClient logger)
        {
            _logger = logger;
        }

        public async Task<List<WeatherForecast>> GetForecastAsync(DateTime startDate)
        {
            var rng = new Random();
            var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToList();

            // send weather forecasts in to Log Analytics
            await _logger.SendLogEntries(forecast, "forecasts");

            return forecast;
        }
    }
}
