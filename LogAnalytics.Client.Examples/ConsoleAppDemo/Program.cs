
using System;
using System.Threading.Tasks;

using LogAnalytics.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleAppDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder() // load configuration
                .AddEnvironmentVariables() // from environment variables
                .AddCommandLine(args) // or command line arguments
                .AddJsonFile("appsettings.json") // or json file
                .AddUserSecrets<Program>() // or user secrets
                .Build();

            var services = new ServiceCollection()
                .AddLogAnalyticsClient(configuration.GetSection("LogAnalytics"))
                .BuildServiceProvider();

            var logger = services.GetRequiredService<LogAnalyticsClient>();

            await logger.SendLogEntry(new TestEntity
            {
                Category = "Foo",
                TestString = $"String Test",
                TestBoolean = true,
                TestDateTime = DateTime.UtcNow,
                TestDouble = 2.1,
                TestGuid = Guid.NewGuid()
            }, "demolog");
        }
    }
}
