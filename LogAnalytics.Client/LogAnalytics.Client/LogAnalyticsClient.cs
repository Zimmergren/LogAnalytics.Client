using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LogAnalytics.Client
{
    /// <summary>
    /// Client to send logs to Azure Log Analytics.
    /// </summary>
    public class LogAnalyticsClient : ILogAnalyticsClient
    {
        private readonly HttpClient _httpClient;

        private string WorkspaceId { get; }

        private string SharedKey { get; }

        private string RequestBaseUrl { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogAnalyticsClient"/> class.
        /// </summary>
        /// <param name="client">HTTP Client instance</param>
        /// <param name="workspaceId">Azure Log Analytics Workspace ID</param>
        /// <param name="sharedKey">Azure Log Analytics Workspace Shared Key</param>
        private LogAnalyticsClient(HttpClient client, string workspaceId, string sharedKey)
        {
            if (string.IsNullOrEmpty(workspaceId))
            {
                throw new ArgumentNullException(nameof(workspaceId), "workspaceId cannot be null or empty");
            }

            if (string.IsNullOrEmpty(sharedKey))
            {
                throw new ArgumentNullException(nameof(sharedKey), "sharedKey cannot be null or empty");
            }

            if (!this.IsBase64String(sharedKey))
            {
                throw new ArgumentException($"{nameof(sharedKey)} must be a valid Base64 encoded string", nameof(sharedKey));
            }

            this.WorkspaceId = workspaceId;
            this.SharedKey = sharedKey;
            this.RequestBaseUrl = $"https://{this.WorkspaceId}.ods.opinsights.azure.com/api/logs?api-version={Consts.ApiVersion}";

            this._httpClient = client;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogAnalyticsClient"/> class.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="options"></param>
        public LogAnalyticsClient(HttpClient client, IOptions<LogAnalyticsClientOptions> options)
            : this(client, options.Value.WorkspaceId, options.Value.SharedKey) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogAnalyticsClient"/> class.
        /// </summary>
        /// <param name="workspaceId">Azure Log Analytics Workspace ID</param>
        /// <param name="sharedKey">Azure Log Analytics Workspace Shared Key</param>
        public LogAnalyticsClient(string workspaceId, string sharedKey)
            : this(new HttpClient(), workspaceId, sharedKey)
        {
        }

        /// <summary>
        /// Send an entity as a single log entry to Azure Log Analytics.
        /// </summary>
        /// <typeparam name="T">Entity Type</typeparam>
        /// <param name="entity">The object</param>
        /// <param name="logType">The log type</param>
        /// <param name="resourceId">The resource id</param>
        /// <param name="timeGeneratedCustomFieldName">The name of the field that contains the Time Generated data</param>
        /// <returns>Does not return anything.</returns>
        public Task SendLogEntry<T>(T entity, string logType, string resourceId = null, string timeGeneratedCustomFieldName = null)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), $"parameter '{nameof(entity)}' cannot be null");
            }

            if (string.IsNullOrEmpty(logType))
            {
                throw new ArgumentNullException(nameof(logType), $"parameter '{nameof(logType)}' cannot be null, and must contain a string.");
            }

            if (logType.Length > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(logType), logType.Length, "The size limit for this parameter is 100 characters.");
            }

            if (!this.IsAlphaNumUnderscore(logType))
            {
                throw new ArgumentOutOfRangeException(nameof(logType), logType, "Log-Type can only contain letters, numbers, and underscore (_). It does not support numerics or special characters.");
            }

            this.ValidatePropertyTypes(entity);
            List<T> list = new List<T> { entity };
            return this.SendLogEntries(list, logType, resourceId, timeGeneratedCustomFieldName);
        }


        /// <summary>
        /// Send a collection of entities in a batch to Azure Log Analytics.
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="entities">The collection of objects</param>
        /// <param name="logType">The log type</param>
        /// <param name="resourceId">The resource id</param>
        /// <param name="timeGeneratedCustomFieldName">The name of the field that contains the Time Generated data</param>
        /// <returns>Does not return anything.</returns>
        public async Task SendLogEntries<T>(List<T> entities, string logType, string resourceId = null, string timeGeneratedCustomFieldName = null)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities), $"parameter '{nameof(entities)}' cannot be null");
            }

            if (string.IsNullOrEmpty(logType))
            {
                throw new ArgumentNullException(nameof(logType), $"parameter '{nameof(logType)}' cannot be null, and must contain a string.");
            }

            if (logType.Length > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(logType), logType.Length, "The size limit for this parameter is 100 characters.");
            }

            if (!this.IsAlphaNumUnderscore(logType))
            {
                throw new ArgumentOutOfRangeException(nameof(logType), logType, "Log-Type can only contain letters, numbers, and underscore (_). It does not support numerics or special characters.");
            }

            foreach (var entity in entities)
            {
                this.ValidatePropertyTypes(entity);
            }

            // Room for improvement: Identify if there is a timeGeneratedCustomFieldName specified, and if so, ensure the value of the field conforms with the ISO 8601 datetime format.

            var dateTimeNow = DateTime.UtcNow.ToString("r", System.Globalization.CultureInfo.InvariantCulture);

            var entityAsJson = JsonConvert.SerializeObject(entities, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            var authSignature = this.GetAuthSignature(entityAsJson, dateTimeNow);

            using var request = new HttpRequestMessage(HttpMethod.Post, this.RequestBaseUrl);
            request.Headers.Clear();
            request.Headers.Add("Authorization", authSignature);
            request.Headers.Add("Log-Type", logType);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("x-ms-date", dateTimeNow);
            if (!string.IsNullOrWhiteSpace(timeGeneratedCustomFieldName))
            {
                // The name of the field that contains custom timestamp data.
                request.Headers.Add("time-generated-field", timeGeneratedCustomFieldName);
            }
            if (!string.IsNullOrWhiteSpace(resourceId))
            {
                // The Resource ID in Azure for a given resource to connect the logs with.
                request.Headers.Add("x-ms-AzureResourceId", resourceId);
            }

            HttpContent httpContent = new StringContent(entityAsJson, Encoding.UTF8);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            request.Content = httpContent;
            var response = await this._httpClient.SendAsync(request).ConfigureAwait(false);

            // Bubble up exceptions if there are any, don't swallow them here. This lets consumers handle it better.
            response.EnsureSuccessStatusCode();
        }

        private string GetAuthSignature(string serializedJsonObject, string dateString)
        {
            string stringToSign = $"POST\n{Encoding.UTF8.GetBytes(serializedJsonObject).Length}\napplication/json\nx-ms-date:{dateString}\n/api/logs";
            string signedString;

            var encoding = new ASCIIEncoding();
            var sharedKeyBytes = Convert.FromBase64String(this.SharedKey);
            var stringToSignBytes = encoding.GetBytes(stringToSign);
            using (var hmacsha256Encryption = new HMACSHA256(sharedKeyBytes))
            {
                var hashBytes = hmacsha256Encryption.ComputeHash(stringToSignBytes);
                signedString = Convert.ToBase64String(hashBytes);
            }

            return $"SharedKey {this.WorkspaceId}:{signedString}";
        }

        private bool IsAlphaNumUnderscore(string str)
        {
            return Regex.IsMatch(str, @"^[a-zA-Z0-9_]+$");
        }

        private bool IsBase64String(string str)
        {
            str = str.Trim();
            return (str.Length % 4 == 0) && Regex.IsMatch(str, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        private void ValidatePropertyTypes<T>(T entity)
        {
            // as of 2018-10-30, the allowed property types for log analytics, as defined here (https://docs.microsoft.com/en-us/azure/log-analytics/log-analytics-data-collector-api#record-type-and-properties) are: string, bool, double, datetime, guid.
            // also allow them as nullable
            // anything else will be throwing an exception here.
            foreach (PropertyInfo propertyInfo in entity.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType != typeof(string) &&
                    propertyInfo.PropertyType != typeof(bool) &&
                    propertyInfo.PropertyType != typeof(bool?) &&
                    propertyInfo.PropertyType != typeof(double) &&
                    propertyInfo.PropertyType != typeof(double?) &&
                    propertyInfo.PropertyType != typeof(int) &&     // Represented as a double in the system.
                    propertyInfo.PropertyType != typeof(int?) &&
                    propertyInfo.PropertyType != typeof(long) &&
                    propertyInfo.PropertyType != typeof(long?) &&
                    propertyInfo.PropertyType != typeof(DateTime) &&
                    propertyInfo.PropertyType != typeof(DateTime?) &&
                    propertyInfo.PropertyType != typeof(Guid) &&
                    propertyInfo.PropertyType != typeof(Guid?))
                {
                    throw new ArgumentOutOfRangeException($"Property '{propertyInfo.Name}' of entity with type '{entity.GetType()}' is not one of the valid properties. Valid properties are String, Boolean, Double, Integer, DateTime, Guid.");
                }
            }
        }
    }
}
