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
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();
            
            // Variables to hold the secrets.
            string lawId;
            string lawKey;
            string lawPrincipalClientId;
            string lawPrincipalClientSecret;
            string lawPrincipalDomain;
            
            try
            {
                string secretsLocation = Environment.GetEnvironmentVariable("SECRETS_LOCATION").ToLower();
                if (secretsLocation == "githubactions")
                {
                    // If these tests are run as part of a GitHub Action, we'll pick the secrets directly from the environment variables during the pipeline execution.
                    lawId = Environment.GetEnvironmentVariable("GHA_LAW_ID");
                    lawKey = Environment.GetEnvironmentVariable("GHA_LAW_KEY");
                    lawPrincipalClientId = Environment.GetEnvironmentVariable("GHA_LAW_PRINCIPAL_CLIENTID");
                    lawPrincipalClientSecret = Environment.GetEnvironmentVariable("GHA_LAW_PRINCIPAL_CLIENTSECRET");
                    lawPrincipalDomain = Environment.GetEnvironmentVariable("GHA_LAW_PRINCIPAL_DOMAIN");
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The environment variable 'SECRETS_LOCATION' needs to have a value of GitHubActions, or not exist");
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
                lawPrincipalClientId = lawPrincipalSection["ClientId"];
                lawPrincipalClientSecret = lawPrincipalSection["ClientSecret"];
                lawPrincipalDomain = lawPrincipalSection["Domain"];
            }

            LawSecrets lawSecrets = new LawSecrets
            {
                LawId = lawId,
                LawKey = lawKey
            };

            LawPrincipalCredentials lawPrincipalCredentials = new LawPrincipalCredentials
            {
                ClientId = lawPrincipalClientId,
                ClientSecret = lawPrincipalClientSecret,
                Domain = lawPrincipalDomain
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
