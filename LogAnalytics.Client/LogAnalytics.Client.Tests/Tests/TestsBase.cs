using LogAnalytics.Client.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace LogAnalytics.Client.Tests.Tests
{
    /// <summary>
    /// Helper class to wire up local configuration and secrets. 
    /// When running this from a local dev box, user secrets are used for the Law Id and Law Key.
    /// </summary>
    public class TestsBase
    {
        /* For reference, this is the secrets.json file structure: 
            {
              "LawConfiguration": {
                "LawId": "",
                "LawKey": ""
              },
              "LawServicePrincipalCredentials": {
                "ClientId": "",
                "ClientSecret": "",
                "Domain": ""
              }
            }
         */
        public static TestSecrets InitSecrets()
        {
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) || devEnvironmentVariable.ToLower() == "development";

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (isDevelopment)
                configBuilder.AddUserSecrets<LawSecrets>();

            var configuration = configBuilder.Build();

            var lawConfigurationSection = configuration.GetSection("LawConfiguration");
            LawSecrets lawSecrets = new LawSecrets
            {
                LawId = lawConfigurationSection["LawId"],
                LawKey = lawConfigurationSection["LawKey"]
            };

            var lawPrincipalSection = configuration.GetSection("LawServicePrincipalCredentials");
            LawPrincipalCredentials lawPrincipalCredentials = new LawPrincipalCredentials
            {
                ClientId = lawPrincipalSection["ClientId"],
                ClientSecret = lawPrincipalSection["ClientSecret"],
                Domain = lawPrincipalSection["Domain"]
            };

            TestSecrets testSecrets = new TestSecrets
            {
                LawSecrets = lawSecrets,
                LawPrincipalCredentials = lawPrincipalCredentials,
            };

            return testSecrets;
        }
    }
}
