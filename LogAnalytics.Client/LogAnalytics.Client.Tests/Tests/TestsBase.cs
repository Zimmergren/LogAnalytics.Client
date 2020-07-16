using LogAnalyticsClient.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace LogAnalyticsClient.Tests.Tests
{
    /// <summary>
    /// Helper class to wire up local configuration and secrets. 
    /// When running this from a local dev box, user secrets are used for the Law Id and Law Key.
    /// </summary>
    public class TestsBase
    {
        public static LawSecrets InitSecrets()
        {
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) || devEnvironmentVariable.ToLower() == "development";

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (isDevelopment)
                configBuilder.AddUserSecrets<LawSecrets>();

            var configuration = configBuilder.Build();

            var lawSection = configuration.GetSection("LawConfiguration");
            LawSecrets secrets = new LawSecrets
            {
                LawId = lawSection["LawId"],
                LawKey = lawSection["LawKey"]
            };

            return secrets;
        }
    }
}
