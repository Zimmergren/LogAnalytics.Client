using LogAnalytics.Client.IntegrationTests.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace LogAnalytics.Client.IntegrationTests
{
    /// <summary>
    /// Helper class to wire up local configuration and secrets. 
    /// When running this from a local dev box, user secrets are used for the Law Id and Law Key.
    /// </summary>
    public static class TestsBase
    {
        public static TestSecrets InitSecrets()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();
            
            // Variables to hold the secrets.
            string lawId;
            string lawKey;
            string lawPrincipalClientId;
            string lawPrincipalClientSecret;
            string lawTenantId;
            
            try
            {
                string secretsLocation = Environment.GetEnvironmentVariable("SECRETS_LOCATION").ToLower();
                if (secretsLocation == "githubactions")
                {
                    Console.WriteLine("Running GitHub Actions - Setting the variables now");

                    // If these tests are run as part of a GitHub Action, we'll pick the secrets directly from the environment variables during the pipeline execution.
                    lawId = Environment.GetEnvironmentVariable("GHA_LAW_ID");
                    if (!string.IsNullOrEmpty(lawId))
                        Console.WriteLine($"Law ID variable is wired up: {lawId.Substring(0,5)}"); // produce something for the GitHub Actions log to verify if variables are wired up.
                    lawKey = Environment.GetEnvironmentVariable("GHA_LAW_KEY");
                    lawTenantId = Environment.GetEnvironmentVariable("GHA_LAW_TENANTID");
                    lawPrincipalClientId = Environment.GetEnvironmentVariable("GHA_LAW_PRINCIPAL_CLIENTID");
                    lawPrincipalClientSecret = Environment.GetEnvironmentVariable("GHA_LAW_PRINCIPAL_CLIENTSECRET");
                }
                else
                {
                    throw new NullReferenceException("The environment variable 'SECRETS_LOCATION' needs to have a value of GitHubActions, or not exist");
                }
            }
            catch
            {
                // No variable is loaded. Assume user secrets. 
                configBuilder.AddUserSecrets<LawSecrets>();
                var configuration = configBuilder.Build();

                var lawConfigurationSection = configuration.GetSection("LawConfiguration");
                lawId = lawConfigurationSection["LawId"];
                lawKey = lawConfigurationSection["LawKey"];

                var lawPrincipalSection = configuration.GetSection("LawServicePrincipalCredentials");
                lawTenantId = lawPrincipalSection["TenantId"];
                lawPrincipalClientId = lawPrincipalSection["ClientId"];
                lawPrincipalClientSecret = lawPrincipalSection["ClientSecret"];
            }

            LawSecrets lawSecrets = new LawSecrets
            {
                LawId = lawId,
                LawKey = lawKey
            };

            LawPrincipalCredentials lawPrincipalCredentials = new LawPrincipalCredentials
            {
                TenantId = lawTenantId,
                ClientId = lawPrincipalClientId,
                ClientSecret = lawPrincipalClientSecret
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
