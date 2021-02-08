using Microsoft.Azure.OperationalInsights;
using Microsoft.Rest.Azure.Authentication;
using System;
using System.Threading.Tasks;

namespace LogAnalytics.Client.IntegrationTests.Helpers
{
    public static class LawDataClientHelper
    {
        public static async Task<OperationalInsightsDataClient> GetLawDataClient(string workspaceId, string lawPrincipalClientId, string lawPrincipalClientSecret, string domain)
        {
            // Note 2020-07-26. This is from the Microsoft.Azure.OperationalInsights nuget, which haven't been updated since 2018. 
            // Possibly we'll look for a REST-approach instead, and create the proper client here.

            var authEndpoint = "https://login.microsoftonline.com";
            var tokenAudience = "https://api.loganalytics.io/";

            var adSettings = new ActiveDirectoryServiceSettings
            {
                AuthenticationEndpoint = new Uri(authEndpoint),
                TokenAudience = new Uri(tokenAudience),
                ValidateAuthority = true
            };

            var credentials = await ApplicationTokenProvider.LoginSilentAsync(domain, lawPrincipalClientId, lawPrincipalClientSecret, adSettings);

            var client = new OperationalInsightsDataClient(credentials)
            {
                WorkspaceId = workspaceId
            };

            return client;
        }
    }
}
