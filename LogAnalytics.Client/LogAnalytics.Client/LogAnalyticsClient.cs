using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogAnalytics.Client
{
    /// <summary>
    /// Client to send logs to Azure Log Analytics.
    /// </summary>
    public class LogAnalyticsClient : ILogAnalyticsClient
    {
        private string WorkspaceId { get; }
        private string SharedKey { get; }
        private string RequestBaseUrl { get; }

        private readonly HttpClient httpClient;

        /// <summary>
        /// Instantiate the LogAnalyticsClient object by specifying your Workspace Id and the Key.
        /// </summary>
        /// <param name="workspaceId">Azure Log Analytics Workspace ID</param>
        /// <param name="sharedKey">Azure Log Analytics Workspace Shared Key</param>
        public LogAnalyticsClient(string workspaceId, string sharedKey)
        {
            if (string.IsNullOrEmpty(workspaceId))
                throw new ArgumentNullException(nameof(workspaceId), "workspaceId cannot be null or empty");

            if (string.IsNullOrEmpty(sharedKey))
                throw new ArgumentNullException(nameof(sharedKey), "sharedKey cannot be null or empty");

            WorkspaceId = workspaceId;
            SharedKey = sharedKey;
            RequestBaseUrl = $"https://{WorkspaceId}.ods.opinsights.azure.com/api/logs?api-version={Consts.ApiVersion}";

            httpClient = new HttpClient();
        }


        /// <summary>
        /// Send an entity as a single log entry to Azure Log Analytics.
        /// </summary>
        /// <typeparam name="T">Entity Type</typeparam>
        /// <param name="entity">The object</param>
        /// <param name="logType">The log type</param>
        /// <returns>Does not return anything.</returns>
        public Task SendLogEntry<T>(T entity, string logType)
        {
            #region Argument validation

            if (entity == null)
                throw new ArgumentNullException(nameof(entity), $"parameter '{nameof(entity)}' cannot be null");

            if (logType.Length > 100)
                throw new ArgumentOutOfRangeException(nameof(logType), logType.Length, "The size limit for this parameter is 100 characters.");

            if (!IsAlphaOnly(logType))
                throw new ArgumentOutOfRangeException(nameof(logType), logType, "Log-Type can only contain alpha characters. It does not support numerics or special characters.");

            ValidatePropertyTypes(entity);

            #endregion

            List<T> list = new List<T> { entity };
            return SendLogEntries(list, logType);
        }


        /// <summary>
        /// Send a collection of entities in a batch to Azure Log Analytics.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="entities">The collection of objects</param>
        /// <param name="logType">The log type</param>
        /// <returns>Does not return anything.</returns>
        public async Task SendLogEntries<T>(List<T> entities, string logType)
        {
            #region Argument validation

            if (entities == null)
                throw new ArgumentNullException(nameof(entities), $"parameter '{nameof(entities)}' cannot be null");

            if (logType.Length > 100)
                throw new ArgumentOutOfRangeException(nameof(logType), logType.Length, "The size limit for this parameter is 100 characters.");

            if (!IsAlphaOnly(logType))
                throw new ArgumentOutOfRangeException(nameof(logType), logType, "Log-Type can only contain alpha characters. It does not support numerics or special characters.");

            foreach (var entity in entities)
                ValidatePropertyTypes(entity);

            #endregion

            var dateTimeNow = DateTime.UtcNow.ToString("r");

            var entityAsJson = JsonConvert.SerializeObject(entities);
            var authSignature = GetAuthSignature(entityAsJson, dateTimeNow);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", authSignature);
            httpClient.DefaultRequestHeaders.Add("Log-Type", logType);
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            httpClient.DefaultRequestHeaders.Add("x-ms-date", dateTimeNow);
            httpClient.DefaultRequestHeaders.Add("time-generated-field", ""); // if we want to extend this in the future to support custom date fields from the entity etc.

            HttpContent httpContent = new StringContent(entityAsJson, Encoding.UTF8);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

#pragma warning disable SecurityIntelliSenseCS // MS Security rules violation: false positive, we already operate over HTTPS (SSL) here.
            var response = await httpClient.PostAsync(new Uri(RequestBaseUrl), httpContent).ConfigureAwait(false);
#pragma warning restore SecurityIntelliSenseCS // MS Security rules violation

            // Bubble up exceptions if there are any, don't swallow them here. This lets consumers handle it better.
            response.EnsureSuccessStatusCode();
        }

        #region Helpers

        private string GetAuthSignature(string serializedJsonObject, string dateString)
        {
            string stringToSign = $"POST\n{Encoding.UTF8.GetBytes(serializedJsonObject).Length}\napplication/json\nx-ms-date:{dateString}\n/api/logs";
            string signedString;

            var encoding = new ASCIIEncoding();
            var sharedKeyBytes = Convert.FromBase64String(SharedKey);
            var stringToSignBytes = encoding.GetBytes(stringToSign);
            using (var hmacsha256Encryption = new HMACSHA256(sharedKeyBytes))
            {
                var hashBytes = hmacsha256Encryption.ComputeHash(stringToSignBytes);
                signedString = Convert.ToBase64String(hashBytes);
            }

            return $"SharedKey {WorkspaceId}:{signedString}";
        }
        private bool IsAlphaOnly(string str)
        {
            return Regex.IsMatch(str, @"^[a-zA-Z]+$");
        }
        private void ValidatePropertyTypes<T>(T entity)
        {
            // as of 2018-10-30, the allowed property types for log analytics, as defined here (https://docs.microsoft.com/en-us/azure/log-analytics/log-analytics-data-collector-api#record-type-and-properties) are: string, bool, double, datetime, guid.
            // anything else will be throwing an exception here.
            foreach (PropertyInfo propertyInfo in entity.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType != typeof(string) &&
                    propertyInfo.PropertyType != typeof(bool) &&
                    propertyInfo.PropertyType != typeof(double) &&
                    propertyInfo.PropertyType != typeof(int) &&     // Represented as a double in the system.
                    propertyInfo.PropertyType != typeof(long) &&
                    propertyInfo.PropertyType != typeof(DateTime) &&
                    propertyInfo.PropertyType != typeof(Guid))
                {
                    throw new ArgumentOutOfRangeException($"Property '{propertyInfo.Name}' of entity with type '{entity.GetType()}' is not one of the valid properties. Valid properties are String, Boolean, Double, Integer, DateTime, Guid.");
                }
            }
        }

        #endregion
    }
}
